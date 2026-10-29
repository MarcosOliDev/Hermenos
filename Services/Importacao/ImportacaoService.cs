using Hermenos.Data;
using Hermenos.DTOs.Importacao;
using Hermenos.Enums;
using Hermenos.Models;
using Hermenos.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Hermenos.Services.Importacao;

public sealed class ImportacaoService(HermenosDbContext db, MotorConciliacao motor, ILogger<ImportacaoService> logger)
	: IImportacaoService
{
	public const long TamanhoMaximoArquivoBytes = 10L * 1024 * 1024;
	public const long TamanhoMaximoTotalBytes = 50L * 1024 * 1024;
	private const int MaximoMensagens = 8;

	private sealed class Contagem
	{
		public int Encontrados;
		public int Rejeitados;
		public int Duplicados;
		public List<string> Mensagens { get; } = [];
		public int Validos => Math.Max(0, Encontrados - Rejeitados - Duplicados);

		public void Avisar(string mensagem)
		{
			if (Mensagens.Count < MaximoMensagens) Mensagens.Add(mensagem);
		}
	}

	public async Task<ResultadoImportacaoDTO> ImportarAsync(int empresaId, DateOnly competencia, string nomeArquivo,
		Stream conteudo, CancellationToken ct = default)
	{
		var nomeSeguro = TextoUtil.Limitar(Path.GetFileName(nomeArquivo), 260);
		var empresa = await db.Empresas.AsNoTracking().SingleOrDefaultAsync(e => e.IdEmpresa == empresaId, ct);
		if (empresa is null)
		{
			logger.LogWarning("Importação recusada: empresa inexistente. EmpresaId {EmpresaId}", empresaId);
			return Recusado(nomeSeguro, "Empresa não encontrada.");
		}

		var formato = Path.GetExtension(nomeSeguro).ToLowerInvariant() switch
		{
			".xml" => "XML",
			".csv" => "CSV",
			".json" => "JSON",
			".xlsx" or ".xls" => null,
			_ => string.Empty
		};
		if (formato is null)
			return Recusado(nomeSeguro, "Planilhas Excel ainda não são lidas. Exporte o arquivo como CSV e envie novamente.");
		if (formato.Length == 0)
			return Recusado(nomeSeguro, "Formato não suportado. Envie arquivos .xml, .csv ou .json.");

		var lote = new LoteImportacao
		{
			IdEmpresa = empresaId,
			Competencia = new DateOnly(competencia.Year, competencia.Month, 1),
			NomeArquivo = nomeSeguro,
			Formato = formato,
			Origem = formato == "XML" ? "SEFAZ / XML" : "ERP",
			RecebidoEm = DateTimeOffset.UtcNow,
			Status = StatusLoteImportacao.EmProcessamento
		};
		db.LotesImportacao.Add(lote);
		await db.SaveChangesAsync(ct);
		logger.LogInformation("Lote {LoteId} recebido. EmpresaId {EmpresaId}; Formato {Formato}", lote.LoteImpId, empresaId, formato);

		try
		{
			await using var transacao = await db.Database.BeginTransactionAsync(ct);
			var contagem = formato == "XML"
				? await ImportarXmlAsync(lote, empresa, conteudo, ct)
				: await ImportarErpAsync(lote, empresa, conteudo, formato, ct);

			lote.RegistrosEncontrados = contagem.Encontrados;
			lote.RegistrosValidos = contagem.Validos;
			lote.RegistrosRejeitados = contagem.Rejeitados;
			lote.RegistrosDuplicados = contagem.Duplicados;
			lote.Status = contagem.Encontrados == 0 || (contagem.Validos == 0 && contagem.Rejeitados > 0)
				? StatusLoteImportacao.FalhaNoProcessamento
				: contagem.Rejeitados > 0 || contagem.Duplicados > 0
					? StatusLoteImportacao.ConcluidoComAlertas
					: StatusLoteImportacao.Concluido;
			await db.SaveChangesAsync(ct);

			await motor.ExecutarAsync(empresaId, ct);
			await transacao.CommitAsync(ct);

			logger.LogInformation("Lote {LoteId} processado. Encontrados {Encontrados}; Válidos {Validos}; Rejeitados {Rejeitados}; Duplicados {Duplicados}",
				lote.LoteImpId, contagem.Encontrados, contagem.Validos, contagem.Rejeitados, contagem.Duplicados);
			return new ResultadoImportacaoDTO(lote.Status != StatusLoteImportacao.FalhaNoProcessamento, lote.LoteImpId,
				nomeSeguro, contagem.Encontrados, contagem.Validos, contagem.Rejeitados, contagem.Duplicados,
				RotulosStatus.Lote(lote.Status), contagem.Mensagens);
		}
		catch (Exception ex) when (ex is not OperationCanceledException)
		{
			logger.LogError(ex, "Falha ao processar o lote {LoteId}. EmpresaId {EmpresaId}", lote.LoteImpId, empresaId);
			db.ChangeTracker.Clear();
			var falho = await db.LotesImportacao.SingleAsync(l => l.LoteImpId == lote.LoteImpId, CancellationToken.None);
			falho.Status = StatusLoteImportacao.FalhaNoProcessamento;
			await db.SaveChangesAsync(CancellationToken.None);
			return new ResultadoImportacaoDTO(false, falho.LoteImpId, nomeSeguro, 0, 0, 0, 0,
				RotulosStatus.Lote(falho.Status), ["Erro inesperado ao processar o arquivo. Nada foi gravado."]);
		}
	}

	private async Task<Contagem> ImportarXmlAsync(LoteImportacao lote, Empresa empresa, Stream conteudo, CancellationToken ct)
	{
		var contagem = new Contagem();
		var leitura = LeitorNFeXml.Ler(conteudo);
		foreach (var erro in leitura.Erros)
		{
			contagem.Encontrados++;
			contagem.Rejeitados++;
			contagem.Avisar(erro);
		}

		var cnpjEmpresa = TextoUtil.Digitos(empresa.Cnpj);
		var chaves = leitura.Notas.Select(n => n.Chave).ToList();
		var existentes = await db.DocumentosFiscais
			.Where(d => d.ChaveAcesso != null && chaves.Contains(d.ChaveAcesso))
			.ToDictionaryAsync(d => d.ChaveAcesso!, ct);
		var cacheFornecedores = new Dictionary<string, int>();
		var vistas = new HashSet<string>();

		foreach (var nota in leitura.Notas)
		{
			contagem.Encontrados++;
			if (!vistas.Add(nota.Chave)) { contagem.Duplicados++; continue; }

			if (existentes.TryGetValue(nota.Chave, out var existente))
			{
				// Mesma chave em outra empresa nunca é aceita: dados de empresas não se misturam.
				if (existente.EmpresaId != empresa.IdEmpresa)
				{
					contagem.Rejeitados++;
					contagem.Avisar($"NF-e {nota.Numero}: chave já registrada em outro cadastro.");
				}
				else if (nota.Cancelada && existente.Status != StatusDocumento.Cancelado)
					existente.Status = StatusDocumento.Cancelado;
				else
					contagem.Duplicados++;
				continue;
			}

			var empresaEhDestino = nota.DestinatarioCnpj == cnpjEmpresa;
			if (!empresaEhDestino && nota.EmitenteCnpj != cnpjEmpresa)
			{
				contagem.Rejeitados++;
				contagem.Avisar($"NF-e {nota.Numero}: o CNPJ da nota não corresponde à empresa selecionada.");
				continue;
			}

			var (cnpjParte, nomeParte) = empresaEhDestino
				? (nota.EmitenteCnpj, nota.EmitenteNome) : (nota.DestinatarioCnpj, nota.DestinatarioNome);
			var fornecedorId = await ObterFornecedorIdAsync(cnpjParte, nomeParte, cacheFornecedores, ct);
			var emissao = TextoUtil.ComoUtc(nota.Emissao);

			// Se o ERP chegou primeiro, o documento "somente ERP" é completado em vez de duplicado.
			var candidatos = await db.DocumentosFiscais
				.Where(d => d.EmpresaId == empresa.IdEmpresa && d.ChaveAcesso == null
					&& d.Tipo == DocumentoFiscal.TipoSomenteErp && d.Numero == nota.Numero)
				.ToListAsync(ct);
			// Entidades já alteradas neste arquivo ainda constam no banco com o valor antigo; o filtro em memória as exclui.
			candidatos = candidatos.Where(d => d.ChaveAcesso == null && d.Tipo == DocumentoFiscal.TipoSomenteErp).ToList();
			var documento = candidatos.FirstOrDefault(d => d.DataEmissao.Date == emissao.Date)
				?? (candidatos.Count == 1 ? candidatos[0] : null);
			if (documento is null)
			{
				documento = new DocumentoFiscal { EmpresaId = empresa.IdEmpresa };
				db.DocumentosFiscais.Add(documento);
			}

			documento.FornecedorId = fornecedorId;
			documento.Numero = nota.Numero;
			documento.Tipo = nota.Tipo;
			documento.ChaveAcesso = nota.Chave;
			documento.DataEmissao = emissao;
			documento.ValorDocumento = nota.Valor;
			documento.QuantidadeItens = nota.QuantidadeItens;
			documento.Status = nota.Cancelada ? StatusDocumento.Cancelado : StatusDocumento.AguardandoConciliacao;
			documento.LoteImportacaoId = lote.LoteImpId;
		}

		// Eventos de cancelamento enviados sem a NF-e no mesmo arquivo.
		var tratadas = leitura.Notas.Select(n => n.Chave).ToHashSet();
		foreach (var chave in leitura.ChavesCanceladas.Where(k => !tratadas.Contains(k)))
		{
			contagem.Encontrados++;
			var documento = await db.DocumentosFiscais
				.FirstOrDefaultAsync(d => d.EmpresaId == empresa.IdEmpresa && d.ChaveAcesso == chave, ct);
			if (documento is null)
			{
				contagem.Rejeitados++;
				contagem.Avisar($"Cancelamento da chave {chave} sem NF-e registrada para esta empresa.");
				continue;
			}
			documento.Status = StatusDocumento.Cancelado;
		}

		return contagem;
	}

	private async Task<Contagem> ImportarErpAsync(LoteImportacao lote, Empresa empresa, Stream conteudo, string formato,
		CancellationToken ct)
	{
		var contagem = new Contagem();
		var leitura = LeitorRegistroErp.Ler(conteudo, formato);
		foreach (var erro in leitura.Erros)
		{
			contagem.Encontrados++;
			contagem.Rejeitados++;
			contagem.Avisar(erro);
		}
		if (leitura.Linhas.Count == 0) return contagem;

		var empresaId = empresa.IdEmpresa;
		var numeros = leitura.Linhas.Select(l => l.Numero).Distinct().ToList();
		var chaves = leitura.Linhas.Where(l => l.Chave is not null).Select(l => l.Chave!).Distinct().ToList();
		var documentos = await db.DocumentosFiscais
			.Where(d => d.EmpresaId == empresaId
				&& (numeros.Contains(d.Numero) || (d.ChaveAcesso != null && chaves.Contains(d.ChaveAcesso))))
			.ToListAsync(ct);
		var jaImportados = await db.RegistrosERP.AsNoTracking()
			.Where(r => r.EmpresaId == empresaId && numeros.Contains(r.NumeroDocumento))
			.Select(r => new { r.NumeroDocumento, r.Fornecedor }).ToListAsync(ct);

		// Mesmo número e fornecedor já visto = possível duplicidade (o registro é guardado e sinalizado).
		var vistos = jaImportados.Select(r => (r.NumeroDocumento, TextoUtil.Normalizar(r.Fornecedor))).ToHashSet();
		var somenteErp = new Dictionary<(string, string), DocumentoFiscal>();
		var alvos = new List<(LinhaErp Linha, DocumentoFiscal Documento)>();

		foreach (var linha in leitura.Linhas)
		{
			contagem.Encontrados++;
			var fornecedor = TextoUtil.Normalizar(linha.Fornecedor);
			if (!vistos.Add((linha.Numero, fornecedor))) contagem.Duplicados++;

			var documento = EncontrarDocumento(documentos, linha);
			if (documento is null && !somenteErp.TryGetValue((linha.Numero, fornecedor), out documento))
			{
				documento = new DocumentoFiscal
				{
					EmpresaId = empresaId,
					Numero = linha.Numero,
					Tipo = DocumentoFiscal.TipoSomenteErp,
					DataEmissao = TextoUtil.ComoUtc(linha.Emissao ?? lote.Competencia.ToDateTime(TimeOnly.MinValue)),
					Status = StatusDocumento.AguardandoConciliacao,
					LoteImportacaoId = lote.LoteImpId
				};
				somenteErp[(linha.Numero, fornecedor)] = documento;
				db.DocumentosFiscais.Add(documento);
			}
			alvos.Add((linha, documento));
		}

		await db.SaveChangesAsync(ct); // gera os ids dos documentos novos antes de ligar os registros
		foreach (var (linha, documento) in alvos)
		{
			db.RegistrosERP.Add(new RegistroERP
			{
				EmpresaId = empresaId,
				DocumentoFiscalId = documento.Id,
				NumeroDocumento = linha.Numero,
				Fornecedor = linha.Fornecedor.Length > 0 ? TextoUtil.Limitar(linha.Fornecedor, 200) : "Não informado",
				DataEmissao = linha.Emissao is DateTime data ? TextoUtil.ComoUtc(data) : null,
				ValorDocumento = linha.Valor,
				QuantidadeItens = linha.Quantidade,
				Status = linha.Cancelado ? StatusDocumento.Cancelado : StatusDocumento.AguardandoConciliacao
			});
		}
		return contagem;
	}

	private static DocumentoFiscal? EncontrarDocumento(List<DocumentoFiscal> documentos, LinhaErp linha)
	{
		if (linha.Chave is not null)
		{
			var porChave = documentos.FirstOrDefault(d => d.ChaveAcesso == linha.Chave);
			if (porChave is not null) return porChave;
		}

		var candidatos = documentos.Where(d => d.Numero == linha.Numero).ToList();
		if (candidatos.Count == 0) return null;
		if (linha.Emissao is DateTime data)
		{
			var mesmaData = candidatos.FirstOrDefault(d => d.DataEmissao.Date == data.Date);
			if (mesmaData is not null) return mesmaData;
			return candidatos.Count == 1 ? candidatos[0] : null;
		}
		return candidatos[0];
	}

	private async Task<int?> ObterFornecedorIdAsync(string cnpjDigitos, string nome, Dictionary<string, int> cache,
		CancellationToken ct)
	{
		if (cnpjDigitos.Length is not (11 or 14)) return null;
		var documento = TextoUtil.FormatarCnpj(cnpjDigitos);
		if (cache.TryGetValue(documento, out var idEmCache)) return idEmCache;

		var existente = await db.Fornecedores.AsNoTracking().FirstOrDefaultAsync(f => f.DocumentoFiscal == documento, ct);
		if (existente is not null)
		{
			cache[documento] = existente.IdFornecedor;
			return existente.IdFornecedor;
		}

		var novo = new Fornecedor
		{
			NomeRazaoSocial = nome.Length > 0 ? TextoUtil.Limitar(nome, 200) : "Não informado",
			DocumentoFiscal = documento,
			Status = StatusFornecedor.Ativo,
			CriadoEm = DateTimeOffset.UtcNow
		};
		db.Fornecedores.Add(novo);
		await db.SaveChangesAsync(ct);
		cache[documento] = novo.IdFornecedor;
		return novo.IdFornecedor;
	}

	private static ResultadoImportacaoDTO Recusado(string arquivo, string mensagem)
		=> new(false, null, arquivo, 0, 0, 0, 0, "Recusado", [mensagem]);
}
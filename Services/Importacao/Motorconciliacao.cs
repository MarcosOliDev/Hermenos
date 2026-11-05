using System.Globalization;
using Hermenos.Data;
using Hermenos.Enums;
using Hermenos.Models;
using Microsoft.EntityFrameworkCore;

namespace Hermenos.Services.Importacao;

public sealed class MotorConciliacao(HermenosDbContext db, ILogger<MotorConciliacao> logger)
{
	public const string CategoriaAusente = "Documento ausente em uma das fontes";
	public const string CategoriaValor = "Diferença de valor";
	public const string CategoriaQuantidade = "Diferença de quantidade";
	public const string CategoriaDuplicidade = "Possível duplicidade";
	public const string CategoriaCancelamento = "Situação de cancelamento para revisão";

	private static readonly CultureInfo Brasil = CultureInfo.GetCultureInfo("pt-BR");

	private sealed record Achado(string Categoria, string Campo, string ValorXml, string ValorErp);

	public async Task ExecutarAsync(int empresaId, CancellationToken ct)
	{
		var agora = DateTime.UtcNow;
		var formatos = await db.LotesImportacao
			.Where(l => l.IdEmpresa == empresaId && l.Status != StatusLoteImportacao.FalhaNoProcessamento)
			.Select(l => l.Formato).ToListAsync(ct);
		var temXml = formatos.Contains("XML");
		var temErp = formatos.Any(f => f != "XML");

		var documentos = await db.DocumentosFiscais.Where(d => d.EmpresaId == empresaId).ToListAsync(ct);
		var registros = (await db.RegistrosERP.Where(r => r.EmpresaId == empresaId && r.DocumentoFiscalId != null)
			.ToListAsync(ct)).ToLookup(r => r.DocumentoFiscalId!.Value);
		var divergencias = (await db.Divergencias.Where(d => d.IdEmpresa == empresaId).ToListAsync(ct))
			.ToLookup(d => d.IdDocumentoFiscal);
		var conciliacoes = (await db.Conciliacoes.Where(c => c.EmpresaId == empresaId).ToListAsync(ct))
			.ToDictionary(c => c.DocumentoFiscalId);

		var totalAchados = 0;
		foreach (var documento in documentos)
		{
			var erp = registros[documento.Id].OrderBy(r => r.RegERPId).ToList();
			var avaliavel = EhAvaliavel(documento, erp, temXml, temErp);
			var achados = avaliavel ? Avaliar(documento, erp) : new List<Achado>();
			totalAchados += achados.Count;

			AtualizarStatus(documento, achados, avaliavel);
			SincronizarDivergencias(empresaId, documento, achados, divergencias[documento.Id].ToList(), agora);
			AtualizarConciliacao(empresaId, documento, erp, achados, avaliavel, conciliacoes, agora);
		}

		await db.SaveChangesAsync(ct);
		logger.LogInformation("Conciliação executada. EmpresaId {EmpresaId}; Documentos {Documentos}; Achados {Achados}",
			empresaId, documentos.Count, totalAchados);
	}

	private static bool EhAvaliavel(DocumentoFiscal documento, List<RegistroERP> erp, bool temXml, bool temErp)
		=> documento.Tipo == DocumentoFiscal.TipoSomenteErp ? temXml : (erp.Count > 0 || temErp);

	private static List<Achado> Avaliar(DocumentoFiscal documento, List<RegistroERP> erp)
	{
		var achados = new List<Achado>();

		if (documento.Tipo == DocumentoFiscal.TipoSomenteErp)
		{
			achados.Add(new Achado(CategoriaAusente, "Documento", "Não localizado", "Localizado no ERP"));
			return achados;
		}
		if (erp.Count == 0)
		{
			achados.Add(new Achado(CategoriaAusente, "Documento", "Localizado no XML", "Não localizado"));
			return achados;
		}

		var principal = erp[0];
		if (documento.ValorDocumento is decimal valorXml && principal.ValorDocumento is decimal valorErp && valorXml != valorErp)
			achados.Add(new Achado(CategoriaValor, "Valor total", Moeda(valorXml), Moeda(valorErp)));

		if (documento.QuantidadeItens is decimal quantidadeXml && principal.QuantidadeItens is decimal quantidadeErp
			&& quantidadeXml != quantidadeErp)
			achados.Add(new Achado(CategoriaQuantidade, "Quantidade de itens",
				quantidadeXml.ToString("0.####", Brasil), quantidadeErp.ToString("0.####", Brasil)));

		if (erp.Count > 1)
			achados.Add(new Achado(CategoriaDuplicidade, "Número do documento", "1 ocorrência", $"{erp.Count} registros no ERP"));

		var canceladoXml = documento.Status == StatusDocumento.Cancelado;
		var canceladoErp = erp.Any(r => r.Status == StatusDocumento.Cancelado);
		if (canceladoXml != canceladoErp)
			achados.Add(new Achado(CategoriaCancelamento, "Status",
				canceladoXml ? "Cancelado no XML" : "Ativo no XML", canceladoErp ? "Cancelado no ERP" : "Ativo no ERP"));

		return achados;
	}

	private static void AtualizarStatus(DocumentoFiscal documento, List<Achado> achados, bool avaliavel)
	{
		if (documento.Status == StatusDocumento.Cancelado) return;
		documento.Status = !avaliavel ? StatusDocumento.AguardandoConciliacao
			: achados.Count > 0 ? StatusDocumento.Divergente : StatusDocumento.Conciliado;
	}

	private void SincronizarDivergencias(int empresaId, DocumentoFiscal documento, List<Achado> achados,
		List<Divergencia> existentes, DateTime agora)
	{
		foreach (var achado in achados)
		{
			var atual = existentes.FirstOrDefault(d => d.Categoria == achado.Categoria);
			if (atual is null)
			{
				db.Divergencias.Add(new Divergencia
				{
					IdEmpresa = empresaId,
					IdDocumentoFiscal = documento.Id,
					Categoria = achado.Categoria,
					CampoAfetado = achado.Campo,
					ValorFonteXml = achado.ValorXml,
					ValorFonteERP = achado.ValorErp,
					IdentificadaEm = agora,
					Status = StatusDivergencia.Pendente
				});
				continue;
			}
			atual.CampoAfetado = achado.Campo;
			atual.ValorFonteXml = achado.ValorXml;
			atual.ValorFonteERP = achado.ValorErp;
		}

		foreach (var obsoleta in existentes.Where(d => achados.All(a => a.Categoria != d.Categoria)
			&& d.Status == StatusDivergencia.Pendente))
			db.Divergencias.Remove(obsoleta);
	}

	private void AtualizarConciliacao(int empresaId, DocumentoFiscal documento, List<RegistroERP> erp,
		List<Achado> achados, bool avaliavel, Dictionary<int, Conciliacao> conciliacoes, DateTime agora)
	{
		var regra = !avaliavel ? "Aguardando a outra fonte de dados"
			: achados.Count == 0 ? "Valor, quantidade e situação conferem entre XML e ERP"
			: string.Join("; ", achados.Select(a => a.Categoria));
		regra = TextoUtil.Limitar(regra, 500);

		if (!conciliacoes.TryGetValue(documento.Id, out var conciliacao))
		{
			conciliacao = new Conciliacao { EmpresaId = empresaId, DocumentoFiscalId = documento.Id };
			db.Conciliacoes.Add(conciliacao);
		}
		conciliacao.RegistroERPId = erp.FirstOrDefault()?.RegERPId;
		conciliacao.ExecutadaEm = agora;
		conciliacao.Status = documento.Status;
		conciliacao.RegraAplicada = regra;
	}

	private static string Moeda(decimal valor) => valor.ToString("C2", Brasil);
}
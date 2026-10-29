using Hermenos.DTOs.Divergencia;
using Hermenos.DTOs.DocumentoFiscal;
using Hermenos.DTOs.Empresa;
using Hermenos.DTOs.Importacao;
using Hermenos.Enums;
using Hermenos.Models;
using Hermenos.Services.Importacao;
using Microsoft.EntityFrameworkCore;

namespace Hermenos.Data;

public sealed class DadosHermenos(HermenosDbContext db)
{
	private IReadOnlyList<EmpresaResumoDTO>? empresas;
	private IReadOnlyList<DocumentoFiscalDTO>? documentos;
	private IReadOnlyList<DivergenciaDTO>? divergencias;
	private IReadOnlyList<LoteImportacaoDTO>? lotes;
	private List<Divergencia>? divergenciasBrutas;

	public IReadOnlyList<EmpresaResumoDTO> Empresas => empresas ??= db.Empresas.AsNoTracking()
		.OrderBy(e => e.IdEmpresa).AsEnumerable()
		.Select(e => new EmpresaResumoDTO(e.IdEmpresa, e.Nome, e.Cnpj, RotulosStatus.Empresa(e.Status))).ToList();

	public IReadOnlyList<DivergenciaDTO> Divergencias => divergencias ??= BrutasDivergencias()
		.Select(d => new DivergenciaDTO(d.IdDivergencia, d.IdEmpresa, d.IdDocumentoFiscal, d.Categoria, d.CampoAfetado,
			d.ValorFonteXml, d.ValorFonteERP, d.IdentificadaEm.ToLocalTime(), RotulosStatus.Divergencia(d.Status),
			string.IsNullOrWhiteSpace(d.ResponsavelDemonstrativo) ? "—" : d.ResponsavelDemonstrativo)).ToList();

	public IReadOnlyList<DocumentoFiscalDTO> Documentos => documentos ??= CarregarDocumentos();

	public IReadOnlyList<LoteImportacaoDTO> Lotes => lotes ??= CarregarLotes();

	public string Competencia => db.LotesImportacao.AsNoTracking().OrderByDescending(l => l.Competencia)
		.Select(l => (DateOnly?)l.Competencia).FirstOrDefault()?.ToString("yyyy-MM")
		?? DateTime.Today.ToString("yyyy-MM");

	public DateTime AtualizadoEm => db.LotesImportacao.AsNoTracking().OrderByDescending(l => l.RecebidoEm)
		.Select(l => (DateTimeOffset?)l.RecebidoEm).FirstOrDefault()?.LocalDateTime ?? DateTime.Now;

	private List<Divergencia> BrutasDivergencias()
		=> divergenciasBrutas ??= db.Divergencias.AsNoTracking().OrderBy(d => d.IdDivergencia).ToList();

	private IReadOnlyList<DocumentoFiscalDTO> CarregarDocumentos()
	{
		var fiscais = db.DocumentosFiscais.AsNoTracking().OrderBy(d => d.Id).ToList();
		var fornecedores = db.Fornecedores.AsNoTracking().ToDictionary(f => f.IdFornecedor, f => f.NomeRazaoSocial);
		var registros = db.RegistrosERP.AsNoTracking().Where(r => r.DocumentoFiscalId != null).ToList()
			.GroupBy(r => r.DocumentoFiscalId!.Value)
			.ToDictionary(g => g.Key, g => g.OrderBy(r => r.RegERPId).First());
		var porDocumento = BrutasDivergencias().ToLookup(d => d.IdDocumentoFiscal);

		return fiscais.Select(d =>
		{
			registros.TryGetValue(d.Id, out var erp);
			var somenteErp = d.Tipo == DocumentoFiscal.TipoSomenteErp;
			var nomeFornecedor = d.FornecedorId is int id && fornecedores.TryGetValue(id, out var nome)
				? nome : erp?.Fornecedor ?? "Não informado";
			return new DocumentoFiscalDTO(d.Id, d.EmpresaId, d.Numero, d.Tipo, d.ChaveAcesso ?? "Não aplicável",
				nomeFornecedor, d.DataEmissao, somenteErp ? null : d.ValorDocumento, erp?.ValorDocumento,
				Situacao(d, porDocumento[d.Id].ToList()), d.LoteImportacaoId ?? 0);
		}).ToList();
	}

	private static string Situacao(DocumentoFiscal documento, List<Divergencia> divergencias)
	{
		var principal = divergencias.FirstOrDefault(d => d.Status is StatusDivergencia.Pendente or StatusDivergencia.EmAnalise)
			?? divergencias.FirstOrDefault();
		if (principal is not null)
		{
			return principal.Categoria switch
			{
				MotorConciliacao.CategoriaAusente => documento.Tipo == DocumentoFiscal.TipoSomenteErp ? "Ausente no XML" : "Ausente no ERP",
				MotorConciliacao.CategoriaCancelamento => "Cancelamento para revisão",
				var categoria => categoria
			};
		}
		return documento.Status switch
		{
			StatusDocumento.AguardandoConciliacao => "Aguardando conciliação",
			StatusDocumento.Divergente => "Divergente",
			_ => "Conciliado"
		};
	}

	private IReadOnlyList<LoteImportacaoDTO> CarregarLotes()
	{
		var usuarios = db.Usuarios.AsNoTracking().ToDictionary(u => u.IdUsuario, u => u.Nome);
		return db.LotesImportacao.AsNoTracking().OrderBy(l => l.LoteImpId).AsEnumerable()
			.Select(l => new LoteImportacaoDTO(l.LoteImpId, l.IdEmpresa, l.NomeArquivo, l.Formato, l.Origem,
				l.RecebidoEm.LocalDateTime,
				l.RecebidoPorUsuarioId is int id && usuarios.TryGetValue(id, out var nome) ? nome : "—",
				l.RegistrosEncontrados, l.RegistrosValidos, l.RegistrosRejeitados, l.RegistrosDuplicados,
				RotulosStatus.Lote(l.Status))).ToList();
	}
}
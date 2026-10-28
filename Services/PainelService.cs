using Hermenos.Services.Interfaces;
using Hermenos.Data.DadosFicticios;
using Hermenos.ViewModels.Empresa;
using Hermenos.ViewModels.Painel;
namespace Hermenos.Services;

public sealed class PainelService(DadosDemonstracao dados, ILogger<PainelService> logger) : IPainelService
{
	public PainelGeralViewModel ObterResumoGeral(string? situacao, int? empresaId)
	{
		logger.LogInformation("Consulta do painel geral. Competência {Competencia}", dados.Competencia);

		var linhas = dados.Empresas.Select(e =>
		{
			var docs = dados.Documentos.Where(d => d.IdEmpresa == e.Id).ToList();
			var divs = dados.Divergencias.Where(d => d.IdEmpresa == e.Id
			&& d.Situacao is "Pendente" or "Em análise").ToList();

			var lotes = dados.Lotes.Where(l => l.IdEmpresa == e.Id).ToList();
			return new EmpresaLinhaViewModel(e.Id, e.Nome, e.Cnpj,
			docs.Count, divs.Count, lotes.Count(l => l.Situacao == "Falha no processamento"),
			lotes.OrderByDescending(l => l.Recebido).Select(l => (DateTime?)l.Recebido).FirstOrDefault(), divs.Count > 0
			|| lotes.Any(l => l.Situacao == "Falha no processamento") ? "Com pendências" : "Em dia");
		}).Where(e => !empresaId.HasValue || e.Id == empresaId).Where(e => string.IsNullOrWhiteSpace(situacao)
		|| e.Situacao == situacao).ToList();

		var empresasParaComparacao = linhas.Select(e => e.Id).ToHashSet();
		var documentosComparaveis = dados.Documentos.Where(d => empresasParaComparacao.Contains(d.IdEmpresa)
		&& d.ValorXml.HasValue && d.ValorErp.HasValue).ToList();

		string[] nomesCategorias = { "Documento ausente em uma das fontes", "Diferença de valor",
		"Diferença de quantidade", "Possível duplicidade", "Situação de cancelamento para revisão"
		};

		var divergenciasFiltradas = dados.Divergencias.Where(d => !empresaId.HasValue
		|| d.IdEmpresa == empresaId).Where(d => string.IsNullOrWhiteSpace(situacao)
		|| d.Situacao == situacao);

		var categorias = nomesCategorias.Select((nome, i) => new CategoriaViewModel(nome,
		divergenciasFiltradas.Count(d => d.Categoria == nome),
		$"c{i + 1}")).ToList();

		var recentes = dados.Divergencias.Where(
		d => d.Situacao is "Pendente" or "Em análise").OrderByDescending(d => d.Identificada).Select(
		d => new PendenciaRecenteViewModel(d.Id, d.IdEmpresa, dados.Empresas.Single(e => e.Id == d.IdEmpresa).Nome,
		dados.Documentos.Single(x => x.Id == d.IdDocumento).Numero, d.Categoria,
		d.Identificada, d.Situacao)).Where(d => string.IsNullOrWhiteSpace(situacao)
		|| d.Situacao == situacao).Where(d => !empresaId.HasValue
		|| d.EmpresaId == empresaId).ToList();

		return new()
		{
			Competencia = dados.Competencia,
			AtualizadoEm = dados.AtualizadoEm,
			EmpresasCadastradas = dados.Empresas.Count,
			EmpresasComPendencias = dados.Empresas.Count(
		e => dados.Divergencias.Any(d => d.IdEmpresa == e.Id && d.Situacao is "Pendente" or "Em análise")
		|| dados.Lotes.Any(l => l.IdEmpresa == e.Id && l.Situacao == "Falha no processamento")),
			DocumentosIdentificados = dados.Documentos.Select(d => d.Id).Distinct().Count(),
			DocumentosSemDivergencias = dados.Documentos.Count(d => d.Situacao == "Conciliado"),
			DivergenciasPendentes = dados.Divergencias.Count(d => d.Situacao is "Pendente" or "Em análise"),
			FalhasImportacao = dados.Lotes.Count(l => l.Situacao == "Falha no processamento"),
			ValorXmlComparavel = documentosComparaveis.Sum(d => d.ValorXml ?? 0m),
			ValorErpComparavel = documentosComparaveis.Sum(d => d.ValorErp ?? 0m),
			DiferencaAbsolutaComparavel = documentosComparaveis.Sum(d => Math.Abs((d.ValorXml ?? 0m) - (d.ValorErp ?? 0m))),
			DocumentosComValoresComparaveis = documentosComparaveis.Count,
			EmpresaOpcoes = dados.Empresas,
			Empresas = linhas,
			Categorias = categorias,
			PendenciasRecentes = recentes,
			SituacaoFiltro = situacao,
			EmpresaFiltro = empresaId
		};
	}
}



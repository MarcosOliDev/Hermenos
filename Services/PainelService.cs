using Hermenos.Services.Interfaces;
using Hermenos.Data;
using Hermenos.ViewModels.Empresa;
using Hermenos.ViewModels.Painel;
namespace Hermenos.Services;

public sealed class PainelService(DadosHermenos dados, ILogger<PainelService> logger) : IPainelService
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

		// O filtro "Em dia/Com pendências" pertence à empresa, não à divergência.
		var empresasFiltradasIds = linhas.Select(e => e.Id).ToHashSet();
		var documentosPainel = dados.Documentos.Where(d => empresasFiltradasIds.Contains(d.IdEmpresa)).ToList();
		var divergenciasFiltradas = dados.Divergencias
			.Where(d => empresasFiltradasIds.Contains(d.IdEmpresa));
		var situacoesDocumentos = new[]
		{
			new SituacaoDocumentoViewModel("Conciliados", documentosPainel.Count(d => Classificar(d) == "Conciliado"), "#16a34a"),
			new SituacaoDocumentoViewModel("Divergentes", documentosPainel.Count(d => Classificar(d) == "Divergente"), "#f59e0b"),
			new SituacaoDocumentoViewModel("Pendentes", documentosPainel.Count(d => Classificar(d) == "Pendente"), "#3b82f6"),
			new SituacaoDocumentoViewModel("Ausentes", documentosPainel.Count(d => Classificar(d) == "Ausente"), "#ef4444")
		};
		var evolucao = documentosPainel.GroupBy(d => (d.Emissao.Year, d.Emissao.Month, Semana: (d.Emissao.Day - 1) / 7 + 1))
			.OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month).ThenBy(g => g.Key.Semana)
			.Select(g => new EvolucaoConciliacaoViewModel(
				$"{Math.Min((g.Key.Semana - 1) * 7 + 1, DateTime.DaysInMonth(g.Key.Year, g.Key.Month)):00}–{Math.Min(g.Key.Semana * 7, DateTime.DaysInMonth(g.Key.Year, g.Key.Month)):00}/{g.Key.Month:00}",
				g.Count(d => Classificar(d) == "Conciliado"),
				g.Count(d => Classificar(d) == "Divergente"))).ToList();
		var empresasComDivergencias = divergenciasFiltradas
			.Where(d => d.Situacao is "Pendente" or "Em análise")
			.GroupBy(d => d.IdEmpresa)
			.Select(g => new EmpresaDivergenciaViewModel(
				dados.Empresas.Single(e => e.Id == g.Key).Nome, g.Count()))
			.OrderByDescending(e => e.Quantidade).Take(5).ToList();
		var documentosPorEmpresa = linhas.Select(e =>
		{
			var docsDaEmpresa = documentosPainel.Where(d => d.IdEmpresa == e.Id).ToList();
			return new DocumentosEmpresaViewModel(e.Nome,
				docsDaEmpresa.Count(d => Classificar(d) == "Conciliado"),
				docsDaEmpresa.Count(d => Classificar(d) == "Pendente"),
				docsDaEmpresa.Count(d => Classificar(d) == "Divergente" || Classificar(d) == "Ausente"));
		}).OrderByDescending(e => e.Pendentes + e.Divergentes + e.Conciliados).Take(5).ToList();

		var categorias = nomesCategorias.Select((nome, i) => new CategoriaViewModel(nome,
		divergenciasFiltradas.Count(d => d.Categoria == nome),
		$"c{i + 1}")).ToList();

		var recentes = dados.Divergencias.Where(
		d => d.Situacao is "Pendente" or "Em análise").OrderByDescending(d => d.Identificada).Select(
		d => new PendenciaRecenteViewModel(d.Id, d.IdEmpresa, dados.Empresas.Single(e => e.Id == d.IdEmpresa).Nome,
		dados.Documentos.Single(x => x.Id == d.IdDocumento).Numero, d.Categoria,
		d.Identificada, d.Situacao)).Where(d => empresasFiltradasIds.Contains(d.EmpresaId)).ToList();

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
			SituacoesDocumentos = situacoesDocumentos,
			EvolucaoConciliacao = evolucao,
			EmpresasComDivergencias = empresasComDivergencias,
			DocumentosPorEmpresa = documentosPorEmpresa,
			SituacaoFiltro = situacao,
			EmpresaFiltro = empresaId
		};
	}

	private string Classificar(Hermenos.DTOs.DocumentoFiscal.DocumentoFiscalDTO documento)
	{
		if (documento.Situacao == "Conciliado") return "Conciliado";
		if (documento.Situacao.Contains("Ausente", StringComparison.OrdinalIgnoreCase)
			|| documento.ValorXml is null || documento.ValorErp is null) return "Ausente";
		if (dados.Divergencias.Any(d => d.IdDocumento == documento.Id
			&& d.Situacao is "Pendente" or "Em análise")) return "Pendente";
		return "Divergente";
	}
}
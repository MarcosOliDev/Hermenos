using Hermenos.Services.Interfaces;
using Hermenos.ViewModels.Divergencia;
using Hermenos.ViewModels.Painel;
using Microsoft.AspNetCore.Mvc;
namespace Hermenos.Controllers;

public sealed class ConciliacaoController(IConciliacaoService service) : Controller
{
	public IActionResult Index(
	int? empresaId,
	string? busca,
	string? situacao)
	{
		if (!ValidarEmpresa(empresaId))
			return BadRequest();

		ViewBag.EmpresaId = empresaId;
		ViewBag.Busca = busca;
		ViewBag.Situacao = situacao;
		ViewBag.NomeEmpresa = empresaId.HasValue
		? service.ObterNomeEmpresa(empresaId.Value)
		: null;
		var documentos = service.ObterDocumentos(empresaId, busca, situacao);
		ViewBag.ComparacaoPorEmpresa = documentos.GroupBy(d => d.IdEmpresa).Select(g => new ComparacaoFonteEmpresaViewModel(
			service.ObterNomeEmpresa(g.Key),
			g.Count(d => d.ValorXml.HasValue),
			g.Count(d => d.ValorErp.HasValue))).ToList();
		return View(documentos);
	}

	public IActionResult Divergencias(
	int? empresaId,
	string? situacao,
	string? categoria,
	string? busca)
	{
		if (!ValidarEmpresa(empresaId))
			return BadRequest();

		ViewBag.EmpresaId = empresaId;
		ViewBag.Situacao = situacao;
		ViewBag.Categoria = categoria;
		ViewBag.Busca = busca;
		ViewBag.NomeEmpresa = empresaId.HasValue
		? service.ObterNomeEmpresa(empresaId.Value)
		: null;
		return View("~/Views/Divergencias/Index.cshtml",
		service.ObterLinhasDivergencias(
		empresaId, situacao, categoria, busca));
	}

	public IActionResult Importacoes(int? empresaId)
	{
		if (!ValidarEmpresa(empresaId))
			return BadRequest();
		ViewBag.EmpresaId = empresaId;
		ViewBag.Empresas = service.ObterEmpresas();
		ViewBag.NomeEmpresa = empresaId.HasValue
		? service.ObterNomeEmpresa(empresaId.Value)
		: null;
		return View("~/Views/Importacoes/Index.cshtml",
		service.ObterLotes(empresaId));
	}

	private bool ValidarEmpresa(int? empresaId)
	=> !empresaId.HasValue || service.EmpresaExiste(empresaId.Value);
}
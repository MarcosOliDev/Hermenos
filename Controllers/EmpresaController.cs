using Hermenos.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
namespace Hermenos.Controllers;

public sealed class EmpresasController(IEmpresaService service) : Controller
{
	public IActionResult Index(string? busca, string? situacao)
	{
		ViewBag.Busca = busca;
		ViewBag.Situacao = situacao;
		return View(service.ObterEmpresas(busca, situacao));
	}

	public IActionResult Detalhes(int id)
	{
		if (id <= 0)
			return BadRequest();

		var modelo = service.ObterEmpresaPorId(id);
		return modelo is null
		? NotFound()
		: View(modelo);
	}
}

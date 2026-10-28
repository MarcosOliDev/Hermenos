using Hermenos.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
namespace Hermenos.Controllers;

public sealed class PainelController(IPainelService service) : Controller
{
	public IActionResult Index(string? situacao, int? empresaId)
	=> View(service.ObterResumoGeral(situacao, empresaId));

	public IActionResult Erro()
	=> View("~/Views/Shared/Error.cshtml");
}

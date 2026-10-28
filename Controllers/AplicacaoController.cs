using Microsoft.AspNetCore.Mvc;

namespace Hermenos.Controllers
{
	public class AplicacaoController : Controller
	{
		public IActionResult Index()
		{
			return View();
		}
	}
}

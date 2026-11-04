using System.Security.Claims;

namespace Hermenos.Helpers
{
	public static class ClaimsExtensions
	{
		public static string? ObterPerfil(this ClaimsPrincipal usuario)
		{
			return usuario.FindFirst(ClaimTypes.Role)?.Value;
		}

		public static int? ObterUsuarioId(this ClaimsPrincipal usuario)
		{
			var valor = usuario.FindFirst(ClaimTypes.NameIdentifier)?.Value;

			return int.TryParse(valor, out var id) ? id : null;
		}
	}
}
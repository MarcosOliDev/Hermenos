using System.Security.Claims;
using Hermenos.Models;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Hermenos.Services.Usuarios;

public static class UsuarioClaimsFactory
{
	public static ClaimsPrincipal Criar(Usuario usuario)
	{
		var claims = new List<Claim>
		{
			new(ClaimTypes.NameIdentifier, usuario.IdUsuario.ToString()),
			new(ClaimTypes.Name, usuario.Nome),
			new(ClaimTypes.Email, usuario.Email),
			new(ClaimTypes.Role, usuario.Perfil),
			new("NomeUsuario", usuario.NomeUsuario),
			new("FotoPerfil", usuario.FotoPerfil ?? string.Empty)
		};

		var identidade = new ClaimsIdentity(
			claims,
			CookieAuthenticationDefaults.AuthenticationScheme
		);

		return new ClaimsPrincipal(identidade);
	}
}
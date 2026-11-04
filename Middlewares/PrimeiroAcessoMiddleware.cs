using System.Security.Claims;
using Hermenos.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

namespace Hermenos.Middlewares;

public sealed class PrimeiroAcessoMiddleware(RequestDelegate next)
{
	public async Task InvokeAsync(HttpContext context, HermenosDbContext banco)
	{
		if (context.User.Identity?.IsAuthenticated != true)
		{
			await next(context);
			return;
		}

		var idClaim = context.User.FindFirstValue(ClaimTypes.NameIdentifier);

		if (!int.TryParse(idClaim, out var id))
		{
			await EncerrarSessao(context);
			return;
		}

		var usuario = await banco.Usuarios
			.AsNoTracking()
			.Where(u => u.IdUsuario == id)
			.Select(u => new
			{
				u.Ativo,
				u.DeveAlterarSenha
			})
			.FirstOrDefaultAsync();

		if (usuario is null || !usuario.Ativo)
		{
			await EncerrarSessao(context);
			return;
		}

		if (usuario.DeveAlterarSenha)
		{
			var caminho = context.Request.Path;

			var podeAcessar =
				caminho.Equals("/Conta/AlterarSenhaObrigatoria",
					StringComparison.OrdinalIgnoreCase) ||
				caminho.Equals("/Conta/Sair",
					StringComparison.OrdinalIgnoreCase);

			if (!podeAcessar)
			{
				context.Response.Redirect("/Conta/AlterarSenhaObrigatoria");
				return;
			}
		}

		await next(context);
	}

	private static async Task EncerrarSessao(HttpContext context)
	{
		await context.SignOutAsync(
			CookieAuthenticationDefaults.AuthenticationScheme);

		context.Response.Redirect("/Conta/Entrar");
	}
}
using System.Security.Claims;
using Hermenos.Data;
using Hermenos.Models;
using Hermenos.Services.Usuarios;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hermenos.Controllers;

public sealed class ContaController(HermenosDbContext context, IWebHostEnvironment environment) : Controller
{
	private readonly PasswordHasher<Usuario> _hasher = new();

	private async Task RenovarAutenticacaoAsync(Usuario usuario)
	{
		var autenticacao = await HttpContext.AuthenticateAsync(
			CookieAuthenticationDefaults.AuthenticationScheme
		);

		var propriedades = autenticacao.Properties
			?? new AuthenticationProperties();

		await HttpContext.SignInAsync(
			CookieAuthenticationDefaults.AuthenticationScheme,
			UsuarioClaimsFactory.Criar(usuario),
			propriedades
		);
	}

	[HttpGet]
	public IActionResult Entrar(string? returnUrl = null)
	{
		if (User.Identity?.IsAuthenticated == true) return RedirectToAction("Index", "Painel");
		ViewData["ReturnUrl"] = returnUrl;
		return View(new EntrarViewModel());
	}

	[HttpPost, ValidateAntiForgeryToken]
	public async Task<IActionResult> Entrar(
	EntrarViewModel modelo,
	string? returnUrl = null)
	{
		ViewData["ReturnUrl"] = returnUrl;

		if (!ModelState.IsValid)
			return View(modelo);

		var login = modelo.Nome.Trim().ToLowerInvariant();

		var usuario = await context.Usuarios.FirstOrDefaultAsync(u =>
			u.NomeUsuario == login || u.Email == login);

		if (usuario is null ||
			string.IsNullOrWhiteSpace(usuario.SenhaHash))
		{
			ModelState.AddModelError(
				string.Empty,
				"Nome de usuário ou senha incorretos."
			);

			return View(modelo);
		}

		var resultado = _hasher.VerifyHashedPassword(
			usuario,
			usuario.SenhaHash,
			modelo.Senha
		);

		if (resultado == PasswordVerificationResult.Failed)
		{
			ModelState.AddModelError(
				string.Empty,
				"Nome de usuário ou senha incorretos."
			);

			return View(modelo);
		}

		await HttpContext.SignInAsync(
			CookieAuthenticationDefaults.AuthenticationScheme,
			UsuarioClaimsFactory.Criar(usuario),
			new AuthenticationProperties
			{
				IsPersistent = modelo.Lembrar
			}
		);

		if (usuario.DeveAlterarSenha)
			return RedirectToAction(nameof(AlterarSenhaObrigatoria));

		if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
			return LocalRedirect(returnUrl);

		return RedirectToAction("Index", "Painel");
	}

	[HttpPost, Authorize, ValidateAntiForgeryToken]
	public async Task<IActionResult> Sair()
	{
		await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
		return RedirectToAction(nameof(Entrar));
	}


	[Authorize, HttpGet]
	public async Task<IActionResult> Perfil()
	{
		var id = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
		var usuario = await context.Usuarios.AsNoTracking().FirstOrDefaultAsync(u => u.IdUsuario == id);
		if (usuario is null) return NotFound();
		ViewBag.FotoPerfil = usuario.FotoPerfil;
		ViewBag.NomeUsuario = usuario.NomeUsuario;
		return View();
	}

	[Authorize, HttpPost, ValidateAntiForgeryToken]
	public async Task<IActionResult> AtualizarFoto(IFormFile? foto, bool remover = false)
	{
		var id = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
		var usuario = await context.Usuarios.FirstOrDefaultAsync(u => u.IdUsuario == id);
		if (usuario is null) return NotFound();
		if (remover)
		{
			ExcluirFoto(usuario.FotoPerfil);
			usuario.FotoPerfil = null;
		}
		else
		{
			if (foto is null || foto.Length is <= 0 or > 2 * 1024 * 1024 || !new[] { "image/jpeg", "image/png", "image/webp" }.Contains(foto.ContentType))
			{
				TempData["Erro"] = "Selecione uma imagem JPG, PNG ou WEBP de até 2 MB.";
				return RedirectToAction(nameof(Perfil));
			}
			ExcluirFoto(usuario.FotoPerfil);
			var pasta = Path.Combine(environment.WebRootPath, "uploads", "perfis");
			Directory.CreateDirectory(pasta);
			var extensao = foto.ContentType == "image/png" ? ".png" : foto.ContentType == "image/webp" ? ".webp" : ".jpg";
			var nomeArquivo = $"{Guid.NewGuid():N}{extensao}";
			await using var stream = System.IO.File.Create(Path.Combine(pasta, nomeArquivo));
			await foto.CopyToAsync(stream);
			usuario.FotoPerfil = $"/uploads/perfis/{nomeArquivo}";
		}
		await context.SaveChangesAsync();
		await RenovarAutenticacaoAsync(usuario);
		TempData["Aviso"] = "Foto de perfil atualizada.";
		return RedirectToAction(nameof(Perfil));
	}

	[Authorize, HttpPost, ValidateAntiForgeryToken]
	public async Task<IActionResult> AlterarSenha(AlterarSenhaViewModel modelo)
	{
		if (!ModelState.IsValid) { TempData["Erro"] = "Confira os campos da nova senha."; return RedirectToAction(nameof(Perfil)); }
		var id = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
		var usuario = await context.Usuarios.FirstOrDefaultAsync(u => u.IdUsuario == id);
		if (usuario is null) return NotFound();
		if (string.IsNullOrWhiteSpace(usuario.SenhaHash))
		{
			TempData["Erro"] = "Sua conta ainda não possui uma senha definida. Solicite a redefinição ao administrador.";
			return RedirectToAction(nameof(Perfil));
		}

		if (string.IsNullOrWhiteSpace(usuario.SenhaHash))
		{
			TempData["Erro"] = "Sua conta ainda não possui uma senha definida. Solicite a redefinição ao administrador.";
			return RedirectToAction(nameof(Perfil));
		}

		var verificacao = _hasher.VerifyHashedPassword(
			usuario,
			usuario.SenhaHash,
			modelo.SenhaAtual
		);

		if (verificacao == PasswordVerificationResult.Failed)
		{
			TempData["Erro"] = "A senha atual está incorreta.";
			return RedirectToAction(nameof(Perfil));
		}

		usuario.SenhaHash = _hasher.HashPassword(usuario, modelo.NovaSenha);
		await context.SaveChangesAsync();
		await RenovarAutenticacaoAsync(usuario);
		TempData["Aviso"] = "Senha alterada com sucesso.";
		return RedirectToAction(nameof(Perfil));
	}

	private void ExcluirFoto(string? caminho)
	{
		if (string.IsNullOrWhiteSpace(caminho) || !caminho.StartsWith("/uploads/perfis/", StringComparison.Ordinal)) return;
		var arquivo = Path.Combine(environment.WebRootPath, caminho.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
		if (System.IO.File.Exists(arquivo)) System.IO.File.Delete(arquivo);
	}

	[Authorize, HttpGet]
	public async Task<IActionResult> AlterarSenhaObrigatoria()
	{
		var id = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

		var usuario = await context.Usuarios
			.AsNoTracking()
			.FirstOrDefaultAsync(u => u.IdUsuario == id);

		if (usuario is null) return NotFound();

		if (!usuario.DeveAlterarSenha)
			return RedirectToAction("Index", "Painel");

		return View(new PrimeiroAcessoSenhaViewModel());
	}

	[Authorize, HttpPost, ValidateAntiForgeryToken]
	public async Task<IActionResult> AlterarSenhaObrigatoria(
	PrimeiroAcessoSenhaViewModel modelo)
	{
		var id = int.Parse(
			User.FindFirstValue(ClaimTypes.NameIdentifier)!
		);

		var usuario = await context.Usuarios
			.FirstOrDefaultAsync(u => u.IdUsuario == id);

		if (usuario is null)
			return NotFound();

		if (!usuario.DeveAlterarSenha)
			return RedirectToAction("Index", "Painel");

		if (!ModelState.IsValid)
			return View(modelo);

		usuario.SenhaHash = _hasher.HashPassword(
			usuario,
			modelo.NovaSenha
		);

		usuario.DeveAlterarSenha = false;

		await context.SaveChangesAsync();

		await RenovarAutenticacaoAsync(usuario);

		TempData["Aviso"] = "Sua senha foi alterada com sucesso.";

		return RedirectToAction("Index", "Painel");
	}

	public IActionResult AcessoNegado() => View();
}

using Hermenos.Data;
using Hermenos.Helpers;
using Hermenos.Models;
using Hermenos.Services.Usuarios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hermenos.Controllers;

[Authorize(Roles = Perfis.Gestao)]
public sealed class EquipeController(HermenosDbContext context, GeradorNomeUsuario gerador, GeradorSenhaTemporaria geradorSenha, IWebHostEnvironment environment) : Controller
{
	private readonly PasswordHasher<Usuario> _hasher = new();

	public async Task<IActionResult> Index()
	{
		var perfisVisiveis = Perfis.PerfisQuePodeCadastrar(User.ObterPerfil());
		var lista = await context.Usuarios.AsNoTracking()
			.Where(u => perfisVisiveis.Contains(u.Perfil))
			.OrderBy(u => u.Nome)
			.Select(u => new EquipeItemViewModel
			{
				Id = u.IdUsuario,
				Nome = u.Nome,
				Email = u.Email,
				NomeUsuario = u.NomeUsuario,
				FotoPerfil = u.FotoPerfil,
				Perfil = u.Perfil,
				Ativo = u.Ativo,
				DataCadastro = u.DataCadastro
			}).ToListAsync();
		return View(lista);
	}

	[HttpGet]
	public IActionResult Create()
	{
		var permitidos = Perfis.PerfisQuePodeCadastrar(User.ObterPerfil()).ToList();
		if (permitidos.Count == 0) return Forbid();
		return View(new CadastroEquipeViewModel { PerfisPermitidos = permitidos, Perfil = permitidos[0] });
	}

	[HttpPost, ValidateAntiForgeryToken]
	public async Task<IActionResult> Create(CadastroEquipeViewModel modelo, IFormFile? foto)
	{
		var permitidos = Perfis.PerfisQuePodeCadastrar(User.ObterPerfil()).ToList();
		modelo.PerfisPermitidos = permitidos;
		if (!permitidos.Contains(modelo.Perfil)) ModelState.AddModelError(nameof(modelo.Perfil), "Você não pode atribuir esse perfil.");
		var email = modelo.Email.Trim().ToLowerInvariant();
		if (await context.Usuarios.AnyAsync(u => u.Email == email)) ModelState.AddModelError(nameof(modelo.Email), "Já existe um usuário com esse e-mail.");
		if (foto is not null && !FotoValida(foto)) ModelState.AddModelError("foto", "Envie uma imagem JPG, PNG ou WEBP de até 2 MB.");
		if (!ModelState.IsValid) return View(modelo);

		var usuario = new Usuario { Nome = modelo.Nome.Trim(), Email = email, Perfil = modelo.Perfil, Ativo = true, DataCadastro = DateTime.UtcNow, CriadoEm = DateTimeOffset.UtcNow };
		usuario.NomeUsuario = await gerador.GerarAsync(usuario.Nome, usuario.Email);
		var senhaTemporaria = geradorSenha.Gerar();
		usuario.SenhaHash = _hasher.HashPassword(usuario, senhaTemporaria);
		usuario.DeveAlterarSenha = true; usuario.FotoPerfil = foto is null ? null : await SalvarFotoAsync(foto);
		context.Usuarios.Add(usuario);
		await context.SaveChangesAsync();
		TempData["LoginGerado"] = usuario.NomeUsuario;
		TempData["SenhaTemporaria"] = senhaTemporaria;
		TempData["Aviso"] = "Usuário cadastrado com sucesso. Entregue as credenciais ao funcionário."; return RedirectToAction(nameof(Index));
	}

	[HttpPost, ValidateAntiForgeryToken]
	public Task<IActionResult> Desativar(int id) => AlterarAtivoAsync(id, false);
	[HttpPost, ValidateAntiForgeryToken]
	public Task<IActionResult> Reativar(int id) => AlterarAtivoAsync(id, true);

	private async Task<IActionResult> AlterarAtivoAsync(int id, bool ativo)
	{
		var alvo = await context.Usuarios.FirstOrDefaultAsync(u => u.IdUsuario == id);
		if (alvo is null) return NotFound();
		if (alvo.IdUsuario == User.ObterUsuarioId() || !Perfis.PodeGerenciar(User.ObterPerfil(), alvo.Perfil)) return Forbid();
		alvo.Ativo = ativo;
		await context.SaveChangesAsync();
		TempData["Aviso"] = ativo ? $"{alvo.Nome} reativado." : $"{alvo.Nome} desativado.";
		return RedirectToAction(nameof(Index));
	}

	private static bool FotoValida(IFormFile arquivo) => arquivo.Length is > 0 and <= 2 * 1024 * 1024 &&
		new[] { "image/jpeg", "image/png", "image/webp" }.Contains(arquivo.ContentType);

	private async Task<string> SalvarFotoAsync(IFormFile foto)
	{
		var pasta = Path.Combine(environment.WebRootPath, "uploads", "perfis");
		Directory.CreateDirectory(pasta);
		var extensao = foto.ContentType switch { "image/png" => ".png", "image/webp" => ".webp", _ => ".jpg" };
		var nome = $"{Guid.NewGuid():N}{extensao}";
		await using var stream = System.IO.File.Create(Path.Combine(pasta, nome));
		await foto.CopyToAsync(stream);
		return $"/uploads/perfis/{nome}";
	}
}

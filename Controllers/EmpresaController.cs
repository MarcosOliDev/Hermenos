using Microsoft.AspNetCore.Authorization;
using Hermenos.Data;
using Hermenos.Enums;
using Hermenos.Models;
using Hermenos.Services.Interfaces;
using Hermenos.ViewModels.Empresa;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hermenos.Controllers;

[Authorize(Roles = Hermenos.Models.Perfis.Admin + "," + Hermenos.Models.Perfis.Gerente + "," + Hermenos.Models.Perfis.ContadorSenior + "," + Hermenos.Models.Perfis.ContadorPleno)]
public sealed class EmpresasController(
	IEmpresaService service,
	HermenosDbContext db,
	ILogger<EmpresasController> logger) : Controller
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
		return modelo is null ? NotFound() : View(modelo);
	}

	[Authorize(Roles = Hermenos.Models.Perfis.Admin + "," + Hermenos.Models.Perfis.Gerente + "," + Hermenos.Models.Perfis.ContadorSenior)]
	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> Cadastrar(
		EmpresaCadastroViewModel modelo,
		CancellationToken ct)
	{
		if (!ModelState.IsValid)
		{
			var erro = ModelState.Values
				.SelectMany(v => v.Errors)
				.Select(e => e.ErrorMessage)
				.FirstOrDefault(m => !string.IsNullOrWhiteSpace(m));

			return BadRequest(new { sucesso = false, mensagem = erro ?? "Confira os campos informados." });
		}

		var cnpjNumerico = new string(modelo.Cnpj.Where(char.IsDigit).ToArray());

		if (cnpjNumerico.Length != 14)
		{
			return BadRequest(new
			{
				sucesso = false,
				mensagem = "O CNPJ deve conter 14 dígitos."
			});
		}

		var cnpjsExistentes = await db.Empresas
			.AsNoTracking()
			.Select(e => e.Cnpj)
			.ToListAsync(ct);

		if (cnpjsExistentes.Any(c =>
			new string(c.Where(char.IsDigit).ToArray()) == cnpjNumerico))
		{
			return Conflict(new
			{
				sucesso = false,
				mensagem = "Já existe uma empresa cadastrada com esse CNPJ."
			});
		}

		var empresa = new Empresa
		{
			Nome = modelo.Nome.Trim(),
			Cnpj = cnpjNumerico,
			DataInscricaoEmpresa = modelo.DataInscricaoEmpresa!.Value,
			Status = StatusEmpresa.Ativa,
			CriadoEm = DateTimeOffset.UtcNow
		};

		db.Empresas.Add(empresa);

		try
		{
			await db.SaveChangesAsync(ct);
		}
		catch (DbUpdateException ex)
		{
			logger.LogWarning(ex, "Não foi possível cadastrar a empresa por conflito no banco.");

			return Conflict(new
			{
				sucesso = false,
				mensagem = "Não foi possível cadastrar. Verifique se esse CNPJ já está registrado."
			});
		}

		logger.LogInformation("Empresa cadastrada. EmpresaId {EmpresaId}", empresa.IdEmpresa);

		return Ok(new
		{
			sucesso = true,
			mensagem = "Empresa cadastrada com sucesso.",
			id = empresa.IdEmpresa,
			nome = empresa.Nome,
			cnpj = empresa.Cnpj
		});
	}
}
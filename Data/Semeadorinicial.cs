using Hermenos.Enums;
using Hermenos.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Hermenos.Data;

public static class SemeadorInicial
{
	public static async Task SemearAdminInicialAsync(
		HermenosDbContext db,
		IConfiguration configuration)
	{
		// Não recria nem redefine um administrador existente.
		if (await db.Usuarios.AnyAsync(u => u.Perfil == Perfis.Admin))
			return;

		var nome = configuration["AdminInicial:Nome"];
		var email = configuration["AdminInicial:Email"];
		var nomeUsuario = configuration["AdminInicial:NomeUsuario"];
		var senha = configuration["AdminInicial:Senha"];

		if (string.IsNullOrWhiteSpace(nome) ||
			string.IsNullOrWhiteSpace(email) ||
			string.IsNullOrWhiteSpace(nomeUsuario) ||
			string.IsNullOrWhiteSpace(senha))
		{
			throw new InvalidOperationException(
				"Configure as variáveis de ambiente do administrador inicial.");
		}

		if (senha.Length < 8)
			throw new InvalidOperationException(
				"A senha do administrador inicial deve ter pelo menos 8 caracteres.");

		var admin = new Usuario
		{
			Nome = nome.Trim(),
			Email = email.Trim().ToLowerInvariant(),
			NomeUsuario = nomeUsuario.Trim().ToLowerInvariant(),
			Perfil = Perfis.Admin,
			Ativo = true,
			DeveAlterarSenha = true,
			DataCadastro = DateTime.UtcNow,
			CriadoEm = DateTimeOffset.UtcNow
		};

		if (await db.Usuarios.AnyAsync(u =>
			u.Email == admin.Email ||
			u.NomeUsuario == admin.NomeUsuario))
		{
			throw new InvalidOperationException(
				"O e-mail ou nome de usuário do administrador inicial já está em uso.");
		}

		admin.SenhaHash = new PasswordHasher<Usuario>()
			.HashPassword(admin, senha);

		db.Usuarios.Add(admin);
		await db.SaveChangesAsync();
	}
	public static async Task SemearAsync(HermenosDbContext db)
	{
		var agora = DateTimeOffset.UtcNow;
		if (!await db.Usuarios.AnyAsync(u => u.Perfil == Perfis.Admin))
		{
			var admin = new Usuario
			{
				Nome = "Administrador HERMENOS",
				Email = "admin@hermenos.local",
				NomeUsuario = "admin.hermenos",
				Perfil = Perfis.Admin,
				Ativo = true,
				DataCadastro = DateTime.UtcNow,
				CriadoEm = agora
			};
			admin.SenhaHash = new PasswordHasher<Usuario>().HashPassword(admin, "Hermenos@2026");
			db.Usuarios.Add(admin);
		}

		if (!await db.Empresas.AnyAsync())
		{
			var inscricao = DateOnly.FromDateTime(DateTime.Today);
			string[][] empresas =
			[
				["Supermercado Alfa", "12.345.678/0001-90"],
				["Construtora Horizonte", "23.456.789/0001-01"],
				["Clínica Bem Viver", "34.567.890/0001-12"],
				["Indústria Vale Verde", "45.678.901/0001-23"],
				["Restaurante Sabor & Arte", "56.789.012/0001-34"]
			];
			foreach (var empresa in empresas)
				db.Empresas.Add(new Empresa { Nome = empresa[0], Cnpj = empresa[1], DataInscricaoEmpresa = inscricao, Status = StatusEmpresa.Ativa, CriadoEm = agora });
		}
		await db.SaveChangesAsync();
	}
}

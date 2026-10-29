using Hermenos.Enums;
using Hermenos.Models;
using Microsoft.EntityFrameworkCore;

namespace Hermenos.Data;

public static class SemeadorInicial
{
	public static async Task SemearAsync(HermenosDbContext db)
	{
		var agora = DateTimeOffset.UtcNow;
		if (!await db.Usuarios.AnyAsync())
		{
			db.Usuarios.AddRange(
				new Usuario { Nome = "Marcos", Email = "marcos.costa@hermenos.local", CriadoEm = agora },
				new Usuario { Nome = "Rafael Lima", Email = "rafael.lima@hermenos.local", CriadoEm = agora });
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
				db.Empresas.Add(new Empresa
				{
					Nome = empresa[0],
					Cnpj = empresa[1],
					DataInscricaoEmpresa = inscricao,
					Status = StatusEmpresa.Ativa,
					CriadoEm = agora
				});
		}

		await db.SaveChangesAsync();
	}
}
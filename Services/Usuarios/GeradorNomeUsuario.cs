using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Hermenos.Data;
using Microsoft.EntityFrameworkCore;

namespace Hermenos.Services.Usuarios;

public sealed class GeradorNomeUsuario(HermenosDbContext context)
{
	public async Task<string> GerarAsync(string nome, string email, int? ignorarId = null)
	{
		var partesNome = Normalizar(nome).Split(' ', StringSplitOptions.RemoveEmptyEntries);
		var localEmail = Normalizar(email.Split('@')[0]).Replace(' ', '.');
		var candidatos = new List<string>();
		if (partesNome.Length >= 2)
		{
			candidatos.Add($"{partesNome[0]}.{partesNome[^1]}");
			candidatos.Add($"{partesNome[0]}.{partesNome[1]}");
			candidatos.Add(string.Join('.', partesNome.Take(2)));
			candidatos.Add(partesNome[0]);
		}
		if (!string.IsNullOrWhiteSpace(localEmail)) candidatos.Add(localEmail);

		foreach (var candidato in candidatos.Distinct())
		{
			var baseNome = Limpar(candidato);
			if (baseNome.Length < 3) continue;
			if (!await ExisteAsync(baseNome, ignorarId)) return baseNome;
			for (var i = 1; i < partesNome.Length; i++)
			{
				var alternativa = Limpar($"{baseNome}.{partesNome[i]}");
				if (alternativa.Length >= 3 && !await ExisteAsync(alternativa, ignorarId)) return alternativa;
			}
		}
		throw new InvalidOperationException("Não foi possível gerar um nome de usuário exclusivo sem utilizar números. Informe um nome ou e-mail diferente.");
	}

	private Task<bool> ExisteAsync(string nome, int? ignorarId) => context.Usuarios
		.AnyAsync(u => u.NomeUsuario == nome && (!ignorarId.HasValue || u.IdUsuario != ignorarId.Value));

	private static string Normalizar(string valor)
	{
		var decomposed = valor.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
		var sb = new StringBuilder();
		foreach (var c in decomposed)
			if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark) sb.Append(c);
		return Regex.Replace(sb.ToString().Normalize(NormalizationForm.FormC), @"[^a-z ]", " ").Trim();
	}
	private static string Limpar(string valor) => Regex.Replace(valor.ToLowerInvariant(), @"[^a-z.]", "").Trim('.');
}

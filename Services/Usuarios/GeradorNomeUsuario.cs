
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Hermenos.Data;
using Microsoft.EntityFrameworkCore;

namespace Hermenos.Services.Usuarios;

public sealed class GeradorNomeUsuario(HermenosDbContext context)
{

	public async Task<string> GerarAsync(
		string nome,
		string email,
		int? ignorarId = null)
	{
		var partesNome = Normalizar(nome)
			.Split(' ', StringSplitOptions.RemoveEmptyEntries);

		var candidatos = new List<string>();

		if (partesNome.Length >= 2)
		{
			var primeiroNome = partesNome[0];

			foreach (var parte in partesNome.Skip(1))
			{
				candidatos.Add($"{primeiroNome}.{parte}");
			}

			foreach (var parte in partesNome.Skip(1))
			{
				var tamanhoInicial = Math.Min(4, parte.Length);

				for (var tamanho = tamanhoInicial; tamanho < parte.Length; tamanho++)
				{
					candidatos.Add($"{primeiroNome}.{parte[..tamanho]}");
				}
			}

			candidatos.Add(primeiroNome);
		}

		var localEmail = email.Contains('@')
			? email.Split('@')[0]
			: email;

		var emailNormalizado = Normalizar(localEmail)
			.Replace(' ', '.');

		if (!string.IsNullOrWhiteSpace(emailNormalizado))
			candidatos.Add(emailNormalizado);

		foreach (var candidato in candidatos.Distinct())
		{
			var usuario = Limpar(candidato);

			if (usuario.Length < 3)
				continue;

			if (!await ExisteAsync(usuario, ignorarId))
				return usuario;
		}

		throw new InvalidOperationException(
			"Não foi possível gerar um nome de usuário exclusivo sem utilizar números. Informe um nome ou e-mail diferente.");
	}


	private Task<bool> ExisteAsync(string nome, int? ignorarId) =>
		context.Usuarios.AnyAsync(u =>
			u.NomeUsuario == nome &&
			(!ignorarId.HasValue || u.IdUsuario != ignorarId.Value));

	private static string Normalizar(string valor)
	{
		var decomposed = valor.Trim()
			.ToLowerInvariant()
			.Normalize(NormalizationForm.FormD);

		var sb = new StringBuilder();

		foreach (var c in decomposed)
		{
			if (CharUnicodeInfo.GetUnicodeCategory(c) !=
				UnicodeCategory.NonSpacingMark)
			{
				sb.Append(c);
			}
		}

		return Regex.Replace(
			sb.ToString().Normalize(NormalizationForm.FormC),
			@"[^a-z ]",
			" ").Trim();
	}

	private static string Limpar(string valor) =>
		Regex.Replace(valor.ToLowerInvariant(), @"[^a-z.]", "").Trim('.');
}

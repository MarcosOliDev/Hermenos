using System.Security.Cryptography;

namespace Hermenos.Services.Usuarios;

public sealed class GeradorSenhaTemporaria
{
	public string Gerar()
	{
		const string caracteres =
			"ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz23456789!@#$%";

		return string.Create(16, caracteres, (senha, conjunto) =>
		{
			for (int i = 0; i < senha.Length; i++)
			{
				senha[i] = conjunto[RandomNumberGenerator.GetInt32(conjunto.Length)];
			}
		});
	}
}
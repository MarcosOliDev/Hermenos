using System.ComponentModel.DataAnnotations;

namespace Hermenos.Models;

public sealed class PrimeiroAcessoSenhaViewModel
{
	[Required(ErrorMessage = "Informe a nova senha.")]
	[MinLength(8, ErrorMessage = "A senha deve ter pelo menos 8 caracteres.")]
	[DataType(DataType.Password)]
	public string NovaSenha { get; set; } = string.Empty;

	[Required(ErrorMessage = "Confirme a nova senha.")]
	[Compare(nameof(NovaSenha), ErrorMessage = "As senhas não coincidem.")]
	[DataType(DataType.Password)]
	public string ConfirmarNovaSenha { get; set; } = string.Empty;
}
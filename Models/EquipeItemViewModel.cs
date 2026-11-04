using System.ComponentModel.DataAnnotations;

namespace Hermenos.Models;

public sealed class EquipeItemViewModel
{
	public int Id { get; set; }
	public string Nome { get; set; } = string.Empty;
	public string Email { get; set; } = string.Empty;
	public string NomeUsuario { get; set; } = string.Empty;
	public string? FotoPerfil { get; set; }
	public string Perfil { get; set; } = string.Empty;
	public bool Ativo { get; set; }
	public DateTime DataCadastro { get; set; }
}

public sealed class CadastroEquipeViewModel
{
	[Required, StringLength(200, MinimumLength = 3)]
	public string Nome { get; set; } = string.Empty;
	[Required, EmailAddress, StringLength(254)]
	public string Email { get; set; } = string.Empty;
	[Required]
	public string Perfil { get; set; } = Perfis.ContadorJunior;
	public List<string> PerfisPermitidos { get; set; } = [];
}

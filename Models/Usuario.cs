namespace Hermenos.Models;

public sealed class Usuario
{
	public int IdUsuario { get; set; }
	public string Nome { get; set; } = string.Empty;
	public string Email { get; set; } = string.Empty;
	public string NomeUsuario { get; set; } = string.Empty;
	public string SenhaHash { get; set; } = string.Empty;
	public string? FotoPerfil { get; set; }
	public DateTime DataCadastro { get; set; }
	public string Perfil { get; set; } = Perfis.ContadorJunior;
	public bool Ativo { get; set; } = true;
	public bool DeveAlterarSenha { get; set; }
	public DateTimeOffset CriadoEm { get; set; }
}

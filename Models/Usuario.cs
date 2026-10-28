namespace Hermenos.Models;

public sealed class Usuario
{
	public int IdUsuario { get; set; }
	public string Nome { get; set; } = string.Empty;
	public string Email { get; set; } = string.Empty;
	public bool Ativo { get; set; } = true;
	public DateTimeOffset CriadoEm { get; set; }
}

using Hermenos.Enums;

namespace Hermenos.Models;

public sealed class Empresa
{
	public int IdEmpresa { get; set; }
	public string Nome { get; set; } = string.Empty;
	public string Cnpj { get; set; } = string.Empty;
	public DateOnly DataInscricaoEmpresa { get; set; }
	public StatusEmpresa Status { get; set; } = StatusEmpresa.Ativa;
	public int? CriadoPorUsuarioId { get; set; }
	public DateTimeOffset CriadoEm { get; set; }
}

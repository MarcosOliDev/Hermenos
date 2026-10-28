using Hermenos.Enums;

namespace Hermenos.Models;

public sealed class Contrato
{
	public int IdContrato { get; set; }
	public int IdEmpresa { get; set; }
	public string Numero { get; set; } = string.Empty;
	public string Descricao { get; set; } = string.Empty;
	public DateOnly VigenciaInicio { get; set; }
	public DateOnly? VigenciaFim { get; set; }
	public StatusContrato Status { get; set; } = StatusContrato.Ativo;
	public int CriadoPorUsuarioId { get; set; }
	public DateTimeOffset CriadoEm { get; set; }
}


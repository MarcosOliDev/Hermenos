using Hermenos.Enums;
namespace Hermenos.Models;

public sealed class Divergencia
{
	public int IdDivergencia { get; set; }
	public int IdEmpresa { get; set; }
	public int IdDocumentoFiscal { get; set; }
	public string Categoria { get; set; } = string.Empty;
	public string CampoAfetado { get; set; } = string.Empty;
	public string ValorFonteXml { get; set; } = string.Empty;
	public string ValorFonteERP { get; set; } = string.Empty;
	public DateTime IdentificadaEm { get; set; }
	public StatusDivergencia Status { get; set; } = StatusDivergencia.Pendente;
	public string? ResponsavelDemonstrativo { get; set; }
}

using Hermenos.Enums;
namespace Hermenos.Models;

public sealed class RegistroERP
{
	public int RegERPId { get; set; }
	public int EmpresaId { get; set; }
	public int? DocumentoFiscalId { get; set; }
	public string NumeroDocumento { get; set; } = string.Empty;
	public string Fornecedor { get; set; } = string.Empty;
	public DateTime? DataEmissao { get; set; }
	public decimal? ValorDocumento { get; set; }
	public decimal? QuantidadeItens { get; set; }
	public StatusDocumento Status { get; set; }
}

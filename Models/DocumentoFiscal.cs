using Hermenos.Enums;

namespace Hermenos.Models;

public sealed class DocumentoFiscal
{
	public const string TipoSomenteErp = "Somente ERP";

	public int Id { get; set; }
	public int EmpresaId { get; set; }
	public int? FornecedorId { get; set; }
	public string Numero { get; set; } = string.Empty;
	public string Tipo { get; set; } = string.Empty;
	public string? ChaveAcesso { get; set; }
	public DateTime DataEmissao { get; set; }
	public decimal? ValorDocumento { get; set; }
	public decimal? QuantidadeItens { get; set; }
	public StatusDocumento Status { get; set; } = StatusDocumento.AguardandoConciliacao;
	public int? LoteImportacaoId { get; set; }
}
using Hermenos.Enums;

namespace Hermenos.Models;

public sealed class Fornecedor
{
	public int IdFornecedor { get; set; }
	public string NomeRazaoSocial { get; set; } = string.Empty;
	public string? NomeFantasia { get; set; }
	public string DocumentoFiscal { get; set; } = string.Empty;
	public string? Email { get; set; }
	public string? Telefone { get; set; }
	public StatusFornecedor Status { get; set; } = StatusFornecedor.Ativo;
	public int? CriadoPorUsuarioId { get; set; }
	public DateTimeOffset CriadoEm { get; set; }
}

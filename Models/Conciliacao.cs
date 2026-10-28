using Hermenos.Enums;
namespace Hermenos.Models;

public sealed class Conciliacao
{
	public int Id { get; set; }
	public int EmpresaId { get; set; }
	public int DocumentoFiscalId { get; set; }
	public int? RegistroERPId { get; set; }
	public DateTime ExecutadaEm { get; set; }
	public StatusDocumento Status { get; set; }
		= StatusDocumento.AguardandoConciliacao;
	public string? RegraAplicada { get; set; }
}

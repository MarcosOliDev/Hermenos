using Hermenos.Enums;

namespace Hermenos.Models;

public sealed class LoteImportacao
{
	public int LoteImpId { get; set; }
	public int IdEmpresa { get; set; }
	public int? ContratoId { get; set; }
	public DateOnly Competencia { get; set; }
	public string NomeArquivo { get; set; } = string.Empty;
	public string Formato { get; set; } = string.Empty;
	public string Origem { get; set; } = string.Empty;
	public DateTimeOffset RecebidoEm { get; set; }
	public int? RecebidoPorUsuarioId { get; set; }
	public int RegistrosEncontrados { get; set; }
	public int RegistrosValidos { get; set; }
	public int RegistrosRejeitados { get; set; }
	public int RegistrosDuplicados { get; set; }
	public StatusLoteImportacao Status { get; set; } = StatusLoteImportacao.AguardandoProcessamento;
}


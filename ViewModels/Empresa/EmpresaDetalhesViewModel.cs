using Hermenos.DTOs.Divergencia;
using Hermenos.DTOs.DocumentoFiscal;
using Hermenos.DTOs.Empresa;
using Hermenos.DTOs.Importacao;
namespace Hermenos.ViewModels.Empresa;

public sealed class EmpresaDetalhesViewModel
{
	public required EmpresaResumoDTO Empresa { get; init; }
	public required string Competencia { get; init; }
	public required DateTime AtualizadoEm { get; init; }
	public required IReadOnlyList<DocumentoFiscalDTO> Documentos { get; init; }
	public required IReadOnlyList<DivergenciaDTO> Divergencias { get; init; }
	public required IReadOnlyList<LoteImportacaoDTO> Lotes { get; init; }
	public DocumentoFiscalDTO? ObterDocumento(int id) => Documentos.FirstOrDefault(d => d.Id == id);
	public int SemDivergencias => Documentos.Count(d => d.Situacao == "Conciliado");
	public int Pendentes => Divergencias.Count(d => d.Situacao == "Pendente");
	public int EmAnalise => Divergencias.Count(d => d.Situacao == "Em análise");
	public int Falhas => Lotes.Count(l => l.Situacao == "Falha no processamento");
}

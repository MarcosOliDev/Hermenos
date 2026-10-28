using Hermenos.DTOs.Empresa;
using Hermenos.ViewModels.Empresa;
namespace Hermenos.ViewModels.Painel;

public sealed class PainelGeralViewModel
{
	public required string Competencia { get; init; }
	public required DateTime AtualizadoEm { get; init; }
	public required int EmpresasCadastradas { get; init; }
	public required int EmpresasComPendencias
	{
		get; init;
	}
	public required int DocumentosIdentificados { get; init; }
	public required int DocumentosSemDivergencias { get; init; }
	public required int DivergenciasPendentes { get; init; }
	public required int FalhasImportacao { get; init; }
	public required decimal ValorXmlComparavel { get; init; }
	public required decimal ValorErpComparavel { get; init; }
	public required decimal DiferencaAbsolutaComparavel { get; init; }
	public required int DocumentosComValoresComparaveis { get; init; }
	public required IReadOnlyList<EmpresaResumoDTO> EmpresaOpcoes { get; init; }
	public required IReadOnlyList<EmpresaLinhaViewModel> Empresas { get; init; }
	public required IReadOnlyList<CategoriaViewModel> Categorias { get; init; }
	public required IReadOnlyList<PendenciaRecenteViewModel> PendenciasRecentes { get; init; }
	public required string? SituacaoFiltro { get; init; }
	public required int? EmpresaFiltro { get; init; }
	public int MaiorCategoria => Categorias.Count == 0 ? 1 : Categorias.Max(c => c.Quantidade);
	public int PercentualSemDivergencias => DocumentosIdentificados == 0
	? 0
	: (int)Math.Round((double)DocumentosSemDivergencias / DocumentosIdentificados * 100);
}





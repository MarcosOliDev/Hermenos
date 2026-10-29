using Hermenos.DTOs.Divergencia;
using Hermenos.DTOs.DocumentoFiscal;
using Hermenos.DTOs.Empresa;
using Hermenos.DTOs.Importacao;
using Hermenos.ViewModels.Divergencia;
namespace Hermenos.Services.Interfaces;

public interface IConciliacaoService
{
	IReadOnlyList<DocumentoFiscalDTO> ObterDocumentos(int? empresaId, string? busca, string? situacao);
	IReadOnlyList<DivergenciaDTO> ObterDivergencias(int? empresaId, string? situacao, string? categoria);
	IReadOnlyList<LoteImportacaoDTO> ObterLotes(int? empresaId); bool EmpresaExiste(int id);
	IReadOnlyList<EmpresaResumoDTO> ObterEmpresas();
	string ObterNomeEmpresa(int id); string ObterNumeroDocumento(int id);
	IReadOnlyList<DivergenciaLinhaViewModel> ObterLinhasDivergencias(int? empresaId, string? situacao, string? categoria, string? busca);
}
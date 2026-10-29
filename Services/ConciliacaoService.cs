using Hermenos.DTOs.Divergencia;
using Hermenos.DTOs.Empresa;
using System.Globalization;
using System.Text;
using Hermenos.Services.Interfaces;
using Hermenos.Data;
using Hermenos.DTOs.DocumentoFiscal;
using Hermenos.DTOs.Importacao;
using Hermenos.ViewModels.Divergencia;
namespace Hermenos.Services;

public sealed class ConciliacaoService(DadosHermenos dados, ILogger<ConciliacaoService> logger) : IConciliacaoService
{
	public IReadOnlyList<DivergenciaDTO> ObterDivergencias(int? empresaId, string? situacao, string? categoria)
	=> dados.Divergencias.Where(d => !empresaId.HasValue
	|| d.IdEmpresa == empresaId).Where(d => string.IsNullOrWhiteSpace(situacao)
	|| d.Situacao == situacao).Where(d => string.IsNullOrWhiteSpace(categoria)
	|| d.Categoria == categoria).OrderByDescending(d => d.Identificada).ToList();

	public IReadOnlyList<DocumentoFiscalDTO> ObterDocumentos(int? empresaId, string? busca, string? situacao)
	=> dados.Documentos.Where(d => !empresaId.HasValue
	|| d.IdEmpresa == empresaId).Where(d => string.IsNullOrWhiteSpace(busca)
	|| d.Numero.Contains(busca, StringComparison.OrdinalIgnoreCase)
	|| d.Chave.Contains(busca, StringComparison.OrdinalIgnoreCase)
	|| d.Fornecedor.Contains(busca, StringComparison.OrdinalIgnoreCase)).Where(d => string.IsNullOrWhiteSpace(situacao)
	|| d.Situacao == situacao).ToList();

	public IReadOnlyList<LoteImportacaoDTO> ObterLotes(int? empresaId)
	=> dados.Lotes.Where(l => !empresaId.HasValue
	|| l.IdEmpresa == empresaId).OrderByDescending(l => l.Recebido).ToList();

	public IReadOnlyList<EmpresaResumoDTO> ObterEmpresas() => dados.Empresas;

	public bool EmpresaExiste(int id)
	=> dados.Empresas.Any(e => e.Id == id);

	public string ObterNomeEmpresa(int id)
	=> dados.Empresas.Single(e => e.Id == id).Nome;

	public string ObterNumeroDocumento(int id)
	=> dados.Documentos.Single(d => d.Id == id).Numero;

	public IReadOnlyList<DivergenciaLinhaViewModel> ObterLinhasDivergencias(int? empresaId, string? situacao,
	string? categoria, string? busca)
	{
		var lista = ObterDivergencias(empresaId, situacao, categoria);
		logger.LogInformation("Consulta de divergências. EmpresaId {EmpresaId}; Quantidade {Quantidade}",
		empresaId, lista.Count);
		var linhas = lista.Select(d => new DivergenciaLinhaViewModel(d, ObterNomeEmpresa(d.IdEmpresa),
			ObterNumeroDocumento(d.IdDocumento)));
		if (string.IsNullOrWhiteSpace(busca))
			return linhas.ToList();

		var termo = NormalizarBusca(busca);
		return linhas.Where(linha => NormalizarBusca(string.Join(' ', linha.Empresa, linha.Documento,
			linha.Item.Categoria, linha.Item.Campo, linha.Item.ValorXml, linha.Item.ValorErp,
			linha.Item.Responsavel)).Contains(termo, StringComparison.Ordinal)).ToList();
	}

	private static string NormalizarBusca(string valor)
	{
		var decomposed = valor.Trim().Normalize(NormalizationForm.FormD);
		var semAcentos = new string(decomposed.Where(caractere =>
			CharUnicodeInfo.GetUnicodeCategory(caractere) != UnicodeCategory.NonSpacingMark).ToArray());
		return semAcentos.Normalize(NormalizationForm.FormC).ToUpperInvariant();
	}
}
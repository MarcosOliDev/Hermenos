using Hermenos.Services.Interfaces;
using System.Globalization;
using System.Text;
using Hermenos.ViewModels;
using Hermenos.Data;
using Hermenos.ViewModels.Empresa;
using Hermenos.ViewModels.Painel;
namespace Hermenos.Services;

public sealed class EmpresaService(DadosHermenos dados, ILogger<EmpresaService> logger) : IEmpresaService
{
	public IReadOnlyList<EmpresaLinhaViewModel> ObterEmpresas(string? busca, string? situacao)
	{
		var empresas = dados.Empresas.Select(
		e =>
		{
			var docs = dados.Documentos.Count(d => d.IdEmpresa == e.Id);
			var pend = dados.Divergencias.Count(d => d.IdEmpresa == e.Id && d.Situacao is "Pendente" or "Em análise");
			var lotes = dados.Lotes.Where(l => l.IdEmpresa == e.Id).ToList();
			return new EmpresaLinhaViewModel(e.Id, e.Nome, e.Cnpj, docs, pend,
			lotes.Count(l => l.Situacao == "Falha no processamento"), lotes.MaxBy(l => l.Recebido)?.Recebido,
			pend > 0 || lotes.Any(l => l.Situacao == "Falha no processamento") ? "Com pendências" : "Em dia");
		});

		if (!string.IsNullOrWhiteSpace(busca))
		{
			var termo = NormalizarBusca(busca);
			var cnpjBusca = new string(busca.Where(char.IsDigit).ToArray());
			empresas = empresas.Where(e => NormalizarBusca(e.Nome).Contains(termo, StringComparison.Ordinal)
				|| e.Cnpj.Contains(busca.Trim(), StringComparison.OrdinalIgnoreCase)
				|| (cnpjBusca.Length > 0 && new string(e.Cnpj.Where(char.IsDigit).ToArray()).Contains(cnpjBusca, StringComparison.Ordinal)));
		}
		if (!string.IsNullOrWhiteSpace(situacao))
			empresas = empresas.Where(e => e.Situacao == situacao);
		logger.LogInformation("Listagem de empresas consultada. Busca informada: {Busca}",
		!string.IsNullOrWhiteSpace(busca)); return empresas.ToList();
	}

	private static string NormalizarBusca(string valor)
	{
		var decomposed = valor.Trim().Normalize(NormalizationForm.FormD);
		var semAcentos = new string(decomposed.Where(caractere =>
			CharUnicodeInfo.GetUnicodeCategory(caractere) != UnicodeCategory.NonSpacingMark).ToArray());
		return semAcentos.Normalize(NormalizationForm.FormC).ToUpperInvariant();
	}

	public EmpresaDetalhesViewModel? ObterEmpresaPorId(int id)
	{
		var empresa = dados.Empresas.SingleOrDefault(e => e.Id == id);
		if (empresa is null)
		{
			logger.LogWarning("Empresa não encontrada. EmpresaId {EmpresaId}", id);
			return null;
		}

		return new()
		{
			Empresa = empresa,
			Competencia = dados.Competencia,
			AtualizadoEm = dados.AtualizadoEm,
			Documentos = dados.Documentos.Where(d => d.IdEmpresa == id).ToList(),
			Divergencias = dados.Divergencias.Where(d => d.IdEmpresa == id).ToList(),
			Lotes = dados.Lotes.Where(l => l.IdEmpresa == id).OrderByDescending(l => l.Recebido).ToList()
		};
	}
}
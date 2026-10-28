using Hermenos.ViewModels;
using Hermenos.ViewModels.Empresa;

namespace Hermenos.Services.Interfaces;

public interface IEmpresaService
{
	IReadOnlyList<EmpresaLinhaViewModel> ObterEmpresas(string? busca, string? situacao);
	EmpresaDetalhesViewModel? ObterEmpresaPorId(int id);
}


using Hermenos.ViewModels.Painel;
namespace Hermenos.Services.Interfaces;

public interface IPainelService
{
	PainelGeralViewModel ObterResumoGeral(string? situacao, int? empresaId);
}

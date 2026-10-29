using Hermenos.DTOs.Importacao;
namespace Hermenos.Services.Interfaces;

public interface IImportacaoService
{
	Task<ResultadoImportacaoDTO> ImportarAsync(int empresaId, DateOnly competencia, string nomeArquivo,
		Stream conteudo, CancellationToken ct = default);
}
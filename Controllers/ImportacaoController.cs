using System.Globalization;
using Hermenos.Services;
using Hermenos.Services.Importacao;
using Hermenos.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Hermenos.Controllers;

public sealed class ImportacoesController(IImportacaoService importacao, IConciliacaoService conciliacao,
	ILogger<ImportacoesController> logger) : Controller
{
	[HttpPost]
	[ValidateAntiForgeryToken]
	[RequestSizeLimit(ImportacaoService.TamanhoMaximoTotalBytes)]
	public async Task<IActionResult> Enviar(int empresaId, string? competencia, List<IFormFile> arquivos, CancellationToken ct)
	{
		if (!conciliacao.EmpresaExiste(empresaId))
		{
			logger.LogWarning("Envio recusado: empresa inexistente. EmpresaId {EmpresaId}", empresaId);
			TempData["ImportacaoErro"] = "Selecione uma empresa válida.";
			return RedirectToAction("Importacoes", "Conciliacao");
		}

		if (!DateOnly.TryParseExact($"{competencia}-01", "yyyy-MM-dd", CultureInfo.InvariantCulture,
			DateTimeStyles.None, out var data))
		{
			TempData["ImportacaoErro"] = "Informe a competência no formato mês/ano.";
			return RedirectToAction("Importacoes", "Conciliacao", new { empresaId });
		}

		var arquivosValidos = arquivos.Where(a => a.Length > 0).ToList();
		if (arquivosValidos.Count == 0)
		{
			TempData["ImportacaoErro"] = "Selecione ao menos um arquivo com conteúdo.";
			return RedirectToAction("Importacoes", "Conciliacao", new { empresaId });
		}

		var sucessos = new List<string>();
		var erros = new List<string>();
		foreach (var arquivo in arquivosValidos)
		{
			if (arquivo.Length > ImportacaoService.TamanhoMaximoArquivoBytes)
			{
				erros.Add($"{arquivo.FileName}: o arquivo excede 10 MB.");
				continue;
			}

			await using var conteudo = arquivo.OpenReadStream();
			var resultado = await importacao.ImportarAsync(empresaId, data, arquivo.FileName, conteudo, ct);
			var resumo = $"{resultado.Arquivo}: {resultado.Situacao} — {resultado.Encontrados} encontrados, "
				+ $"{resultado.Validos} válidos, {resultado.Rejeitados} rejeitados, {resultado.Duplicados} duplicados.";
			var linhas = new[] { resumo }.Concat(resultado.Mensagens.Select(m => $"   • {m}"));
			(resultado.Sucesso ? sucessos : erros).Add(string.Join('\n', linhas));
		}

		if (sucessos.Count > 0) TempData["ImportacaoSucesso"] = string.Join('\n', sucessos);
		if (erros.Count > 0) TempData["ImportacaoErro"] = string.Join('\n', erros);
		return RedirectToAction("Importacoes", "Conciliacao", new { empresaId });
	}
}
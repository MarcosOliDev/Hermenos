using System.Text;
using System.Text.Json;

namespace Hermenos.Services.Importacao;

public sealed record LinhaErp(
	string Numero, string? Chave, string Fornecedor, DateTime? Emissao, decimal? Valor, decimal? Quantidade, bool Cancelado);

public sealed record ResultadoLeituraErp(IReadOnlyList<LinhaErp> Linhas, IReadOnlyList<string> Erros);

public static class LeitorRegistroErp
{
	private static readonly Dictionary<string, string> Sinonimos = new()
	{
		["numero"] = "numero",
		["numerodocumento"] = "numero",
		["numeronota"] = "numero",
		["nf"] = "numero",
		["nfe"] = "numero",
		["documento"] = "numero",
		["nnf"] = "numero",
		["chave"] = "chave",
		["chaveacesso"] = "chave",
		["chavenfe"] = "chave",
		["fornecedor"] = "fornecedor",
		["nomefornecedor"] = "fornecedor",
		["razaosocial"] = "fornecedor",
		["emissao"] = "emissao",
		["dataemissao"] = "emissao",
		["data"] = "emissao",
		["valor"] = "valor",
		["valortotal"] = "valor",
		["valordocumento"] = "valor",
		["vnf"] = "valor",
		["quantidade"] = "quantidade",
		["quantidadeitens"] = "quantidade",
		["qtd"] = "quantidade",
		["qtditens"] = "quantidade",
		["itens"] = "quantidade",
		["situacao"] = "situacao",
		["status"] = "situacao"
	};

	public static ResultadoLeituraErp Ler(Stream stream, string formato)
	{
		var linhas = new List<LinhaErp>();
		var erros = new List<string>();
		List<(int Numero, Dictionary<string, string> Campos)> registros;

		try
		{
			using var leitor = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
			var texto = leitor.ReadToEnd();
			registros = formato == "JSON" ? LerJson(texto) : LerCsv(texto);
		}
		catch (Exception ex) when (ex is FormatException or JsonException)
		{
			erros.Add(ex.Message);
			return new ResultadoLeituraErp(linhas, erros);
		}

		foreach (var (numeroLinha, campos) in registros)
		{
			var numero = TextoUtil.NormalizarNumero(campos.GetValueOrDefault("numero"));
			if (numero.Length == 0) { erros.Add($"Registro {numeroLinha}: número do documento ausente."); continue; }

			var textoValor = campos.GetValueOrDefault("valor");
			var valor = TextoUtil.ParseDecimal(textoValor);
			if (!string.IsNullOrWhiteSpace(textoValor) && valor is null) { erros.Add($"Registro {numeroLinha} (NF {numero}): valor inválido."); continue; }

			var textoData = campos.GetValueOrDefault("emissao");
			var emissao = TextoUtil.ParseData(textoData);
			if (!string.IsNullOrWhiteSpace(textoData) && emissao is null) { erros.Add($"Registro {numeroLinha} (NF {numero}): data de emissão inválida."); continue; }

			var textoQuantidade = campos.GetValueOrDefault("quantidade");
			var quantidade = TextoUtil.ParseDecimal(textoQuantidade);
			if (!string.IsNullOrWhiteSpace(textoQuantidade) && quantidade is null) { erros.Add($"Registro {numeroLinha} (NF {numero}): quantidade inválida."); continue; }

			var chave = TextoUtil.Digitos(campos.GetValueOrDefault("chave"));
			linhas.Add(new LinhaErp(
				numero, chave.Length == 44 ? chave : null, (campos.GetValueOrDefault("fornecedor") ?? string.Empty).Trim(),
				emissao, valor, quantidade, TextoUtil.Normalizar(campos.GetValueOrDefault("situacao")).Contains("cancel")));
		}

		if (linhas.Count == 0 && erros.Count == 0) erros.Add("Nenhum registro encontrado no arquivo.");
		return new ResultadoLeituraErp(linhas, erros);
	}

	private static List<(int, Dictionary<string, string>)> LerCsv(string texto)
	{
		var linhas = texto.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
		if (linhas.Length == 0) throw new FormatException("O arquivo está vazio.");

		var separador = linhas[0].Count(c => c == ';') >= linhas[0].Count(c => c == ',') ? ';' : ',';
		var cabecalho = DividirLinha(linhas[0], separador)
			.Select(h => Sinonimos.GetValueOrDefault(TextoUtil.Normalizar(h), string.Empty)).ToList();
		if (!cabecalho.Contains("numero"))
			throw new FormatException("Cabeçalho sem a coluna do número do documento (numero).");

		var registros = new List<(int, Dictionary<string, string>)>();
		for (var i = 1; i < linhas.Length; i++)
		{
			var celulas = DividirLinha(linhas[i], separador);
			var campos = new Dictionary<string, string>();
			for (var j = 0; j < Math.Min(celulas.Count, cabecalho.Count); j++)
				if (cabecalho[j].Length > 0) campos[cabecalho[j]] = celulas[j].Trim();
			registros.Add((i + 1, campos));
		}
		return registros;
	}

	private static List<string> DividirLinha(string linha, char separador)
	{
		var celulas = new List<string>();
		var atual = new StringBuilder();
		var entreAspas = false;
		for (var i = 0; i < linha.Length; i++)
		{
			var c = linha[i];
			if (c == '"')
			{
				if (entreAspas && i + 1 < linha.Length && linha[i + 1] == '"') { atual.Append('"'); i++; }
				else entreAspas = !entreAspas;
			}
			else if (c == separador && !entreAspas) { celulas.Add(atual.ToString()); atual.Clear(); }
			else atual.Append(c);
		}
		celulas.Add(atual.ToString());
		return celulas;
	}

	private static List<(int, Dictionary<string, string>)> LerJson(string texto)
	{
		using var documento = JsonDocument.Parse(texto);
		var raiz = documento.RootElement;
		JsonElement lista;
		if (raiz.ValueKind == JsonValueKind.Array) lista = raiz;
		else if (raiz.ValueKind == JsonValueKind.Object)
		{
			var propriedade = raiz.EnumerateObject().FirstOrDefault(p => p.Value.ValueKind == JsonValueKind.Array);
			if (propriedade.Value.ValueKind != JsonValueKind.Array)
				throw new FormatException("O JSON precisa conter uma lista de registros.");
			lista = propriedade.Value;
		}
		else throw new FormatException("O JSON precisa conter uma lista de registros.");

		var registros = new List<(int, Dictionary<string, string>)>();
		var numero = 0;
		foreach (var item in lista.EnumerateArray())
		{
			numero++;
			if (item.ValueKind != JsonValueKind.Object) continue;
			var campos = new Dictionary<string, string>();
			foreach (var propriedade in item.EnumerateObject())
			{
				if (!Sinonimos.TryGetValue(TextoUtil.Normalizar(propriedade.Name), out var campo)) continue;
				campos[campo] = propriedade.Value.ValueKind switch
				{
					JsonValueKind.String => propriedade.Value.GetString() ?? string.Empty,
					JsonValueKind.Number => propriedade.Value.GetRawText(),
					JsonValueKind.True => "true",
					_ => string.Empty
				};
			}
			registros.Add((numero, campos));
		}
		return registros;
	}
}
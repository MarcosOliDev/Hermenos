using System.Xml;
using System.Xml.Linq;

namespace Hermenos.Services.Importacao;

public sealed record NotaLida(
	string Chave, string Numero, string Tipo, DateTime Emissao, decimal Valor, decimal QuantidadeItens,
	string EmitenteCnpj, string EmitenteNome, string DestinatarioCnpj, string DestinatarioNome, bool Cancelada);

public sealed record ResultadoLeituraXml(
	IReadOnlyList<NotaLida> Notas, IReadOnlySet<string> ChavesCanceladas, IReadOnlyList<string> Erros);

public static class LeitorNFeXml
{
	public static ResultadoLeituraXml Ler(Stream stream)
	{
		var notas = new List<NotaLida>();
		var erros = new List<string>();
		var canceladas = new HashSet<string>();

		XDocument documento;
		try
		{
			var configuracao = new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null };
			using var leitor = XmlReader.Create(stream, configuracao);
			documento = XDocument.Load(leitor);
		}
		catch (XmlException ex)
		{
			erros.Add($"XML inválido: {ex.Message}");
			return new ResultadoLeituraXml(notas, canceladas, erros);
		}

		foreach (var protocolo in Descendentes(documento, "infProt"))
		{
			var chave = TextoUtil.Digitos(Valor(protocolo, "chNFe"));
			if (Valor(protocolo, "cStat") == "101" && chave.Length == 44) canceladas.Add(chave);
		}
		foreach (var evento in Descendentes(documento, "infEvento"))
		{
			var chave = TextoUtil.Digitos(Valor(evento, "chNFe"));
			if (Valor(evento, "tpEvento") == "110111" && chave.Length == 44) canceladas.Add(chave);
		}

		foreach (var inf in Descendentes(documento, "infNFe"))
		{
			var chave = TextoUtil.Digitos((string?)inf.Attribute("Id"));
			if (chave.Length != 44)
			{
				erros.Add("NF-e sem chave de acesso válida (44 dígitos).");
				continue;
			}

			var ide = Filho(inf, "ide");
			var numero = TextoUtil.NormalizarNumero(Valor(ide, "nNF"));
			var textoData = Valor(ide, "dhEmi");
			if (textoData.Length == 0) textoData = Valor(ide, "dEmi");
			var emissao = TextoUtil.ParseData(textoData);
			var valor = TextoUtil.ParseDecimal(Valor(Filho(Filho(inf, "total"), "ICMSTot"), "vNF"));

			if (numero.Length == 0) { erros.Add($"Chave {chave}: número da nota ausente."); continue; }
			if (emissao is null) { erros.Add($"NF-e {numero}: data de emissão inválida."); continue; }
			if (valor is null) { erros.Add($"NF-e {numero}: valor total inválido."); continue; }

			var emitente = Filho(inf, "emit");
			var destinatario = Filho(inf, "dest");
			notas.Add(new NotaLida(
				chave, numero, Valor(ide, "mod") == "65" ? "NFC-e" : "NF-e", emissao.Value, valor.Value,
				inf.Elements().Count(e => e.Name.LocalName == "det"),
				Documento(emitente), Valor(emitente, "xNome"), Documento(destinatario), Valor(destinatario, "xNome"),
				canceladas.Contains(chave)));
		}

		if (notas.Count == 0 && erros.Count == 0 && canceladas.Count == 0)
			erros.Add("Nenhuma NF-e encontrada no arquivo.");

		return new ResultadoLeituraXml(notas, canceladas, erros);
	}

	private static IEnumerable<XElement> Descendentes(XDocument documento, string nome)
		=> documento.Descendants().Where(e => e.Name.LocalName == nome);

	private static XElement? Filho(XElement? pai, string nome)
		=> pai?.Elements().FirstOrDefault(e => e.Name.LocalName == nome);

	private static string Valor(XElement? pai, string nome)
		=> pai?.Descendants().FirstOrDefault(e => e.Name.LocalName == nome)?.Value.Trim() ?? string.Empty;

	private static string Documento(XElement? parte)
	{
		var cnpj = TextoUtil.Digitos(Valor(parte, "CNPJ"));
		return cnpj.Length > 0 ? cnpj : TextoUtil.Digitos(Valor(parte, "CPF"));
	}
}
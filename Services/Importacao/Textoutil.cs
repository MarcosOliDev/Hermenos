using System.Globalization;
using System.Text;

namespace Hermenos.Services.Importacao;

internal static class TextoUtil
{
	private static readonly string[] FormatosData = ["dd/MM/yyyy", "d/M/yyyy", "yyyy-MM-dd", "dd-MM-yyyy"];

	public static string Digitos(string? valor) => new((valor ?? string.Empty).Where(char.IsDigit).ToArray());

	public static string Normalizar(string? valor)
	{
		if (string.IsNullOrWhiteSpace(valor)) return string.Empty;
		var construtor = new StringBuilder();
		foreach (var caractere in valor.Trim().Normalize(NormalizationForm.FormD))
		{
			if (CharUnicodeInfo.GetUnicodeCategory(caractere) == UnicodeCategory.NonSpacingMark) continue;
			if (char.IsLetterOrDigit(caractere)) construtor.Append(char.ToLowerInvariant(caractere));
		}
		return construtor.ToString();
	}

	public static string NormalizarNumero(string? valor)
	{
		var texto = (valor ?? string.Empty).Trim();
		if (texto.Length == 0) return string.Empty;
		var semZeros = texto.TrimStart('0');
		return semZeros.Length == 0 ? "0" : semZeros;
	}

	public static string FormatarCnpj(string digitos)
		=> digitos.Length == 14 ? ulong.Parse(digitos, CultureInfo.InvariantCulture).ToString(@"00\.000\.000\/0000\-00", CultureInfo.InvariantCulture) : digitos;

	public static string Limitar(string? texto, int maximo)
	{
		if (string.IsNullOrEmpty(texto)) return string.Empty;
		return texto.Length <= maximo ? texto : texto[..maximo];
	}

	public static decimal? ParseDecimal(string? valor)
	{
		if (string.IsNullOrWhiteSpace(valor)) return null;
		var texto = valor.Replace("R$", "", StringComparison.OrdinalIgnoreCase).Replace(" ", "").Replace("\u00A0", "").Trim();
		var ultimaVirgula = texto.LastIndexOf(',');
		var ultimoPonto = texto.LastIndexOf('.');
		if (ultimaVirgula >= 0 && ultimoPonto >= 0)
			texto = ultimaVirgula > ultimoPonto ? texto.Replace(".", "").Replace(',', '.') : texto.Replace(",", "");
		else if (ultimaVirgula >= 0)
			texto = texto.Replace(',', '.');
		else if (texto.Count(c => c == '.') > 1)
			texto = texto.Replace(".", "");
		return decimal.TryParse(texto, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint,
			CultureInfo.InvariantCulture, out var resultado) ? resultado : null;
	}

	public static DateTime? ParseData(string? valor)
	{
		if (string.IsNullOrWhiteSpace(valor)) return null;
		var texto = valor.Trim();
		if (texto.Length > 10 && (texto[10] == 'T' || texto[10] == ' ')) texto = texto[..10];
		return DateTime.TryParseExact(texto, FormatosData, CultureInfo.InvariantCulture, DateTimeStyles.None, out var data)
			? data.Date : null;
	}

	public static DateTime ComoUtc(DateTime data) => DateTime.SpecifyKind(data.Date, DateTimeKind.Utc);
}
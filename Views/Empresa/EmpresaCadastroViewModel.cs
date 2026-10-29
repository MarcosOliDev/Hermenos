
using System.ComponentModel.DataAnnotations;

namespace Hermenos.ViewModels.Empresa;

public sealed class EmpresaCadastroViewModel
{
	[Required(ErrorMessage = "Informe o nome da empresa.")]
	[StringLength(200, ErrorMessage = "O nome deve ter no máximo 200 caracteres.")]
	public string Nome { get; set; } = string.Empty;

	[Required(ErrorMessage = "Informe o CNPJ.")]
	[StringLength(18, ErrorMessage = "O CNPJ deve ter no máximo 18 caracteres.")]
	public string Cnpj { get; set; } = string.Empty;

	[Required(ErrorMessage = "Informe a data de inscrição.")]
	[DataType(DataType.Date)]
	public DateOnly? DataInscricaoEmpresa { get; set; }
}

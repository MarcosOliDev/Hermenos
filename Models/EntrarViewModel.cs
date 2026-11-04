using System.ComponentModel.DataAnnotations;

namespace Hermenos.Models
{
    public class EntrarViewModel
    {
        [Required(ErrorMessage = "Informe seu nome de usuário.")]
        [StringLength(254, ErrorMessage = "O nome está incorreto.")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "Informe sua senha.")]
        [DataType(DataType.Password)]
        public string Senha { get; set; } = string.Empty;

        public bool Lembrar { get; set; }
    }

	public sealed class AlterarSenhaViewModel
	{
		[Required, DataType(DataType.Password)]
		public string SenhaAtual { get; set; } = string.Empty;
		[Required, MinLength(8), DataType(DataType.Password)]
		public string NovaSenha { get; set; } = string.Empty;
		[Required, Compare(nameof(NovaSenha)), DataType(DataType.Password)]
		public string ConfirmarSenha { get; set; } = string.Empty;
	}

}

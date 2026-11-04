namespace Hermenos.Models
{
	public static class Perfis
	{
		public const string Admin = "Admin";
		public const string Gerente = "Gerente";
		public const string ContadorSenior = "ContadorSenior";
		public const string ContadorJunior = "ContadorJunior";
		public const string ContadorPleno = "ContadorPleno";

		public const string Equipe = Admin + "," + Gerente + "," + ContadorSenior + "," + ContadorPleno + "," + ContadorJunior;
		public const string Gestao = Admin + "," + Gerente;

		public static int Nivel(string? perfil) => perfil switch
		{
			Admin => 0,
			Gerente => 1,
			ContadorSenior => 2,
			ContadorPleno => 3,
			_ => 4
		};

		public static string Rotulo(string? perfil) => perfil switch
		{
			Admin => "Administrador",
			Gerente => "Gerente",
			ContadorSenior => "Contador Sênior",
			ContadorPleno => "Contador Pleno",
			_ => "Contador Júnior"
		};

		public static bool EhEquipe(string? perfil) => perfil is Admin or Gerente or ContadorSenior or ContadorPleno or ContadorJunior;

		public static bool EhGestao(string? perfil) => perfil is Admin or Gerente;

		public static bool PodeGerenciar(string? ator, string? alvo) =>
			EhGestao(ator) && Nivel(ator) < Nivel(alvo);

		public static string[] PerfisQuePodeCadastrar(string? ator) => ator switch
		{
			Admin => new[] { Gerente, ContadorSenior, ContadorPleno, ContadorJunior },
			Gerente => new[] { ContadorSenior, ContadorPleno, ContadorJunior },
			_ => Array.Empty<string>()
		};
	}
}
namespace Hermenos.Enums;

public static class RotulosStatus
{
	public static string Empresa(StatusEmpresa status) => status switch
	{
		StatusEmpresa.Ativa => "Ativa",
		StatusEmpresa.Inativa => "Inativa",
		StatusEmpresa.Bloqueada => "Bloqueada",
		_ => status.ToString()
	};

	public static string Lote(StatusLoteImportacao status) => status switch
	{
		StatusLoteImportacao.AguardandoProcessamento => "Aguardando processamento",
		StatusLoteImportacao.Recebido => "Recebido",
		StatusLoteImportacao.EmProcessamento => "Em processamento",
		StatusLoteImportacao.Concluido => "Concluído",
		StatusLoteImportacao.ConcluidoComAlertas => "Concluído com alertas",
		StatusLoteImportacao.FalhaNoProcessamento => "Falha no processamento",
		_ => status.ToString()
	};

	public static string Divergencia(StatusDivergencia status) => status switch
	{
		StatusDivergencia.Pendente => "Pendente",
		StatusDivergencia.EmAnalise => "Em análise",
		StatusDivergencia.Tratada => "Tratada",
		StatusDivergencia.Encerrada => "Encerrada",
		_ => status.ToString()
	};
}
namespace Hermenos.ViewModels.Empresa;

public sealed record EmpresaLinhaViewModel(int Id, string Nome, string Cnpj, int Documentos,
int Pendencias, int Falhas,
DateTime? UltimaImportacao, string Situacao);

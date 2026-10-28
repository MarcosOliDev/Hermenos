namespace Hermenos.ViewModels.Painel;

public sealed record CategoriaViewModel(string Nome, int Quantidade, string Classe);
public sealed record PendenciaRecenteViewModel(int Id, int EmpresaId, string Empresa, string Documento,
string Categoria, DateTime Identificada, string Situacao);

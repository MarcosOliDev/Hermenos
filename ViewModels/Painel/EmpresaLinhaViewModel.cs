namespace Hermenos.ViewModels.Painel;

public sealed record CategoriaViewModel(string Nome, int Quantidade, string Classe);
public sealed record PendenciaRecenteViewModel(int Id, int EmpresaId, string Empresa, string Documento,
string Categoria, DateTime Identificada, string Situacao);
public sealed record SituacaoDocumentoViewModel(string Nome, int Quantidade, string Cor);
public sealed record EvolucaoConciliacaoViewModel(string Periodo, int Conciliados, int Divergentes);
public sealed record EmpresaDivergenciaViewModel(string Nome, int Quantidade);
public sealed record DocumentosEmpresaViewModel(string Nome, int Conciliados, int Pendentes, int Divergentes);
public sealed record ComparacaoFonteEmpresaViewModel(string Empresa, int Xml, int Erp);

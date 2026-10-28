namespace Hermenos.DTOs.Conciliacao;

public sealed record ResultadoConciliacaoDTO
(
int IdDocumento,
string Situacao,
IReadOnlyList<string> CamposIguais,
IReadOnlyList<string> CamposDivergentes,
IReadOnlyList<string> RegrasAplicadas,
int? IdLoteOrigem
);

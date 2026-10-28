namespace Hermenos.DTOs.Divergencia;

public sealed record DivergenciaDTO
(
int Id,
int IdEmpresa,
int IdDocumento,
string Categoria,
string Campo,
string ValorXml,
string ValorErp,
DateTime Identificada,
string Situacao,
string Responsavel
);

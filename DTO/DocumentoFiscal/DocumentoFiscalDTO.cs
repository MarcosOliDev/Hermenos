namespace Hermenos.DTOs.DocumentoFiscal;

public sealed record DocumentoFiscalDTO(
int Id,
int IdEmpresa,
string Numero,
string Tipo,
string Chave,
string Fornecedor,
DateTime Emissao,
decimal? ValorXml,
decimal? ValorErp,
string Situacao,
int IdLote
);

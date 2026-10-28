namespace Hermenos.DTOs.Importacao;

public sealed record LoteImportacaoDTO(
int Id,
int IdEmpresa,
string Arquivo,
string Formato,
string Origem,
DateTime Recebido,
string Usuario,
int Encontrados,
int Validos,
int Rejeitados,
int Duplicados,
string Situacao);

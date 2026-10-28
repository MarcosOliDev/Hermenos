using Hermenos.DTOs.Divergencia;
using Hermenos.DTOs.DocumentoFiscal;
using Hermenos.DTOs.Empresa;
using Hermenos.DTOs.Importacao;
namespace Hermenos.Data.DadosFicticios;

public sealed class DadosDemonstracao
{
	public DateTime AtualizadoEm { get; } = new(2026, 10, 27, 9, 30, 0); public string Competencia { get; } = "2026-09";
	public IReadOnlyList<EmpresaResumoDTO> Empresas { get; } = new List<EmpresaResumoDTO>
	{
	new(1, "Supermercado Alfa", "12.345.678/0001-90", "Ativa"),
	new(2, "Construtora Horizonte", "23.456.789/0001-01", "Ativa"),
	new(3, "Clínica Bem Viver", "34.567.890/0001-12", "Ativa"),
	new(4, "Indústria Vale Verde", "45.678.901/0001-23", "Ativa"),
	new(5, "Restaurante Sabor & Arte", "56.789.012/0001-34", "Ativa")
	};
	public IReadOnlyList<DocumentoFiscalDTO> Documentos { get; } = new List<DocumentoFiscalDTO>
	{
 new(101,1,"45821","NF-e","35261012345678000190550010000458211000045821","Distribuidora Central",
 new(2026,9,2),1580.40m,1580.40m,"Conciliado",1001),
 new(102,1,"45822","NF-e","35261012345678000190550010000458221000045822","Alimentos do Norte",
 new(2026,9,4),2420m,2390m,"Diferença de valor",1001),
 new(103,1,"45823","NF-e","35261012345678000190550010000458231000045823","Bebidas União",
 new(2026,9,7),890m,null,"Ausente no ERP",1001),
 new(104,1,"45824","NF-e","35261012345678000190550010000458241000045824","Frigorífico Real",
 new(2026,9,9),3440m,3440m,"Possível duplicidade",1002),
 new(105,1,"45825","NF-e","35261012345678000190550010000458251000045825","Hortifruti Campo",
 new(2026,9,12),720m,720m,"Cancelamento para revisão",1002),
 new(201,2,"7721","NFS-e","Não aplicável","Serviços Urbanos",
 new(2026,9,3),4500m,4500m,"Conciliado",2001),
 new(202,2,"7722","NFS-e","Não aplicável","Locações Brasil",
 new(2026,9,8),6200m,6000m,"Diferença de valor",2001),
 new(301,3,"99201","NF-e","35261034567890000112550010000992011000099201","Med Supply",
 new(2026,9,5),1800m,1800m,"Conciliado",3001),
 new(302,3,"99202","NF-e","35261034567890000112550010000992021000099202","Laboratório Vida",
 new(2026,9,14),930m,930m,"Diferença de quantidade",3001),
 new(401,4,"18440","NF-e","35261045678901000123550010000184401000184401","Aço Forte",
 new(2026,9,1),9100m,9100m,"Conciliado",4001),
 new(402,4,"18441","NF-e","35261045678901000123550010000184411000184411","Peças e Cia",
 new(2026,9,18),1230m,null,"Ausente no ERP",4001),
 new(501,5,"5508","NFS-e","Não aplicável","Limpeza Express",
 new(2026,9,10),680m,680m,"Conciliado",5001)};
	public IReadOnlyList<DivergenciaDTO> Divergencias { get; } = new List<DivergenciaDTO> {
	new(1, 1, 102, "Diferença de valor", "Valor total", "R$ 2.420,00", "R$ 2.390,00",
	new(2026, 10, 2), "Pendente", "—"),
	new(2, 1, 103, "Documento ausente em uma das fontes", "Documento", "Localizado no XML", "Não localizado",
	new(2026, 10, 3), "Em análise", "Mariana Costa"),
	new(3, 1, 104, "Possível duplicidade", "Número do documento", "1 ocorrência", "2 registros no ERP",
	new(2026, 10, 4), "Pendente", "—"),
	new(4, 1, 105, "Situação de cancelamento para revisão", "Status", "Cancelado no XML", "Ativo no ERP",
	new(2026, 10, 5), "Em análise", "Rafael Lima"),
	new(5, 2, 202, "Diferença de valor", "Valor total", "R$ 6.200,00", "R$ 6.000,00",
	new(2026, 10, 7), "Tratada", "Mariana Costa"),
	new(6, 3, 302, "Diferença de quantidade", "Quantidade de itens", "12", "11",
	new(2026, 10, 8), "Pendente", "—"),
	new(7, 4, 402, "Documento ausente em uma das fontes", "Documento", "Localizado no XML", "Não localizado",
	new(2026, 10, 9), "Encerrada", "Rafael Lima")
	};
	public IReadOnlyList<LoteImportacaoDTO> Lotes { get; } = new List<LoteImportacaoDTO> {
	new(1001, 1, "nfe_supermercado_2026-09.xml", "XML", "SEFAZ / XML",
	new(2026, 10, 1, 8, 30, 0), "Mariana Costa", 3, 3, 0, 0, "Concluído com alertas"),
	new(1002, 1, "erp_supermercado_setembro.csv", "CSV", "ERP demonstrativo",
	new(2026, 10, 2, 9, 15, 0), "Rafael Lima", 3, 2, 0, 1, "Concluído com alertas"),
	new(2001, 2, "servicos_setembro.xlsx", "XLSX", "ERP demonstrativo",
	new(2026, 10, 3, 10, 0, 0), "Mariana Costa", 2, 2, 0, 0, "Concluído"),
	new(3001, 3, "nf_clinica.json", "JSON", "ERP demonstrativo",
	new(2026, 10, 4, 14, 20, 0), "Rafael Lima", 2, 2, 0, 0, "Concluído"),
	new(4001, 4, "notas_vale.xml", "XML", "SEFAZ / XML",
	new(2026, 10, 5, 11, 5, 0), "Mariana Costa", 2, 1, 1, 0, "Falha no processamento"),
	new(5001, 5, "servicos_sabor.csv", "CSV", "ERP demonstrativo",
	new(2026, 10, 6, 16, 40, 0), "Rafael Lima", 1, 1, 0, 0, "Concluído")
	};
}


using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Hermenos.Migrations
{
    /// <inheritdoc />
    public partial class EstruturaInicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    IdUsuario = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nome = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: false),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false),
                    CriadoEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.IdUsuario);
                });

            migrationBuilder.CreateTable(
                name: "Empresas",
                columns: table => new
                {
                    IdEmpresa = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nome = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Cnpj = table.Column<string>(type: "character varying(18)", maxLength: 18, nullable: false),
                    DataInscricaoEmpresa = table.Column<DateOnly>(type: "date", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CriadoPorUsuarioId = table.Column<int>(type: "integer", nullable: true),
                    CriadoEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Empresas", x => x.IdEmpresa);
                    table.ForeignKey(
                        name: "FK_Empresas_Usuarios_CriadoPorUsuarioId",
                        column: x => x.CriadoPorUsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "IdUsuario",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Fornecedores",
                columns: table => new
                {
                    IdFornecedor = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NomeRazaoSocial = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    NomeFantasia = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    DocumentoFiscal = table.Column<string>(type: "character varying(18)", maxLength: 18, nullable: false),
                    Email = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: true),
                    Telefone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CriadoPorUsuarioId = table.Column<int>(type: "integer", nullable: true),
                    CriadoEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fornecedores", x => x.IdFornecedor);
                    table.ForeignKey(
                        name: "FK_Fornecedores_Usuarios_CriadoPorUsuarioId",
                        column: x => x.CriadoPorUsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "IdUsuario",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Contratos",
                columns: table => new
                {
                    IdContrato = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdEmpresa = table.Column<int>(type: "integer", nullable: false),
                    Numero = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Descricao = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    VigenciaInicio = table.Column<DateOnly>(type: "date", nullable: false),
                    VigenciaFim = table.Column<DateOnly>(type: "date", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CriadoPorUsuarioId = table.Column<int>(type: "integer", nullable: false),
                    CriadoEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contratos", x => x.IdContrato);
                    table.ForeignKey(
                        name: "FK_Contratos_Empresas_IdEmpresa",
                        column: x => x.IdEmpresa,
                        principalTable: "Empresas",
                        principalColumn: "IdEmpresa",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Contratos_Usuarios_CriadoPorUsuarioId",
                        column: x => x.CriadoPorUsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "IdUsuario",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LotesImportacao",
                columns: table => new
                {
                    LoteImpId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdEmpresa = table.Column<int>(type: "integer", nullable: false),
                    ContratoId = table.Column<int>(type: "integer", nullable: true),
                    Competencia = table.Column<DateOnly>(type: "date", nullable: false),
                    NomeArquivo = table.Column<string>(type: "character varying(260)", maxLength: 260, nullable: false),
                    Formato = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Origem = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    RecebidoEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    RecebidoPorUsuarioId = table.Column<int>(type: "integer", nullable: true),
                    RegistrosEncontrados = table.Column<int>(type: "integer", nullable: false),
                    RegistrosValidos = table.Column<int>(type: "integer", nullable: false),
                    RegistrosRejeitados = table.Column<int>(type: "integer", nullable: false),
                    RegistrosDuplicados = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LotesImportacao", x => x.LoteImpId);
                    table.ForeignKey(
                        name: "FK_LotesImportacao_Contratos_ContratoId",
                        column: x => x.ContratoId,
                        principalTable: "Contratos",
                        principalColumn: "IdContrato",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LotesImportacao_Empresas_IdEmpresa",
                        column: x => x.IdEmpresa,
                        principalTable: "Empresas",
                        principalColumn: "IdEmpresa",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LotesImportacao_Usuarios_RecebidoPorUsuarioId",
                        column: x => x.RecebidoPorUsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "IdUsuario",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "DocumentosFiscais",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EmpresaId = table.Column<int>(type: "integer", nullable: false),
                    FornecedorId = table.Column<int>(type: "integer", nullable: true),
                    Numero = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    Tipo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ChaveAcesso = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    DataEmissao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ValorDocumento = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    LoteImportacaoId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentosFiscais", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DocumentosFiscais_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "IdEmpresa",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DocumentosFiscais_Fornecedores_FornecedorId",
                        column: x => x.FornecedorId,
                        principalTable: "Fornecedores",
                        principalColumn: "IdFornecedor",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DocumentosFiscais_LotesImportacao_LoteImportacaoId",
                        column: x => x.LoteImportacaoId,
                        principalTable: "LotesImportacao",
                        principalColumn: "LoteImpId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Divergencias",
                columns: table => new
                {
                    IdDivergencia = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdEmpresa = table.Column<int>(type: "integer", nullable: false),
                    IdDocumentoFiscal = table.Column<int>(type: "integer", nullable: false),
                    Categoria = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CampoAfetado = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ValorFonteXml = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    ValorFonteERP = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    IdentificadaEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ResponsavelDemonstrativo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Divergencias", x => x.IdDivergencia);
                    table.ForeignKey(
                        name: "FK_Divergencias_DocumentosFiscais_IdDocumentoFiscal",
                        column: x => x.IdDocumentoFiscal,
                        principalTable: "DocumentosFiscais",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Divergencias_Empresas_IdEmpresa",
                        column: x => x.IdEmpresa,
                        principalTable: "Empresas",
                        principalColumn: "IdEmpresa",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RegistrosERP",
                columns: table => new
                {
                    RegERPId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EmpresaId = table.Column<int>(type: "integer", nullable: false),
                    DocumentoFiscalId = table.Column<int>(type: "integer", nullable: true),
                    NumeroDocumento = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    Fornecedor = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    DataEmissao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ValorDocumento = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    QuantidadeItens = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegistrosERP", x => x.RegERPId);
                    table.ForeignKey(
                        name: "FK_RegistrosERP_DocumentosFiscais_DocumentoFiscalId",
                        column: x => x.DocumentoFiscalId,
                        principalTable: "DocumentosFiscais",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RegistrosERP_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "IdEmpresa",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Conciliacoes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EmpresaId = table.Column<int>(type: "integer", nullable: false),
                    DocumentoFiscalId = table.Column<int>(type: "integer", nullable: false),
                    RegistroERPId = table.Column<int>(type: "integer", nullable: true),
                    ExecutadaEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    RegraAplicada = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Conciliacoes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Conciliacoes_DocumentosFiscais_DocumentoFiscalId",
                        column: x => x.DocumentoFiscalId,
                        principalTable: "DocumentosFiscais",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Conciliacoes_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "IdEmpresa",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Conciliacoes_RegistrosERP_RegistroERPId",
                        column: x => x.RegistroERPId,
                        principalTable: "RegistrosERP",
                        principalColumn: "RegERPId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Conciliacoes_DocumentoFiscalId",
                table: "Conciliacoes",
                column: "DocumentoFiscalId");

            migrationBuilder.CreateIndex(
                name: "IX_Conciliacoes_EmpresaId",
                table: "Conciliacoes",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_Conciliacoes_RegistroERPId",
                table: "Conciliacoes",
                column: "RegistroERPId");

            migrationBuilder.CreateIndex(
                name: "IX_Contratos_CriadoPorUsuarioId",
                table: "Contratos",
                column: "CriadoPorUsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Contratos_IdEmpresa_Numero",
                table: "Contratos",
                columns: new[] { "IdEmpresa", "Numero" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Divergencias_IdDocumentoFiscal",
                table: "Divergencias",
                column: "IdDocumentoFiscal");

            migrationBuilder.CreateIndex(
                name: "IX_Divergencias_IdEmpresa",
                table: "Divergencias",
                column: "IdEmpresa");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentosFiscais_ChaveAcesso",
                table: "DocumentosFiscais",
                column: "ChaveAcesso",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DocumentosFiscais_EmpresaId",
                table: "DocumentosFiscais",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentosFiscais_FornecedorId",
                table: "DocumentosFiscais",
                column: "FornecedorId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentosFiscais_LoteImportacaoId",
                table: "DocumentosFiscais",
                column: "LoteImportacaoId");

            migrationBuilder.CreateIndex(
                name: "IX_Empresas_Cnpj",
                table: "Empresas",
                column: "Cnpj",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Empresas_CriadoPorUsuarioId",
                table: "Empresas",
                column: "CriadoPorUsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Fornecedores_CriadoPorUsuarioId",
                table: "Fornecedores",
                column: "CriadoPorUsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Fornecedores_DocumentoFiscal",
                table: "Fornecedores",
                column: "DocumentoFiscal",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LotesImportacao_ContratoId",
                table: "LotesImportacao",
                column: "ContratoId");

            migrationBuilder.CreateIndex(
                name: "IX_LotesImportacao_IdEmpresa_Competencia",
                table: "LotesImportacao",
                columns: new[] { "IdEmpresa", "Competencia" });

            migrationBuilder.CreateIndex(
                name: "IX_LotesImportacao_RecebidoPorUsuarioId",
                table: "LotesImportacao",
                column: "RecebidoPorUsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_RegistrosERP_DocumentoFiscalId",
                table: "RegistrosERP",
                column: "DocumentoFiscalId");

            migrationBuilder.CreateIndex(
                name: "IX_RegistrosERP_EmpresaId",
                table: "RegistrosERP",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_Email",
                table: "Usuarios",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Conciliacoes");

            migrationBuilder.DropTable(
                name: "Divergencias");

            migrationBuilder.DropTable(
                name: "RegistrosERP");

            migrationBuilder.DropTable(
                name: "DocumentosFiscais");

            migrationBuilder.DropTable(
                name: "Fornecedores");

            migrationBuilder.DropTable(
                name: "LotesImportacao");

            migrationBuilder.DropTable(
                name: "Contratos");

            migrationBuilder.DropTable(
                name: "Empresas");

            migrationBuilder.DropTable(
                name: "Usuarios");
        }
    }
}

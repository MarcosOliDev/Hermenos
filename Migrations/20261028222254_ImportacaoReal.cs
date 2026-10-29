using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hermenos.Migrations
{
    /// <inheritdoc />
    public partial class ImportacaoReal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DocumentosFiscais_EmpresaId",
                table: "DocumentosFiscais");

            migrationBuilder.AddColumn<decimal>(
                name: "QuantidadeItens",
                table: "DocumentosFiscais",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DocumentosFiscais_EmpresaId_Numero",
                table: "DocumentosFiscais",
                columns: new[] { "EmpresaId", "Numero" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DocumentosFiscais_EmpresaId_Numero",
                table: "DocumentosFiscais");

            migrationBuilder.DropColumn(
                name: "QuantidadeItens",
                table: "DocumentosFiscais");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentosFiscais_EmpresaId",
                table: "DocumentosFiscais",
                column: "EmpresaId");
        }
    }
}

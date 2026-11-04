using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hermenos.Migrations
{
	public partial class PrimeiroLogineCredenciais : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			// 1. Adiciona as novas colunas.
			// As colunas que precisam de preenchimento começam permitindo NULL.

			migrationBuilder.AddColumn<DateTime>(
				name: "DataCadastro",
				table: "Usuarios",
				type: "timestamp with time zone",
				nullable: true);

			migrationBuilder.AddColumn<bool>(
				name: "DeveAlterarSenha",
				table: "Usuarios",
				type: "boolean",
				nullable: false,
				defaultValue: false);

			migrationBuilder.AddColumn<string>(
				name: "FotoPerfil",
				table: "Usuarios",
				type: "character varying(500)",
				maxLength: 500,
				nullable: true);

			migrationBuilder.AddColumn<string>(
				name: "NomeUsuario",
				table: "Usuarios",
				type: "character varying(80)",
				maxLength: 80,
				nullable: true);

			migrationBuilder.AddColumn<string>(
				name: "Perfil",
				table: "Usuarios",
				type: "character varying(20)",
				unicode: false,
				maxLength: 20,
				nullable: true);

			migrationBuilder.AddColumn<string>(
				name: "SenhaHash",
				table: "Usuarios",
				type: "character varying(500)",
				maxLength: 500,
				nullable: false,
				defaultValue: "");

			// 2. Preserva os dados dos usuários existentes e preenche
			// os novos campos antes de torná-los obrigatórios.

			migrationBuilder.Sql("""
                UPDATE "Usuarios"
                SET
                    "NomeUsuario" = CASE
                        WHEN lower("Email") = 'mariana.costa@hermenos.local'
                            THEN 'mariana.costa'
                        WHEN lower("Email") = 'rafael.lima@hermenos.local'
                            THEN 'rafael.lima'
                        ELSE
                            lower(
                                regexp_replace(
                                    split_part("Email", '@', 1),
                                    '[^a-z0-9._-]',
                                    '',
                                    'g'
                                )
                            ) || '.' || "IdUsuario"::text
                    END,

                    "Perfil" = CASE
                        WHEN lower("Email") = 'mariana.costa@hermenos.local'
                            THEN 'ContadorPleno'
                        WHEN lower("Email") = 'rafael.lima@hermenos.local'
                            THEN 'ContadorSenior'
                        ELSE 'ContadorJunior'
                    END,

                    "DataCadastro" = "CriadoEm",
                    "DeveAlterarSenha" = TRUE;
                """);

			// 3. Torna obrigatórias as colunas preenchidas.

			migrationBuilder.AlterColumn<DateTime>(
				name: "DataCadastro",
				table: "Usuarios",
				type: "timestamp with time zone",
				nullable: false,
				oldClrType: typeof(DateTime),
				oldType: "timestamp with time zone",
				oldNullable: true);

			migrationBuilder.AlterColumn<string>(
				name: "NomeUsuario",
				table: "Usuarios",
				type: "character varying(80)",
				maxLength: 80,
				nullable: false,
				oldClrType: typeof(string),
				oldType: "character varying(80)",
				oldMaxLength: 80,
				oldNullable: true);

			migrationBuilder.AlterColumn<string>(
				name: "Perfil",
				table: "Usuarios",
				type: "character varying(20)",
				unicode: false,
				maxLength: 20,
				nullable: false,
				oldClrType: typeof(string),
				oldType: "character varying(20)",
				oldUnicode: false,
				oldMaxLength: 20,
				oldNullable: true);

			// 4. Cria o índice somente depois de preencher os usernames.

			migrationBuilder.CreateIndex(
				name: "IX_Usuarios_NomeUsuario",
				table: "Usuarios",
				column: "NomeUsuario",
				unique: true);
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropIndex(
				name: "IX_Usuarios_NomeUsuario",
				table: "Usuarios");

			migrationBuilder.DropColumn(
				name: "DataCadastro",
				table: "Usuarios");

			migrationBuilder.DropColumn(
				name: "DeveAlterarSenha",
				table: "Usuarios");

			migrationBuilder.DropColumn(
				name: "FotoPerfil",
				table: "Usuarios");

			migrationBuilder.DropColumn(
				name: "NomeUsuario",
				table: "Usuarios");

			migrationBuilder.DropColumn(
				name: "Perfil",
				table: "Usuarios");

			migrationBuilder.DropColumn(
				name: "SenhaHash",
				table: "Usuarios");
		}
	}
}
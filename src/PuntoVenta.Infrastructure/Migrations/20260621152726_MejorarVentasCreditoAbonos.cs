using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PuntoVenta.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MejorarVentasCreditoAbonos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Anulado",
                table: "DocumentosVentaPagos",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaAnulacionUtc",
                table: "DocumentosVentaPagos",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaRegistroUtc",
                table: "DocumentosVentaPagos",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "MotivoAnulacion",
                table: "DocumentosVentaPagos",
                type: "TEXT",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NumeroAbono",
                table: "DocumentosVentaPagos",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "UsuarioAnulaId",
                table: "DocumentosVentaPagos",
                type: "TEXT",
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE "DocumentosVentaPagos"
                SET "FechaRegistroUtc" = COALESCE("FechaPago", CURRENT_TIMESTAMP)
                WHERE "FechaRegistroUtc" = '0001-01-01 00:00:00';
                """);

            migrationBuilder.Sql(
                """
                WITH pagos_ordenados AS (
                    SELECT
                        p."Id",
                        CASE
                            WHEN d."TipoDocumento" = 2 THEN
                                ROW_NUMBER() OVER (
                                    PARTITION BY p."DocumentoVentaId"
                                    ORDER BY p."FechaPago", p."Id")
                            WHEN d."TipoDocumento" = 1
                                AND d."CondicionVentaCodigo" IN ('02', '10') THEN
                                ROW_NUMBER() OVER (
                                    PARTITION BY p."DocumentoVentaId"
                                    ORDER BY p."FechaPago", p."Id")
                            ELSE 0
                        END AS "NumeroAbono"
                    FROM "DocumentosVentaPagos" p
                    INNER JOIN "DocumentosVenta" d ON d."Id" = p."DocumentoVentaId"
                )
                UPDATE "DocumentosVentaPagos"
                SET "NumeroAbono" = (
                    SELECT po."NumeroAbono"
                    FROM pagos_ordenados po
                    WHERE po."Id" = "DocumentosVentaPagos"."Id")
                WHERE EXISTS (
                    SELECT 1
                    FROM pagos_ordenados po
                    WHERE po."Id" = "DocumentosVentaPagos"."Id");
                """);

            migrationBuilder.CreateIndex(
                name: "IX_DocumentosVentaPagos_UsuarioAnulaId",
                table: "DocumentosVentaPagos",
                column: "UsuarioAnulaId");

            migrationBuilder.AddForeignKey(
                name: "FK_DocumentosVentaPagos_Usuarios_UsuarioAnulaId",
                table: "DocumentosVentaPagos",
                column: "UsuarioAnulaId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DocumentosVentaPagos_Usuarios_UsuarioAnulaId",
                table: "DocumentosVentaPagos");

            migrationBuilder.DropIndex(
                name: "IX_DocumentosVentaPagos_UsuarioAnulaId",
                table: "DocumentosVentaPagos");

            migrationBuilder.DropColumn(
                name: "Anulado",
                table: "DocumentosVentaPagos");

            migrationBuilder.DropColumn(
                name: "FechaAnulacionUtc",
                table: "DocumentosVentaPagos");

            migrationBuilder.DropColumn(
                name: "FechaRegistroUtc",
                table: "DocumentosVentaPagos");

            migrationBuilder.DropColumn(
                name: "MotivoAnulacion",
                table: "DocumentosVentaPagos");

            migrationBuilder.DropColumn(
                name: "NumeroAbono",
                table: "DocumentosVentaPagos");

            migrationBuilder.DropColumn(
                name: "UsuarioAnulaId",
                table: "DocumentosVentaPagos");
        }
    }
}

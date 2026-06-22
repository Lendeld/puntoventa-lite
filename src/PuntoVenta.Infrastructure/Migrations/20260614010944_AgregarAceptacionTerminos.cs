using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PuntoVenta.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AgregarAceptacionTerminos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "TerminosAceptadosFechaUtc",
                table: "Negocios",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TerminosAceptadosVersion",
                table: "Negocios",
                type: "TEXT",
                maxLength: 40,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TerminosAceptadosFechaUtc",
                table: "Negocios");

            migrationBuilder.DropColumn(
                name: "TerminosAceptadosVersion",
                table: "Negocios");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PuntoVenta.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class PodaFina_ReferenciaSinCodigoRazon : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CodigoRazon",
                table: "DocumentosVentaReferencias");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CodigoRazon",
                table: "DocumentosVentaReferencias",
                type: "TEXT",
                maxLength: 2,
                nullable: false,
                defaultValue: "");
        }
    }
}

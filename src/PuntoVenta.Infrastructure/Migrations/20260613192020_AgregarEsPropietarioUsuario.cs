using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PuntoVenta.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AgregarEsPropietarioUsuario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "EsPropietario",
                table: "Usuarios",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            // Backfill: marca al admin sembrado (el usuario más antiguo) como
            // propietario en bases existentes. En instalaciones nuevas lo marca el seeder.
            migrationBuilder.Sql(
                "UPDATE Usuarios SET EsPropietario = 1 " +
                "WHERE Id = (SELECT Id FROM Usuarios ORDER BY FechaCreacion ASC LIMIT 1);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EsPropietario",
                table: "Usuarios");
        }
    }
}

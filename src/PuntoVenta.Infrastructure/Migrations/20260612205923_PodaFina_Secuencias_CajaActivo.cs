using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PuntoVenta.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class PodaFina_Secuencias_CajaActivo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SecuenciasDocumentoCaja");

            migrationBuilder.RenameColumn(
                name: "AplicaGestionCajas",
                table: "Negocios",
                newName: "AplicaCajas");

            migrationBuilder.CreateTable(
                name: "Secuencias",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TipoDocumento = table.Column<int>(type: "INTEGER", nullable: false),
                    UltimoNumero = table.Column<long>(type: "INTEGER", nullable: false),
                    Activo = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    UsuarioCreacionId = table.Column<Guid>(type: "TEXT", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UsuarioModificacionId = table.Column<Guid>(type: "TEXT", nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Secuencias", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Secuencias_Usuarios_UsuarioCreacionId",
                        column: x => x.UsuarioCreacionId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Secuencias_Usuarios_UsuarioModificacionId",
                        column: x => x.UsuarioModificacionId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Secuencias_TipoDocumento",
                table: "Secuencias",
                column: "TipoDocumento",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Secuencias_UsuarioCreacionId",
                table: "Secuencias",
                column: "UsuarioCreacionId");

            migrationBuilder.CreateIndex(
                name: "IX_Secuencias_UsuarioModificacionId",
                table: "Secuencias",
                column: "UsuarioModificacionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Secuencias");

            migrationBuilder.RenameColumn(
                name: "AplicaCajas",
                table: "Negocios",
                newName: "AplicaGestionCajas");

            migrationBuilder.CreateTable(
                name: "SecuenciasDocumentoCaja",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UsuarioCreacionId = table.Column<Guid>(type: "TEXT", nullable: true),
                    UsuarioModificacionId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Activo = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    CajaId = table.Column<Guid>(type: "TEXT", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FechaDocumento = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    FechaModificacion = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UltimoNumero = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SecuenciasDocumentoCaja", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SecuenciasDocumentoCaja_Usuarios_UsuarioCreacionId",
                        column: x => x.UsuarioCreacionId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SecuenciasDocumentoCaja_Usuarios_UsuarioModificacionId",
                        column: x => x.UsuarioModificacionId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SecuenciasDocumentoCaja_CajaId_FechaDocumento",
                table: "SecuenciasDocumentoCaja",
                columns: new[] { "CajaId", "FechaDocumento" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SecuenciasDocumentoCaja_UsuarioCreacionId",
                table: "SecuenciasDocumentoCaja",
                column: "UsuarioCreacionId");

            migrationBuilder.CreateIndex(
                name: "IX_SecuenciasDocumentoCaja_UsuarioModificacionId",
                table: "SecuenciasDocumentoCaja",
                column: "UsuarioModificacionId");
        }
    }
}

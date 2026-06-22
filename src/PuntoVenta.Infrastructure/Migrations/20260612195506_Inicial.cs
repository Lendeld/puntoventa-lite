using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PuntoVenta.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Inicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DataProtectionKeys",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FriendlyName = table.Column<string>(type: "TEXT", nullable: true),
                    Xml = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataProtectionKeys", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RefreshTokenSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TokenHash = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    ExpiracionUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreadoEnUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    RevocadoEnUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ReemplazadoPorTokenHash = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true),
                    UltimoUsoEnUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreadoPorIp = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    UltimoUsoPorIp = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokenSessions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TokensRevocados",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Jti = table.Column<string>(type: "TEXT", maxLength: 36, nullable: false),
                    FechaExpiracion = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FechaRevocacion = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TokensRevocados", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    NombreUsuario = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Nombre = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    Correo = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Identificacion = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Telefono = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    PasswordHash = table.Column<string>(type: "TEXT", maxLength: 512, nullable: false),
                    DebeCambiarPassword = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    PasswordTemporalExpiraEnUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    EsSuperAdmin = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    Activo = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    RolId = table.Column<Guid>(type: "TEXT", nullable: true),
                    UsuarioCreacionId = table.Column<Guid>(type: "TEXT", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UsuarioModificacionId = table.Column<Guid>(type: "TEXT", nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Usuarios_Usuarios_UsuarioCreacionId",
                        column: x => x.UsuarioCreacionId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Usuarios_Usuarios_UsuarioModificacionId",
                        column: x => x.UsuarioModificacionId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Cajas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Codigo = table.Column<string>(type: "TEXT", maxLength: 8, nullable: false),
                    CodigoNormalizado = table.Column<string>(type: "TEXT", maxLength: 8, nullable: false),
                    Nombre = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Activo = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    UsuarioCreacionId = table.Column<Guid>(type: "TEXT", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UsuarioModificacionId = table.Column<Guid>(type: "TEXT", nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cajas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Cajas_Usuarios_UsuarioCreacionId",
                        column: x => x.UsuarioCreacionId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Cajas_Usuarios_UsuarioModificacionId",
                        column: x => x.UsuarioModificacionId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Categorias",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Nombre = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    NombreNormalizado = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    Descripcion = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    Activo = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    UsuarioCreacionId = table.Column<Guid>(type: "TEXT", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UsuarioModificacionId = table.Column<Guid>(type: "TEXT", nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categorias", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Categorias_Usuarios_UsuarioCreacionId",
                        column: x => x.UsuarioCreacionId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Categorias_Usuarios_UsuarioModificacionId",
                        column: x => x.UsuarioModificacionId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Clientes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Nombre = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    NombreNormalizado = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Identificacion = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    Correo = table.Column<string>(type: "TEXT", maxLength: 160, nullable: true),
                    Telefono = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    Observaciones = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Activo = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    UsuarioCreacionId = table.Column<Guid>(type: "TEXT", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UsuarioModificacionId = table.Column<Guid>(type: "TEXT", nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clientes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Clientes_Usuarios_UsuarioCreacionId",
                        column: x => x.UsuarioCreacionId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Clientes_Usuarios_UsuarioModificacionId",
                        column: x => x.UsuarioModificacionId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CodigosImpuesto",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Codigo = table.Column<string>(type: "TEXT", maxLength: 2, nullable: false),
                    Detalle = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    Comentario = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    Activo = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    UsuarioCreacionId = table.Column<Guid>(type: "TEXT", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UsuarioModificacionId = table.Column<Guid>(type: "TEXT", nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CodigosImpuesto", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CodigosImpuesto_Usuarios_UsuarioCreacionId",
                        column: x => x.UsuarioCreacionId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CodigosImpuesto_Usuarios_UsuarioModificacionId",
                        column: x => x.UsuarioModificacionId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CondicionesVenta",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Codigo = table.Column<string>(type: "TEXT", maxLength: 2, nullable: false),
                    Detalle = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Comentario = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    Activo = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    UsuarioCreacionId = table.Column<Guid>(type: "TEXT", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UsuarioModificacionId = table.Column<Guid>(type: "TEXT", nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CondicionesVenta", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CondicionesVenta_Usuarios_UsuarioCreacionId",
                        column: x => x.UsuarioCreacionId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CondicionesVenta_Usuarios_UsuarioModificacionId",
                        column: x => x.UsuarioModificacionId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MediosPago",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Codigo = table.Column<string>(type: "TEXT", maxLength: 2, nullable: false),
                    Detalle = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Comentario = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    Activo = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    UsuarioCreacionId = table.Column<Guid>(type: "TEXT", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UsuarioModificacionId = table.Column<Guid>(type: "TEXT", nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MediosPago", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MediosPago_Usuarios_UsuarioCreacionId",
                        column: x => x.UsuarioCreacionId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MediosPago_Usuarios_UsuarioModificacionId",
                        column: x => x.UsuarioModificacionId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Negocios",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Nombre = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    NombreComercial = table.Column<string>(type: "TEXT", maxLength: 80, nullable: true),
                    Direccion = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    Identificacion = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    Correo = table.Column<string>(type: "TEXT", maxLength: 160, nullable: true),
                    Telefono = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    AplicaVendedores = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    AplicaGestionCajas = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    LogoUrl = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Activo = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    UsuarioCreacionId = table.Column<Guid>(type: "TEXT", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UsuarioModificacionId = table.Column<Guid>(type: "TEXT", nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Negocios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Negocios_Usuarios_UsuarioCreacionId",
                        column: x => x.UsuarioCreacionId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Negocios_Usuarios_UsuarioModificacionId",
                        column: x => x.UsuarioModificacionId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "NegocioTicketConfigs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    MensajePie = table.Column<string>(type: "TEXT", maxLength: 240, nullable: true),
                    MostrarLogo = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    AplicaCopiaClienteNegocio = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    MostrarCodigoBarras = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    Activo = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    UsuarioCreacionId = table.Column<Guid>(type: "TEXT", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UsuarioModificacionId = table.Column<Guid>(type: "TEXT", nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Configuraciones = table.Column<string>(type: "TEXT", nullable: true),
                    ElementosEncabezado = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NegocioTicketConfigs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NegocioTicketConfigs_Usuarios_UsuarioCreacionId",
                        column: x => x.UsuarioCreacionId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NegocioTicketConfigs_Usuarios_UsuarioModificacionId",
                        column: x => x.UsuarioModificacionId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Paginas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Nombre = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    Ruta = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Icono = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Orden = table.Column<int>(type: "INTEGER", nullable: false),
                    PaginaPadreId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Activo = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    UsuarioCreacionId = table.Column<Guid>(type: "TEXT", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UsuarioModificacionId = table.Column<Guid>(type: "TEXT", nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Paginas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Paginas_Paginas_PaginaPadreId",
                        column: x => x.PaginaPadreId,
                        principalTable: "Paginas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Paginas_Usuarios_UsuarioCreacionId",
                        column: x => x.UsuarioCreacionId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Paginas_Usuarios_UsuarioModificacionId",
                        column: x => x.UsuarioModificacionId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PerfilesImpresoraTicket",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Clave = table.Column<string>(type: "TEXT", maxLength: 60, nullable: false),
                    Nombre = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    AnchoMm = table.Column<int>(type: "INTEGER", nullable: false),
                    CharsPorLinea = table.Column<int>(type: "INTEGER", nullable: false),
                    Codepage = table.Column<string>(type: "TEXT", maxLength: 16, nullable: false),
                    DrawerPin = table.Column<byte>(type: "INTEGER", nullable: false),
                    ComandoCorte = table.Column<int>(type: "INTEGER", nullable: false),
                    Densidad = table.Column<byte>(type: "INTEGER", nullable: false),
                    Activo = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    UsuarioCreacionId = table.Column<Guid>(type: "TEXT", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UsuarioModificacionId = table.Column<Guid>(type: "TEXT", nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PerfilesImpresoraTicket", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PerfilesImpresoraTicket_Usuarios_UsuarioCreacionId",
                        column: x => x.UsuarioCreacionId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PerfilesImpresoraTicket_Usuarios_UsuarioModificacionId",
                        column: x => x.UsuarioModificacionId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Permisos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Clave = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Modulo = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Activo = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    UsuarioCreacionId = table.Column<Guid>(type: "TEXT", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UsuarioModificacionId = table.Column<Guid>(type: "TEXT", nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permisos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Permisos_Usuarios_UsuarioCreacionId",
                        column: x => x.UsuarioCreacionId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Permisos_Usuarios_UsuarioModificacionId",
                        column: x => x.UsuarioModificacionId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Nombre = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    IsPrincipal = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    Activo = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    UsuarioCreacionId = table.Column<Guid>(type: "TEXT", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UsuarioModificacionId = table.Column<Guid>(type: "TEXT", nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Roles_Usuarios_UsuarioCreacionId",
                        column: x => x.UsuarioCreacionId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Roles_Usuarios_UsuarioModificacionId",
                        column: x => x.UsuarioModificacionId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SecuenciasDocumentoCaja",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    CajaId = table.Column<Guid>(type: "TEXT", nullable: false),
                    FechaDocumento = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    UltimoNumero = table.Column<int>(type: "INTEGER", nullable: false),
                    Activo = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    UsuarioCreacionId = table.Column<Guid>(type: "TEXT", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UsuarioModificacionId = table.Column<Guid>(type: "TEXT", nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "TEXT", nullable: true)
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

            migrationBuilder.CreateTable(
                name: "TarifasIvaImpuesto",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Codigo = table.Column<string>(type: "TEXT", maxLength: 2, nullable: false),
                    Detalle = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Porcentaje = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    Comentario = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    Activo = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    UsuarioCreacionId = table.Column<Guid>(type: "TEXT", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UsuarioModificacionId = table.Column<Guid>(type: "TEXT", nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TarifasIvaImpuesto", x => x.Id);
                    table.UniqueConstraint("AK_TarifasIvaImpuesto_Codigo", x => x.Codigo);
                    table.ForeignKey(
                        name: "FK_TarifasIvaImpuesto_Usuarios_UsuarioCreacionId",
                        column: x => x.UsuarioCreacionId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TarifasIvaImpuesto_Usuarios_UsuarioModificacionId",
                        column: x => x.UsuarioModificacionId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TipoDocumentoVentaEvento",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Codigo = table.Column<string>(type: "TEXT", nullable: false),
                    Nombre = table.Column<string>(type: "TEXT", nullable: false),
                    Descripcion = table.Column<string>(type: "TEXT", nullable: true),
                    Categoria = table.Column<string>(type: "TEXT", nullable: false),
                    IconoSugerido = table.Column<string>(type: "TEXT", nullable: true),
                    ColorSugerido = table.Column<string>(type: "TEXT", nullable: true),
                    Activo = table.Column<bool>(type: "INTEGER", nullable: false),
                    UsuarioCreacionId = table.Column<Guid>(type: "TEXT", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UsuarioModificacionId = table.Column<Guid>(type: "TEXT", nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TipoDocumentoVentaEvento", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TipoDocumentoVentaEvento_Usuarios_UsuarioCreacionId",
                        column: x => x.UsuarioCreacionId,
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TipoDocumentoVentaEvento_Usuarios_UsuarioModificacionId",
                        column: x => x.UsuarioModificacionId,
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TiposIdentificacion",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Codigo = table.Column<string>(type: "TEXT", nullable: false),
                    Detalle = table.Column<string>(type: "TEXT", nullable: false),
                    Comentario = table.Column<string>(type: "TEXT", nullable: true),
                    Activo = table.Column<bool>(type: "INTEGER", nullable: false),
                    UsuarioCreacionId = table.Column<Guid>(type: "TEXT", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UsuarioModificacionId = table.Column<Guid>(type: "TEXT", nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TiposIdentificacion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TiposIdentificacion_Usuarios_UsuarioCreacionId",
                        column: x => x.UsuarioCreacionId,
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TiposIdentificacion_Usuarios_UsuarioModificacionId",
                        column: x => x.UsuarioModificacionId,
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Vendedores",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Nombre = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    NombreNormalizado = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    IsPrincipal = table.Column<bool>(type: "INTEGER", nullable: false),
                    Activo = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    UsuarioCreacionId = table.Column<Guid>(type: "TEXT", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UsuarioModificacionId = table.Column<Guid>(type: "TEXT", nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vendedores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Vendedores_Usuarios_UsuarioCreacionId",
                        column: x => x.UsuarioCreacionId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Vendedores_Usuarios_UsuarioModificacionId",
                        column: x => x.UsuarioModificacionId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PaginaPermisos",
                columns: table => new
                {
                    PaginaId = table.Column<Guid>(type: "TEXT", nullable: false),
                    PermisoId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaginaPermisos", x => new { x.PaginaId, x.PermisoId });
                    table.ForeignKey(
                        name: "FK_PaginaPermisos_Paginas_PaginaId",
                        column: x => x.PaginaId,
                        principalTable: "Paginas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PaginaPermisos_Permisos_PermisoId",
                        column: x => x.PermisoId,
                        principalTable: "Permisos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RolPermisos",
                columns: table => new
                {
                    RolId = table.Column<Guid>(type: "TEXT", nullable: false),
                    PermisoId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolPermisos", x => new { x.RolId, x.PermisoId });
                    table.ForeignKey(
                        name: "FK_RolPermisos_Permisos_PermisoId",
                        column: x => x.PermisoId,
                        principalTable: "Permisos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RolPermisos_Roles_RolId",
                        column: x => x.RolId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Productos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Codigo = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    CodigoBarras = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    Nombre = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    Descripcion = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    TipoItem = table.Column<int>(type: "INTEGER", nullable: false),
                    ImagenUrl = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    PrecioUnitario = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    PrecioCosto = table.Column<decimal>(type: "decimal(18,5)", nullable: true),
                    CategoriaId = table.Column<Guid>(type: "TEXT", nullable: true),
                    TarifaIvaImpuestoCodigo = table.Column<string>(type: "TEXT", maxLength: 2, nullable: true),
                    NoAplicaExistencias = table.Column<bool>(type: "INTEGER", nullable: false),
                    PermiteModificarPrecioUnitario = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    Activo = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    UsuarioCreacionId = table.Column<Guid>(type: "TEXT", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UsuarioModificacionId = table.Column<Guid>(type: "TEXT", nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Productos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Productos_Categorias_CategoriaId",
                        column: x => x.CategoriaId,
                        principalTable: "Categorias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Productos_TarifasIvaImpuesto_TarifaIvaImpuestoCodigo",
                        column: x => x.TarifaIvaImpuestoCodigo,
                        principalTable: "TarifasIvaImpuesto",
                        principalColumn: "Codigo",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Productos_Usuarios_UsuarioCreacionId",
                        column: x => x.UsuarioCreacionId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Productos_Usuarios_UsuarioModificacionId",
                        column: x => x.UsuarioModificacionId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DocumentosVenta",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TipoDocumento = table.Column<int>(type: "INTEGER", nullable: false),
                    Estado = table.Column<int>(type: "INTEGER", nullable: false),
                    ClienteId = table.Column<Guid>(type: "TEXT", nullable: true),
                    VendedorId = table.Column<Guid>(type: "TEXT", nullable: true),
                    CondicionVentaCodigo = table.Column<string>(type: "TEXT", maxLength: 2, nullable: false),
                    CondicionVentaDetalleSnapshot = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    DocumentoOrigenId = table.Column<Guid>(type: "TEXT", nullable: true),
                    PlazoCreditoDias = table.Column<int>(type: "INTEGER", nullable: true),
                    FechaVencimiento = table.Column<DateTime>(type: "TEXT", nullable: true),
                    FechaDocumento = table.Column<DateTime>(type: "TEXT", nullable: false),
                    MonedaCodigo = table.Column<string>(type: "TEXT", maxLength: 3, nullable: false),
                    TipoCambio = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    NumeroConsecutivo = table.Column<long>(type: "INTEGER", nullable: true),
                    Consecutivo = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    TotalServiciosGravados = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    TotalServiciosExentos = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    TotalMercanciasGravadas = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    TotalMercanciasExentas = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    TotalVenta = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    TotalDescuentos = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    TotalImpuesto = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    TotalComprobante = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    TotalPagado = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    SaldoPendiente = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    FechaCancelacion = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Observaciones = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    CajaId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Activo = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    UsuarioCreacionId = table.Column<Guid>(type: "TEXT", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UsuarioModificacionId = table.Column<Guid>(type: "TEXT", nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentosVenta", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DocumentosVenta_Cajas_CajaId",
                        column: x => x.CajaId,
                        principalTable: "Cajas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DocumentosVenta_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DocumentosVenta_DocumentosVenta_DocumentoOrigenId",
                        column: x => x.DocumentoOrigenId,
                        principalTable: "DocumentosVenta",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DocumentosVenta_Usuarios_UsuarioCreacionId",
                        column: x => x.UsuarioCreacionId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DocumentosVenta_Usuarios_UsuarioModificacionId",
                        column: x => x.UsuarioModificacionId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DocumentosVenta_Vendedores_VendedorId",
                        column: x => x.VendedorId,
                        principalTable: "Vendedores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DocumentosVentaEventos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    DocumentoVentaId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TipoEventoCodigo = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    TipoEventoId = table.Column<Guid>(type: "TEXT", nullable: true),
                    OcurridoEn = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Resumen = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Payload = table.Column<string>(type: "TEXT", nullable: true),
                    CorrelacionId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Activo = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    UsuarioCreacionId = table.Column<Guid>(type: "TEXT", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UsuarioModificacionId = table.Column<Guid>(type: "TEXT", nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentosVentaEventos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DocumentosVentaEventos_DocumentosVenta_DocumentoVentaId",
                        column: x => x.DocumentoVentaId,
                        principalTable: "DocumentosVenta",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DocumentosVentaEventos_TipoDocumentoVentaEvento_TipoEventoId",
                        column: x => x.TipoEventoId,
                        principalTable: "TipoDocumentoVentaEvento",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DocumentosVentaEventos_Usuarios_UsuarioCreacionId",
                        column: x => x.UsuarioCreacionId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DocumentosVentaEventos_Usuarios_UsuarioModificacionId",
                        column: x => x.UsuarioModificacionId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DocumentosVentaLineas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    DocumentoVentaId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProductoId = table.Column<Guid>(type: "TEXT", nullable: true),
                    TipoItem = table.Column<int>(type: "INTEGER", nullable: false),
                    Codigo = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Descripcion = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    UnidadMedidaCodigo = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    TarifaIvaImpuestoCodigo = table.Column<string>(type: "TEXT", maxLength: 2, nullable: true),
                    PorcentajeImpuesto = table.Column<decimal>(type: "decimal(7,4)", nullable: false),
                    Cantidad = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    PrecioUnitario = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    MontoDescuento = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    Subtotal = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    MontoImpuesto = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    TotalLinea = table.Column<decimal>(type: "decimal(18,5)", nullable: false),
                    DevuelveInventario = table.Column<bool>(type: "INTEGER", nullable: false),
                    NoAplicaExistencias = table.Column<bool>(type: "INTEGER", nullable: false),
                    PermiteModificarPrecioUnitario = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentosVentaLineas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DocumentosVentaLineas_DocumentosVenta_DocumentoVentaId",
                        column: x => x.DocumentoVentaId,
                        principalTable: "DocumentosVenta",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DocumentosVentaLineas_Productos_ProductoId",
                        column: x => x.ProductoId,
                        principalTable: "Productos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DocumentosVentaPagos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    DocumentoVentaId = table.Column<Guid>(type: "TEXT", nullable: false),
                    MonedaCodigo = table.Column<string>(type: "TEXT", maxLength: 3, nullable: false),
                    TipoCambioAplicado = table.Column<decimal>(type: "numeric(18,6)", nullable: false),
                    MedioPagoCodigo = table.Column<string>(type: "TEXT", maxLength: 2, nullable: false),
                    MedioPagoDetalleSnapshot = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    MontoEntregado = table.Column<decimal>(type: "numeric(18,5)", nullable: false),
                    MontoAplicadoMonedaPago = table.Column<decimal>(type: "numeric(18,5)", nullable: false),
                    MontoAplicadoDocumento = table.Column<decimal>(type: "numeric(18,5)", nullable: false),
                    MontoVueltoMonedaPago = table.Column<decimal>(type: "numeric(18,5)", nullable: false),
                    MontoVueltoDocumento = table.Column<decimal>(type: "numeric(18,5)", nullable: false),
                    FechaPago = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UsuarioRegistroId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Referencia = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Observacion = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    ClaveHaciendaREP = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    ConsecutivoHaciendaREP = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    EstadoElectronicoREP = table.Column<string>(type: "TEXT", maxLength: 40, nullable: true),
                    FechaAceptacionREP = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentosVentaPagos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DocumentosVentaPagos_DocumentosVenta_DocumentoVentaId",
                        column: x => x.DocumentoVentaId,
                        principalTable: "DocumentosVenta",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DocumentosVentaPagos_Usuarios_UsuarioRegistroId",
                        column: x => x.UsuarioRegistroId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DocumentosVentaReferencias",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    DocumentoVentaId = table.Column<Guid>(type: "TEXT", nullable: false),
                    DocumentoReferenciaId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TipoDocReferencia = table.Column<string>(type: "TEXT", maxLength: 2, nullable: false),
                    FechaDocumentoReferencia = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CodigoRazon = table.Column<string>(type: "TEXT", maxLength: 2, nullable: false),
                    Razon = table.Column<string>(type: "TEXT", maxLength: 180, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentosVentaReferencias", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DocumentosVentaReferencias_DocumentosVenta_DocumentoReferenciaId",
                        column: x => x.DocumentoReferenciaId,
                        principalTable: "DocumentosVenta",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DocumentosVentaReferencias_DocumentosVenta_DocumentoVentaId",
                        column: x => x.DocumentoVentaId,
                        principalTable: "DocumentosVenta",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Cajas_CodigoNormalizado",
                table: "Cajas",
                column: "CodigoNormalizado",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cajas_UsuarioCreacionId",
                table: "Cajas",
                column: "UsuarioCreacionId");

            migrationBuilder.CreateIndex(
                name: "IX_Cajas_UsuarioModificacionId",
                table: "Cajas",
                column: "UsuarioModificacionId");

            migrationBuilder.CreateIndex(
                name: "IX_Categorias_NombreNormalizado",
                table: "Categorias",
                column: "NombreNormalizado",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Categorias_UsuarioCreacionId",
                table: "Categorias",
                column: "UsuarioCreacionId");

            migrationBuilder.CreateIndex(
                name: "IX_Categorias_UsuarioModificacionId",
                table: "Categorias",
                column: "UsuarioModificacionId");

            migrationBuilder.CreateIndex(
                name: "IX_Clientes_Identificacion",
                table: "Clientes",
                column: "Identificacion",
                unique: true,
                filter: "\"Identificacion\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Clientes_NombreNormalizado",
                table: "Clientes",
                column: "NombreNormalizado");

            migrationBuilder.CreateIndex(
                name: "IX_Clientes_UsuarioCreacionId",
                table: "Clientes",
                column: "UsuarioCreacionId");

            migrationBuilder.CreateIndex(
                name: "IX_Clientes_UsuarioModificacionId",
                table: "Clientes",
                column: "UsuarioModificacionId");

            migrationBuilder.CreateIndex(
                name: "IX_CodigosImpuesto_Codigo",
                table: "CodigosImpuesto",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CodigosImpuesto_UsuarioCreacionId",
                table: "CodigosImpuesto",
                column: "UsuarioCreacionId");

            migrationBuilder.CreateIndex(
                name: "IX_CodigosImpuesto_UsuarioModificacionId",
                table: "CodigosImpuesto",
                column: "UsuarioModificacionId");

            migrationBuilder.CreateIndex(
                name: "IX_CondicionesVenta_Codigo",
                table: "CondicionesVenta",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CondicionesVenta_UsuarioCreacionId",
                table: "CondicionesVenta",
                column: "UsuarioCreacionId");

            migrationBuilder.CreateIndex(
                name: "IX_CondicionesVenta_UsuarioModificacionId",
                table: "CondicionesVenta",
                column: "UsuarioModificacionId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentosVenta_CajaId",
                table: "DocumentosVenta",
                column: "CajaId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentosVenta_ClienteId",
                table: "DocumentosVenta",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentosVenta_DocumentoOrigenId",
                table: "DocumentosVenta",
                column: "DocumentoOrigenId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentosVenta_Estado_FechaDocumento",
                table: "DocumentosVenta",
                columns: new[] { "Estado", "FechaDocumento" });

            migrationBuilder.CreateIndex(
                name: "IX_DocumentosVenta_TipoDocumento_CajaId_NumeroConsecutivo",
                table: "DocumentosVenta",
                columns: new[] { "TipoDocumento", "CajaId", "NumeroConsecutivo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DocumentosVenta_UsuarioCreacionId",
                table: "DocumentosVenta",
                column: "UsuarioCreacionId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentosVenta_UsuarioModificacionId",
                table: "DocumentosVenta",
                column: "UsuarioModificacionId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentosVenta_VendedorId",
                table: "DocumentosVenta",
                column: "VendedorId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentosVentaEventos_DocumentoVentaId_OcurridoEn",
                table: "DocumentosVentaEventos",
                columns: new[] { "DocumentoVentaId", "OcurridoEn" });

            migrationBuilder.CreateIndex(
                name: "IX_DocumentosVentaEventos_TipoEventoCodigo_OcurridoEn",
                table: "DocumentosVentaEventos",
                columns: new[] { "TipoEventoCodigo", "OcurridoEn" });

            migrationBuilder.CreateIndex(
                name: "IX_DocumentosVentaEventos_TipoEventoId",
                table: "DocumentosVentaEventos",
                column: "TipoEventoId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentosVentaEventos_UsuarioCreacionId",
                table: "DocumentosVentaEventos",
                column: "UsuarioCreacionId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentosVentaEventos_UsuarioModificacionId",
                table: "DocumentosVentaEventos",
                column: "UsuarioModificacionId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentosVentaLineas_DocumentoVentaId",
                table: "DocumentosVentaLineas",
                column: "DocumentoVentaId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentosVentaLineas_ProductoId",
                table: "DocumentosVentaLineas",
                column: "ProductoId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentosVentaPagos_DocumentoVentaId",
                table: "DocumentosVentaPagos",
                column: "DocumentoVentaId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentosVentaPagos_UsuarioRegistroId",
                table: "DocumentosVentaPagos",
                column: "UsuarioRegistroId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentosVentaReferencias_DocumentoReferenciaId",
                table: "DocumentosVentaReferencias",
                column: "DocumentoReferenciaId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentosVentaReferencias_DocumentoVentaId",
                table: "DocumentosVentaReferencias",
                column: "DocumentoVentaId");

            migrationBuilder.CreateIndex(
                name: "IX_MediosPago_Codigo",
                table: "MediosPago",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MediosPago_UsuarioCreacionId",
                table: "MediosPago",
                column: "UsuarioCreacionId");

            migrationBuilder.CreateIndex(
                name: "IX_MediosPago_UsuarioModificacionId",
                table: "MediosPago",
                column: "UsuarioModificacionId");

            migrationBuilder.CreateIndex(
                name: "IX_Negocios_UsuarioCreacionId",
                table: "Negocios",
                column: "UsuarioCreacionId");

            migrationBuilder.CreateIndex(
                name: "IX_Negocios_UsuarioModificacionId",
                table: "Negocios",
                column: "UsuarioModificacionId");

            migrationBuilder.CreateIndex(
                name: "IX_NegocioTicketConfigs_UsuarioCreacionId",
                table: "NegocioTicketConfigs",
                column: "UsuarioCreacionId");

            migrationBuilder.CreateIndex(
                name: "IX_NegocioTicketConfigs_UsuarioModificacionId",
                table: "NegocioTicketConfigs",
                column: "UsuarioModificacionId");

            migrationBuilder.CreateIndex(
                name: "IX_PaginaPermisos_PermisoId",
                table: "PaginaPermisos",
                column: "PermisoId");

            migrationBuilder.CreateIndex(
                name: "IX_Paginas_PaginaPadreId",
                table: "Paginas",
                column: "PaginaPadreId");

            migrationBuilder.CreateIndex(
                name: "IX_Paginas_UsuarioCreacionId",
                table: "Paginas",
                column: "UsuarioCreacionId");

            migrationBuilder.CreateIndex(
                name: "IX_Paginas_UsuarioModificacionId",
                table: "Paginas",
                column: "UsuarioModificacionId");

            migrationBuilder.CreateIndex(
                name: "IX_PerfilesImpresoraTicket_Clave",
                table: "PerfilesImpresoraTicket",
                column: "Clave",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PerfilesImpresoraTicket_UsuarioCreacionId",
                table: "PerfilesImpresoraTicket",
                column: "UsuarioCreacionId");

            migrationBuilder.CreateIndex(
                name: "IX_PerfilesImpresoraTicket_UsuarioModificacionId",
                table: "PerfilesImpresoraTicket",
                column: "UsuarioModificacionId");

            migrationBuilder.CreateIndex(
                name: "IX_Permisos_Clave",
                table: "Permisos",
                column: "Clave",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Permisos_Modulo",
                table: "Permisos",
                column: "Modulo");

            migrationBuilder.CreateIndex(
                name: "IX_Permisos_UsuarioCreacionId",
                table: "Permisos",
                column: "UsuarioCreacionId");

            migrationBuilder.CreateIndex(
                name: "IX_Permisos_UsuarioModificacionId",
                table: "Permisos",
                column: "UsuarioModificacionId");

            migrationBuilder.CreateIndex(
                name: "IX_Productos_CategoriaId",
                table: "Productos",
                column: "CategoriaId");

            migrationBuilder.CreateIndex(
                name: "IX_Productos_Codigo",
                table: "Productos",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Productos_CodigoBarras",
                table: "Productos",
                column: "CodigoBarras",
                unique: true,
                filter: "\"CodigoBarras\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Productos_Nombre",
                table: "Productos",
                column: "Nombre");

            migrationBuilder.CreateIndex(
                name: "IX_Productos_TarifaIvaImpuestoCodigo",
                table: "Productos",
                column: "TarifaIvaImpuestoCodigo");

            migrationBuilder.CreateIndex(
                name: "IX_Productos_UsuarioCreacionId",
                table: "Productos",
                column: "UsuarioCreacionId");

            migrationBuilder.CreateIndex(
                name: "IX_Productos_UsuarioModificacionId",
                table: "Productos",
                column: "UsuarioModificacionId");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokenSessions_TokenHash",
                table: "RefreshTokenSessions",
                column: "TokenHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokenSessions_UsuarioId_ExpiracionUtc",
                table: "RefreshTokenSessions",
                columns: new[] { "UsuarioId", "ExpiracionUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_Roles_Nombre",
                table: "Roles",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Roles_UsuarioCreacionId",
                table: "Roles",
                column: "UsuarioCreacionId");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_UsuarioModificacionId",
                table: "Roles",
                column: "UsuarioModificacionId");

            migrationBuilder.CreateIndex(
                name: "IX_RolPermisos_PermisoId",
                table: "RolPermisos",
                column: "PermisoId");

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

            migrationBuilder.CreateIndex(
                name: "IX_TarifasIvaImpuesto_Codigo",
                table: "TarifasIvaImpuesto",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TarifasIvaImpuesto_UsuarioCreacionId",
                table: "TarifasIvaImpuesto",
                column: "UsuarioCreacionId");

            migrationBuilder.CreateIndex(
                name: "IX_TarifasIvaImpuesto_UsuarioModificacionId",
                table: "TarifasIvaImpuesto",
                column: "UsuarioModificacionId");

            migrationBuilder.CreateIndex(
                name: "IX_TipoDocumentoVentaEvento_UsuarioCreacionId",
                table: "TipoDocumentoVentaEvento",
                column: "UsuarioCreacionId");

            migrationBuilder.CreateIndex(
                name: "IX_TipoDocumentoVentaEvento_UsuarioModificacionId",
                table: "TipoDocumentoVentaEvento",
                column: "UsuarioModificacionId");

            migrationBuilder.CreateIndex(
                name: "IX_TiposIdentificacion_UsuarioCreacionId",
                table: "TiposIdentificacion",
                column: "UsuarioCreacionId");

            migrationBuilder.CreateIndex(
                name: "IX_TiposIdentificacion_UsuarioModificacionId",
                table: "TiposIdentificacion",
                column: "UsuarioModificacionId");

            migrationBuilder.CreateIndex(
                name: "IX_TokensRevocados_FechaExpiracion",
                table: "TokensRevocados",
                column: "FechaExpiracion");

            migrationBuilder.CreateIndex(
                name: "IX_TokensRevocados_Jti",
                table: "TokensRevocados",
                column: "Jti",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_NombreUsuario",
                table: "Usuarios",
                column: "NombreUsuario",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_UsuarioCreacionId",
                table: "Usuarios",
                column: "UsuarioCreacionId");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_UsuarioModificacionId",
                table: "Usuarios",
                column: "UsuarioModificacionId");

            migrationBuilder.CreateIndex(
                name: "IX_Vendedores_NombreNormalizado",
                table: "Vendedores",
                column: "NombreNormalizado",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Vendedores_UsuarioCreacionId",
                table: "Vendedores",
                column: "UsuarioCreacionId");

            migrationBuilder.CreateIndex(
                name: "IX_Vendedores_UsuarioModificacionId",
                table: "Vendedores",
                column: "UsuarioModificacionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CodigosImpuesto");

            migrationBuilder.DropTable(
                name: "CondicionesVenta");

            migrationBuilder.DropTable(
                name: "DataProtectionKeys");

            migrationBuilder.DropTable(
                name: "DocumentosVentaEventos");

            migrationBuilder.DropTable(
                name: "DocumentosVentaLineas");

            migrationBuilder.DropTable(
                name: "DocumentosVentaPagos");

            migrationBuilder.DropTable(
                name: "DocumentosVentaReferencias");

            migrationBuilder.DropTable(
                name: "MediosPago");

            migrationBuilder.DropTable(
                name: "Negocios");

            migrationBuilder.DropTable(
                name: "NegocioTicketConfigs");

            migrationBuilder.DropTable(
                name: "PaginaPermisos");

            migrationBuilder.DropTable(
                name: "PerfilesImpresoraTicket");

            migrationBuilder.DropTable(
                name: "RefreshTokenSessions");

            migrationBuilder.DropTable(
                name: "RolPermisos");

            migrationBuilder.DropTable(
                name: "SecuenciasDocumentoCaja");

            migrationBuilder.DropTable(
                name: "TiposIdentificacion");

            migrationBuilder.DropTable(
                name: "TokensRevocados");

            migrationBuilder.DropTable(
                name: "TipoDocumentoVentaEvento");

            migrationBuilder.DropTable(
                name: "Productos");

            migrationBuilder.DropTable(
                name: "DocumentosVenta");

            migrationBuilder.DropTable(
                name: "Paginas");

            migrationBuilder.DropTable(
                name: "Permisos");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "Categorias");

            migrationBuilder.DropTable(
                name: "TarifasIvaImpuesto");

            migrationBuilder.DropTable(
                name: "Cajas");

            migrationBuilder.DropTable(
                name: "Clientes");

            migrationBuilder.DropTable(
                name: "Vendedores");

            migrationBuilder.DropTable(
                name: "Usuarios");
        }
    }
}

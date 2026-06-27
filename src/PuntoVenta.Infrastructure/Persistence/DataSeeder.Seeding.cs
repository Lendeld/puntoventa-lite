using Microsoft.EntityFrameworkCore;
using PuntoVenta.Domain.Common;
using PuntoVenta.Domain.Entities.Cajas;
using PuntoVenta.Domain.Entities.CodigosImpuesto;
using PuntoVenta.Domain.Entities.CondicionesVenta;
using PuntoVenta.Domain.Entities.MediosPago;
using PuntoVenta.Domain.Entities.Negocios;
using PuntoVenta.Domain.Entities.Paginas;
using PuntoVenta.Domain.Entities.Permisos;
using PuntoVenta.Domain.Entities.Roles;
using PuntoVenta.Domain.Entities.TarifasIvaImpuesto;
using PuntoVenta.Domain.Entities.TiposIdentificacion;
using PuntoVenta.Domain.Entities.Usuarios;
using PuntoVenta.Infrastructure.Security;

namespace PuntoVenta.Infrastructure.Persistence;

public static partial class DataSeeder
{
    #region Permisos por página

    private static async Task PermisosUsuariosAsync(ApplicationDbContext context)
    {
        await AsociarPermisosAPaginaAsync(context, "Usuarios", [
            PermisosRegistrar.Claves.UsuariosVer,
            PermisosRegistrar.Claves.UsuariosCrear,
            PermisosRegistrar.Claves.UsuariosEditar,
            PermisosRegistrar.Claves.UsuariosToggle
        ]);
    }

    private static async Task PermisosRolesAsync(ApplicationDbContext context)
    {
        await AsociarPermisosAPaginaAsync(context, "Roles", [
            PermisosRegistrar.Claves.RolesVer,
            PermisosRegistrar.Claves.RolesCrear,
            PermisosRegistrar.Claves.RolesEditar,
            PermisosRegistrar.Claves.RolesToggle,
            PermisosRegistrar.Claves.RolesPermisosAdministrar
        ]);
    }

    private static async Task PermisosCatalogosAsync(ApplicationDbContext context)
    {
        var pagina = await context.Paginas.FirstOrDefaultAsync(p => p.Nombre == "Catálogos");
        if (pagina == null) return;

        var listaPermisos = await context.PaginaPermisos.Where(p => p.PaginaId == pagina.Id).ToListAsync();

        string[] listaClavesPermisos = [
            PermisosRegistrar.Claves.CatalogosVer,
            PermisosRegistrar.Claves.TiposIdentificacionVer,
            PermisosRegistrar.Claves.TiposIdentificacionToggle,
            PermisosRegistrar.Claves.CondicionesVentaVer,
            PermisosRegistrar.Claves.CondicionesVentaToggle,
            PermisosRegistrar.Claves.MediosPagoVer,
            PermisosRegistrar.Claves.MediosPagoToggle,
            PermisosRegistrar.Claves.CodigosImpuestoVer,
            PermisosRegistrar.Claves.CodigosImpuestoToggle,
            PermisosRegistrar.Claves.TarifasIvaVer,
            PermisosRegistrar.Claves.TarifasIvaToggle,
        ];

        foreach (var clave in listaClavesPermisos)
        {
            var permiso = await context.Permisos.FirstOrDefaultAsync(p => p.Clave == clave);
            if (permiso == null) continue;
            if (!listaPermisos.Any(p => p.PermisoId == permiso.Id))
                context.PaginaPermisos.Add(PaginaPermiso.Crear(pagina.Id, permiso.Id));
        }

        string[] clavesMovidasAMiNegocio = [PermisosRegistrar.Claves.NegocioVer, PermisosRegistrar.Claves.NegocioEditar];
        foreach (var clave in clavesMovidasAMiNegocio)
        {
            var permiso = await context.Permisos.FirstOrDefaultAsync(p => p.Clave == clave);
            if (permiso == null) continue;
            var asociacion = listaPermisos.FirstOrDefault(p => p.PermisoId == permiso.Id);
            if (asociacion != null) context.PaginaPermisos.Remove(asociacion);
        }

        await context.SaveChangesAsync();
    }

    private static async Task PermisosMiNegocioAsync(ApplicationDbContext context)
    {
        await AsociarPermisosAPaginaAsync(context, "Mi Negocio", [
            PermisosRegistrar.Claves.NegocioVer,
            PermisosRegistrar.Claves.NegocioEditar,
        ]);
    }

    private static async Task PermisosMantenimientoAsync(ApplicationDbContext context)
    {
        await AsociarPermisosAPaginaAsync(context, "Categorías", [
            PermisosRegistrar.Claves.CategoriasVer,
            PermisosRegistrar.Claves.CategoriasCrear,
            PermisosRegistrar.Claves.CategoriasEditar,
            PermisosRegistrar.Claves.CategoriasToggle
        ]);

        await AsociarPermisosAPaginaAsync(context, "Vendedores", [
            PermisosRegistrar.Claves.VendedoresVer,
            PermisosRegistrar.Claves.VendedoresCrear,
            PermisosRegistrar.Claves.VendedoresEditar,
            PermisosRegistrar.Claves.VendedoresToggle
        ]);

        await AsociarPermisosAPaginaAsync(context, "Productos", [
            PermisosRegistrar.Claves.ProductosVer,
            PermisosRegistrar.Claves.ProductosCrear,
            PermisosRegistrar.Claves.ProductosEditar,
            PermisosRegistrar.Claves.ProductosToggle,
            PermisosRegistrar.Claves.ProductosNoAplicaExistencias,
        ]);
    }

    private static async Task PermisosClientesAsync(ApplicationDbContext context)
    {
        await AsociarPermisosAPaginaAsync(context, "Clientes", [
            PermisosRegistrar.Claves.ClientesVer,
            PermisosRegistrar.Claves.ClientesCrear,
            PermisosRegistrar.Claves.ClientesEditar,
            PermisosRegistrar.Claves.ClientesToggle,
        ]);
    }

    private static async Task PermisosEmisionAsync(ApplicationDbContext context)
    {
        await AsociarPermisosAPaginaAsync(context, NombresPaginas.Facturacion, [
            PermisosRegistrar.Claves.FacturacionVer,
            PermisosRegistrar.Claves.VentasApartadosCrear,
            PermisosRegistrar.Claves.VentasNotasCreditoCrear,
            PermisosRegistrar.Claves.VentasNotasDebitoCrear
        ]);

        await AsociarPermisosAPaginaAsync(context, NombresPaginas.Ventas, [
            PermisosRegistrar.Claves.FacturacionVer,
            PermisosRegistrar.Claves.VentasApartadosAbonar,
            PermisosRegistrar.Claves.VentasApartadosExtender,
            PermisosRegistrar.Claves.VentasApartadosConvertir,
            PermisosRegistrar.Claves.VentasApartadosCancelar,
            PermisosRegistrar.Claves.NotificacionesCrear,
            PermisosRegistrar.Claves.NotificacionesReintentar
        ]);

        await AsociarPermisosAPaginaAsync(context, NombresPaginas.CuentasPorCobrar, [
            PermisosRegistrar.Claves.VentasCreditoVer,
            PermisosRegistrar.Claves.VentasFacturasAbonar
        ]);
    }

    private static async Task PermisosReportesAsync(ApplicationDbContext context)
    {
        await AsociarPermisosAPaginaAsync(context, NombresPaginas.Reportes, [
            PermisosRegistrar.Claves.ReportesVer
        ]);

        await AsociarPermisosAPaginaAsync(context, NombresPaginas.ReporteVentasRango, [
            PermisosRegistrar.Claves.ReportesVentasRangoVer
        ]);

        await AsociarPermisosAPaginaAsync(context, NombresPaginas.ReportesInventario, [
            PermisosRegistrar.Claves.ReportesVer
        ]);

        await AsociarPermisosAPaginaAsync(context, NombresPaginas.ReporteExistencias, [
            PermisosRegistrar.Claves.ReportesInventarioVer
        ]);
    }

    private static async Task PermisosRespaldoAsync(ApplicationDbContext context)
    {
        await AsociarPermisosAPaginaAsync(context, "Respaldo", [
            PermisosRegistrar.Claves.BackupAdministrar
        ]);
    }

    private static async Task AsociarPermisosAPaginaAsync(ApplicationDbContext context, string nombrePagina, string[] clavesPermisos)
    {
        var pagina = await context.Paginas.FirstOrDefaultAsync(p => p.Nombre == nombrePagina);
        if (pagina is null) return;

        var listaPermisos = await context.PaginaPermisos.Where(p => p.PaginaId == pagina.Id).ToListAsync();

        foreach (var clave in clavesPermisos)
        {
            var permiso = await context.Permisos.FirstOrDefaultAsync(p => p.Clave == clave);
            if (permiso is null) continue;
            if (!listaPermisos.Any(p => p.PermisoId == permiso.Id))
                context.PaginaPermisos.Add(PaginaPermiso.Crear(pagina.Id, permiso.Id));
        }

        await context.SaveChangesAsync();
    }

    #endregion

    #region Sembrar datos iniciales

    public static async Task SembrarRolesAsync(ApplicationDbContext context)
    {
        if (await context.Roles.AnyAsync()) return;

        var rolAdmin = Rol.Crear("Administrador", "Rol con todos los permisos").Value;
        rolAdmin.MarcarComoPrincipal();
        await context.Roles.AddAsync(rolAdmin);
        await context.SaveChangesAsync();
    }

    public static async Task SembrarUsuariosAsync(ApplicationDbContext context, Microsoft.Extensions.Configuration.IConfiguration configuration)
    {
        if (await context.Usuarios.AnyAsync()) return;

        var nombreUsuario = configuration["Seed:Admin:Username"];
        var password = configuration["Seed:Admin:Password"];

        if (string.IsNullOrWhiteSpace(nombreUsuario) || string.IsNullOrWhiteSpace(password))
            throw new InvalidOperationException("Seed:Admin:Username y Seed:Admin:Password deben estar configurados.");

        var rolAdmin = await context.Roles.FirstAsync(r => r.Nombre == "Administrador");
        var usuarioAdmin = Usuario.Crear(
            nombreUsuario: nombreUsuario,
            nombre: "Administrador del Sistema",
            identificacion: "0000000000",
            passwordHash: new BCryptPasswordHasher().Hash(password),
            rolId: rolAdmin.Id).Value;

        usuarioAdmin.MarcarComoPropietario();

        // Desktop siembra credenciales default conocidas — forzar cambio en
        // el primer login. Opt-in para no bloquear entornos de dev/tests.
        if (bool.TryParse(configuration["Seed:Admin:RequiereCambioPassword"], out var requiereCambio) && requiereCambio)
        {
            usuarioAdmin.RequerirCambioPassword();
        }

        await context.Usuarios.AddAsync(usuarioAdmin);
        await context.SaveChangesAsync();
    }

    public static async Task SembrarNegocioAsync(ApplicationDbContext context)
    {
        if (!await context.Negocios.AnyAsync())
        {
            var negocio = Negocio.Crear(
                nombre: "Mi Negocio",
                direccion: "San Jose, Costa Rica",
                correo: "tienda@demo.com").Value;

            await context.Negocios.AddAsync(negocio);
            await context.SaveChangesAsync();
        }

        if (!await context.Cajas.AnyAsync())
        {
            var caja = Caja.Crear("CAJA01", "Caja Principal").Value;
            await context.Cajas.AddAsync(caja);
            await context.SaveChangesAsync();
        }
    }

    public static async Task SembrarTiposIdentificacionAsync(ApplicationDbContext context)
    {
        var codigosExistentes = await context.TiposIdentificacion.Select(t => t.Codigo).ToHashSetAsync();

        foreach (var (codigo, detalle, comentario) in CatalogosPredefinidos.TiposIdentificacion)
        {
            if (codigosExistentes.Contains(codigo)) continue;
            var item = TipoIdentificacion.Crear(codigo, detalle, comentario).Value;
            await context.TiposIdentificacion.AddAsync(item);
        }

        await context.SaveChangesAsync();
    }

    public static async Task SembrarCondicionesVentaAsync(ApplicationDbContext context)
    {
        var codigosExistentes = await context.CondicionesVenta.Select(t => t.Codigo).ToHashSetAsync();

        foreach (var (codigo, detalle, comentario) in CatalogosPredefinidos.CondicionesVenta)
        {
            if (codigosExistentes.Contains(codigo)) continue;
            var item = CondicionVenta.Crear(codigo, detalle, comentario).Value;
            await context.CondicionesVenta.AddAsync(item);
        }

        await context.SaveChangesAsync();
    }

    public static async Task SembrarMediosPagoAsync(ApplicationDbContext context)
    {
        var codigosExistentes = await context.MediosPago.Select(t => t.Codigo).ToHashSetAsync();

        foreach (var (codigo, detalle, comentario) in CatalogosPredefinidos.MediosPago)
        {
            if (codigosExistentes.Contains(codigo)) continue;
            var item = MedioPago.Crear(codigo, detalle, comentario).Value;
            await context.MediosPago.AddAsync(item);
        }

        await context.SaveChangesAsync();
    }

    public static async Task SembrarCodigosImpuestoAsync(ApplicationDbContext context)
    {
        var codigosExistentes = await context.CodigosImpuesto.Select(t => t.Codigo).ToHashSetAsync();

        foreach (var (codigo, detalle, comentario) in CatalogosPredefinidos.CodigosImpuesto)
        {
            if (codigosExistentes.Contains(codigo)) continue;
            var item = CodigoImpuesto.Crear(codigo, detalle, comentario).Value;
            await context.CodigosImpuesto.AddAsync(item);
        }

        await context.SaveChangesAsync();
    }

    public static async Task SembrarTarifasIvaImpuestoAsync(ApplicationDbContext context)
    {
        var codigosExistentes = await context.TarifasIvaImpuesto.Select(t => t.Codigo).ToHashSetAsync();

        foreach (var (codigo, detalle, porcentaje, comentario) in CatalogosPredefinidos.TarifasIvaImpuesto)
        {
            if (codigosExistentes.Contains(codigo)) continue;
            var item = TarifaIvaImpuesto.Crear(codigo, detalle, porcentaje, comentario).Value;
            await context.TarifasIvaImpuesto.AddAsync(item);
        }

        await context.SaveChangesAsync();
    }

    #endregion
}

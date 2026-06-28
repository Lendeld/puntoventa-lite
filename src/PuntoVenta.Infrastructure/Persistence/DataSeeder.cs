using Microsoft.EntityFrameworkCore;
using PuntoVenta.Domain.Entities.Cajas;
using PuntoVenta.Domain.Entities.CodigosImpuesto;
using PuntoVenta.Domain.Entities.CondicionesVenta;
using PuntoVenta.Domain.Entities.MediosPago;
using PuntoVenta.Domain.Entities.Impresion;
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
    private static class NombresPaginas
    {
        public const string Emision = "Emisión";
        public const string Facturacion = "Facturación";
        public const string Ventas = "Ventas";
        public const string CuentasPorCobrar = "Cuentas por Cobrar";
        public const string Reportes = "Reportes Ventas";
        public const string ReporteVentasRango = "Reporte de ventas por rango";
        public const string ReportesInventario = "Reportes Inventario";
        public const string ReporteExistencias = "Existencias";
    }

    public static async Task SembrarPermisosAsync(ApplicationDbContext context)
    {
        await NormalizarPermisoFacturacionAsync(context);
        await NormalizarPermisoCatalogosAsync(context);

        var definiciones = new[]
        {
            (PermisosRegistrar.Claves.RolesVer,           "Ver listado de roles",            "roles"),
            (PermisosRegistrar.Claves.RolesCrear,         "Crear roles",                     "roles"),
            (PermisosRegistrar.Claves.RolesEditar,        "Editar roles",                    "roles"),
            (PermisosRegistrar.Claves.RolesToggle,        "Activar/desactivar roles",         "roles"),
            (PermisosRegistrar.Claves.RolesPermisosAdministrar, "Administrar permisos de roles", "roles"),
            (PermisosRegistrar.Claves.UsuariosVer,        "Ver listado de usuarios",          "usuarios"),
            (PermisosRegistrar.Claves.UsuariosCrear,      "Crear usuarios",                   "usuarios"),
            (PermisosRegistrar.Claves.UsuariosEditar,     "Editar acceso de usuarios",        "usuarios"),
            (PermisosRegistrar.Claves.UsuariosToggle,     "Activar/desactivar usuarios",      "usuarios"),
            (PermisosRegistrar.Claves.NegocioVer,         "Ver datos del negocio",            "negocio"),
            (PermisosRegistrar.Claves.NegocioEditar,      "Modificar datos del negocio",      "negocio"),
            (PermisosRegistrar.Claves.TiposIdentificacionVer,    "Ver tipos de identificación",         "tipos-identificacion"),
            (PermisosRegistrar.Claves.TiposIdentificacionToggle, "Activar/desactivar tipos de identificación", "tipos-identificacion"),
            (PermisosRegistrar.Claves.CondicionesVentaVer,    "Ver condiciones de venta",              "condiciones-venta"),
            (PermisosRegistrar.Claves.CondicionesVentaToggle, "Activar/desactivar condiciones de venta", "condiciones-venta"),
            (PermisosRegistrar.Claves.MediosPagoVer,          "Ver medios de pago",                    "medios-pago"),
            (PermisosRegistrar.Claves.MediosPagoToggle,       "Activar/desactivar medios de pago",     "medios-pago"),
            (PermisosRegistrar.Claves.CodigosImpuestoVer,     "Ver códigos de impuesto",               "codigos-impuesto"),
            (PermisosRegistrar.Claves.CodigosImpuestoToggle,  "Activar/desactivar códigos de impuesto","codigos-impuesto"),
            (PermisosRegistrar.Claves.TarifasIvaVer,          "Ver tarifas IVA",                       "tarifas-iva"),
            (PermisosRegistrar.Claves.TarifasIvaToggle,       "Activar/desactivar tarifas IVA",        "tarifas-iva"),
            (PermisosRegistrar.Claves.CatalogosVer, "Ver catálogos del sistema", "catalogos"),
            (PermisosRegistrar.Claves.MantenimientoVer, "Ver mantenimiento del sistema", "mantenimiento"),
            (PermisosRegistrar.Claves.CategoriasVer, "Ver listado de categorías", "categorias"),
            (PermisosRegistrar.Claves.CategoriasCrear, "Crear categorías", "categorias"),
            (PermisosRegistrar.Claves.CategoriasEditar, "Editar categorías", "categorias"),
            (PermisosRegistrar.Claves.CategoriasToggle, "Activar/desactivar categorías", "categorias"),
            (PermisosRegistrar.Claves.VendedoresVer, "Ver listado de vendedores", "vendedores"),
            (PermisosRegistrar.Claves.VendedoresCrear, "Crear vendedores", "vendedores"),
            (PermisosRegistrar.Claves.VendedoresEditar, "Editar vendedores", "vendedores"),
            (PermisosRegistrar.Claves.VendedoresToggle, "Activar/desactivar vendedores", "vendedores"),
            (PermisosRegistrar.Claves.ProveedoresVer, "Ver listado de proveedores", "proveedores"),
            (PermisosRegistrar.Claves.ProveedoresCrear, "Crear proveedores", "proveedores"),
            (PermisosRegistrar.Claves.ProveedoresEditar, "Editar proveedores", "proveedores"),
            (PermisosRegistrar.Claves.ProveedoresToggle, "Activar/desactivar proveedores", "proveedores"),
            (PermisosRegistrar.Claves.ProductosVer, "Ver listado de productos", "productos"),
            (PermisosRegistrar.Claves.ProductosCrear, "Crear productos", "productos"),
            (PermisosRegistrar.Claves.ProductosEditar, "Editar productos", "productos"),
            (PermisosRegistrar.Claves.ProductosToggle, "Activar/desactivar productos", "productos"),
            (PermisosRegistrar.Claves.ProductosNoAplicaExistencias, "Ver y modificar No aplica existencias en productos", "productos"),
            (PermisosRegistrar.Claves.ClientesVer, "Ver listado de clientes", "clientes"),
            (PermisosRegistrar.Claves.ClientesCrear, "Crear clientes", "clientes"),
            (PermisosRegistrar.Claves.ClientesEditar, "Editar clientes", "clientes"),
            (PermisosRegistrar.Claves.ClientesToggle, "Activar/desactivar clientes", "clientes"),
            (PermisosRegistrar.Claves.FacturacionVer, "Usar módulo de facturación", "facturacion"),
            (PermisosRegistrar.Claves.VentasApartadosCrear, "Crear apartados", "ventas"),
            (PermisosRegistrar.Claves.VentasApartadosAbonar, "Registrar abonos de apartados", "ventas"),
            (PermisosRegistrar.Claves.VentasApartadosExtender, "Extender vencimiento de apartados", "ventas"),
            (PermisosRegistrar.Claves.VentasApartadosConvertir, "Convertir apartados a factura", "ventas"),
            (PermisosRegistrar.Claves.VentasApartadosCancelar, "Cancelar apartados", "ventas"),
            (PermisosRegistrar.Claves.VentasNotasCreditoCrear, "Crear notas de crédito", "ventas"),
            (PermisosRegistrar.Claves.VentasNotasDebitoCrear, "Crear notas de débito", "ventas"),
            (PermisosRegistrar.Claves.VentasFacturasAbonar, "Registrar abonos de facturas a crédito", "ventas"),
            (PermisosRegistrar.Claves.VentasFacturasAbonoAnular, "Anular abonos de facturas a crédito", "ventas"),
            (PermisosRegistrar.Claves.VentasCreditoVer, "Ver cuentas por cobrar (facturas a crédito)", "ventas"),
            (PermisosRegistrar.Claves.NotificacionesCrear, "Enviar notificación de correo", "notificaciones"),
            (PermisosRegistrar.Claves.NotificacionesReintentar, "Reenviar notificación de correo", "notificaciones"),
            (PermisosRegistrar.Claves.CajasVer, "Ver cajas registradas", "cajas"),
            (PermisosRegistrar.Claves.CajasCrear, "Crear cajas", "cajas"),
            (PermisosRegistrar.Claves.CajasEditar, "Editar cajas", "cajas"),
            (PermisosRegistrar.Claves.CajasToggle, "Activar/desactivar cajas", "cajas"),
            (PermisosRegistrar.Claves.ProductosAjustarStock, "Ajustar stock de productos", "productos"),
            (PermisosRegistrar.Claves.ProductosMovimientosVer, "Ver movimientos de stock", "productos"),
            (PermisosRegistrar.Claves.ReportesVer, "Ver módulo de reportes", "reportes"),
            (PermisosRegistrar.Claves.ReportesVentasRangoVer, "Ver reporte de ventas por rango", "reportes"),
            (PermisosRegistrar.Claves.ReportesInventarioVer, "Ver reporte de inventario", "reportes"),
            (PermisosRegistrar.Claves.DashboardVer, "Ver panel principal", "dashboard"),
            (PermisosRegistrar.Claves.BackupAdministrar, "Administrar respaldo de la base de datos", "backup"),
        };

        var clavesExistentes = await context.Permisos
            .Select(p => p.Clave)
            .ToHashSetAsync();

        foreach (var (clave, descripcion, modulo) in definiciones)
        {
            if (clavesExistentes.Contains(clave)) continue;
            var permiso = Permiso.Crear(clave, descripcion, modulo).Value;
            await context.Permisos.AddAsync(permiso);
        }

        await context.SaveChangesAsync();
        await HeredarPermisosVentasDesdeFacturacionAsync(context);
    }

    private static async Task HeredarPermisosVentasDesdeFacturacionAsync(ApplicationDbContext context)
    {
        var permisoFacturacionId = await context.Permisos
            .Where(p => p.Clave == PermisosRegistrar.Claves.FacturacionVer)
            .Select(p => p.Id)
            .FirstOrDefaultAsync();

        if (permisoFacturacionId == Guid.Empty) return;

        var permisosVentasIds = await context.Permisos
            .Where(p => p.Clave == PermisosRegistrar.Claves.VentasApartadosCrear
                || p.Clave == PermisosRegistrar.Claves.VentasApartadosAbonar
                || p.Clave == PermisosRegistrar.Claves.VentasApartadosExtender
                || p.Clave == PermisosRegistrar.Claves.VentasApartadosConvertir
                || p.Clave == PermisosRegistrar.Claves.VentasApartadosCancelar
                || p.Clave == PermisosRegistrar.Claves.VentasNotasCreditoCrear
                || p.Clave == PermisosRegistrar.Claves.VentasNotasDebitoCrear
                || p.Clave == PermisosRegistrar.Claves.VentasFacturasAbonar
                || p.Clave == PermisosRegistrar.Claves.VentasFacturasAbonoAnular
                || p.Clave == PermisosRegistrar.Claves.VentasCreditoVer)
            .Select(p => p.Id)
            .ToListAsync();

        if (permisosVentasIds.Count == 0) return;

        var rolIdsConFacturacion = await context.RolPermisos
            .Where(rp => rp.PermisoId == permisoFacturacionId)
            .Select(rp => rp.RolId)
            .Distinct()
            .ToListAsync();

        if (rolIdsConFacturacion.Count == 0) return;

        var permisosExistentes = await context.RolPermisos
            .Where(rp => rolIdsConFacturacion.Contains(rp.RolId) && permisosVentasIds.Contains(rp.PermisoId))
            .Select(rp => new { rp.RolId, rp.PermisoId })
            .ToListAsync();

        foreach (var rolId in rolIdsConFacturacion)
        {
            foreach (var permisoId in permisosVentasIds)
            {
                if (!permisosExistentes.Any(rp => rp.RolId == rolId && rp.PermisoId == permisoId))
                    context.RolPermisos.Add(RolPermiso.Crear(rolId, permisoId));
            }
        }

        await context.SaveChangesAsync();
    }

    private static async Task NormalizarPermisoFacturacionAsync(ApplicationDbContext context)
    {
        const string permisoVentasVerLegacy = "ventas:ver";
        const string permisoVentasFacturarLegacy = "ventas:facturar";

        var permisoFacturacion = await context.Permisos.FirstOrDefaultAsync(p => p.Clave == PermisosRegistrar.Claves.FacturacionVer);
        var permisoVentasVer = await context.Permisos.FirstOrDefaultAsync(p => p.Clave == permisoVentasVerLegacy);
        var permisoVentasFacturar = await context.Permisos.FirstOrDefaultAsync(p => p.Clave == permisoVentasFacturarLegacy);

        var permisoDestino = permisoFacturacion ?? permisoVentasVer ?? permisoVentasFacturar;
        if (permisoDestino is null) return;

        if (permisoFacturacion is null)
            context.Entry(permisoDestino).Property(nameof(Permiso.Clave)).CurrentValue = PermisosRegistrar.Claves.FacturacionVer;

        context.Entry(permisoDestino).Property(nameof(Permiso.Descripcion)).CurrentValue = "Usar módulo de facturación";
        context.Entry(permisoDestino).Property(nameof(Permiso.Modulo)).CurrentValue = "facturacion";
        await context.SaveChangesAsync();

        permisoFacturacion = permisoDestino;

        foreach (var permisoLegacy in new[] { permisoVentasVer, permisoVentasFacturar })
        {
            if (permisoLegacy is null || permisoLegacy.Id == permisoFacturacion.Id) continue;

            var rolPermisosLegacy = await context.RolPermisos.Where(rp => rp.PermisoId == permisoLegacy.Id).ToListAsync();
            var rolIdsDestino = await context.RolPermisos.Where(rp => rp.PermisoId == permisoFacturacion.Id).Select(rp => rp.RolId).ToHashSetAsync();

            foreach (var rolPermiso in rolPermisosLegacy)
            {
                if (!rolIdsDestino.Contains(rolPermiso.RolId))
                {
                    context.RolPermisos.Add(RolPermiso.Crear(rolPermiso.RolId, permisoFacturacion.Id));
                    rolIdsDestino.Add(rolPermiso.RolId);
                }
                context.RolPermisos.Remove(rolPermiso);
            }

            var paginaPermisosLegacy = await context.PaginaPermisos.Where(pp => pp.PermisoId == permisoLegacy.Id).ToListAsync();
            var paginaIdsDestino = await context.PaginaPermisos.Where(pp => pp.PermisoId == permisoFacturacion.Id).Select(pp => pp.PaginaId).ToHashSetAsync();

            foreach (var paginaPermiso in paginaPermisosLegacy)
            {
                if (!paginaIdsDestino.Contains(paginaPermiso.PaginaId))
                {
                    context.PaginaPermisos.Add(PaginaPermiso.Crear(paginaPermiso.PaginaId, permisoFacturacion.Id));
                    paginaIdsDestino.Add(paginaPermiso.PaginaId);
                }
                context.PaginaPermisos.Remove(paginaPermiso);
            }

            context.Permisos.Remove(permisoLegacy);
            await context.SaveChangesAsync();
        }
    }

    private static async Task NormalizarPermisoCatalogosAsync(ApplicationDbContext context)
    {
        const string permisoLegacyClave = "configuracion:ver";

        var permisoNuevo = await context.Permisos.FirstOrDefaultAsync(p => p.Clave == PermisosRegistrar.Claves.CatalogosVer);
        var permisoLegacy = await context.Permisos.FirstOrDefaultAsync(p => p.Clave == permisoLegacyClave);

        var permisoDestino = permisoNuevo ?? permisoLegacy;
        if (permisoDestino is null) return;

        if (permisoNuevo is null)
            context.Entry(permisoDestino).Property(nameof(Permiso.Clave)).CurrentValue = PermisosRegistrar.Claves.CatalogosVer;

        context.Entry(permisoDestino).Property(nameof(Permiso.Descripcion)).CurrentValue = "Ver catálogos del sistema";
        context.Entry(permisoDestino).Property(nameof(Permiso.Modulo)).CurrentValue = "catalogos";
        await context.SaveChangesAsync();

        if (permisoLegacy is null || permisoLegacy.Id == permisoDestino.Id) return;

        var rolPermisosLegacy = await context.RolPermisos.Where(rp => rp.PermisoId == permisoLegacy.Id).ToListAsync();
        var rolIdsDestino = await context.RolPermisos.Where(rp => rp.PermisoId == permisoDestino.Id).Select(rp => rp.RolId).ToHashSetAsync();

        foreach (var rolPermiso in rolPermisosLegacy)
        {
            if (!rolIdsDestino.Contains(rolPermiso.RolId))
            {
                context.RolPermisos.Add(RolPermiso.Crear(rolPermiso.RolId, permisoDestino.Id));
                rolIdsDestino.Add(rolPermiso.RolId);
            }
            context.RolPermisos.Remove(rolPermiso);
        }

        var paginaPermisosLegacy = await context.PaginaPermisos.Where(pp => pp.PermisoId == permisoLegacy.Id).ToListAsync();
        var paginaIdsDestino = await context.PaginaPermisos.Where(pp => pp.PermisoId == permisoDestino.Id).Select(pp => pp.PaginaId).ToHashSetAsync();

        foreach (var paginaPermiso in paginaPermisosLegacy)
        {
            if (!paginaIdsDestino.Contains(paginaPermiso.PaginaId))
            {
                context.PaginaPermisos.Add(PaginaPermiso.Crear(paginaPermiso.PaginaId, permisoDestino.Id));
                paginaIdsDestino.Add(paginaPermiso.PaginaId);
            }
            context.PaginaPermisos.Remove(paginaPermiso);
        }

        context.Permisos.Remove(permisoLegacy);
        await context.SaveChangesAsync();
    }

    public static async Task SembrarPaginasAsync(ApplicationDbContext context)
    {
        var paginaAdmin = await context.Paginas.FirstOrDefaultAsync(p => p.Nombre == "Sistema");
        if (paginaAdmin == null)
        {
            paginaAdmin = Pagina.Crear("Sistema", "/sistema", orden: 1).Value;
            await context.Paginas.AddAsync(paginaAdmin);
            await context.SaveChangesAsync();
        }

        await CargaPaginaRolAsync(context, paginaAdmin.Id);
        await CargaPaginaUsuarioAsync(context, paginaAdmin.Id);
        await CargarPaginaCatalogosAsync(context, paginaAdmin.Id);
        await CargarPaginaMiNegocioAsync(context, paginaAdmin.Id);
        await CargarPaginaRespaldoAsync(context, paginaAdmin.Id);

        var paginaMantenimiento = await context.Paginas.FirstOrDefaultAsync(p => p.Nombre == "Mantenimiento");
        if (paginaMantenimiento == null)
        {
            paginaMantenimiento = Pagina.Crear("Mantenimiento", "/mantenimiento", orden: 2).Value;
            await context.Paginas.AddAsync(paginaMantenimiento);
            await context.SaveChangesAsync();
        }

        await CargarPaginaCategoriasAsync(context, paginaMantenimiento.Id);
        await CargarPaginaVendedoresAsync(context, paginaMantenimiento.Id);
        await CargarPaginaProveedoresAsync(context, paginaMantenimiento.Id);

        var paginaProductos = await context.Paginas.FirstOrDefaultAsync(p => p.Nombre == "Productos");
        if (paginaProductos == null)
        {
            paginaProductos = Pagina.Crear("Productos", "/productos", orden: 3, icono: "package").Value;
            await context.Paginas.AddAsync(paginaProductos);
            await context.SaveChangesAsync();
        }

        var paginaClientes = await context.Paginas.FirstOrDefaultAsync(p => p.Nombre == "Clientes");
        if (paginaClientes == null)
        {
            paginaClientes = Pagina.Crear("Clientes", "/clientes", orden: 4, icono: "users").Value;
            await context.Paginas.AddAsync(paginaClientes);
            await context.SaveChangesAsync();
        }

        var paginaEmision = await context.Paginas.FirstOrDefaultAsync(p => p.Nombre == NombresPaginas.Emision);
        if (paginaEmision == null)
        {
            paginaEmision = Pagina.Crear(NombresPaginas.Emision, "/emision", orden: 5, icono: "receipt-2").Value;
            await context.Paginas.AddAsync(paginaEmision);
            await context.SaveChangesAsync();
        }

        await CargarPaginaFacturacionAsync(context, paginaEmision.Id);
        await CargarPaginaVentasAsync(context, paginaEmision.Id);
        await CargarPaginaCuentasPorCobrarAsync(context, paginaEmision.Id);

        var paginaReportes = await context.Paginas.FirstOrDefaultAsync(p => p.Nombre == NombresPaginas.Reportes || p.Nombre == "Reportes");
        if (paginaReportes == null)
        {
            paginaReportes = Pagina.Crear(NombresPaginas.Reportes, "/reportes-ventas", orden: 6, icono: "report").Value;
            await context.Paginas.AddAsync(paginaReportes);
            await context.SaveChangesAsync();
        }
        else
        {
            context.Entry(paginaReportes).Property(nameof(Pagina.Nombre)).CurrentValue = NombresPaginas.Reportes;
            context.Entry(paginaReportes).Property(nameof(Pagina.Ruta)).CurrentValue = "/reportes-ventas";
            await context.SaveChangesAsync();
        }

        await CargarPaginaReporteVentasRangoAsync(context, paginaReportes.Id);

        var paginaReportesInventario = await context.Paginas.FirstOrDefaultAsync(p => p.Nombre == NombresPaginas.ReportesInventario);
        if (paginaReportesInventario == null)
        {
            paginaReportesInventario = Pagina.Crear(NombresPaginas.ReportesInventario, "/reportes-inventario", orden: 7, icono: "report-search").Value;
            await context.Paginas.AddAsync(paginaReportesInventario);
            await context.SaveChangesAsync();
        }

        await CargarPaginaReporteExistenciasAsync(context, paginaReportesInventario.Id);
    }

    public static async Task SembrarPermisosPaginaAsync(ApplicationDbContext context)
    {
        await PermisosUsuariosAsync(context);
        await PermisosRolesAsync(context);
        await PermisosCatalogosAsync(context);
        await PermisosMiNegocioAsync(context);
        await PermisosMantenimientoAsync(context);
        await PermisosClientesAsync(context);
        await PermisosEmisionAsync(context);
        await PermisosReportesAsync(context);
        await PermisosRespaldoAsync(context);
    }

    #region Sembrar páginas

    private static async Task CargaPaginaRolAsync(ApplicationDbContext context, Guid paginaAdminId)
    {
        var pagina = await context.Paginas.FirstOrDefaultAsync(p => p.Nombre == "Roles" && p.PaginaPadreId == paginaAdminId);
        if (pagina == null)
        {
            pagina = Pagina.Crear("Roles", "/sistema/roles", orden: 1, icono: "shield", paginaPadreId: paginaAdminId).Value;
            await context.Paginas.AddAsync(pagina);
        }
        else
        {
            context.Entry(pagina).Property(nameof(Pagina.Ruta)).CurrentValue = "/sistema/roles";
            context.Entry(pagina).Property(nameof(Pagina.Orden)).CurrentValue = 1;
            context.Entry(pagina).Property(nameof(Pagina.Icono)).CurrentValue = "shield";
        }
        await context.SaveChangesAsync();
    }

    private static async Task CargaPaginaUsuarioAsync(ApplicationDbContext context, Guid paginaAdminId)
    {
        var pagina = await context.Paginas.FirstOrDefaultAsync(p => p.Nombre == "Usuarios" && p.PaginaPadreId == paginaAdminId);
        if (pagina == null)
        {
            pagina = Pagina.Crear("Usuarios", "/sistema/usuarios", orden: 2, icono: "users", paginaPadreId: paginaAdminId).Value;
            await context.Paginas.AddAsync(pagina);
        }
        else
        {
            context.Entry(pagina).Property(nameof(Pagina.Ruta)).CurrentValue = "/sistema/usuarios";
            context.Entry(pagina).Property(nameof(Pagina.Orden)).CurrentValue = 2;
            context.Entry(pagina).Property(nameof(Pagina.Icono)).CurrentValue = "users";
        }
        await context.SaveChangesAsync();
    }

    private static async Task CargarPaginaCatalogosAsync(ApplicationDbContext context, Guid paginaAdminId)
    {
        var pagina = await context.Paginas.FirstOrDefaultAsync(p => p.PaginaPadreId == paginaAdminId && (p.Nombre == "Catálogos" || p.Nombre == "Configuración"));
        if (pagina == null)
        {
            pagina = Pagina.Crear("Catálogos", "/sistema/catalogos", orden: 3, icono: "settings", paginaPadreId: paginaAdminId).Value;
            await context.Paginas.AddAsync(pagina);
        }
        else
        {
            context.Entry(pagina).Property(nameof(Pagina.Nombre)).CurrentValue = "Catálogos";
            context.Entry(pagina).Property(nameof(Pagina.Ruta)).CurrentValue = "/sistema/catalogos";
            context.Entry(pagina).Property(nameof(Pagina.Orden)).CurrentValue = 3;
            context.Entry(pagina).Property(nameof(Pagina.Icono)).CurrentValue = "settings";
        }
        await context.SaveChangesAsync();
    }

    private static async Task CargarPaginaMiNegocioAsync(ApplicationDbContext context, Guid paginaAdminId)
    {
        var pagina = await context.Paginas.FirstOrDefaultAsync(p => p.Nombre == "Mi Negocio" && p.PaginaPadreId == paginaAdminId);
        if (pagina == null)
        {
            pagina = Pagina.Crear("Mi Negocio", "/sistema/mi-negocio", orden: 4, icono: "building-store", paginaPadreId: paginaAdminId).Value;
            await context.Paginas.AddAsync(pagina);
        }
        else
        {
            context.Entry(pagina).Property(nameof(Pagina.Ruta)).CurrentValue = "/sistema/mi-negocio";
            context.Entry(pagina).Property(nameof(Pagina.Orden)).CurrentValue = 4;
            context.Entry(pagina).Property(nameof(Pagina.Icono)).CurrentValue = "building-store";
        }
        await context.SaveChangesAsync();
    }

    private static async Task CargarPaginaRespaldoAsync(ApplicationDbContext context, Guid paginaAdminId)
    {
        if (await context.Paginas.AnyAsync(p => p.Nombre == "Respaldo" && p.PaginaPadreId == paginaAdminId)) return;
        var pagina = Pagina.Crear("Respaldo", "/sistema/respaldo", orden: 5, icono: "database-export", paginaPadreId: paginaAdminId).Value;
        await context.Paginas.AddAsync(pagina);
        await context.SaveChangesAsync();
    }

    private static async Task CargarPaginaCategoriasAsync(ApplicationDbContext context, Guid paginaMantenimientoId)
    {
        if (await context.Paginas.AnyAsync(p => p.Nombre == "Categorías" && p.PaginaPadreId == paginaMantenimientoId)) return;
        var pagina = Pagina.Crear("Categorías", "/mantenimiento/categorias", orden: 1, icono: "category", paginaPadreId: paginaMantenimientoId).Value;
        await context.Paginas.AddAsync(pagina);
        await context.SaveChangesAsync();
    }

    private static async Task CargarPaginaVendedoresAsync(ApplicationDbContext context, Guid paginaMantenimientoId)
    {
        if (await context.Paginas.AnyAsync(p => p.Nombre == "Vendedores" && p.PaginaPadreId == paginaMantenimientoId)) return;
        var pagina = Pagina.Crear("Vendedores", "/mantenimiento/vendedores", orden: 2, icono: "user-check", paginaPadreId: paginaMantenimientoId).Value;
        await context.Paginas.AddAsync(pagina);
        await context.SaveChangesAsync();
    }

    private static async Task CargarPaginaProveedoresAsync(ApplicationDbContext context, Guid paginaMantenimientoId)
    {
        if (await context.Paginas.AnyAsync(p => p.Nombre == "Proveedores" && p.PaginaPadreId == paginaMantenimientoId)) return;
        var pagina = Pagina.Crear("Proveedores", "/mantenimiento/proveedores", orden: 3, icono: "truck", paginaPadreId: paginaMantenimientoId).Value;
        await context.Paginas.AddAsync(pagina);
        await context.SaveChangesAsync();
    }

    private static async Task CargarPaginaFacturacionAsync(ApplicationDbContext context, Guid paginaEmisionId)
    {
        if (await context.Paginas.AnyAsync(p => p.Nombre == NombresPaginas.Facturacion && p.PaginaPadreId == paginaEmisionId)) return;
        var pagina = Pagina.Crear(NombresPaginas.Facturacion, "/emision/facturacion", orden: 1, icono: "receipt-2", paginaPadreId: paginaEmisionId).Value;
        await context.Paginas.AddAsync(pagina);
        await context.SaveChangesAsync();
    }

    private static async Task CargarPaginaVentasAsync(ApplicationDbContext context, Guid paginaEmisionId)
    {
        if (await context.Paginas.AnyAsync(p => p.Nombre == NombresPaginas.Ventas && p.PaginaPadreId == paginaEmisionId)) return;
        var pagina = Pagina.Crear(NombresPaginas.Ventas, "/emision/ventas", orden: 2, icono: "list-details", paginaPadreId: paginaEmisionId).Value;
        await context.Paginas.AddAsync(pagina);
        await context.SaveChangesAsync();
    }

    private static async Task CargarPaginaCuentasPorCobrarAsync(ApplicationDbContext context, Guid paginaEmisionId)
    {
        if (await context.Paginas.AnyAsync(p => p.Nombre == NombresPaginas.CuentasPorCobrar && p.PaginaPadreId == paginaEmisionId)) return;
        var pagina = Pagina.Crear(NombresPaginas.CuentasPorCobrar, "/emision/cobros/credito", orden: 3, icono: "cash-banknote", paginaPadreId: paginaEmisionId).Value;
        await context.Paginas.AddAsync(pagina);
        await context.SaveChangesAsync();
    }

    private static async Task CargarPaginaReporteVentasRangoAsync(ApplicationDbContext context, Guid paginaReportesId)
    {
        var pagina = await context.Paginas.FirstOrDefaultAsync(p => p.Nombre == NombresPaginas.ReporteVentasRango && p.PaginaPadreId == paginaReportesId);
        if (pagina == null)
        {
            pagina = Pagina.Crear(NombresPaginas.ReporteVentasRango, "/reportes-ventas/ventas-rango", orden: 1, icono: "report-money", paginaPadreId: paginaReportesId).Value;
            await context.Paginas.AddAsync(pagina);
        }
        else
        {
            context.Entry(pagina).Property(nameof(Pagina.Ruta)).CurrentValue = "/reportes-ventas/ventas-rango";
        }
        await context.SaveChangesAsync();
    }

    private static async Task CargarPaginaReporteExistenciasAsync(ApplicationDbContext context, Guid paginaReportesInventarioId)
    {
        var pagina = await context.Paginas.FirstOrDefaultAsync(p => p.Nombre == NombresPaginas.ReporteExistencias && p.PaginaPadreId == paginaReportesInventarioId);
        if (pagina == null)
        {
            pagina = Pagina.Crear(NombresPaginas.ReporteExistencias, "/reportes-inventario/existencias", orden: 1, icono: "report-money", paginaPadreId: paginaReportesInventarioId).Value;
            await context.Paginas.AddAsync(pagina);
            await context.SaveChangesAsync();
        }
    }

    #endregion

    public static async Task SembrarNegocioTicketConfigAsync(ApplicationDbContext context)
    {
        if (await context.NegocioTicketConfigs.AnyAsync()) return;

        var resultado = NegocioTicketConfig.Crear();
        if (!resultado.IsError)
            await context.NegocioTicketConfigs.AddAsync(resultado.Value);

        await context.SaveChangesAsync();
    }

    public static async Task SembrarPerfilesImpresoraTicketAsync(ApplicationDbContext context)
    {
        var definiciones = new[]
        {
            new
            {
                Clave = "generico-escpos-80",
                Nombre = "Genérico ESC/POS 80mm",
                AnchoMm = 80, CharsPorLinea = 48, Codepage = "CP437",
                DrawerPin = (byte)0, Corte = ComandoCorteTicket.PartialCut, Densidad = (byte)8,
            },
            new
            {
                Clave = "generico-escpos-58",
                Nombre = "Genérico ESC/POS 58mm",
                AnchoMm = 58, CharsPorLinea = 32, Codepage = "CP437",
                DrawerPin = (byte)0, Corte = ComandoCorteTicket.PartialCut, Densidad = (byte)8,
            },
            new
            {
                Clave = "epson-tm-t20-80",
                Nombre = "Epson TM-T20 (80mm)",
                AnchoMm = 80, CharsPorLinea = 48, Codepage = "CP858",
                DrawerPin = (byte)0, Corte = ComandoCorteTicket.PartialCut, Densidad = (byte)8,
            },
            new
            {
                Clave = "xprinter-xp58",
                Nombre = "Xprinter XP-58 (58mm)",
                AnchoMm = 58, CharsPorLinea = 32, Codepage = "CP858",
                DrawerPin = (byte)0, Corte = ComandoCorteTicket.FullCut, Densidad = (byte)8,
            },
            new
            {
                Clave = "xprinter-xp80",
                Nombre = "Xprinter XP-80 (80mm)",
                AnchoMm = 80, CharsPorLinea = 48, Codepage = "CP858",
                DrawerPin = (byte)0, Corte = ComandoCorteTicket.PartialCut, Densidad = (byte)8,
            },
        };

        var clavesExistentes = await context.PerfilesImpresoraTicket
            .Select(p => p.Clave)
            .ToHashSetAsync();

        foreach (var def in definiciones)
        {
            if (clavesExistentes.Contains(def.Clave))
            {
                continue;
            }

            var resultado = PerfilImpresoraTicket.Crear(
                def.Clave, def.Nombre, def.AnchoMm, def.CharsPorLinea,
                def.Codepage, def.DrawerPin, def.Corte, def.Densidad);
            if (!resultado.IsError)
            {
                await context.PerfilesImpresoraTicket.AddAsync(resultado.Value);
            }
        }

        await context.SaveChangesAsync();
    }
}

using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using PuntoVenta.Application.Common.Permisos;

namespace PuntoVenta.Infrastructure.Security;

public static class PermisosRegistrar
{
    public static class Claves
    {
        // Módulo: roles
        public const string RolesVer = "roles:ver";
        public const string RolesCrear = "roles:crear";
        public const string RolesEditar = "roles:editar";
        public const string RolesToggle = "roles:toggle";

        // Módulo: usuarios
        public const string UsuariosVer = "usuarios:ver";
        public const string UsuariosCrear = "usuarios:crear";
        public const string UsuariosEditar = "usuarios:editar";
        public const string UsuariosToggle = "usuarios:toggle";

        // Módulo: negocio
        public const string NegocioVer = "negocio:ver";
        public const string NegocioEditar = "negocio:editar";

        // Módulo: tipos-identificacion
        public const string TiposIdentificacionVer = "tipos-identificacion:ver";
        public const string TiposIdentificacionToggle = "tipos-identificacion:toggle";

        // Módulo: permisos
        public const string RolesPermisosAdministrar = "roles:permisos:administrar";

        // Módulo: condiciones-venta
        public const string CondicionesVentaVer = "condiciones-venta:ver";
        public const string CondicionesVentaToggle = "condiciones-venta:toggle";

        // Módulo: medios-pago
        public const string MediosPagoVer = "medios-pago:ver";
        public const string MediosPagoToggle = "medios-pago:toggle";

        // Módulo: codigos-impuesto
        public const string CodigosImpuestoVer = "codigos-impuesto:ver";
        public const string CodigosImpuestoToggle = "codigos-impuesto:toggle";

        // Módulo: tarifas-iva
        public const string TarifasIvaVer = "tarifas-iva:ver";
        public const string TarifasIvaToggle = "tarifas-iva:toggle";

        // Módulo: catálogos
        public const string CatalogosVer = "catalogos:ver";

        // Módulo: mantenimiento
        public const string MantenimientoVer = "mantenimiento:ver";

        // Módulo: marcas

        // Módulo: categorías
        public const string CategoriasVer = "categorias:ver";
        public const string CategoriasCrear = "categorias:crear";
        public const string CategoriasEditar = "categorias:editar";
        public const string CategoriasToggle = "categorias:toggle";

        // Módulo: vendedores
        public const string VendedoresVer = "vendedores:ver";
        public const string VendedoresCrear = "vendedores:crear";
        public const string VendedoresEditar = "vendedores:editar";
        public const string VendedoresToggle = "vendedores:toggle";

        // Módulo: proveedores

        // Módulo: bodegas

        // Módulo: inventario

        // Módulo: productos
        public const string ProductosVer = "productos:ver";
        public const string ProductosCrear = "productos:crear";
        public const string ProductosEditar = "productos:editar";
        public const string ProductosToggle = "productos:toggle";
        public const string ProductosNoAplicaExistencias = "productos:no-aplica-existencias";

        // Módulo: clientes
        public const string ClientesVer = ClientesPermisos.Ver;
        public const string ClientesCrear = ClientesPermisos.Crear;
        public const string ClientesEditar = ClientesPermisos.Editar;
        public const string ClientesToggle = ClientesPermisos.Toggle;

        // Módulo: facturación
        public const string FacturacionVer = "facturacion:ver";

        // Módulo: ventas
        public const string VentasApartadosCrear = "ventas:apartados:crear";
        public const string VentasApartadosAbonar = "ventas:apartados:abonar";
        public const string VentasApartadosExtender = "ventas:apartados:extender";
        public const string VentasApartadosConvertir = "ventas:apartados:convertir";
        public const string VentasApartadosCancelar = "ventas:apartados:cancelar";
        public const string VentasNotasCreditoCrear = "ventas:notas-credito:crear";
        public const string VentasNotasDebitoCrear = "ventas:notas-debito:crear";
        public const string VentasFacturasAbonar = "ventas:facturas:abonar";
        public const string VentasFacturasAbonoAnular = "ventas:facturas:abonos:anular";
        public const string VentasCreditoVer = "ventas:credito:ver";

        // Módulo: notificaciones
        public const string NotificacionesCrear = "notificaciones:crear";
        public const string NotificacionesReintentar = "notificaciones:reintentar";

        // Módulo: cajas
        public const string CajasVer = "cajas:ver";
        public const string CajasCrear = "cajas:crear";
        public const string CajasEditar = "cajas:editar";
        public const string CajasToggle = "cajas:toggle";

        // Módulo: productos — stock
        public const string ProductosAjustarStock = "productos:ajustar-stock";
        public const string ProductosMovimientosVer = "productos:movimientos-ver";

        // Módulo: sesiones de caja (apertura/cierre/movimientos)

        // Módulo: motivos de movimiento de caja (catálogo gestionable)

        // Módulo: reportes
        public const string ReportesVer = "reportes:ver";
        public const string ReportesVentasRangoVer = "reportes:ventas-rango:ver";

        // Módulo: dashboard (panel principal)
        public const string DashboardVer = "dashboard:ver";

        // Módulo: bitácora (auditoría de actividad)

        // Módulo: backup/restore de la base de datos (solo Desktop)
        public const string BackupAdministrar = "backup:administrar";
    }

    public static IServiceCollection AddPermisoPolicies(this IServiceCollection services)
    {
        services.AddSingleton<IAuthorizationHandler, PermisoAuthorizationHandler>();

        services.AddAuthorization(options =>
        {
            foreach (var clave in ObtenerTodasLasClaves())
            {
                options.AddPolicy(clave,
                    policy => policy.Requirements.Add(new PermisoRequirement(clave)));
            }
        });

        return services;
    }

    public static IEnumerable<string> ObtenerTodasLasClaves()
    {
        return typeof(Claves)
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
            .Where(f => f.IsLiteral && !f.IsInitOnly && f.FieldType == typeof(string))
            .Select(f => (string)f.GetRawConstantValue()!);
    }
}

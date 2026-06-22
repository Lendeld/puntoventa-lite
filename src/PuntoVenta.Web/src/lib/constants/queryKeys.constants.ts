import type { ObtenerUsuariosPaginadoParams } from "@lib/types/usuarios.types";
import type { ObtenerRolesPaginadoParams } from "@lib/types/roles.types";
import type { ObtenerCategoriasPaginadoParams } from "@lib/types/categorias.types";
import type { ObtenerVendedoresPaginadoParams } from "@lib/types/vendedores.types";
import type { ObtenerProductosPaginadoParams } from "@lib/types/productos.types";
import type { ObtenerClientesPaginadoParams } from "@lib/types/clientes.types";
import type {
    ObtenerDocumentosVentaPaginadoParams,
    ObtenerReporteMovimientosDineroParams,
    ObtenerReporteVentasRangoParams,
} from "@lib/types/ventas.types";
import type { ObtenerMovimientosStockParams } from "@lib/types/inventario.types";

export const QUERY_KEYS = {
    auth: {
        usuarioActual: ["auth", "usuario-actual"] as const,
        permiso: (permiso: string) => ["auth", "permiso", permiso] as const,
    },
    configuracion: {
        all: ["configuracion"] as const,
        negocio: ["configuracion", "negocio"] as const,
        tienda: ["configuracion", "negocio"] as const,
        tiposIdentificacion: (activo?: boolean) =>
            ["configuracion", "tipos-identificacion", activo ?? "todos"] as const,
        condicionesVenta: (activo?: boolean) =>
            ["configuracion", "condiciones-venta", activo ?? "todos"] as const,
        mediosPago: (activo?: boolean) =>
            ["configuracion", "medios-pago", activo ?? "todos"] as const,
        codigosImpuesto: (activo?: boolean) =>
            ["configuracion", "codigos-impuesto", activo ?? "todos"] as const,
        tarifasIva: (activo?: boolean) =>
            ["configuracion", "tarifas-iva", activo ?? "todos"] as const,
    },
    roles: {
        all: ["roles"] as const,
        activos: ["roles", "activos"] as const,
        lista: (params: ObtenerRolesPaginadoParams) => ["roles", "lista", params] as const,
        detalle: (id: string) => ["roles", "detalle", id] as const,
        paginasPermisos: (rolId: string) => ["roles", rolId, "permisos", "paginas"] as const,
        permisosPagina: (rolId: string, paginaId: string) =>
            ["roles", rolId, "permisos", "pagina", paginaId] as const,
    },
    usuarios: {
        all: ["usuarios"] as const,
        lista: (params: ObtenerUsuariosPaginadoParams) => ["usuarios", "lista", params] as const,
        detalle: (id: string) => ["usuarios", "detalle", id] as const,
    },
    cajas: {
        all: ["cajas"] as const,
        lista: ["cajas", "lista"] as const,
    },
    categorias: {
        all: ["categorias"] as const,
        activas: ["categorias", "activas"] as const,
        lista: (params: ObtenerCategoriasPaginadoParams) => ["categorias", "lista", params] as const,
        detalle: (id: string) => ["categorias", "detalle", id] as const,
    },
    vendedores: {
        all: ["vendedores"] as const,
        activas: ["vendedores", "activas"] as const,
        lista: (params: ObtenerVendedoresPaginadoParams) => ["vendedores", "lista", params] as const,
        detalle: (id: string) => ["vendedores", "detalle", id] as const,
    },
    productos: {
        all: ["productos"] as const,
        lista: (params: ObtenerProductosPaginadoParams) => ["productos", "lista", params] as const,
        detalle: (id: string) => ["productos", "detalle", id] as const,
    },
    clientes: {
        all: ["clientes"] as const,
        lista: (params: ObtenerClientesPaginadoParams) => ["clientes", "lista", params] as const,
        detalle: (id: string) => ["clientes", "detalle", id] as const,
    },
    ventas: {
        all: ["ventas"] as const,
        lista: (params: ObtenerDocumentosVentaPaginadoParams) =>
            ["ventas", "lista", params] as const,
        detalle: (id: string) => ["ventas", "detalle", id] as const,
        catalogos: ["ventas", "catalogos"] as const,
        credito: (params: unknown) => ["ventas", "credito", params] as const,
        saldoCliente: (clienteId: string) => ["ventas", "credito", "cliente", clienteId] as const,
        eventos: (id: string, skip: number, take: number) =>
            ["ventas", "eventos", id, skip, take] as const,
        eventosPorVenta: (id: string) => ["ventas", "eventos", id] as const,
        ticketData: (id: string, pagoId?: string) =>
            ["ventas", "ticket-data", id, pagoId ?? null] as const,
        reporteRango: (params: ObtenerReporteVentasRangoParams) =>
            ["ventas", "reporte-rango", params] as const,
        reporteMovimientosDinero: (params: ObtenerReporteMovimientosDineroParams) =>
            ["ventas", "reporte-movimientos-dinero", params] as const,
    },
    inventario: {
        all: ["inventario"] as const,
        movimientos: (params: ObtenerMovimientosStockParams) =>
            ["inventario", "movimientos", params] as const,
    },
    dashboardResumen: ["dashboard", "resumen"] as const,
    impresion: {
        ticketConfig: ["impresion", "ticket-config"] as const,
        perfiles: ["impresion", "perfiles"] as const,
    },
} as const;

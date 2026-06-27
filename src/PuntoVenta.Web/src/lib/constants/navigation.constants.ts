export const NAVIGATION_MENU = [
    { label: 'Principal', href: '/', icon: 'IconLayoutDashboard' },
    {
        label: 'Clientes',
        href: '/clientes',
        icon: 'IconUsers',
    },
    {
        label: 'EMISIÓN',
        items: [
            {
                label: 'Facturación',
                href: '/emision/facturacion',
                icon: 'IconReceipt2',
            },
            {
                label: 'Ventas',
                href: '/emision/ventas',
                icon: 'IconListDetails',
            },
            {
                label: 'Créditos',
                href: '/emision/cobros/credito',
                icon: 'IconCashBanknote',
            },
        ],
    },
    {
        label: 'INVENTARIO',
        items: [
            {
                label: 'Productos',
                href: '/inventario/productos',
                icon: 'IconPackage',
            },
            {
                label: 'Movimientos',
                href: '/inventario/movimientos',
                icon: 'IconArrowsExchange',
            },
        ],
    },

    {
        label: 'MANTENIMIENTO',
        items: [
            {
                label: 'Categorías',
                href: '/mantenimiento/categorias',
                icon: 'IconCategory',
            },
            {
                label: 'Vendedores',
                href: '/mantenimiento/vendedores',
                icon: 'IconUserCheck',
            },
        ],
    },
    {
        label: 'REPORTES',
        items: [
            {
                label: 'Reportes Ventas',
                href: '/reportes-ventas',
                icon: 'IconReportAnalytics',
            },
            {
                label: 'Reportes Inventario',
                href: '/reportes-inventario',
                icon: 'IconReportSearch',
            },
        ],
    },
    {
        label: 'SISTEMA',
        items: [
            { label: 'Usuarios', href: '/sistema/usuarios', icon: 'IconUserCog' },
            { label: 'Cajas', href: '/sistema/cajas', icon: 'IconCashRegister' },
            { label: 'Roles', href: '/sistema/roles', icon: 'IconShieldCog' },
            { label: 'Mi Negocio', href: '/sistema/mi-negocio', icon: 'IconBuildingStore' },
            { label: 'Catálogos', href: '/sistema/catalogos', icon: 'IconSettingsCog' },
            { label: 'Respaldo', href: '/sistema/respaldo', icon: 'IconDatabaseExport' },
            { label: 'Acerca de', href: '/acerca', icon: 'IconInfoCircle' },
        ],
    },

] as const;

export type NavigationMenu = typeof NAVIGATION_MENU;

export type NavLeaf = {
    readonly label: string;
    readonly href: string;
    readonly icon: string;
    readonly badge?: number;
    readonly exact?: boolean;
};

export type NavGroup = {
    readonly label: string;
    readonly items: readonly NavLeaf[];
};

export type MenuItem = NavLeaf;

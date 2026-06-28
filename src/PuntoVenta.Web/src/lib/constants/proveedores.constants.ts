export const PROVEEDOR_FIELDS = {
    NOMBRE: "Proveedor_Nombre",
    CORREO: "Proveedor_Correo",
    TELEFONO: "Proveedor_Telefono",
    OBSERVACION: "Proveedor_Observacion",
    ACTIVO: "Proveedor_Activo",
} as const;

export const PROVEEDOR_MAX = {
    NOMBRE: 100,
    CORREO: 160,
    TELEFONO: 20,
    OBSERVACION: 500,
} as const;

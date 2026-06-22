export const NEGOCIO_TICKET_CONFIG_FIELDS = {
    MENSAJE_PIE: "NegocioTicketConfig_MensajePie",
    MOSTRAR_LOGO: "NegocioTicketConfig_MostrarLogo",
    APLICA_COPIA_CLIENTE_NEGOCIO: "NegocioTicketConfig_AplicaCopiaClienteNegocio",
    MOSTRAR_CODIGO_BARRAS: "NegocioTicketConfig_MostrarCodigoBarras",
    CONFIGURACIONES: "NegocioTicketConfig_Configuraciones",
    ELEMENTOS_ENCABEZADO: "NegocioTicketConfig_ElementosEncabezado",
} as const;

// Encabezado ordenable del ticket térmico.
export const ELEMENTO_ENCABEZADO_TEXTO_MAX = 60;
export const MAX_ELEMENTOS_TEXTO_ENCABEZADO = 5;

// Tipos fijos (siempre presentes, solo se ocultan/reordenan). "Texto" se agrega/borra.
export const ELEMENTO_ENCABEZADO_TIPOS_FIJOS = [
    "Nombre",
    "NombreComercial",
    "Correo",
    "Telefono",
    "Direccion",
    "IdentificacionFiscal",
    "Fecha",
] as const;

// Encabezado por defecto (coincide con NegocioTicketConfig.EncabezadoPorDefecto del
// backend): se usa solo como respaldo si el config llega sin elementos.
export const ENCABEZADO_POR_DEFECTO: ReadonlyArray<{
    tipo: string;
    visible: boolean;
}> = [
    { tipo: "NombreComercial", visible: true },
    { tipo: "Nombre", visible: false },
    { tipo: "IdentificacionFiscal", visible: true },
    { tipo: "Telefono", visible: true },
    { tipo: "Correo", visible: true },
    { tipo: "Direccion", visible: true },
    { tipo: "Fecha", visible: true },
];

export const ELEMENTO_ENCABEZADO_LABELS: Record<string, string> = {
    Nombre: "Nombre legal",
    NombreComercial: "Nombre comercial",
    Correo: "Correo",
    Telefono: "Teléfono",
    Direccion: "Dirección",
    IdentificacionFiscal: "Cédula / ID fiscal",
    Fecha: "Fecha y hora",
    Texto: "Texto libre",
};

export const LINEA_PIE_TEXTO_MAX = 120;
export const CONFIG_NOMBRE_MAX = 60;
// Máximo de líneas por configuración.
export const MAX_LINEAS = 10;
// Máximo de configuraciones por destino (PDF / Ticket).
export const MAX_CONFIGS_POR_DESTINO = 5;

export const ALINEACION_LINEA_PIE_OPTIONS = [
    { value: "Izquierda", label: "Izquierda" },
    { value: "Centro", label: "Centro" },
    { value: "Derecha", label: "Derecha" },
] as const;

// Coincide con TipoDocumentoVenta del backend (se serializa como string).
export const TIPO_DOCUMENTO_LINEA_PIE_OPTIONS = [
    { value: "Factura", label: "Factura" },
    { value: "Apartado", label: "Apartado" },
    { value: "NotaCredito", label: "Nota de crédito" },
    { value: "NotaDebito", label: "Nota de débito" },
    { value: "Proforma", label: "Proforma" },
] as const;

import type { FormValues } from "@lib/types/base.types";
import type { actualizarNegocioTicketConfigSchema } from "@lib/schemas/negocio-ticket-config.schema";

export type SistemaOperativoAgente = "Windows" | "MacOS" | "Linux";
export type ArquitecturaAgente = "X64" | "X86" | "Arm64";
export type CanalAgente = "Stable" | "Beta";
export type ComandoCorteTicket = "None" | "PartialCut" | "FullCut";

export type AlineacionLineaPie = "Izquierda" | "Centro" | "Derecha";
export type DestinoLineaPie = "Pdf" | "Ticket";
export type TipoDocumentoLineaPie =
    | "Factura"
    | "Apartado"
    | "NotaCredito"
    | "NotaDebito"
    | "Proforma";

export interface LineaPieDocumentoDto {
    texto: string;
    alineacion: AlineacionLineaPie;
    negrita: boolean;
    orden: number;
}

export interface ConfiguracionPieDocumentoDto {
    nombre: string;
    destino: DestinoLineaPie;
    tiposDocumento: TipoDocumentoLineaPie[];
    lineas: LineaPieDocumentoDto[];
}

export type ElementoEncabezadoTipo =
    | "Nombre"
    | "NombreComercial"
    | "Correo"
    | "Telefono"
    | "Direccion"
    | "IdentificacionFiscal"
    | "Fecha"
    | "Texto";

export interface ElementoEncabezadoDto {
    tipo: ElementoEncabezadoTipo;
    orden: number;
    visible: boolean;
    textoLibre: string | null;
}

export interface NegocioTicketConfigDto {
    id: string;
    mensajePie: string | null;
    mostrarLogo: boolean;
    aplicaCopiaClienteNegocio: boolean;
    mostrarCodigoBarras: boolean;
    configuraciones: ConfiguracionPieDocumentoDto[];
    elementosEncabezado: ElementoEncabezadoDto[];
}

export interface PerfilImpresoraTicketDto {
    id: string;
    clave: string;
    nombre: string;
    anchoMm: number;
    charsPorLinea: number;
    codepage: string;
    drawerPin: number;
    comandoCorte: ComandoCorteTicket;
    densidad: number;
    activo: boolean;
}

export type ActualizarNegocioTicketConfigFormValues = FormValues<
    typeof actualizarNegocioTicketConfigSchema
>;

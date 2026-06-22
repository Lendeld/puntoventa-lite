using ErrorOr;

namespace PuntoVenta.Domain.Entities.Negocios;

public static class NegocioTicketConfigErrors
{
    public static Error NegocioRequerido =>
        Error.Validation("NegocioTicketConfig_Negocio", "El negocio es requerido.");

    public static Error MensajePieExcedeLongitud =>
        Error.Validation(
            "NegocioTicketConfig_MensajePie",
            $"El mensaje de pie no puede exceder {NegocioTicketConfig.MensajePieMaxLength} caracteres.");

    public static Error DemasiadasConfiguraciones =>
        Error.Validation(
            "NegocioTicketConfig_Configuraciones",
            $"No se pueden configurar más de {NegocioTicketConfig.MaxConfiguracionesPorDestino} configuraciones por destino.");

    public static Error TiposDocumentoTraslapan =>
        Error.Validation(
            "NegocioTicketConfig_Configuraciones",
            "Un tipo de documento no puede estar en más de una configuración del mismo destino.");

    public static Error ConfigTodosExclusiva =>
        Error.Validation(
            "NegocioTicketConfig_Configuraciones",
            "Una configuración para todos los documentos debe ser la única de su destino.");

    public static Error NombreDuplicado =>
        Error.Validation(
            "NegocioTicketConfig_Configuraciones",
            "No puede haber dos configuraciones con el mismo nombre en el mismo destino.");

    public static Error NoEncontrada =>
        Error.NotFound("NegocioTicketConfig_NoEncontrada", "La configuración de ticket del negocio no existe.");

    public static Error EncabezadoFaltaTipoFijo =>
        Error.Validation(
            "NegocioTicketConfig_ElementosEncabezado",
            "El encabezado debe incluir todos los elementos fijos (no se pueden eliminar).");

    public static Error EncabezadoTipoFijoDuplicado =>
        Error.Validation(
            "NegocioTicketConfig_ElementosEncabezado",
            "Un elemento fijo del encabezado no puede repetirse.");

    public static Error EncabezadoDemasiadosTexto =>
        Error.Validation(
            "NegocioTicketConfig_ElementosEncabezado",
            $"No se pueden agregar más de {NegocioTicketConfig.MaxElementosTextoEncabezado} elementos de texto al encabezado.");
}

using ErrorOr;

namespace PuntoVenta.Domain.Entities.Negocios;

public static class ConfiguracionPieDocumentoErrors
{
    public static Error NombreRequerido =>
        Error.Validation("ConfiguracionPieDocumento_Nombre", "El nombre de la configuración es requerido.");

    public static Error NombreExcedeLongitud =>
        Error.Validation(
            "ConfiguracionPieDocumento_Nombre",
            $"El nombre de la configuración no puede exceder {ConfiguracionPieDocumento.MaxNombreLength} caracteres.");

    public static Error TipoDocumentoInvalido =>
        Error.Validation(
            "ConfiguracionPieDocumento_TiposDocumento",
            "Uno de los tipos de documento seleccionados no es válido.");

    public static Error DemasiadasLineas =>
        Error.Validation(
            "ConfiguracionPieDocumento_Lineas",
            $"Una configuración no puede tener más de {ConfiguracionPieDocumento.MaxLineas} líneas de pie.");
}

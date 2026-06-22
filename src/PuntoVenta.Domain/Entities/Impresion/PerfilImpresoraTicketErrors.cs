using ErrorOr;

namespace PuntoVenta.Domain.Entities.Impresion;

public static class PerfilImpresoraTicketErrors
{
    public static Error ClaveRequerida =>
        Error.Validation("PerfilImpresoraTicket_Clave", "La clave del perfil es requerida.");

    public static Error ClaveExcedeLongitud =>
        Error.Validation("PerfilImpresoraTicket_Clave",
            $"La clave no puede exceder {PerfilImpresoraTicket.ClaveMaxLength} caracteres.");

    public static Error NombreRequerido =>
        Error.Validation("PerfilImpresoraTicket_Nombre", "El nombre del perfil es requerido.");

    public static Error NombreExcedeLongitud =>
        Error.Validation("PerfilImpresoraTicket_Nombre",
            $"El nombre no puede exceder {PerfilImpresoraTicket.NombreMaxLength} caracteres.");

    public static Error AnchoInvalido =>
        Error.Validation("PerfilImpresoraTicket_AnchoMm", "El ancho debe ser 58 o 80 milímetros.");

    public static Error CharsPorLineaInvalido =>
        Error.Validation("PerfilImpresoraTicket_CharsPorLinea", "Caracteres por línea debe estar entre 16 y 64.");

    public static Error CodepageRequerida =>
        Error.Validation("PerfilImpresoraTicket_Codepage", "El codepage es requerido.");

    public static Error CodepageExcedeLongitud =>
        Error.Validation("PerfilImpresoraTicket_Codepage",
            $"El codepage no puede exceder {PerfilImpresoraTicket.CodepageMaxLength} caracteres.");

    public static Error DrawerPinInvalido =>
        Error.Validation("PerfilImpresoraTicket_DrawerPin", "El pin de la gaveta debe ser 0 o 1.");

    public static Error ClaveYaExiste =>
        Error.Conflict("PerfilImpresoraTicket_Clave", "Ya existe un perfil con esa clave.");

    public static Error NoEncontrado =>
        Error.NotFound("PerfilImpresoraTicket_NoEncontrado", "El perfil de impresora no existe.");
}

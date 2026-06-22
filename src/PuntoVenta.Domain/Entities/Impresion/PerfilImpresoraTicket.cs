using ErrorOr;

namespace PuntoVenta.Domain.Entities.Impresion;

public sealed class PerfilImpresoraTicket : BaseAuditableEntity
{
    public const int ClaveMaxLength = 60;
    public const int NombreMaxLength = 100;
    public const int CodepageMaxLength = 16;

    private PerfilImpresoraTicket() { }

    public string Clave { get; private set; } = string.Empty;
    public string Nombre { get; private set; } = string.Empty;
    public int AnchoMm { get; private set; }
    public int CharsPorLinea { get; private set; }
    public string Codepage { get; private set; } = string.Empty;
    public byte DrawerPin { get; private set; }
    public ComandoCorteTicket ComandoCorte { get; private set; }
    public byte Densidad { get; private set; }

    public static ErrorOr<PerfilImpresoraTicket> Crear(
        string clave,
        string nombre,
        int anchoMm,
        int charsPorLinea,
        string codepage,
        byte drawerPin,
        ComandoCorteTicket comandoCorte,
        byte densidad)
    {
        var errores = Validar(clave, nombre, anchoMm, charsPorLinea, codepage, drawerPin);
        if (errores.Count > 0)
        {
            return errores;
        }

        return new PerfilImpresoraTicket
        {
            Clave = clave.Trim(),
            Nombre = nombre.Trim(),
            AnchoMm = anchoMm,
            CharsPorLinea = charsPorLinea,
            Codepage = codepage.Trim(),
            DrawerPin = drawerPin,
            ComandoCorte = comandoCorte,
            Densidad = densidad,
        };
    }

    private static List<Error> Validar(
        string clave,
        string nombre,
        int anchoMm,
        int charsPorLinea,
        string codepage,
        byte drawerPin)
    {
        var errores = new List<Error>();

        if (string.IsNullOrWhiteSpace(clave))
        {
            errores.Add(PerfilImpresoraTicketErrors.ClaveRequerida);
        }
        else if (clave.Trim().Length > ClaveMaxLength)
        {
            errores.Add(PerfilImpresoraTicketErrors.ClaveExcedeLongitud);
        }

        if (string.IsNullOrWhiteSpace(nombre))
        {
            errores.Add(PerfilImpresoraTicketErrors.NombreRequerido);
        }
        else if (nombre.Trim().Length > NombreMaxLength)
        {
            errores.Add(PerfilImpresoraTicketErrors.NombreExcedeLongitud);
        }

        if (anchoMm != 58 && anchoMm != 80)
        {
            errores.Add(PerfilImpresoraTicketErrors.AnchoInvalido);
        }

        if (charsPorLinea < 16 || charsPorLinea > 64)
        {
            errores.Add(PerfilImpresoraTicketErrors.CharsPorLineaInvalido);
        }

        if (string.IsNullOrWhiteSpace(codepage))
        {
            errores.Add(PerfilImpresoraTicketErrors.CodepageRequerida);
        }
        else if (codepage.Trim().Length > CodepageMaxLength)
        {
            errores.Add(PerfilImpresoraTicketErrors.CodepageExcedeLongitud);
        }

        if (drawerPin > 1)
        {
            errores.Add(PerfilImpresoraTicketErrors.DrawerPinInvalido);
        }

        return errores;
    }
}

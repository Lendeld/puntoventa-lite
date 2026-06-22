using ErrorOr;

namespace PuntoVenta.Domain.Entities.Paginas;

public sealed class Pagina : BaseAuditableEntity
{
    public const int NombreMaxLength = 150;
    public const int RutaMaxLength   = 500;
    public const int IconoMaxLength  = 100;

    private Pagina() { }

    public string  Nombre        { get; private set; } = string.Empty;
    public string  Ruta          { get; private set; } = string.Empty;
    public string? Icono         { get; private set; }
    public int     Orden         { get; private set; }
    public Guid?   PaginaPadreId { get; private set; }

    public Pagina?  PaginaPadre   { get; private set; }

    private readonly List<PaginaPermiso> _paginaPermisos = [];
    public IReadOnlyCollection<PaginaPermiso> PaginaPermisos => _paginaPermisos;

    public static ErrorOr<Pagina> Crear(
        string nombre,
        string ruta,
        int orden,
        string? icono = null,
        Guid? paginaPadreId = null)
    {
        var errores = new List<Error>();

        if (string.IsNullOrWhiteSpace(nombre))
        {
            errores.Add(PaginaErrors.NombreRequerido);
        }
        else if (nombre.Trim().Length > NombreMaxLength)
        {
            errores.Add(PaginaErrors.NombreExcedeLongitud);
        }

        if (string.IsNullOrWhiteSpace(ruta))
        {
            errores.Add(PaginaErrors.RutaRequerida);
        }
        else if (ruta.Trim().Length > RutaMaxLength)
        {
            errores.Add(PaginaErrors.RutaExcedeLongitud);
        }

        if (icono is not null && icono.Trim().Length > IconoMaxLength)
        {
            errores.Add(PaginaErrors.IconoExcedeLongitud);
        }

        if (errores.Count > 0)
        {
            return errores;
        }

        return new Pagina
        {
            Nombre        = nombre.Trim(),
            Ruta          = ruta.Trim(),
            Orden         = orden,
            Icono         = icono?.Trim(),
            PaginaPadreId = paginaPadreId
        };
    }
}

using ErrorOr;
using PuntoVenta.Domain.Entities.Ventas;

namespace PuntoVenta.Domain.Entities.Negocios;

public sealed class ConfiguracionPieDocumento
{
    public const int MaxNombreLength = 60;
    public const int MaxLineas = 10;

    private List<TipoDocumentoVenta> _tiposDocumento = [];
    private List<LineaPieDocumento> _lineas = [];

    private ConfiguracionPieDocumento() { }

    public string Nombre { get; private set; } = string.Empty;

    public DestinoLineaPie Destino { get; private set; }

    /// <summary>Tipos de documento que cubre la configuración. Lista vacía = todos los tipos.</summary>
    public IReadOnlyList<TipoDocumentoVenta> TiposDocumento => _tiposDocumento;

    public IReadOnlyList<LineaPieDocumento> Lineas => _lineas;

    public static ErrorOr<ConfiguracionPieDocumento> Crear(
        string nombre,
        DestinoLineaPie destino,
        IEnumerable<TipoDocumentoVenta>? tiposDocumento,
        IEnumerable<LineaPieDocumento>? lineas)
    {
        var errores = new List<Error>();

        if (string.IsNullOrWhiteSpace(nombre))
        {
            errores.Add(ConfiguracionPieDocumentoErrors.NombreRequerido);
        }
        else if (nombre.Trim().Length > MaxNombreLength)
        {
            errores.Add(ConfiguracionPieDocumentoErrors.NombreExcedeLongitud);
        }

        var tipos = (tiposDocumento ?? []).Distinct().ToList();
        if (tipos.Any(t => !Enum.IsDefined(t)))
        {
            errores.Add(ConfiguracionPieDocumentoErrors.TipoDocumentoInvalido);
        }

        var listaLineas = (lineas ?? []).ToList();
        if (listaLineas.Count > MaxLineas)
        {
            errores.Add(ConfiguracionPieDocumentoErrors.DemasiadasLineas);
        }

        if (errores.Count > 0)
        {
            return errores;
        }

        var config = new ConfiguracionPieDocumento
        {
            Nombre = nombre.Trim(),
            Destino = destino
        };
        config._tiposDocumento.AddRange(tipos);
        config._lineas.AddRange(listaLineas);
        return config;
    }

    /// <summary>True si la configuración no filtra por tipo (aplica a todos los documentos).</summary>
    public bool EsTodos => _tiposDocumento.Count == 0;

    /// <summary>Indica si la configuración aplica al tipo de documento dado (vacío = todos).</summary>
    public bool AplicaA(TipoDocumentoVenta tipo) => EsTodos || _tiposDocumento.Contains(tipo);
}

using ErrorOr;
using PuntoVenta.Domain.Entities.Ventas;

namespace PuntoVenta.Domain.Entities.Negocios;

public sealed class NegocioTicketConfig : BaseAuditableEntity
{
    public const int MensajePieMaxLength = 240;
    public const int MaxConfiguracionesPorDestino = 5;
    public const int MaxElementosTextoEncabezado = 5;

    // Tipos fijos del encabezado (siempre presentes; solo se ocultan/reordenan).
    private static readonly ElementoEncabezadoTipo[] TiposFijosEncabezado =
    [
        ElementoEncabezadoTipo.Nombre,
        ElementoEncabezadoTipo.NombreComercial,
        ElementoEncabezadoTipo.Correo,
        ElementoEncabezadoTipo.Telefono,
        ElementoEncabezadoTipo.Direccion,
        ElementoEncabezadoTipo.IdentificacionFiscal,
        ElementoEncabezadoTipo.Fecha
    ];

    private List<ConfiguracionPieDocumento> _configuraciones = [];
    private List<ElementoEncabezado> _elementosEncabezado = [];

    private NegocioTicketConfig() { }

    public string? MensajePie { get; private set; }

    public bool MostrarLogo { get; private set; }

    // Si está activo, el ticket sale en 2 copias: una rotulada "Cliente" y otra "Negocio" al pie.
    public bool AplicaCopiaClienteNegocio { get; private set; }

    // Si está activo, ticket y PDF imprimen el código de barras del consecutivo al pie.
    public bool MostrarCodigoBarras { get; private set; }

    public IReadOnlyList<ConfiguracionPieDocumento> Configuraciones => _configuraciones;

    // Encabezado ordenable del ticket térmico.
    public IReadOnlyList<ElementoEncabezado> ElementosEncabezado => _elementosEncabezado;

    public static ErrorOr<NegocioTicketConfig> Crear()
    {
        // Logo y código de barras arrancan inactivos por defecto; el negocio los activa
        // desde la configuración de ticket si los quiere.
        var config = new NegocioTicketConfig
        {
            MensajePie = null,
            MostrarLogo = false,
            MostrarCodigoBarras = false
        };
        config._elementosEncabezado.AddRange(EncabezadoPorDefecto());
        return config;
    }

    public static List<ElementoEncabezado> EncabezadoPorDefecto()
    {
        var defaults = new[]
        {
            (ElementoEncabezadoTipo.NombreComercial, true),
            (ElementoEncabezadoTipo.Nombre, false),
            (ElementoEncabezadoTipo.IdentificacionFiscal, true),
            (ElementoEncabezadoTipo.Telefono, true),
            (ElementoEncabezadoTipo.Correo, true),
            (ElementoEncabezadoTipo.Direccion, true),
            (ElementoEncabezadoTipo.Fecha, true)
        };

        var lista = new List<ElementoEncabezado>();
        for (var i = 0; i < defaults.Length; i++)
        {
            var (tipo, visible) = defaults[i];
            lista.Add(ElementoEncabezado.Crear(tipo, i, visible, null).Value);
        }
        return lista;
    }

    public ErrorOr<Success> Actualizar(
        string? mensajePie,
        bool mostrarLogo,
        bool aplicaCopiaClienteNegocio,
        bool mostrarCodigoBarras,
        IReadOnlyList<ConfiguracionPieDocumento>? configuraciones = null,
        IReadOnlyList<ElementoEncabezado>? elementosEncabezado = null)
    {
        var errores = Validar(mensajePie);

        var configs = configuraciones ?? [];
        errores.AddRange(ValidarConfiguraciones(configs));

        if (elementosEncabezado is not null)
        {
            errores.AddRange(ValidarEncabezado(elementosEncabezado));
        }

        if (errores.Count > 0)
        {
            return errores;
        }

        MensajePie = string.IsNullOrWhiteSpace(mensajePie) ? null : mensajePie.Trim();
        MostrarLogo = mostrarLogo;
        AplicaCopiaClienteNegocio = aplicaCopiaClienteNegocio;
        MostrarCodigoBarras = mostrarCodigoBarras;

        _configuraciones.Clear();
        _configuraciones.AddRange(configs);

        if (elementosEncabezado is not null)
        {
            _elementosEncabezado.Clear();
            _elementosEncabezado.AddRange(elementosEncabezado);
        }

        return Result.Success;
    }

    public IReadOnlyList<(string Texto, bool Negrita)> ResolverEncabezado(
        string nombre,
        string? nombreComercial,
        string? identificacionFiscal,
        string? telefono,
        string? correo,
        string? direccion,
        string fechaFormateada)
    {
        var resultado = new List<(string, bool)>();
        foreach (var elemento in _elementosEncabezado.Where(e => e.Visible).OrderBy(e => e.Orden))
        {
            var texto = elemento.Tipo switch
            {
                ElementoEncabezadoTipo.NombreComercial =>
                    string.IsNullOrWhiteSpace(nombreComercial) ? nombre : nombreComercial,
                ElementoEncabezadoTipo.Nombre => nombre,
                ElementoEncabezadoTipo.IdentificacionFiscal =>
                    string.IsNullOrWhiteSpace(identificacionFiscal) ? null : $"Cedula: {identificacionFiscal}",
                ElementoEncabezadoTipo.Telefono =>
                    string.IsNullOrWhiteSpace(telefono) ? null : $"Tel: {telefono}",
                ElementoEncabezadoTipo.Correo => correo,
                ElementoEncabezadoTipo.Direccion => direccion,
                ElementoEncabezadoTipo.Fecha => fechaFormateada,
                ElementoEncabezadoTipo.Texto => elemento.TextoLibre,
                _ => null
            };

            if (!string.IsNullOrWhiteSpace(texto))
            {
                var negrita = elemento.Tipo is ElementoEncabezadoTipo.Nombre
                    or ElementoEncabezadoTipo.NombreComercial;
                resultado.Add((texto, negrita));
            }
        }
        return resultado;
    }

    public IReadOnlyList<LineaPieDocumento> ResolverLineas(DestinoLineaPie destino, TipoDocumentoVenta tipo)
    {
        var candidatos = _configuraciones.Where(c => c.Destino == destino && c.AplicaA(tipo));
        var config = candidatos
            .OrderBy(c => c.EsTodos ? 1 : 0)
            .FirstOrDefault();
        return config is null
            ? []
            : config.Lineas.OrderBy(l => l.Orden).ToList();
    }

    private static List<Error> ValidarEncabezado(IReadOnlyList<ElementoEncabezado> elementos)
    {
        var errores = new List<Error>();

        var fijos = elementos.Where(e => e.EsFijo).ToList();
        var tiposFijos = fijos.Select(e => e.Tipo).ToList();

        if (tiposFijos.Distinct().Count() != tiposFijos.Count)
        {
            errores.Add(NegocioTicketConfigErrors.EncabezadoTipoFijoDuplicado);
        }

        if (TiposFijosEncabezado.Any(t => !tiposFijos.Contains(t)))
        {
            errores.Add(NegocioTicketConfigErrors.EncabezadoFaltaTipoFijo);
        }

        var textos = elementos.Count(e => e.Tipo == ElementoEncabezadoTipo.Texto);
        if (textos > MaxElementosTextoEncabezado)
        {
            errores.Add(NegocioTicketConfigErrors.EncabezadoDemasiadosTexto);
        }

        return errores;
    }

    private static List<Error> Validar(string? mensajePie)
    {
        var errores = new List<Error>();

        if (mensajePie is not null && mensajePie.Trim().Length > MensajePieMaxLength)
        {
            errores.Add(NegocioTicketConfigErrors.MensajePieExcedeLongitud);
        }

        return errores;
    }

    private static List<Error> ValidarConfiguraciones(IReadOnlyList<ConfiguracionPieDocumento> configs)
    {
        var errores = new List<Error>();

        foreach (var grupo in configs.GroupBy(c => c.Destino))
        {
            var delDestino = grupo.ToList();

            if (delDestino.Count > MaxConfiguracionesPorDestino)
            {
                errores.Add(NegocioTicketConfigErrors.DemasiadasConfiguraciones);
            }

            if (delDestino.Any(c => c.EsTodos) && delDestino.Count > 1)
            {
                errores.Add(NegocioTicketConfigErrors.ConfigTodosExclusiva);
            }

            var tiposVistos = new HashSet<TipoDocumentoVenta>();
            foreach (var tipo in delDestino.SelectMany(c => c.TiposDocumento))
            {
                if (!tiposVistos.Add(tipo))
                {
                    errores.Add(NegocioTicketConfigErrors.TiposDocumentoTraslapan);
                    break;
                }
            }

            var nombresDuplicados = delDestino
                .GroupBy(c => c.Nombre, StringComparer.OrdinalIgnoreCase)
                .Any(g => g.Count() > 1);
            if (nombresDuplicados)
            {
                errores.Add(NegocioTicketConfigErrors.NombreDuplicado);
            }
        }

        return errores;
    }
}

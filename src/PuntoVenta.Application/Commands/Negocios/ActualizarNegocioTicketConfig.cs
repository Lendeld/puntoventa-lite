using ErrorOr;
using Mediator;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Domain.Entities.Negocios;
using PuntoVenta.Domain.Entities.Ventas;

namespace PuntoVenta.Application.Commands.Negocios;

public sealed record LineaPieInput(
    string Texto,
    AlineacionLineaPie Alineacion,
    bool Negrita);

public sealed record ConfiguracionPieInput(
    string Nombre,
    DestinoLineaPie Destino,
    IReadOnlyList<TipoDocumentoVenta>? TiposDocumento,
    IReadOnlyList<LineaPieInput>? Lineas);

public sealed record ElementoEncabezadoInput(
    ElementoEncabezadoTipo Tipo,
    bool Visible,
    string? TextoLibre);

public sealed record ActualizarNegocioTicketConfigCommand(
    string? MensajePie,
    bool MostrarLogo,
    bool AplicaCopiaClienteNegocio = false,
    bool MostrarCodigoBarras = true,
    IReadOnlyList<ConfiguracionPieInput>? Configuraciones = null,
    IReadOnlyList<ElementoEncabezadoInput>? ElementosEncabezado = null) : IRequest<ErrorOr<Success>>;

public sealed class ActualizarNegocioTicketConfigHandler(
    INegocioTicketConfigRepository repository)
        : IRequestHandler<ActualizarNegocioTicketConfigCommand, ErrorOr<Success>>
{
    private readonly INegocioTicketConfigRepository _repository = repository;

    public async ValueTask<ErrorOr<Success>> Handle(
        ActualizarNegocioTicketConfigCommand command,
        CancellationToken cancellationToken)
    {
        var configuraciones = ConstruirConfiguraciones(command.Configuraciones);
        if (configuraciones.IsError) return configuraciones.Errors;

        var elementos = ConstruirElementos(command.ElementosEncabezado);
        if (elementos.IsError) return elementos.Errors;

        var config = await _repository.ObtenerEditableAsync(cancellationToken);

        if (config is null)
        {
            var creacion = NegocioTicketConfig.Crear();
            if (creacion.IsError) return creacion.Errors;
            config = creacion.Value;

            var actualizar = config.Actualizar(command.MensajePie, command.MostrarLogo, command.AplicaCopiaClienteNegocio, command.MostrarCodigoBarras, configuraciones.Value, elementos.Value);
            if (actualizar.IsError) return actualizar.Errors;

            await _repository.AddAsync(config, cancellationToken);
            return Result.Success;
        }

        var resultado = config.Actualizar(command.MensajePie, command.MostrarLogo, command.AplicaCopiaClienteNegocio, command.MostrarCodigoBarras, configuraciones.Value, elementos.Value);
        if (resultado.IsError) return resultado.Errors;

        await _repository.UpdateAsync(config, cancellationToken);
        return Result.Success;
    }

    private static ErrorOr<List<ConfiguracionPieDocumento>> ConstruirConfiguraciones(
        IReadOnlyList<ConfiguracionPieInput>? inputs)
    {
        if (inputs is null || inputs.Count == 0)
        {
            return new List<ConfiguracionPieDocumento>();
        }

        var errores = new List<Error>();
        var configuraciones = new List<ConfiguracionPieDocumento>();

        foreach (var input in inputs)
        {
            var lineas = ConstruirLineas(input.Lineas);
            if (lineas.IsError)
            {
                errores.AddRange(lineas.Errors);
                continue;
            }

            var creacion = ConfiguracionPieDocumento.Crear(
                input.Nombre,
                input.Destino,
                input.TiposDocumento,
                lineas.Value);

            if (creacion.IsError)
            {
                errores.AddRange(creacion.Errors);
            }
            else
            {
                configuraciones.Add(creacion.Value);
            }
        }

        if (errores.Count > 0) return errores;
        return configuraciones;
    }

    private static ErrorOr<List<ElementoEncabezado>?> ConstruirElementos(
        IReadOnlyList<ElementoEncabezadoInput>? inputs)
    {
        if (inputs is null)
        {
            return (List<ElementoEncabezado>?)null;
        }

        var errores = new List<Error>();
        var elementos = new List<ElementoEncabezado>();

        for (var i = 0; i < inputs.Count; i++)
        {
            var input = inputs[i];
            var creacion = ElementoEncabezado.Crear(input.Tipo, i, input.Visible, input.TextoLibre);
            if (creacion.IsError)
            {
                errores.AddRange(creacion.Errors);
            }
            else
            {
                elementos.Add(creacion.Value);
            }
        }

        if (errores.Count > 0) return errores;
        return elementos;
    }

    private static ErrorOr<List<LineaPieDocumento>> ConstruirLineas(IReadOnlyList<LineaPieInput>? inputs)
    {
        if (inputs is null || inputs.Count == 0)
        {
            return new List<LineaPieDocumento>();
        }

        var errores = new List<Error>();
        var lineas = new List<LineaPieDocumento>();

        for (var i = 0; i < inputs.Count; i++)
        {
            var input = inputs[i];
            var creacion = LineaPieDocumento.Crear(input.Texto, input.Alineacion, input.Negrita, i);
            if (creacion.IsError)
            {
                errores.AddRange(creacion.Errors);
            }
            else
            {
                lineas.Add(creacion.Value);
            }
        }

        if (errores.Count > 0) return errores;
        return lineas;
    }
}

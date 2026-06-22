using ErrorOr;
using Mediator;
using PuntoVenta.Application.Common;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Domain.Entities.Negocios;

namespace PuntoVenta.Application.Commands.Negocios;

public sealed record ActualizarNegocioCommand(
    Guid Id,
    string Nombre,
    string? NombreComercial,
    string? Direccion,
    string? Identificacion,
    string? Correo,
    string? Telefono = null,
    bool AplicaVendedores = false,
    bool AplicaCajas = false,
    decimal TipoCambioPredeterminado = Negocio.TipoCambioPredeterminadoDefault) : IRequest<ErrorOr<Success>>;

public sealed class ActualizarNegocioHandler(
    INegocioRepository negocioRepository) : IRequestHandler<ActualizarNegocioCommand, ErrorOr<Success>>
{
    private readonly INegocioRepository _negocioRepository = negocioRepository;

    public async ValueTask<ErrorOr<Success>> Handle(ActualizarNegocioCommand command, CancellationToken cancellationToken)
    {
        var negocio = await _negocioRepository.GetByIdAsync(command.Id, cancellationToken);

        if (negocio is null)
        {
            return NegocioErrors.NoEncontrado;
        }

        var resultado = negocio.Actualizar(
            command.Nombre,
            command.NombreComercial,
            command.Direccion,
            command.Identificacion,
            command.Correo,
            command.Telefono,
            command.AplicaVendedores,
            command.AplicaCajas,
            command.TipoCambioPredeterminado);

        if (resultado.IsError)
        {
            return resultado.Errors;
        }

        await _negocioRepository.UpdateAsync(negocio, cancellationToken);

        return Result.Success;
    }
}

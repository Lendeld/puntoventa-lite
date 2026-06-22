using ErrorOr;
using Mediator;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Domain.Entities.Vendedores;

namespace PuntoVenta.Application.Commands.Vendedores;

public sealed record CrearVendedorCommand(string Nombre, bool IsPrincipal = false) : IRequest<ErrorOr<Guid>>;

public sealed class CrearVendedorHandler(IVendedorRepository vendedorRepository) : IRequestHandler<CrearVendedorCommand, ErrorOr<Guid>>
{
    private readonly IVendedorRepository _vendedorRepository = vendedorRepository;

    public async ValueTask<ErrorOr<Guid>> Handle(CrearVendedorCommand command, CancellationToken cancellationToken)
    {
        var nombreNormalizado = Vendedor.NormalizarNombre(command.Nombre);
        if (await _vendedorRepository.ExisteNombreAsync(nombreNormalizado, cancellationToken))
        {
            return VendedorErrors.NombreYaExiste;
        }

        var principalActual = await _vendedorRepository.ObtenerPrincipalEditableAsync(cancellationToken: cancellationToken);
        var esPrincipal = command.IsPrincipal || principalActual is null;

        var resultado = Vendedor.Crear(command.Nombre, esPrincipal);
        if (resultado.IsError)
        {
            return resultado.Errors;
        }

        if (esPrincipal && principalActual is not null)
        {
            principalActual.QuitarPrincipal();
        }

        await _vendedorRepository.AddAsync(resultado.Value, cancellationToken);
        return resultado.Value.Id;
    }
}

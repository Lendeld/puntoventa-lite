using ErrorOr;
using Mediator;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Domain.Entities.Categorias;

namespace PuntoVenta.Application.Commands.Categorias;

public sealed record ActualizarEstadoCategoriaCommand(Guid Id) : IRequest<ErrorOr<bool>>;

public sealed class ActualizarEstadoCategoriaHandler(ICategoriaRepository categoriaRepository) : IRequestHandler<ActualizarEstadoCategoriaCommand, ErrorOr<bool>>
{
    private readonly ICategoriaRepository _categoriaRepository = categoriaRepository;

    public async ValueTask<ErrorOr<bool>> Handle(ActualizarEstadoCategoriaCommand command, CancellationToken cancellationToken)
    {
        var categoria = await _categoriaRepository.GetByIdAsync(command.Id, cancellationToken);

        if (categoria is null)
        {
            return CategoriaErrors.NoEncontrado;
        }

        if (categoria.Activo)
        {
            categoria.Desactivar();
        }
        else
        {
            categoria.Activar();
        }

        await _categoriaRepository.UpdateAsync(categoria, cancellationToken);

        return categoria.Activo;
    }
}

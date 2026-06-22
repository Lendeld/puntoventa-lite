using ErrorOr;
using Mediator;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Domain.Entities.Categorias;

namespace PuntoVenta.Application.Commands.Categorias;

public sealed record CrearCategoriaCommand(string Nombre, string? Descripcion = null) : IRequest<ErrorOr<Guid>>;

public sealed class CrearCategoriaHandler(ICategoriaRepository categoriaRepository) : IRequestHandler<CrearCategoriaCommand, ErrorOr<Guid>>
{
    private readonly ICategoriaRepository _categoriaRepository = categoriaRepository;

    public async ValueTask<ErrorOr<Guid>> Handle(CrearCategoriaCommand command, CancellationToken cancellationToken)
    {
        var nombreNormalizado = Categoria.NormalizarNombre(command.Nombre);
        if (await _categoriaRepository.ExisteNombreAsync(nombreNormalizado, cancellationToken))
        {
            return CategoriaErrors.NombreYaExiste;
        }

        var resultado = Categoria.Crear(command.Nombre, command.Descripcion);
        if (resultado.IsError)
        {
            return resultado.Errors;
        }

        await _categoriaRepository.AddAsync(resultado.Value, cancellationToken);
        return resultado.Value.Id;
    }
}

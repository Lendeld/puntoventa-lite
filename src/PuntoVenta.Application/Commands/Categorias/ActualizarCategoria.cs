using ErrorOr;
using Mediator;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Domain.Entities.Categorias;

namespace PuntoVenta.Application.Commands.Categorias;

public sealed record ActualizarCategoriaCommand(Guid Id, string Nombre, string? Descripcion = null, bool Activo = true) : IRequest<ErrorOr<Success>>;

public sealed class ActualizarCategoriaHandler(ICategoriaRepository categoriaRepository) : IRequestHandler<ActualizarCategoriaCommand, ErrorOr<Success>>
{
    private readonly ICategoriaRepository _categoriaRepository = categoriaRepository;

    public async ValueTask<ErrorOr<Success>> Handle(ActualizarCategoriaCommand command, CancellationToken cancellationToken)
    {
        var categoria = await _categoriaRepository.ObtenerPorIdConAuditoriaAsync(command.Id, cancellationToken);

        if (categoria is null)
        {
            return CategoriaErrors.NoEncontrado;
        }

        var nombreNormalizado = Categoria.NormalizarNombre(command.Nombre);
        if (await _categoriaRepository.ExisteNombreExcluyendoAsync(nombreNormalizado, command.Id, cancellationToken))
        {
            return CategoriaErrors.NombreYaExiste;
        }

        var resultado = categoria.Actualizar(command.Nombre, command.Descripcion);

        if (resultado.IsError)
        {
            return resultado.Errors;
        }

        if (command.Activo && !categoria.Activo)
        {
            categoria.Activar();
        }
        else if (!command.Activo && categoria.Activo)
        {
            categoria.Desactivar();
        }

        await _categoriaRepository.UpdateAsync(categoria, cancellationToken);

        return Result.Success;
    }
}

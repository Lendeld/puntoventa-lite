namespace PuntoVenta.Application.Interfaces;

public interface IPermisoCache
{
    Task<IReadOnlyList<string>> ObtenerPermisosAsync(
        Guid usuarioId,
        CancellationToken cancellationToken = default);
    void Invalidar(Guid usuarioId);
    void InvalidarTodos();
}

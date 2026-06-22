namespace PuntoVenta.Application.Interfaces;

public interface IUsuarioActual
{
    Guid UsuarioId { get; }
    string NombreUsuario { get; }
    bool RequiereCambioPassword { get; }
}

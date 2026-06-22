namespace PuntoVenta.Domain.Entities.Tokens;

public sealed class TokenRevocado
{
    private TokenRevocado() { }

    public Guid Id { get; private set; }

    public string Jti { get; private set; } = string.Empty;

    public DateTime FechaExpiracion { get; private set; }

    public DateTime FechaRevocacion { get; private set; }

    public static TokenRevocado Crear(string jti, DateTime fechaExpiracion) =>
        new()
        {
            Id = Guid.NewGuid(),
            Jti = jti,
            FechaExpiracion = fechaExpiracion,
            FechaRevocacion = DateTime.UtcNow
        };
}

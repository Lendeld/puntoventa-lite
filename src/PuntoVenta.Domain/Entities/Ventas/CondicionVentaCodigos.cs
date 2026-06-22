namespace PuntoVenta.Domain.Entities.Ventas;

public static class CondicionVentaCodigos
{
    public const string Credito = "02";
    public const string CreditoApartado = "10";

    public static bool EsCredito(string? codigo)
        => codigo is Credito or CreditoApartado;
}

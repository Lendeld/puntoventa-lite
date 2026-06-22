namespace PuntoVenta.Application.Common;

public static class Paginacion
{
    public const int TamanoMinimo = 1;
    public const int TamanoMaximo = 100;
    public const int PaginaMinima = 1;

    public static (int Pagina, int Tamano) Normalizar(int pagina, int tamano) =>
    (
        Pagina: Math.Max(pagina, PaginaMinima),
        Tamano: Math.Clamp(tamano, TamanoMinimo, TamanoMaximo)
    );
}

namespace PuntoVenta.Application.Common;

public sealed record PagedResult<T>(
    IReadOnlyList<T> Items,
    int Pagina,
    int Tamano,
    int TotalRegistros,
    int TotalPaginas)
{
    public static PagedResult<T> Crear(
        IReadOnlyList<T> items,
        int pagina,
        int tamano,
        int totalRegistros)
    {
        var totalPaginas = tamano > 0
            ? (int)Math.Ceiling((double)totalRegistros / tamano)
            : 0;

        return new PagedResult<T>(items, pagina, tamano, totalRegistros, totalPaginas);
    }
}

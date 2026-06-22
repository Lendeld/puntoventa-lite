namespace PuntoVenta.Domain.Common;

public static class CatalogosPredefinidos
{
    public static readonly IReadOnlyList<(string Codigo, string Detalle, string? Comentario)> TiposIdentificacion =
    [
        ("01", "Cédula Física", "9 dígitos sin cero inicial ni guiones."),
        ("02", "Cédula Jurídica", "10 caracteres sin guiones."),
        ("03", "DIMEX", "11 o 12 dígitos sin cero inicial ni guiones."),
        ("04", "NITE", "10 dígitos sin guiones."),
        ("05", "Extranjero No Domiciliado", "Hasta 20 caracteres alfanuméricos."),
        ("06", "No Contribuyente", "Hasta 20 caracteres alfanuméricos.")
    ];

    public static readonly IReadOnlyList<(string Codigo, string Detalle, string? Comentario)> CondicionesVenta =
    [
        ("01", "Contado", null),
        ("02", "Crédito", null),
        ("99", "Otros", null)
    ];

    public static readonly IReadOnlyList<(string Codigo, string Detalle, string? Comentario)> MediosPago =
    [
        ("01", "Efectivo", null),
        ("02", "Tarjeta", null),
        ("04", "Transferencia – depósito bancario", null),
        ("06", "SINPE MÓVIL", null),
        ("99", "Otros", null)
    ];

    public static readonly IReadOnlyList<(string Codigo, string Detalle, string? Comentario)> CodigosImpuesto =
    [
        ("01", "Impuesto al Valor Agregado", null)
    ];

    public static readonly IReadOnlyList<(string Codigo, string Detalle, decimal Porcentaje, string? Comentario)> TarifasIvaImpuesto =
    [
        ("01", "Tarifa 0% (Artículo 32, num 1, RLIVA)", 0m, "Transacciones del Art. 32, num 1 del Reglamento LIVA. Otorga derecho a crédito pleno."),
        ("02", "Tarifa reducida 1%", 1m, null),
        ("03", "Tarifa reducida 2%", 2m, null),
        ("04", "Tarifa reducida 4%", 4m, null),
        ("05", "Transitorio 0%", 0m, "Uso exclusivo para Notas de Crédito y Débito."),
        ("06", "Transitorio 4%", 4m, "Uso exclusivo para Notas de Crédito y Débito."),
        ("07", "Tarifa transitoria 8%", 8m, "Uso exclusivo para Notas de Crédito y Débito."),
        ("08", "Tarifa general 13%", 13m, null),
        ("09", "Tarifa reducida 0.5%", 0.5m, null),
        ("10", "Tarifa Exenta", 0m, "Ley 9635, Artículo 8."),
        ("11", "Tarifa 0% sin derecho a crédito", 0m, "Para transacciones de bienes o servicios no sujetos que no otorgan derecho a crédito.")
    ];
}

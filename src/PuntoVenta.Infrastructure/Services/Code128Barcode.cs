using System.Text;

namespace PuntoVenta.Infrastructure.Services;

// Genera un Code128B como SVG (barras negras) para incrustar en el PDF via
// QuestPDF .Svg(). Sin dependencias externas: la tabla de patrones es la
// estandar de Code128 (cada simbolo son 6 modulos bar/space que suman 11; el
// Stop son 7 que suman 13). Pensado para escanear el consecutivo con pistola.
internal static class Code128Barcode
{
    // Patrones de modulos por valor de simbolo (0..106). Indices 103=StartA,
    // 104=StartB, 105=StartC, 106=Stop.
    private static readonly string[] Patrones =
    [
        "212222", "222122", "222221", "121223", "121322", "131222", "122213",
        "122312", "132212", "221213", "221312", "231212", "112232", "122132",
        "122231", "113222", "123122", "123221", "223211", "221132", "221231",
        "213212", "223112", "312131", "311222", "321122", "321221", "312212",
        "322112", "322211", "212123", "212321", "232121", "111323", "131123",
        "131321", "112313", "132113", "132311", "211313", "231113", "231311",
        "112133", "112331", "132131", "113123", "113321", "133121", "313121",
        "211331", "231131", "213113", "213311", "213131", "311123", "311321",
        "331121", "312113", "312311", "332111", "314111", "221411", "431111",
        "111224", "111422", "121124", "121421", "141122", "141221", "112214",
        "112412", "122114", "122411", "142112", "142211", "241211", "221114",
        "413111", "241112", "134111", "111242", "121142", "121241", "114212",
        "124112", "124211", "411212", "421112", "421211", "212141", "214121",
        "412121", "111143", "111341", "131141", "114113", "114311", "411113",
        "411311", "113141", "114131", "311141", "411131", "211412", "211214",
        "211232", "2331112",
    ];

    private const int StartB = 104;
    private const int Stop = 106;

    public static string? GenerarSvg(string? data, int alturaModulos = 40)
    {
        var limpio = new string((data ?? string.Empty).Where(c => c >= 32 && c < 127).ToArray());
        if (limpio.Length == 0)
        {
            return null;
        }

        var simbolos = new List<int> { StartB };
        long suma = StartB;
        for (var i = 0; i < limpio.Length; i++)
        {
            var valor = limpio[i] - 32;
            simbolos.Add(valor);
            suma += (long)(i + 1) * valor;
        }
        simbolos.Add((int)(suma % 103)); // digito verificador
        simbolos.Add(Stop);

        const int quiet = 10; // zona muda recomendada a cada lado
        var bars = new StringBuilder();
        var x = quiet;
        var esBarra = true;
        foreach (var simbolo in simbolos)
        {
            foreach (var ch in Patrones[simbolo])
            {
                var ancho = ch - '0';
                if (esBarra)
                {
                    bars.Append("<rect x='").Append(x)
                        .Append("' y='0' width='").Append(ancho)
                        .Append("' height='").Append(alturaModulos)
                        .Append("' fill='black'/>");
                }
                x += ancho;
                esBarra = !esBarra;
            }
        }
        var anchoTotal = x + quiet;

        return $"<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 {anchoTotal} {alturaModulos}' "
            + $"preserveAspectRatio='none'><rect width='{anchoTotal}' height='{alturaModulos}' fill='white'/>"
            + bars + "</svg>";
    }
}

using System.Text;
using ErrorOr;
using Mediator;
using PuntoVenta.Application.DTOs.Ventas;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Domain.Entities.Ventas;

namespace PuntoVenta.Application.Commands.Ventas;

public sealed record ObtenerEventosVentaQuery(
    Guid DocumentoVentaId,
    int Skip = 0,
    int Take = 50) : IRequest<ErrorOr<DocumentoVentaEventoListaDto>>;

public sealed class ObtenerEventosVentaHandler(
    IDocumentoVentaRepository documentoRepository,
    IDocumentoVentaEventoRepository eventoRepository)
        : IRequestHandler<ObtenerEventosVentaQuery, ErrorOr<DocumentoVentaEventoListaDto>>
{
    public const int MaxTake = 200;

    private static readonly IReadOnlyDictionary<string, string> TiposEventoHumanos =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["FacturaEmitida"] = "Factura Emitida",
            ["FacturaEmitidaDesdeProforma"] = "Factura Emitida desde Proforma",
            ["ApartadoCreado"] = "Apartado Creado",
            ["ApartadoCancelado"] = "Apartado Cancelado",
            ["ApartadoConvertidoAFactura"] = "Apartado Convertido a Factura",
            ["AbonoRegistrado"] = "Abono Registrado",
            ["AbonoRevertido"] = "Abono Revertido",
            ["SaldoCancelado"] = "Saldo Cancelado",
            ["VencimientoExtendido"] = "Vencimiento Extendido",
            ["NotaCreditoEmitida"] = "Nota de Crédito Emitida",
            ["NotaCreditoAplicada"] = "Nota de Crédito Aplicada",
            ["NotaDebitoEmitida"] = "Nota de Débito Emitida",
            ["NotaDebitoAplicada"] = "Nota de Débito Aplicada"
        };

    private readonly IDocumentoVentaRepository _documentoRepository = documentoRepository;
    private readonly IDocumentoVentaEventoRepository _eventoRepository = eventoRepository;

    public async ValueTask<ErrorOr<DocumentoVentaEventoListaDto>> Handle(
        ObtenerEventosVentaQuery query,
        CancellationToken cancellationToken)
    {
        var documento = await _documentoRepository.GetByIdAsync(query.DocumentoVentaId, cancellationToken);
        if (documento is null)
        {
            return DocumentoVentaErrors.NoEncontrado;
        }

        var skip = Math.Max(0, query.Skip);
        var take = Math.Clamp(query.Take, 1, MaxTake);

        var (eventos, total) = await _eventoRepository.ObtenerPorDocumentoAsync(
            query.DocumentoVentaId,
            skip,
            take,
            cancellationToken);

        var items = eventos.Select(e => new DocumentoVentaEventoDto
        {
            Id = e.Id,
            DocumentoVentaId = e.DocumentoVentaId,
            TipoCodigo = e.TipoEventoCodigo,
            TipoNombre = ResolverTipoNombre(e.TipoEvento?.Nombre, e.TipoEventoCodigo),
            Categoria = e.TipoEvento?.Categoria ?? string.Empty,
            IconoSugerido = e.TipoEvento?.IconoSugerido,
            ColorSugerido = e.TipoEvento?.ColorSugerido,
            OcurridoEn = e.OcurridoEn,
            UsuarioId = e.UsuarioId,
            UsuarioNombre = e.UsuarioCreacion?.Nombre,
            Resumen = e.Resumen,
            CorrelacionId = e.CorrelacionId
        }).ToList();

        return new DocumentoVentaEventoListaDto
        {
            Items = items,
            Total = total,
            Skip = skip,
            Take = take
        };
    }

    private static string ResolverTipoNombre(string? nombreCatalogo, string codigo)
    {
        if (TiposEventoHumanos.TryGetValue(codigo, out var nombreHumano))
        {
            return nombreHumano;
        }

        if (!string.IsNullOrWhiteSpace(nombreCatalogo)
            && !string.Equals(nombreCatalogo.Trim(), codigo, StringComparison.Ordinal))
        {
            return nombreCatalogo.Trim();
        }

        return SepararCamelCase(codigo);
    }

    private static string SepararCamelCase(string valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
        {
            return "Evento";
        }

        var texto = valor.Trim();
        var builder = new StringBuilder(texto.Length + 8);
        for (var i = 0; i < texto.Length; i++)
        {
            var actual = texto[i];
            if (i > 0 && char.IsUpper(actual))
            {
                var anterior = texto[i - 1];
                var siguienteEsMinuscula = i + 1 < texto.Length && char.IsLower(texto[i + 1]);
                if (char.IsLower(anterior) || siguienteEsMinuscula)
                {
                    builder.Append(' ');
                }
            }
            builder.Append(actual);
        }

        return builder.ToString();
    }
}

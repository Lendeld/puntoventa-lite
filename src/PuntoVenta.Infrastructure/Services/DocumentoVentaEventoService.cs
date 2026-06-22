using System.Text.Json;
using ErrorOr;
using Microsoft.Extensions.Logging;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Domain.Entities.Ventas;

namespace PuntoVenta.Infrastructure.Services;

public sealed class DocumentoVentaEventoService(
    IUsuarioActual usuarioActual,
    IFechaActual fechaActual,
    IDocumentoVentaEventoRepository eventoRepository,
    ILogger<DocumentoVentaEventoService> logger) : IDocumentoVentaEventoService
{
    private static readonly JsonSerializerOptions PayloadOptions = new()
    {
        WriteIndented = false,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    private readonly IUsuarioActual _usuarioActual = usuarioActual;
    private readonly IFechaActual _fechaActual = fechaActual;
    private readonly IDocumentoVentaEventoRepository _eventoRepository = eventoRepository;
    private readonly ILogger<DocumentoVentaEventoService> _logger = logger;

    public async Task<ErrorOr<Success>> RegistrarAsync(
        Guid documentoVentaId,
        string tipoEventoCodigo,
        string resumen,
        object? payload = null,
        Guid? correlacionId = null,
        DateTime? ocurridoEn = null,
        CancellationToken cancellationToken = default)
    {
        if (!await _eventoRepository.ExisteTipoAsync(tipoEventoCodigo, cancellationToken))
        {
            _logger.LogWarning(
                "DocumentoVentaEvento: tipo '{Tipo}' no existe en catálogo; evento omitido para doc {DocId}.",
                tipoEventoCodigo, documentoVentaId);
            return Result.Success;
        }

        Guid? usuarioId = null;
        try { usuarioId = _usuarioActual.UsuarioId; } catch { /* contexto sin usuario */ }

        var payloadJson = payload is null ? null : JsonSerializer.Serialize(payload, PayloadOptions);
        var fecha = ocurridoEn ?? _fechaActual.AhoraUtc;

        var resultado = DocumentoVentaEvento.Crear(
            documentoVentaId,
            tipoEventoCodigo,
            resumen,
            fecha,
            usuarioId,
            payloadJson,
            correlacionId);

        if (resultado.IsError) return resultado.Errors;

        await _eventoRepository.AgregarSinPersistirAsync(resultado.Value, cancellationToken);
        return Result.Success;
    }

    public async Task<ErrorOr<Success>> RegistrarSistemaAsync(
        Guid negocioId,
        Guid documentoVentaId,
        string tipoEventoCodigo,
        string resumen,
        object? payload = null,
        Guid? correlacionId = null,
        DateTime? ocurridoEn = null,
        CancellationToken cancellationToken = default)
    {
        if (!await _eventoRepository.ExisteTipoAsync(tipoEventoCodigo, cancellationToken))
        {
            _logger.LogWarning(
                "DocumentoVentaEvento: tipo '{Tipo}' no existe en catálogo; evento omitido para doc {DocId}.",
                tipoEventoCodigo, documentoVentaId);
            return Result.Success;
        }

        var payloadJson = payload is null ? null : JsonSerializer.Serialize(payload, PayloadOptions);
        var fecha = ocurridoEn ?? _fechaActual.AhoraUtc;

        var resultado = DocumentoVentaEvento.Crear(
            documentoVentaId,
            tipoEventoCodigo,
            resumen,
            fecha,
            usuarioId: null,
            payloadJson,
            correlacionId);

        if (resultado.IsError) return resultado.Errors;

        await _eventoRepository.AgregarSinPersistirAsync(resultado.Value, cancellationToken);
        return Result.Success;
    }
}

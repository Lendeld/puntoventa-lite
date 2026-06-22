using Microsoft.EntityFrameworkCore;
using PuntoVenta.Application.DTOs.Ventas;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Application.Queries.Dashboard;
using PuntoVenta.Domain.Common;
using PuntoVenta.Domain.Entities.Ventas;

namespace PuntoVenta.Infrastructure.Persistence.Repositories;

public sealed class DocumentoVentaRepository(ApplicationDbContext context) : Repository<DocumentoVenta>(context), IDocumentoVentaRepository
{
    public async Task<IReadOnlyList<VentaRangoProyeccionDto>> ObtenerVentasRangoProyectadoAsync(
        DateTime desdeUtc,
        DateTime hastaUtc,
        string? consecutivo,
        int maxFilas,
        CancellationToken cancellationToken = default)
    {
        var tiposIncluidos = new[]
        {
            TipoDocumentoVenta.Factura,
            TipoDocumentoVenta.NotaCredito,
            TipoDocumentoVenta.NotaDebito
        };

        var hastaExclusivoUtc = hastaUtc.AddDays(1);

        var query = DbSet.AsNoTracking()
            .Where(d => tiposIncluidos.Contains(d.TipoDocumento)
                && d.Estado == EstadoDocumentoVenta.Emitido
                && d.FechaDocumento >= desdeUtc
                && d.FechaDocumento < hastaExclusivoUtc);

        if (!string.IsNullOrWhiteSpace(consecutivo))
        {
            var term = consecutivo.Trim().ToLower();
            query = query.Where(d => d.Consecutivo != null && d.Consecutivo.ToLower().Contains(term));
        }

        var mediosPorDocumento = await query
            .SelectMany(d => d.Pagos)
            .Select(p => new { p.DocumentoVentaId, p.MedioPagoDetalleSnapshot })
            .Distinct()
            .ToListAsync(cancellationToken);

        var mediosPorDoc = mediosPorDocumento
            .GroupBy(m => m.DocumentoVentaId)
            .ToDictionary(
                g => g.Key,
                g => (IReadOnlyList<string>)[.. g.Select(m => m.MedioPagoDetalleSnapshot)]);

        var filas = await query
            .SelectMany(d => d.Lineas, (d, l) => new VentaRangoProyeccionDto
            {
                DocumentoId = d.Id,
                Consecutivo = d.Consecutivo ?? string.Empty,
                FechaDocumento = d.FechaDocumento,
                TipoDocumento = (TipoDocumentoVentaProyeccion)(int)d.TipoDocumento,
                MonedaCodigo = d.MonedaCodigo,
                TipoCambio = d.TipoCambio,
                CondicionVentaDetalle = d.CondicionVentaDetalleSnapshot,
                ClienteIdentificacion = d.Cliente != null && d.Cliente.Identificacion != null ? d.Cliente.Identificacion : string.Empty,
                ClienteNombre = d.Cliente != null ? d.Cliente.Nombre : string.Empty,
                NumeroLinea = 0,
                ProductoCodigo = l.Codigo,
                ProductoDetalle = l.Descripcion,
                Cantidad = l.Cantidad,
                PrecioUnitario = l.PrecioUnitario,
                MontoDescuento = l.MontoDescuento,
                Subtotal = l.Subtotal,
                TarifaPorcentaje = l.PorcentajeImpuesto,
                MontoImpuesto = l.MontoImpuesto,
                TotalLinea = l.TotalLinea
            })
            .OrderBy(f => f.FechaDocumento)
            .ThenBy(f => f.Consecutivo)
            .Take(maxFilas + 1)
            .ToListAsync(cancellationToken);

        var contadorPorDoc = new Dictionary<Guid, int>();
        var resultado = new List<VentaRangoProyeccionDto>(filas.Count);
        foreach (var fila in filas)
        {
            contadorPorDoc.TryGetValue(fila.DocumentoId, out var actual);
            actual++;
            contadorPorDoc[fila.DocumentoId] = actual;
            var medios = mediosPorDoc.TryGetValue(fila.DocumentoId, out var m) ? m : [];
            resultado.Add(fila with { NumeroLinea = actual, MediosPago = medios });
        }

        return resultado;
    }

    private IQueryable<DocumentoVenta> VentasEmitidas(DateTime desdeUtc, DateTime hastaUtc)
        => DbSet.AsNoTracking()
            .Where(d => d.TipoDocumento == TipoDocumentoVenta.Factura
                && d.Estado == EstadoDocumentoVenta.Emitido
                && d.FechaDocumento >= desdeUtc
                && d.FechaDocumento < hastaUtc);

    public async Task<VentasPeriodoDto> ObtenerResumenVentasAsync(
        DateTime desdeUtc,
        DateTime hastaUtc,
        CancellationToken cancellationToken = default)
    {
        // Agregación global directa (Count + Sum) en vez de GroupBy(_ => 1):
        // SQLite no traduce ese patrón de grupo-constante + FirstOrDefault.
        var query = VentasEmitidas(desdeUtc, hastaUtc);
        var cantidad = await query.CountAsync(cancellationToken);
        var total = cantidad == 0
            ? 0m
            : await query.SumAsync(d => d.TotalComprobante, cancellationToken);

        return new VentasPeriodoDto(Math.Round(total, 2), cantidad);
    }

    public async Task<IReadOnlyList<PuntoTendenciaDto>> ObtenerTendenciaVentasAsync(
        DateTime desdeUtc,
        DateTime hastaUtc,
        CancellationToken cancellationToken = default)
    {
        // SQLite no traduce DateTime.AddHours().Date dentro de un GroupBy
        // server-side (Npgsql sí lo hacía). Traemos fecha+total del rango y
        // agrupamos por día contable CR (UTC-6) en memoria — el rango del
        // dashboard es acotado, así que el costo es despreciable.
        var datos = await VentasEmitidas(desdeUtc, hastaUtc)
            .Select(d => new { d.FechaDocumento, d.TotalComprobante })
            .ToListAsync(cancellationToken);

        return datos
            .GroupBy(d => DateOnly.FromDateTime(d.FechaDocumento.AddHours(-6)))
            .Select(g => new PuntoTendenciaDto(g.Key, g.Sum(d => Math.Round(d.TotalComprobante, 2))))
            .OrderBy(p => p.Fecha)
            .ToList();
    }

    public async Task<IReadOnlyList<MetodoPagoDto>> ObtenerVentasPorMetodoPagoAsync(
        DateTime desdeUtc,
        DateTime hastaUtc,
        CancellationToken cancellationToken = default)
    {
        var datos = await VentasEmitidas(desdeUtc, hastaUtc)
            .SelectMany(d => d.Pagos)
            .Where(p => !p.Anulado)
            .GroupBy(p => new { p.MedioPagoCodigo, p.MedioPagoDetalleSnapshot })
            .Select(g => new
            {
                g.Key.MedioPagoCodigo,
                g.Key.MedioPagoDetalleSnapshot,
                Total = g.Sum(p => p.MontoAplicadoDocumento),
            })
            .ToListAsync(cancellationToken);

        return datos
            .OrderByDescending(x => x.Total)
            .Select(x => new MetodoPagoDto(x.MedioPagoCodigo, x.MedioPagoDetalleSnapshot, x.Total))
            .ToList();
    }

    public async Task<IReadOnlyList<TopProductoDto>> ObtenerTopProductosAsync(
        DateTime desdeUtc,
        DateTime hastaUtc,
        int top,
        CancellationToken cancellationToken = default)
    {
        // SQLite no traduce Math.Round sobre decimal (lo guarda como TEXT);
        // sumamos el monto ya persistido (redondeado a 2 dec) y redondeamos
        // el total en memoria.
        var datos = await VentasEmitidas(desdeUtc, hastaUtc)
            .SelectMany(d => d.Lineas)
            .GroupBy(l => l.Descripcion)
            .Select(g => new
            {
                Nombre = g.Key,
                Cantidad = g.Sum(l => l.Cantidad),
                Total = g.Sum(l => l.TotalLinea),
            })
            .OrderByDescending(x => x.Total)
            .Take(top)
            .ToListAsync(cancellationToken);

        return datos
            .Select(x => new TopProductoDto(x.Nombre, x.Cantidad, Math.Round(x.Total, 2)))
            .ToList();
    }

    public async Task<CuentasPorCobrarDto> ObtenerCuentasPorCobrarVencidasAsync(
        DateTime hastaUtc,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet.AsNoTracking()
            .Where(d => d.TipoDocumento == TipoDocumentoVenta.Factura
                && (d.CondicionVentaCodigo == CondicionVentaCodigos.Credito
                    || d.CondicionVentaCodigo == CondicionVentaCodigos.CreditoApartado)
                && d.SaldoPendiente > 0m
                && d.FechaVencimiento != null
                && d.FechaVencimiento < hastaUtc);

        // Agregación global directa (Count + Sum); SQLite no traduce el
        // GroupBy(_ => 1) + FirstOrDefault.
        var cantidad = await query.CountAsync(cancellationToken);
        var total = cantidad == 0
            ? 0m
            : await query.SumAsync(d => d.SaldoPendiente, cancellationToken);

        return new CuentasPorCobrarDto(total, cantidad);
    }

    public override async Task UpdateAsync(DocumentoVenta entity, CancellationToken cancellationToken = default)
    {
        if (Context.Entry(entity).State == EntityState.Detached)
        {
            DbSet.Update(entity);
        }

        foreach (var pago in entity.Pagos)
        {
            if (Context.Entry(pago).State == EntityState.Detached)
            {
                Context.DocumentosVentaPagos.Add(pago);
            }
        }

        await Context.SaveChangesAsync(cancellationToken);
    }

    public async Task RegistrarAbonoAsync(
        DocumentoVenta documento,
        DocumentoVentaPago pagoNuevo,
        CancellationToken cancellationToken = default)
    {
        DbSet.Attach(documento);
        Context.Entry(documento).Property(d => d.TotalPagado).IsModified = true;
        Context.Entry(documento).Property(d => d.SaldoPendiente).IsModified = true;
        Context.Entry(documento).Property(d => d.FechaCancelacion).IsModified = true;
        Context.Entry(pagoNuevo).State = EntityState.Added;

        await Context.SaveChangesAsync(cancellationToken);
    }

    public async Task AnularAbonoAsync(
        DocumentoVenta documento,
        DocumentoVentaPago pagoAnulado,
        CancellationToken cancellationToken = default)
    {
        DbSet.Attach(documento);
        Context.Entry(documento).Property(d => d.TotalPagado).IsModified = true;
        Context.Entry(documento).Property(d => d.SaldoPendiente).IsModified = true;
        Context.Entry(documento).Property(d => d.FechaCancelacion).IsModified = true;

        Context.DocumentosVentaPagos.Attach(pagoAnulado);
        Context.Entry(pagoAnulado).Property(p => p.Anulado).IsModified = true;
        Context.Entry(pagoAnulado).Property(p => p.FechaAnulacionUtc).IsModified = true;
        Context.Entry(pagoAnulado).Property(p => p.UsuarioAnulaId).IsModified = true;
        Context.Entry(pagoAnulado).Property(p => p.MotivoAnulacion).IsModified = true;

        await Context.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> ObtenerMaxNumeroConsecutivoAsync(
        Guid negocioId, Guid cajaId, TipoDocumentoVenta tipoDocumento, CancellationToken cancellationToken = default)
        => await DbSet.AsNoTracking()
            .Where(d => d.CajaId == cajaId && d.TipoDocumento == tipoDocumento && d.NumeroConsecutivo != null)
            .MaxAsync(d => (int?)d.NumeroConsecutivo, cancellationToken) ?? 0;

    public async Task<(decimal SaldoVigente, decimal SaldoVencido, int FacturasVencidas, int DiasAtrasoMax)> ObtenerSaldosCreditoClienteAsync(
        Guid clienteId,
        DateTime ahora,
        CancellationToken cancellationToken = default)
    {
        var hoy = ahora.Date;
        var facturas = await DbSet.AsNoTracking()
            .Where(d => d.TipoDocumento == TipoDocumentoVenta.Factura
                && d.Estado == EstadoDocumentoVenta.Emitido
                && d.ClienteId == clienteId
                && (d.CondicionVentaCodigo == CondicionVentaCodigos.Credito || d.CondicionVentaCodigo == CondicionVentaCodigos.CreditoApartado)
                && d.SaldoPendiente > 0m)
            .Select(d => new { d.Id, d.SaldoPendiente, d.FechaVencimiento })
            .ToListAsync(cancellationToken);

        var saldosNetos = await ObtenerSaldosNetosFacturasAsync(
            facturas.Select(f => (f.Id, f.SaldoPendiente)),
            cancellationToken);

        decimal vigente = 0m, vencido = 0m;
        int facturasVencidas = 0, diasMax = 0;

        foreach (var f in facturas)
        {
            if (!saldosNetos.TryGetValue(f.Id, out var saldoNeto) || saldoNeto <= 0m)
            {
                continue;
            }

            var estaVencida = f.FechaVencimiento.HasValue && f.FechaVencimiento.Value.Date < hoy;
            if (estaVencida)
            {
                vencido += saldoNeto;
                facturasVencidas++;
                var dias = (int)(hoy - f.FechaVencimiento!.Value.Date).TotalDays;
                if (dias > diasMax) diasMax = dias;
            }
            else
            {
                vigente += saldoNeto;
            }
        }

        return (vigente, vencido, facturasVencidas, diasMax);
    }

    public async Task<IReadOnlyList<DocumentoVenta>> ObtenerFacturasCreditoClienteAsync(
        Guid clienteId,
        bool? soloConSaldo,
        CancellationToken cancellationToken = default)
    {
        var items = await DbSet.AsNoTracking()
            .Where(d => d.TipoDocumento == TipoDocumentoVenta.Factura
                && d.ClienteId == clienteId)
            .OrderByDescending(d => d.FechaDocumento)
            .ToListAsync(cancellationToken);

        var saldosNetos = await ObtenerSaldosNetosFacturasAsync(
            items.Select(d => (d.Id, d.SaldoPendiente)),
            cancellationToken);

        if (soloConSaldo == true)
        {
            items = [.. items.Where(d => saldosNetos.TryGetValue(d.Id, out var saldo) && saldo > 0m)];
        }

        return items;
    }

    public async Task<(IReadOnlyList<DocumentoVenta> Items, int Total)> ObtenerListaCreditoAsync(
        int pagina,
        int tamano,
        string? filtroDinamico,
        Guid? clienteId,
        bool? soloVencidas,
        DateTime ahora,
        CancellationToken cancellationToken = default)
    {
        var hoy = ahora.Date;
        var query = DbSet.AsNoTracking()
            .Include(d => d.Cliente)
            .Where(d => d.TipoDocumento == TipoDocumentoVenta.Factura
                && d.Estado == EstadoDocumentoVenta.Emitido
                && (d.CondicionVentaCodigo == CondicionVentaCodigos.Credito || d.CondicionVentaCodigo == CondicionVentaCodigos.CreditoApartado)
                && d.SaldoPendiente > 0m);

        if (clienteId.HasValue)
            query = query.Where(d => d.ClienteId == clienteId.Value);

        if (soloVencidas == true)
            query = query.Where(d => d.FechaVencimiento != null && d.FechaVencimiento < hoy);

        if (!string.IsNullOrWhiteSpace(filtroDinamico))
        {
            var term = filtroDinamico.Trim().ToLower();
            query = query.Where(d =>
                (d.Consecutivo != null && d.Consecutivo.ToLower().Contains(term)) ||
                (d.Cliente != null && d.Cliente.Nombre.ToLower().Contains(term)) ||
                (d.Cliente != null && d.Cliente.Identificacion != null && d.Cliente.Identificacion.ToLower().Contains(term)));
        }

        var candidatos = await query
            .OrderBy(d => d.FechaVencimiento)
            .ThenByDescending(d => d.FechaDocumento)
            .ToListAsync(cancellationToken);

        var saldosNetos = await ObtenerSaldosNetosFacturasAsync(
            candidatos.Select(d => (d.Id, d.SaldoPendiente)),
            cancellationToken);

        var filtrados = candidatos
            .Where(d => saldosNetos.TryGetValue(d.Id, out var saldoNeto) && saldoNeto > 0m)
            .ToList();

        var total = filtrados.Count;
        var items = filtrados
            .Skip((pagina - 1) * tamano)
            .Take(tamano)
            .ToList();

        return (items, total);
    }

    public async Task<DocumentoVenta?> ObtenerDetalleAsync(Guid id, CancellationToken cancellationToken = default)
        => await DbSet.AsNoTracking()
            .Include(d => d.Cliente)
            .Include(d => d.Vendedor)
            .Include(d => d.Caja)
            .Include(d => d.DocumentoOrigen)
            .Include(d => d.Lineas)
            .Include(d => d.Pagos)
                .ThenInclude(p => p.UsuarioRegistro)
            .Include(d => d.Pagos)
                .ThenInclude(p => p.UsuarioAnula)
            .Include(d => d.Referencias)
            .Include(d => d.UsuarioCreacion)
            .AsSplitQuery()
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);

    public async Task<DocumentoVenta?> ObtenerDetalleParaSistemaAsync(Guid negocioId, Guid id, CancellationToken cancellationToken = default)
        => await DbSet.AsNoTracking()
            .Include(d => d.Cliente)
            .Include(d => d.Vendedor)
            .Include(d => d.Lineas)
            .Include(d => d.Pagos)
                .ThenInclude(p => p.UsuarioRegistro)
            .Include(d => d.Pagos)
                .ThenInclude(p => p.UsuarioAnula)
            .Include(d => d.Referencias)
            .AsSplitQuery()
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);

    public async Task<IReadOnlyList<DocumentoVenta>> ObtenerDocumentosGeneradosAsync(Guid documentoOrigenId, CancellationToken cancellationToken = default)
        => await DbSet.AsNoTracking()
            .Where(d => d.DocumentoOrigenId == documentoOrigenId)
            .OrderByDescending(d => d.FechaDocumento)
            .ThenByDescending(d => d.FechaCreacion)
            .ToListAsync(cancellationToken);

    public async Task<decimal> ObtenerMontoNotasEmitidasAsync(Guid documentoOrigenId, TipoDocumentoVenta tipoNota, CancellationToken cancellationToken = default)
        => await DbSet.AsNoTracking()
            .Where(d => d.DocumentoOrigenId == documentoOrigenId
                && d.TipoDocumento == tipoNota
                && d.Estado == EstadoDocumentoVenta.Emitido)
            .SumAsync(d => (decimal?)d.TotalComprobante, cancellationToken) ?? 0m;

    public async Task<IReadOnlyDictionary<Guid, decimal>> ObtenerMontoNotasCreditoPorDocumentosAsync(
        IReadOnlyCollection<Guid> documentoOrigenIds,
        CancellationToken cancellationToken = default)
    {
        if (documentoOrigenIds.Count == 0) return new Dictionary<Guid, decimal>();

        var totales = await DbSet.AsNoTracking()
            .Where(d => d.TipoDocumento == TipoDocumentoVenta.NotaCredito
                && d.Estado == EstadoDocumentoVenta.Emitido
                && d.DocumentoOrigenId != null
                && documentoOrigenIds.Contains(d.DocumentoOrigenId.Value))
            .GroupBy(d => d.DocumentoOrigenId!.Value)
            .Select(g => new { OrigenId = g.Key, Total = g.Sum(d => d.TotalComprobante) })
            .ToListAsync(cancellationToken);
        return totales.ToDictionary(t => t.OrigenId, t => t.Total);
    }

    public async Task<IReadOnlyDictionary<Guid, decimal>> ObtenerMontoNotasDebitoPorDocumentosAsync(
        IReadOnlyCollection<Guid> documentoOrigenIds,
        CancellationToken cancellationToken = default)
    {
        if (documentoOrigenIds.Count == 0) return new Dictionary<Guid, decimal>();

        var notasDebito = await DbSet.AsNoTracking()
            .Where(d => d.TipoDocumento == TipoDocumentoVenta.NotaDebito
                && d.Estado == EstadoDocumentoVenta.Emitido
                && d.DocumentoOrigenId != null
                && documentoOrigenIds.Contains(d.DocumentoOrigenId.Value))
            .Select(d => new { d.Id, OrigenId = d.DocumentoOrigenId!.Value, d.TotalComprobante })
            .ToListAsync(cancellationToken);

        if (notasDebito.Count == 0) return new Dictionary<Guid, decimal>();

        var notaDebitoIds = notasDebito.Select(n => n.Id).ToList();
        var creditosPorNd = await DbSet.AsNoTracking()
            .Where(d => d.TipoDocumento == TipoDocumentoVenta.NotaCredito
                && d.Estado == EstadoDocumentoVenta.Emitido
                && d.DocumentoOrigenId != null
                && notaDebitoIds.Contains(d.DocumentoOrigenId.Value))
            .GroupBy(d => d.DocumentoOrigenId!.Value)
            .Select(g => new { NdId = g.Key, Total = g.Sum(d => d.TotalComprobante) })
            .ToListAsync(cancellationToken);
        var creditos = creditosPorNd.ToDictionary(c => c.NdId, c => c.Total);

        return notasDebito
            .GroupBy(n => n.OrigenId)
            .ToDictionary(
                g => g.Key,
                g => g.Sum(n => Math.Max(
                    0m,
                    n.TotalComprobante - (creditos.TryGetValue(n.Id, out var nc) ? nc : 0m))));
    }

    public async Task<IReadOnlyList<string>> ObtenerNotasDebitoVigentesAsync(
        Guid documentoOrigenId,
        CancellationToken cancellationToken = default)
    {
        var notasDebito = await DbSet.AsNoTracking()
            .Where(d => d.DocumentoOrigenId == documentoOrigenId
                && d.TipoDocumento == TipoDocumentoVenta.NotaDebito
                && d.Estado == EstadoDocumentoVenta.Emitido)
            .Select(d => new { d.Id, d.Consecutivo, d.TotalComprobante })
            .ToListAsync(cancellationToken);

        if (notasDebito.Count == 0) return [];

        var notaDebitoIds = notasDebito.Select(n => n.Id).ToList();
        var creditosPorNd = await DbSet.AsNoTracking()
            .Where(d => d.TipoDocumento == TipoDocumentoVenta.NotaCredito
                && d.Estado == EstadoDocumentoVenta.Emitido
                && d.DocumentoOrigenId != null
                && notaDebitoIds.Contains(d.DocumentoOrigenId.Value))
            .GroupBy(d => d.DocumentoOrigenId!.Value)
            .Select(g => new { NdId = g.Key, Total = g.Sum(d => d.TotalComprobante) })
            .ToListAsync(cancellationToken);
        var creditos = creditosPorNd.ToDictionary(c => c.NdId, c => c.Total);

        return [.. notasDebito
            .Where(n => n.TotalComprobante - (creditos.TryGetValue(n.Id, out var nc) ? nc : 0m) > 0.005m)
            .Select(n => n.Consecutivo ?? string.Empty)
            .Where(c => c.Length > 0)];
    }

    public async Task<IReadOnlyDictionary<Guid, ConsumoNotaCreditoPorProductoDto>> ObtenerConsumoNotasCreditoPorProductoAsync(
        Guid documentoOrigenId,
        CancellationToken cancellationToken = default)
    {
        var consumos = await DbSet.AsNoTracking()
            .Where(d => d.DocumentoOrigenId == documentoOrigenId
                && d.TipoDocumento == TipoDocumentoVenta.NotaCredito
                && d.Estado == EstadoDocumentoVenta.Emitido)
            .SelectMany(d => d.Lineas)
            .Where(l => l.ProductoId != null)
            .GroupBy(l => l.ProductoId!.Value)
            .Select(g => new
            {
                ProductoId = g.Key,
                CantidadDevuelta = g.Where(l => l.DevuelveInventario)
                    .Sum(l => (decimal?)l.Cantidad) ?? 0m,
                Subtotal = g.Sum(l => l.Subtotal),
            })
            .ToListAsync(cancellationToken);

        return consumos.ToDictionary(
            c => c.ProductoId,
            c => new ConsumoNotaCreditoPorProductoDto(c.CantidadDevuelta, c.Subtotal));
    }

    public async Task<DocumentoVenta?> ObtenerEditableAsync(Guid id, CancellationToken cancellationToken = default)
        => await DbSet.AsTracking()
            .Include(d => d.Cliente)
            .Include(d => d.Vendedor)
            .Include(d => d.Lineas)
            .Include(d => d.Pagos)
                .ThenInclude(p => p.UsuarioRegistro)
            .Include(d => d.Pagos)
                .ThenInclude(p => p.UsuarioAnula)
            .Include(d => d.Referencias)
            .AsSplitQuery()
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);

    public async Task<IReadOnlyList<DocumentoVenta>> ObtenerApartadosReservadosVencidosAsync(
        DateTime ahora,
        CancellationToken cancellationToken = default)
        => await DbSet.AsTracking()
            .Where(d => d.TipoDocumento == TipoDocumentoVenta.Apartado
                && d.Estado == EstadoDocumentoVenta.Reservado
                && d.FechaVencimiento != null
                && d.FechaVencimiento < ahora.Date)
            .ToListAsync(cancellationToken);

    public async Task<(IReadOnlyList<DocumentoVenta> Items, int Total)> ObtenerListaPaginadoAsync(
        int pagina,
        int tamano,
        string? filtroDinamico,
        TipoDocumentoVenta? tipoDocumento,
        EstadoDocumentoVenta? estado,
        Guid? clienteId,
        DateTime? fechaDesde,
        DateTime? fechaHasta,
        CancellationToken cancellationToken = default)
    {
        IQueryable<DocumentoVenta> query = DbSet.AsNoTracking()
            .Include(d => d.UsuarioCreacion)
            .Include(d => d.Cliente)
            .Include(d => d.Vendedor);

        if (tipoDocumento.HasValue)
            query = query.Where(d => d.TipoDocumento == tipoDocumento.Value);

        if (estado.HasValue)
            query = query.Where(d => d.Estado == estado.Value);

        if (clienteId.HasValue)
            query = query.Where(d => d.ClienteId == clienteId.Value);

        if (fechaDesde.HasValue)
            query = query.Where(d => d.FechaDocumento >= fechaDesde.Value);

        if (fechaHasta.HasValue)
            query = query.Where(d => d.FechaDocumento <= fechaHasta.Value);

        if (!string.IsNullOrWhiteSpace(filtroDinamico))
        {
            var term = filtroDinamico.Trim().ToLower();
            query = query.Where(d =>
                (d.Consecutivo != null && d.Consecutivo.ToLower().Contains(term)) ||
                (d.Cliente != null && d.Cliente.Nombre.ToLower().Contains(term)) ||
                (d.Cliente != null && d.Cliente.Identificacion != null && d.Cliente.Identificacion.ToLower().Contains(term)) ||
                d.CondicionVentaDetalleSnapshot.ToLower().Contains(term));
        }

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(d => d.FechaDocumento)
            .ThenByDescending(d => d.FechaCreacion)
            .Skip((pagina - 1) * tamano)
            .Take(tamano)
            .ToListAsync(cancellationToken);

        return (items, total);
    }

    private async Task<Dictionary<Guid, decimal>> ObtenerSaldosNetosFacturasAsync(
        IEnumerable<(Guid Id, decimal SaldoPendiente)> facturas,
        CancellationToken cancellationToken)
    {
        var baseFacturas = facturas.ToList();
        if (baseFacturas.Count == 0)
        {
            return [];
        }

        var ids = baseFacturas.Select(f => f.Id).ToList();
        var notasCredito = await ObtenerMontoNotasCreditoPorDocumentosAsync(ids, cancellationToken);
        var notasDebito = await ObtenerMontoNotasDebitoPorDocumentosAsync(ids, cancellationToken);

        return baseFacturas.ToDictionary(
            f => f.Id,
            f =>
            {
                var saldo = f.SaldoPendiente
                    - (notasCredito.TryGetValue(f.Id, out var nc) ? nc : 0m)
                    + (notasDebito.TryGetValue(f.Id, out var nd) ? nd : 0m);
                return Math.Max(0m, Dinero.RedondearPago(saldo));
            });
    }

    public async Task<ReporteMovimientosDineroResultadoDto> ObtenerReporteMovimientosDineroAsync(
        DateTime fechaDesdeUtc,
        DateTime fechaHastaUtc,
        Guid? cajaId,
        CancellationToken cancellationToken = default)
    {
        var pagos = await Context.DocumentosVentaPagos.AsNoTracking()
            .Where(p => p.DocumentoVenta != null
                && (p.DocumentoVenta.Estado == EstadoDocumentoVenta.Emitido
                    || p.DocumentoVenta.TipoDocumento == TipoDocumentoVenta.Apartado)
                && (
                    (p.FechaRegistroUtc >= fechaDesdeUtc && p.FechaRegistroUtc <= fechaHastaUtc)
                    || (p.Anulado
                        && p.FechaAnulacionUtc != null
                        && p.FechaAnulacionUtc >= fechaDesdeUtc
                        && p.FechaAnulacionUtc <= fechaHastaUtc)))
            .Where(p => !cajaId.HasValue || p.DocumentoVenta!.CajaId == cajaId.Value)
            .Select(p => new MovimientoDineroPagoProjection(
                p.Id,
                p.DocumentoVentaId,
                p.DocumentoVenta!.Consecutivo,
                p.DocumentoVenta.TipoDocumento,
                p.DocumentoVenta.CondicionVentaCodigo,
                p.DocumentoVenta.DocumentoOrigen != null ? p.DocumentoVenta.DocumentoOrigen.TipoDocumento : null,
                p.DocumentoVenta.CajaId,
                p.DocumentoVenta.Caja != null ? p.DocumentoVenta.Caja.Codigo : null,
                p.DocumentoVenta.Caja != null ? p.DocumentoVenta.Caja.Nombre : null,
                p.DocumentoVenta.ClienteId,
                p.DocumentoVenta.Cliente != null ? p.DocumentoVenta.Cliente.Nombre : null,
                p.DocumentoVenta.Cliente != null ? p.DocumentoVenta.Cliente.Identificacion : null,
                p.MonedaCodigo,
                p.MedioPagoCodigo,
                p.MedioPagoDetalleSnapshot,
                p.Referencia,
                p.NumeroAbono,
                p.MontoAplicadoDocumento,
                p.FechaPago,
                p.FechaRegistroUtc,
                p.UsuarioRegistroId,
                p.UsuarioRegistro != null ? p.UsuarioRegistro.Nombre : null,
                p.Anulado,
                p.FechaAnulacionUtc,
                p.UsuarioAnulaId,
                p.UsuarioAnula != null ? p.UsuarioAnula.Nombre : null,
                p.MotivoAnulacion))
            .ToListAsync(cancellationToken);

        var movimientos = new List<MovimientoDineroFilaDto>();

        foreach (var pago in pagos)
        {
            var tipoEntrada = ResolverTipoMovimientoEntrada(pago);
            if (tipoEntrada is not null
                && pago.FechaRegistroUtc >= fechaDesdeUtc
                && pago.FechaRegistroUtc <= fechaHastaUtc)
            {
                movimientos.Add(new MovimientoDineroFilaDto(
                    pago.PagoId,
                    pago.DocumentoId,
                    pago.DocumentoConsecutivo,
                    tipoEntrada,
                    TipoDocumentoDetalle(pago.DocumentoTipo),
                    pago.FechaRegistroUtc,
                    pago.FechaPago,
                    pago.FechaRegistroUtc,
                    pago.FechaAnulacionUtc,
                    pago.CajaId,
                    pago.CajaCodigo,
                    pago.CajaNombre,
                    pago.ClienteId,
                    pago.ClienteNombre,
                    pago.ClienteIdentificacion,
                    pago.UsuarioRegistroId,
                    pago.UsuarioRegistroNombre,
                    pago.MedioPagoCodigo,
                    pago.MedioPagoDetalle,
                    pago.Referencia,
                    pago.MonedaCodigo,
                    pago.MontoAplicadoDocumento,
                    pago.NumeroAbono,
                    null,
                    null,
                    null,
                    null,
                    null));
            }

            if (pago.Anulado
                && pago.FechaAnulacionUtc.HasValue
                && pago.FechaAnulacionUtc.Value >= fechaDesdeUtc
                && pago.FechaAnulacionUtc.Value <= fechaHastaUtc)
            {
                movimientos.Add(new MovimientoDineroFilaDto(
                    pago.PagoId,
                    pago.DocumentoId,
                    pago.DocumentoConsecutivo,
                    "AnulacionAbono",
                    TipoDocumentoDetalle(pago.DocumentoTipo),
                    pago.FechaAnulacionUtc.Value,
                    pago.FechaPago,
                    pago.FechaRegistroUtc,
                    pago.FechaAnulacionUtc,
                    pago.CajaId,
                    pago.CajaCodigo,
                    pago.CajaNombre,
                    pago.ClienteId,
                    pago.ClienteNombre,
                    pago.ClienteIdentificacion,
                    pago.UsuarioAnulaId,
                    pago.UsuarioAnulaNombre,
                    pago.MedioPagoCodigo,
                    pago.MedioPagoDetalle,
                    pago.Referencia,
                    pago.MonedaCodigo,
                    -pago.MontoAplicadoDocumento,
                    pago.NumeroAbono,
                    pago.MotivoAnulacion,
                    null,
                    null,
                    null,
                    null));
            }
        }

        var documentoIds = movimientos.Select(m => m.DocumentoId).Distinct().ToList();
        var eventos = documentoIds.Count == 0
            ? []
            : await Context.DocumentosVentaEventos.AsNoTracking()
                .Where(e => documentoIds.Contains(e.DocumentoVentaId))
                .OrderByDescending(e => e.OcurridoEn)
                .Select(e => new DocumentoVentaEventoMovimientoDto(
                    e.Id,
                    e.DocumentoVentaId,
                    e.TipoEventoCodigo,
                    e.Resumen,
                    e.OcurridoEn,
                    e.Payload))
                .ToListAsync(cancellationToken);

        var eventosPorDocumento = eventos
            .GroupBy(e => e.DocumentoVentaId)
            .ToDictionary(g => g.Key, g => (IReadOnlyList<DocumentoVentaEventoMovimientoDto>)g.ToList());

        var movimientosConEvento = movimientos
            .Select(m =>
            {
                var evento = eventosPorDocumento.TryGetValue(m.DocumentoId, out var eventosDocumento)
                    ? ResolverEventoMovimiento(m, eventosDocumento)
                    : null;

                return m with
                {
                    EventoId = evento?.Id,
                    EventoTipoCodigo = evento?.TipoEventoCodigo,
                    EventoResumen = evento?.Resumen,
                    EventoOcurridoEn = evento?.OcurridoEn
                };
            })
            .OrderBy(m => m.FechaMovimientoUtc)
            .ThenBy(m => m.DocumentoConsecutivo)
            .ThenBy(m => m.PagoId)
            .ToList();

        var totalesPorMedio = movimientosConEvento
            .GroupBy(m => new { m.MedioPagoCodigo, m.MedioPagoDetalle })
            .Select(g =>
            {
                var entradas = g.Where(x => x.Monto > 0m).Sum(x => x.Monto);
                var salidas = g.Where(x => x.Monto < 0m).Sum(x => Math.Abs(x.Monto));
                return new MovimientoDineroMedioDto(
                    g.Key.MedioPagoCodigo,
                    g.Key.MedioPagoDetalle,
                    entradas,
                    salidas,
                    entradas - salidas);
            })
            .OrderBy(x => x.Detalle)
            .ToList();

        var totalEntradas = movimientosConEvento.Where(m => m.Monto > 0m).Sum(m => m.Monto);
        var totalSalidas = movimientosConEvento.Where(m => m.Monto < 0m).Sum(m => Math.Abs(m.Monto));

        return new ReporteMovimientosDineroResultadoDto(
            movimientosConEvento,
            totalesPorMedio,
            totalEntradas,
            totalSalidas,
            totalEntradas - totalSalidas);
    }

    private static string? ResolverTipoMovimientoEntrada(MovimientoDineroPagoProjection pago)
    {
        return pago.DocumentoTipo switch
        {
            TipoDocumentoVenta.Factura when CondicionVentaCodigos.EsCredito(pago.CondicionVentaCodigo)
                => "AbonoFacturaCredito",
            TipoDocumentoVenta.Factura when pago.DocumentoOrigenTipo != TipoDocumentoVenta.Apartado
                => "VentaContado",
            TipoDocumentoVenta.Apartado => "AbonoApartado",
            _ => null,
        };
    }

    private static DocumentoVentaEventoMovimientoDto? ResolverEventoMovimiento(
        MovimientoDineroFilaDto movimiento,
        IReadOnlyList<DocumentoVentaEventoMovimientoDto> eventos)
    {
        var tiposPreferidos = movimiento.TipoMovimiento switch
        {
            "VentaContado" => new[] { "FacturaEmitida", "FacturaEmitidaDesdeProforma" },
            "AbonoFacturaCredito" => new[] { "AbonoRegistrado", "SaldoCancelado" },
            "AbonoApartado" => new[] { "AbonoRegistrado", "SaldoCancelado", "ApartadoCreado" },
            "AnulacionAbono" => new[] { "AbonoRevertido" },
            _ => []
        };

        var conPago = eventos
            .Where(e => e.Payload?.Contains(movimiento.PagoId.ToString(), StringComparison.OrdinalIgnoreCase) == true)
            .ToList();

        return conPago.FirstOrDefault(e => tiposPreferidos.Contains(e.TipoEventoCodigo, StringComparer.OrdinalIgnoreCase))
            ?? eventos.FirstOrDefault(e => tiposPreferidos.Contains(e.TipoEventoCodigo, StringComparer.OrdinalIgnoreCase));
    }

    private static string TipoDocumentoDetalle(TipoDocumentoVenta tipoDocumento)
        => tipoDocumento switch
        {
            TipoDocumentoVenta.Factura => "Factura",
            TipoDocumentoVenta.Apartado => "Apartado",
            TipoDocumentoVenta.NotaCredito => "Nota de crédito",
            TipoDocumentoVenta.NotaDebito => "Nota de débito",
            TipoDocumentoVenta.Proforma => "Proforma",
            _ => tipoDocumento.ToString()
        };

    private sealed record MovimientoDineroPagoProjection(
        Guid PagoId,
        Guid DocumentoId,
        string? DocumentoConsecutivo,
        TipoDocumentoVenta DocumentoTipo,
        string CondicionVentaCodigo,
        TipoDocumentoVenta? DocumentoOrigenTipo,
        Guid? CajaId,
        string? CajaCodigo,
        string? CajaNombre,
        Guid? ClienteId,
        string? ClienteNombre,
        string? ClienteIdentificacion,
        string MonedaCodigo,
        string MedioPagoCodigo,
        string MedioPagoDetalle,
        string? Referencia,
        int NumeroAbono,
        decimal MontoAplicadoDocumento,
        DateTime FechaPago,
        DateTime FechaRegistroUtc,
        Guid? UsuarioRegistroId,
        string? UsuarioRegistroNombre,
        bool Anulado,
        DateTime? FechaAnulacionUtc,
        Guid? UsuarioAnulaId,
        string? UsuarioAnulaNombre,
        string? MotivoAnulacion);

    private sealed record DocumentoVentaEventoMovimientoDto(
        Guid Id,
        Guid DocumentoVentaId,
        string TipoEventoCodigo,
        string Resumen,
        DateTime OcurridoEn,
        string? Payload);

}

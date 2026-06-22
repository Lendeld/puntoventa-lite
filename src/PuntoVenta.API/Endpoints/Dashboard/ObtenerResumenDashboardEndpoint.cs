using FastEndpoints;
using Mediator;
using PuntoVenta.Application.Queries.Dashboard;
using PuntoVenta.Application.Queries.Dashboard.ObtenerResumenDashboard;
using PuntoVenta.API.Extensions;
using PuntoVenta.Infrastructure.Security;

namespace PuntoVenta.API.Endpoints.Dashboard;

public sealed class ObtenerResumenDashboardEndpoint : EndpointWithoutRequest<ResumenDashboardDto>
{
    private readonly IMediator _mediator;

    public ObtenerResumenDashboardEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Get("/dashboard/resumen");
        Tags("Dashboard");
        Options(b => b.RequireAuthorization(PermisosRegistrar.Claves.DashboardVer));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var result = await _mediator.Send(new ObtenerResumenDashboardQuery(), ct);
        if (result.IsError) { await Send.ResultAsync(result.Errors.ToProblem()); return; }
        await Send.ResponseAsync(result.Value);
    }
}

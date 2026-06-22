using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Infrastructure.Persistence;
using PuntoVenta.Infrastructure.Persistence.Repositories;
using PuntoVenta.Infrastructure.Security;
using PuntoVenta.Infrastructure.Services;

namespace PuntoVenta.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlite(configuration.GetConnectionString("DefaultConnection"))
                   .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTrackingWithIdentityResolution));

        services.AddMemoryCache(opts =>
        {
            opts.SizeLimit = 100_000;
            opts.CompactionPercentage = 0.25;
        });

        services.AddHostedService<TokenCleanupService>();
        services.AddHostedService<ApartadosVencimientoService>();

        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

        // Registro por convención: clases marcadas con IScopedService/ITransientService/
        // ISingletonService se registran solas (mismo mecanismo que el repo principal).
        services.Scan(scan => scan
            .FromAssemblies(typeof(DependencyInjection).Assembly)
            .AddClasses(c => c.AssignableTo<IScopedService>())
                .AsImplementedInterfaces()
                .WithScopedLifetime()
            .AddClasses(c => c.AssignableTo<ITransientService>())
                .AsImplementedInterfaces()
                .WithTransientLifetime()
            .AddClasses(c => c.AssignableTo<ISingletonService>())
                .AsImplementedInterfaces()
                .WithSingletonLifetime());

        services.Configure<JwtSettings>(configuration.GetSection("Jwt"));
        services.AddDataProtection();
        services.AddHttpContextAccessor();

        services.AddPermisoPolicies();

        // Necesario para que QuestPdfDocumentoVentaService pueda descargar logos via HTTP.
        services.AddHttpClient();

        QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
        services.AddSingleton<IDocumentoVentaPdfService, QuestPdfDocumentoVentaService>();
        services.AddSingleton<IReporteMovimientosDineroPdfService, QuestPdfReporteMovimientosDineroService>();
        services.AddSingleton<IReporteVentasExcelService, ClosedXmlReporteVentasService>();

        // Repositorios
        services.AddScoped<ICajaRepository, CajaRepository>();
        services.AddScoped<ICategoriaRepository, CategoriaRepository>();
        services.AddScoped<IClienteRepository, ClienteRepository>();
        services.AddScoped<ICodigoImpuestoRepository, CodigoImpuestoRepository>();
        services.AddScoped<ICondicionVentaRepository, CondicionVentaRepository>();
        services.AddScoped<IDocumentoVentaEventoRepository, DocumentoVentaEventoRepository>();
        services.AddScoped<IDocumentoVentaRepository, DocumentoVentaRepository>();
        services.AddScoped<IMedioPagoRepository, MedioPagoRepository>();
        services.AddScoped<INegocioRepository, NegocioRepository>();
        services.AddScoped<INegocioTicketConfigRepository, NegocioTicketConfigRepository>();
        services.AddScoped<IPerfilImpresoraTicketRepository, PerfilImpresoraTicketRepository>();
        services.AddScoped<IPermisoRepository, PermisoRepository>();
        services.AddScoped<IProductoRepository, ProductoRepository>();
        services.AddScoped<IRefreshTokenSessionRepository, RefreshTokenSessionRepository>();
        services.AddScoped<IRolRepository, RolRepository>();
        services.AddScoped<ISecuenciaRepository, SecuenciaRepository>();
        services.AddScoped<ITarifaIvaImpuestoRepository, TarifaIvaImpuestoRepository>();
        services.AddScoped<ITokenRevocadoRepository, TokenRevocadoRepository>();
        services.AddScoped<IUsuarioPermisoRepository, UsuarioPermisoRepository>();
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddScoped<IVendedorRepository, VendedorRepository>();

        // Servicios de seguridad e infraestructura
        services.AddScoped<IUsuarioActual, UsuarioActual>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IOpaqueTokenService, OpaqueTokenService>();
        services.AddScoped<IDocumentoVentaEventoService, DocumentoVentaEventoService>();
        services.AddScoped<IAuthSettings, AuthSettingsAccessor>();
        services.AddScoped<IImagenStorageService, LocalImagenStorageService>();

        return services;
    }
}

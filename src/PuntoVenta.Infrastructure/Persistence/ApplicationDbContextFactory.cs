using Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using PuntoVenta.Application.Interfaces;

namespace PuntoVenta.Infrastructure.Persistence;

public sealed class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var configurationBuilder = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true);

        var externalAppSettings = Environment.GetEnvironmentVariable("PUNTO_VENTA_API_DEV_SETTINGS_PATH");

        if (!string.IsNullOrEmpty(externalAppSettings) && File.Exists(externalAppSettings))
        {
            configurationBuilder.AddJsonFile(externalAppSettings, optional: true, reloadOnChange: true);
        }

        var configuration = configurationBuilder
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("La cadena de conexión no se encontró.");
        }

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseSqlite(connectionString);

        return new ApplicationDbContext(optionsBuilder.Options, new UsuarioActualDesignTime(), new FechaActualDesignTime(), new PublisherDesignTime());
    }

    private sealed class UsuarioActualDesignTime : IUsuarioActual
    {
        public Guid UsuarioId => Guid.Empty;
        public string NombreUsuario => "design-time";
        public bool RequiereCambioPassword => false;
    }

    private sealed class FechaActualDesignTime : IFechaActual
    {
        public DateTime AhoraUtc => DateTime.UtcNow;
        public DateTime Ahora => DateTime.UtcNow;
        public DateOnly Hoy => DateOnly.FromDateTime(Ahora);
        public DateOnly ALocal(DateTime utc) => DateOnly.FromDateTime(utc);
    }

    private sealed class PublisherDesignTime : IPublisher
    {
        public ValueTask Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
            where TNotification : INotification
            => ValueTask.CompletedTask;

        public ValueTask Publish(object notification, CancellationToken cancellationToken = default)
            => ValueTask.CompletedTask;
    }
}

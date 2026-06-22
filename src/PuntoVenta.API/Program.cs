using System.IdentityModel.Tokens.Jwt;
using System.Text;
using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PuntoVenta.API.Hosting;
using PuntoVenta.API.Middleware;
using PuntoVenta.Application;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Infrastructure;
using PuntoVenta.Infrastructure.Persistence;
using PuntoVenta.Infrastructure.Security;

// Mac + Electron: ocultar del Dock antes de que ASP.NET cree el host.
MacOSDockHider.HideIfElectronChild();

var builder = WebApplication.CreateBuilder(args);

// Lite corre en loopback, un solo usuario, offline: sin tope de body. El cap
// solo tendría sentido contra uploads maliciosos remotos, que aquí no existen.
builder.WebHost.ConfigureKestrel(o => o.Limits.MaxRequestBodySize = null);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddRequestDecompression();
builder.Services.AddResponseCompression(o => o.EnableForHttps = true);

builder.Services.ConfigureHttpJsonOptions(o => o.SerializerOptions.Converters.Add(
        new System.Text.Json.Serialization.JsonStringEnumConverter(
            namingPolicy: null,
            allowIntegerValues: true)));

// JWT Authentication
var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>()!;

if (jwtSettings.SecretKey.Length < 32)
{
    throw new InvalidOperationException("Jwt:SecretKey debe tener al menos 32 caracteres.");
}

var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = signingKey,
            ClockSkew = TimeSpan.FromMinutes(2)
        };
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = async context =>
            {
                var repo = context.HttpContext.RequestServices
                    .GetRequiredService<ITokenRevocadoRepository>();
                var jti = context.Principal?.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;

                if (jti is not null && await repo.EstaRevocadoAsync(jti))
                {
                    context.Fail("Token revocado.");
                }
            }
        };
    });

// FastEndpoints + Swagger
builder.Services.AddFastEndpoints();
builder.Services.SwaggerDocument(options => options.DocumentSettings = settings =>
    {
        settings.Title = "PuntoVenta API";
        settings.Version = "v1";
    });

// Forwarded headers (proxies tipo Render/nginx)
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

// CORS
builder.Services.AddCors(options =>
{
    var allowedOriginsRaw = builder.Configuration["Cors:AllowedOrigins"];
    var allowedOrigins = allowedOriginsRaw?.Split(',', StringSplitOptions.RemoveEmptyEntries) ?? [];
    options.AddDefaultPolicy(policy =>
    {
        if (allowedOrigins.Length > 0)
        {
            policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod().AllowCredentials();
        }
        else if (builder.Environment.IsDevelopment())
        {
            policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
        }
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwaggerGen();
}

// Lite es offline-first: la DB SQLite local es la única, así que migrar y
// sembrar corre siempre (idempotente) — incluido packaged (Production).
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await db.Database.MigrateAsync();

    await DataSeeder.SembrarPermisosAsync(db);
    await DataSeeder.SembrarPaginasAsync(db);
    await DataSeeder.SembrarNegocioAsync(db);
    await DataSeeder.SembrarTiposIdentificacionAsync(db);
    await DataSeeder.SembrarRolesAsync(db);
    await DataSeeder.SembrarUsuariosAsync(db, builder.Configuration);
    await DataSeeder.SembrarCondicionesVentaAsync(db);
    await DataSeeder.SembrarMediosPagoAsync(db);
    await DataSeeder.SembrarCodigosImpuestoAsync(db);
    await DataSeeder.SembrarTarifasIvaImpuestoAsync(db);
    await DataSeeder.SembrarNegocioTicketConfigAsync(db);
    await DataSeeder.SembrarPerfilesImpresoraTicketAsync(db);
    await DataSeeder.SembrarPermisosPaginaAsync(db);
}

app.UseForwardedHeaders();
// Sin UseHttpsRedirection: Lite sirve solo HTTP en loopback (child de
// Electron) — el middleware no tendria puerto https y solo generaria
// warnings "Failed to determine the https port" en cada request.
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseCors();
app.UseAuthentication();
app.UseRequestDecompression();
app.UseMiddleware<PasswordChangeRequiredMiddleware>();
app.UseAuthorization();

app.UseFastEndpoints(c =>
{
    c.Endpoints.Configurator = ep => ep.Options(b => b.RequireAuthorization());
    c.Serializer.Options.Converters.Add(
        new System.Text.Json.Serialization.JsonStringEnumConverter(
            namingPolicy: null,
            allowIntegerValues: true));
});

app.Run();

// Expuesto para WebApplicationFactory<Program> en tests de integración HTTP.
public partial class Program { }

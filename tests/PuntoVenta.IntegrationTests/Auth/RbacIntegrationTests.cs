using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Domain.Entities.Roles;
using PuntoVenta.Domain.Entities.Usuarios;
using PuntoVenta.Infrastructure.Persistence;
using PuntoVenta.Infrastructure.Security;

namespace PuntoVenta.IntegrationTests.Auth;

[Trait("Category", "Integration")]
[Collection("Integration")]
public sealed class RbacIntegrationTests(PuntoVenta.IntegrationTests.Fixtures.IntegrationTestFixture fixture)
{
    // ──────────────────────────────────────────────
    // Usuario sin permiso → 403 Forbidden
    // ──────────────────────────────────────────────

    [Fact]
    public async Task EndpointProtegido_Retorna403_CuandoUsuarioSinPermiso()
    {
        // Preparar: crear rol sin permisos y usuario con ese rol.
        var (_, token) = await CrearUsuarioConRolSinPermisosAsync("rbac-test-sin-permiso", "RolSinPermisos-403");

        var cliente = fixture.Factory.CreateClient();
        cliente.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // /categorias requiere CategoriasVer — el rol no tiene ningún permiso.
        var response = await cliente.GetAsync("/categorias?pagina=1&tamano=10", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    // ──────────────────────────────────────────────
    // Usuario con el permiso correcto → 200
    // ──────────────────────────────────────────────

    [Fact]
    public async Task EndpointProtegido_Retorna200_CuandoUsuarioConPermiso()
    {
        // Preparar: crear rol, usuario, luego agregar permiso al rol.
        var (rolId, token) = await CrearUsuarioConRolSinPermisosAsync("rbac-test-con-permiso", "RolConPermisos-200");

        // Agregar el permiso CategoriasVer al rol recién creado.
        await AgregarPermisoARolAsync(rolId, PermisosRegistrar.Claves.CategoriasVer);

        var cliente = fixture.Factory.CreateClient();
        cliente.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await cliente.GetAsync("/categorias?pagina=1&tamano=10", TestContext.Current.CancellationToken);

        // El endpoint de categorías puede devolver 200 (con datos) o 204 (sin datos).
        Assert.True(
            response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.NoContent,
            $"Status inesperado después de asignar permiso: {response.StatusCode}");
    }

    // ──────────────────────────────────────────────
    // Helpers privados
    // ──────────────────────────────────────────────

    private async Task<(Guid RolId, string Token)> CrearUsuarioConRolSinPermisosAsync(
        string nombreUsuario,
        string nombreRol)
    {
        using var scope = fixture.Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        // Crear un rol sin permisos (no es principal).
        var rol = Rol.Crear(nombreRol, "Rol de prueba sin permisos").Value;
        await db.Roles.AddAsync(rol);
        await db.SaveChangesAsync();

        // Crear usuario con ese rol.
        var passwordPlano = "Test1234!";
        var usuario = Usuario.Crear(
            nombreUsuario: nombreUsuario,
            nombre: "Usuario Test RBAC",
            identificacion: $"1{Guid.NewGuid():N}"[..9],
            passwordHash: hasher.Hash(passwordPlano),
            rolId: rol.Id).Value;

        await db.Usuarios.AddAsync(usuario);
        await db.SaveChangesAsync();

        // Hacer login para obtener token.
        var loginResponse = await fixture.Client.PostAsJsonAsync("/auth/login", new
        {
            NombreUsuario = nombreUsuario,
            Password = passwordPlano
        });
        loginResponse.EnsureSuccessStatusCode();
        var loginBody = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();

        return (rol.Id, loginBody?.AccessToken ?? throw new InvalidOperationException("Sin token."));
    }

    private async Task AgregarPermisoARolAsync(Guid rolId, string clavePermiso)
    {
        using var scope = fixture.Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var permisoCache = scope.ServiceProvider.GetRequiredService<IPermisoCache>();

        var permiso = await db.Permisos.FirstOrDefaultAsync(p => p.Clave == clavePermiso);
        if (permiso is null) return;

        // Verificar si ya existe la asociación.
        var yaExiste = await db.RolPermisos.AnyAsync(rp => rp.RolId == rolId && rp.PermisoId == permiso.Id);
        if (!yaExiste)
        {
            var rolPermiso = RolPermiso.Crear(rolId, permiso.Id);
            await db.RolPermisos.AddAsync(rolPermiso);
            await db.SaveChangesAsync();
        }

        // Invalidar la caché para que el siguiente request lea de DB.
        permisoCache.InvalidarTodos();
    }

    private sealed record AuthResponse(
        string AccessToken,
        string RefreshToken,
        bool RequiresPasswordChange);
}

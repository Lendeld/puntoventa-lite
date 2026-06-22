using ErrorOr;
using Mediator;
using PuntoVenta.Application.DTOs.Auth;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Domain.Entities.Usuarios;

namespace PuntoVenta.Application.Commands.Auth;

public sealed record LoginCommand(string NombreUsuario, string Password, string? IpAddress = null) : IRequest<ErrorOr<AuthFlowResponse>>;

public sealed class LoginHandler(
    IUsuarioRepository usuarioRepository,
    IPasswordHasher passwordHasher,
    IJwtTokenService jwtTokenService,
    IOpaqueTokenService opaqueTokenService,
    IRefreshTokenSessionRepository refreshTokenSessionRepository,
    IAuthSettings authSettings) : IRequestHandler<LoginCommand, ErrorOr<AuthFlowResponse>>
{
    private readonly IUsuarioRepository _usuarioRepository = usuarioRepository;
    private readonly IPasswordHasher _passwordHasher = passwordHasher;
    private readonly IJwtTokenService _jwtTokenService = jwtTokenService;
    private readonly IOpaqueTokenService _opaqueTokenService = opaqueTokenService;
    private readonly IRefreshTokenSessionRepository _refreshTokenSessionRepository = refreshTokenSessionRepository;
    private readonly IAuthSettings _authSettings = authSettings;

    public async ValueTask<ErrorOr<AuthFlowResponse>> Handle(LoginCommand command, CancellationToken cancellationToken)
    {
        var usuario = await _usuarioRepository.ObtenerPorNombreUsuarioAsync(
            command.NombreUsuario.Trim(), cancellationToken);

        if (usuario is null || !_passwordHasher.Verify(command.Password, usuario.PasswordHash))
        {
            return UsuarioErrors.CredencialesInvalidas;
        }

        if (!usuario.Activo)
        {
            return UsuarioErrors.UsuarioInactivo;
        }

        var ahoraUtc = DateTime.UtcNow;

        if (usuario.PasswordTemporalExpirada(ahoraUtc))
        {
            return UsuarioErrors.PasswordTemporalExpirada;
        }

        return await AuthFlowFactory.CrearSesionAsync(
            usuario,
            requiereCambioPassword: usuario.DebeCambiarPassword,
            ipAddress: command.IpAddress,
            ahoraUtc: ahoraUtc,
            _authSettings,
            _jwtTokenService,
            _opaqueTokenService,
            _refreshTokenSessionRepository,
            cancellationToken);
    }
}

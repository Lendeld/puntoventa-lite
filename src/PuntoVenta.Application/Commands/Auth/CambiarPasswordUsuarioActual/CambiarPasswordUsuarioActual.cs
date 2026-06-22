using ErrorOr;
using Mediator;
using PuntoVenta.Application.DTOs.Auth;
using PuntoVenta.Application.Interfaces;
using PuntoVenta.Domain.Entities.Usuarios;

namespace PuntoVenta.Application.Commands.Auth.CambiarPasswordUsuarioActual;

public sealed record CambiarPasswordUsuarioActualCommand(string PasswordActual, string PasswordNueva) : IRequest<ErrorOr<AuthFlowResponse>>;

public sealed class CambiarPasswordUsuarioActualHandler(
    IUsuarioActual usuarioActual,
    IUsuarioRepository usuarioRepository,
    IPasswordHasher passwordHasher,
    IRefreshTokenSessionRepository refreshTokenSessionRepository,
    IAuthSettings authSettings,
    IJwtTokenService jwtTokenService,
    IOpaqueTokenService opaqueTokenService) : IRequestHandler<CambiarPasswordUsuarioActualCommand, ErrorOr<AuthFlowResponse>>
{
    private readonly IUsuarioActual _usuarioActual = usuarioActual;
    private readonly IUsuarioRepository _usuarioRepository = usuarioRepository;
    private readonly IPasswordHasher _passwordHasher = passwordHasher;
    private readonly IRefreshTokenSessionRepository _refreshTokenSessionRepository = refreshTokenSessionRepository;
    private readonly IAuthSettings _authSettings = authSettings;
    private readonly IJwtTokenService _jwtTokenService = jwtTokenService;
    private readonly IOpaqueTokenService _opaqueTokenService = opaqueTokenService;

    public async ValueTask<ErrorOr<AuthFlowResponse>> Handle(CambiarPasswordUsuarioActualCommand command, CancellationToken cancellationToken)
    {
        var usuario = await _usuarioRepository.GetByIdAsync(_usuarioActual.UsuarioId, cancellationToken);

        if (usuario is null)
        {
            return UsuarioErrors.NoEncontrado;
        }

        if (!_passwordHasher.Verify(command.PasswordActual, usuario.PasswordHash))
        {
            return UsuarioErrors.PasswordActualIncorrecta;
        }

        var cambioPassword = usuario.CambiarPassword(_passwordHasher.Hash(command.PasswordNueva));

        if (cambioPassword.IsError)
        {
            return cambioPassword.Errors;
        }

        usuario.MarcarCambioPasswordCompletado();

        await _usuarioRepository.UpdateAsync(usuario, cancellationToken);
        await _refreshTokenSessionRepository.RevocarSesionesUsuarioAsync(usuario.Id, DateTime.UtcNow, cancellationToken);

        return await AuthFlowFactory.CrearSesionAsync(
            usuario,
            requiereCambioPassword: false,
            ipAddress: null,
            ahoraUtc: DateTime.UtcNow,
            _authSettings,
            _jwtTokenService,
            _opaqueTokenService,
            _refreshTokenSessionRepository,
            cancellationToken);
    }
}

using ErrorOr;

namespace PuntoVenta.Domain.Entities.Usuarios;

public sealed class Usuario : BaseAuditableEntity
{
    public const int NombreUsuarioMaxLength = 50;
    public const int NombreMaxLength = 150;
    public const int CorreoMaxLength = 256;
    public const int IdentificacionMaxLength = 50;
    public const int TelefonoMaxLength = 20;
    public const int PinLongitud = 6;

    private Usuario() { }

    public string NombreUsuario { get; private set; } = string.Empty;
    public string Nombre { get; private set; } = string.Empty;
    public string? Correo { get; private set; }
    public string Identificacion { get; private set; } = string.Empty;
    public string? Telefono { get; private set; }
    public string PasswordHash { get; private set; } = string.Empty;
    public string? PinHash { get; private set; }
    public bool TienePin => PinHash is not null;
    public bool DebeCambiarPassword { get; private set; }
    public DateTime? PasswordTemporalExpiraEnUtc { get; private set; }
    public Guid? RolId { get; private set; }

    /// <summary>
    /// Cuenta dueña del negocio (el admin sembrado). Solo ella puede editarse a sí
    /// misma; nadie más la modifica y su rol no se puede cambiar.
    /// </summary>
    public bool EsPropietario { get; private set; }

    public void AsignarRol(Guid rolId) => RolId = rolId;

    public void MarcarComoPropietario() => EsPropietario = true;

    public static ErrorOr<Usuario> Crear(
        string nombreUsuario,
        string nombre,
        string identificacion,
        string passwordHash,
        string? correo = null,
        string? telefono = null,
        Guid? rolId = null)
    {
        var errores = new List<Error>();

        if (string.IsNullOrWhiteSpace(nombreUsuario))
        {
            errores.Add(UsuarioErrors.NombreUsuarioRequerido);
        }
        else if (nombreUsuario.Trim().Length > NombreUsuarioMaxLength)
        {
            errores.Add(UsuarioErrors.NombreUsuarioExcedeLongitud);
        }

        if (string.IsNullOrWhiteSpace(nombre))
        {
            errores.Add(UsuarioErrors.NombreRequerido);
        }
        else if (nombre.Trim().Length > NombreMaxLength)
        {
            errores.Add(UsuarioErrors.NombreExcedeLongitud);
        }

        // Identificación opcional al crear (POS: alta rápida de usuarios).
        if (!string.IsNullOrWhiteSpace(identificacion) && identificacion.Trim().Length > IdentificacionMaxLength)
        {
            errores.Add(UsuarioErrors.IdentificacionExcedeLongitud);
        }

        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            errores.Add(UsuarioErrors.PasswordRequerido);
        }

        if (correo is not null && correo.Trim().Length > CorreoMaxLength)
        {
            errores.Add(UsuarioErrors.CorreoExcedeLongitud);
        }

        if (telefono is not null && telefono.Trim().Length > TelefonoMaxLength)
        {
            errores.Add(UsuarioErrors.TelefonoExcedeLongitud);
        }

        if (errores.Count > 0)
        {
            return errores;
        }

        return new Usuario
        {
            NombreUsuario = nombreUsuario.Trim(),
            Nombre = nombre.Trim(),
            Identificacion = string.IsNullOrWhiteSpace(identificacion) ? string.Empty : identificacion.Trim(),
            PasswordHash = passwordHash,
            DebeCambiarPassword = false,
            Correo = correo?.Trim(),
            Telefono = telefono?.Trim(),
            RolId = rolId
        };
    }

    public ErrorOr<Success> Actualizar(
        string nombreUsuario,
        string nombre,
        string identificacion,
        string? correo = null,
        string? telefono = null)
    {
        var errores = new List<Error>();

        if (string.IsNullOrWhiteSpace(nombreUsuario))
        {
            errores.Add(UsuarioErrors.NombreUsuarioRequerido);
        }
        else if (nombreUsuario.Trim().Length > NombreUsuarioMaxLength)
        {
            errores.Add(UsuarioErrors.NombreUsuarioExcedeLongitud);
        }

        if (string.IsNullOrWhiteSpace(nombre))
        {
            errores.Add(UsuarioErrors.NombreRequerido);
        }
        else if (nombre.Trim().Length > NombreMaxLength)
        {
            errores.Add(UsuarioErrors.NombreExcedeLongitud);
        }

        if (string.IsNullOrWhiteSpace(identificacion))
        {
            errores.Add(UsuarioErrors.IdentificacionRequerida);
        }
        else if (identificacion.Trim().Length > IdentificacionMaxLength)
        {
            errores.Add(UsuarioErrors.IdentificacionExcedeLongitud);
        }

        if (correo is not null && correo.Trim().Length > CorreoMaxLength)
        {
            errores.Add(UsuarioErrors.CorreoExcedeLongitud);
        }

        if (telefono is not null && telefono.Trim().Length > TelefonoMaxLength)
        {
            errores.Add(UsuarioErrors.TelefonoExcedeLongitud);
        }

        if (errores.Count > 0)
        {
            return errores;
        }

        NombreUsuario = nombreUsuario.Trim();
        Nombre = nombre.Trim();
        Identificacion = identificacion.Trim();
        Correo = correo?.Trim();
        Telefono = telefono?.Trim();

        return Result.Success;
    }

    public ErrorOr<Success> ActualizarPerfil(
        string nombre,
        string identificacion,
        string? correo = null,
        string? telefono = null)
    {
        var errores = new List<Error>();

        if (string.IsNullOrWhiteSpace(nombre))
        {
            errores.Add(UsuarioErrors.NombreRequerido);
        }
        else if (nombre.Trim().Length > NombreMaxLength)
        {
            errores.Add(UsuarioErrors.NombreExcedeLongitud);
        }

        // Identificación opcional (igual que al crear).
        if (!string.IsNullOrWhiteSpace(identificacion) && identificacion.Trim().Length > IdentificacionMaxLength)
        {
            errores.Add(UsuarioErrors.IdentificacionExcedeLongitud);
        }

        if (correo is not null && correo.Trim().Length > CorreoMaxLength)
        {
            errores.Add(UsuarioErrors.CorreoExcedeLongitud);
        }

        if (telefono is not null && telefono.Trim().Length > TelefonoMaxLength)
        {
            errores.Add(UsuarioErrors.TelefonoExcedeLongitud);
        }

        if (errores.Count > 0)
        {
            return errores;
        }

        Nombre = nombre.Trim();
        Identificacion = string.IsNullOrWhiteSpace(identificacion) ? string.Empty : identificacion.Trim();
        Correo = correo?.Trim();
        Telefono = telefono?.Trim();

        return Result.Success;
    }

    public ErrorOr<Success> CambiarPassword(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            return UsuarioErrors.PasswordRequerido;
        }

        PasswordHash = passwordHash;
        return Result.Success;
    }

    public ErrorOr<Success> EstablecerPin(string pinHash)
    {
        if (string.IsNullOrWhiteSpace(pinHash))
        {
            return UsuarioErrors.PinRequerido;
        }

        PinHash = pinHash;
        return Result.Success;
    }

    public void RequerirCambioPassword(DateTime? passwordTemporalExpiraEnUtc = null)
    {
        DebeCambiarPassword = true;
        PasswordTemporalExpiraEnUtc = passwordTemporalExpiraEnUtc;
    }

    public bool PasswordTemporalExpirada(DateTime ahoraUtc) =>
        DebeCambiarPassword &&
        PasswordTemporalExpiraEnUtc.HasValue &&
        PasswordTemporalExpiraEnUtc.Value <= ahoraUtc;

    public void MarcarCambioPasswordCompletado()
    {
        DebeCambiarPassword = false;
        PasswordTemporalExpiraEnUtc = null;
    }
}

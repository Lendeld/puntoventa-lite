using ErrorOr;

namespace PuntoVenta.Domain.Entities.Usuarios;

public static class UsuarioErrors
{
    public static Error NombreUsuarioRequerido =>
        Error.Validation("Usuario_NombreUsuario", "El nombre de usuario es requerido.");

    public static Error NombreUsuarioExcedeLongitud =>
        Error.Validation("Usuario_NombreUsuario", $"El nombre de usuario no puede exceder {Usuario.NombreUsuarioMaxLength} caracteres.");

    public static Error NombreRequerido =>
        Error.Validation("Usuario_Nombre", "El nombre es requerido.");

    public static Error NombreExcedeLongitud =>
        Error.Validation("Usuario_Nombre", $"El nombre no puede exceder {Usuario.NombreMaxLength} caracteres.");

    public static Error IdentificacionRequerida =>
        Error.Validation("Usuario_Identificacion", "La identificación es requerida.");

    public static Error IdentificacionExcedeLongitud =>
        Error.Validation("Usuario_Identificacion", $"La identificación no puede exceder {Usuario.IdentificacionMaxLength} caracteres.");

    public static Error PasswordRequerido =>
        Error.Validation("Usuario_Password", "La contraseña es requerida.");

    public static Error PasswordActualRequerido =>
        Error.Validation("Usuario_PasswordActual", "La contraseña actual es requerida.");

    public static Error PasswordNuevaRequerida =>
        Error.Validation("Usuario_PasswordNueva", "La nueva contraseña es requerida.");

    public static Error PasswordNuevaDemasiadoCorta =>
        Error.Validation("Usuario_PasswordNueva", "La contraseña debe tener al menos 8 caracteres.");

    public static Error PasswordNuevaRequiereMayuscula =>
        Error.Validation("Usuario_PasswordNueva", "La contraseña debe contener al menos una letra mayúscula.");

    public static Error PasswordNuevaRequiereDigito =>
        Error.Validation("Usuario_PasswordNueva", "La contraseña debe contener al menos un número.");

    public static Error PasswordNuevaRequiereMinuscula =>
        Error.Validation("Usuario_PasswordNueva", "La contraseña debe contener al menos una letra minúscula.");

    public static Error PasswordNuevaRequiereSimbolo =>
        Error.Validation("Usuario_PasswordNueva", "La contraseña debe contener al menos un símbolo especial.");

    public static Error PasswordDemasiadoCorta =>
        Error.Validation("Usuario_Password", "La contraseña debe tener al menos 8 caracteres.");

    public static Error PasswordRequiereMayuscula =>
        Error.Validation("Usuario_Password", "La contraseña debe contener al menos una letra mayúscula.");

    public static Error PasswordRequiereMinuscula =>
        Error.Validation("Usuario_Password", "La contraseña debe contener al menos una letra minúscula.");

    public static Error PasswordRequiereDigito =>
        Error.Validation("Usuario_Password", "La contraseña debe contener al menos un número.");

    public static Error PasswordRequiereSimbolo =>
        Error.Validation("Usuario_Password", "La contraseña debe contener al menos un símbolo especial.");

    // Validation (no Unauthorized): el usuario YA está autenticado y solo confirma su
    // contraseña. Un 401 haría que el cliente lo trate como sesión vencida y cierre sesión.
    // Code = "Usuario_PasswordActual" para que mapee al campo del form (cambiar password +
    // establecer PIN) y muestre el mensaje inline en vez de cerrar sesión.
    public static Error PasswordActualIncorrecta =>
        Error.Validation("Usuario_PasswordActual", "La contraseña actual es incorrecta.");

    public static Error CorreoExcedeLongitud =>
        Error.Validation("Usuario_Correo", $"El correo no puede exceder {Usuario.CorreoMaxLength} caracteres.");

    public static Error TelefonoExcedeLongitud =>
        Error.Validation("Usuario_Telefono", $"El teléfono no puede exceder {Usuario.TelefonoMaxLength} caracteres.");

    public static Error NombreUsuarioYaExiste =>
        Error.Conflict("Usuario_NombreUsuario", "El nombre de usuario ya está registrado.");

    public static Error IdentificacionYaExiste =>
        Error.Conflict("Usuario_Identificacion", "La identificación ya está registrada.");

    public static Error NoEncontrado =>
        Error.NotFound("Usuario_NoEncontrado", "El usuario no fue encontrado.");

    public static Error CredencialesInvalidas =>
        Error.Unauthorized("Auth_CredencialesInvalidas", "El nombre de usuario o contraseña son incorrectos.");

    public static Error LoginChallengeInvalido =>
        Error.Unauthorized("Auth_LoginChallengeInvalido", "El desafío de autenticación no es válido o expiró.");

    public static Error CodigoOtpInvalido =>
        Error.Unauthorized("Auth_CodigoOtpInvalido", "El código OTP no es válido.");

    public static Error UsuarioInactivo =>
        Error.Forbidden("Auth_UsuarioInactivo", "El usuario está inactivo.");

    public static Error SinRolAsignado =>
        Error.Forbidden("Auth_SinRolAsignado", "El usuario no tiene un rol asignado.");

    public static Error SinNegociosActivos =>
        Error.Forbidden("Auth_SinNegociosActivos", "El usuario no tiene negocios activos asignados.");

    public static Error NegocioNoAsignado =>
        Error.Forbidden("Auth_NegocioNoAsignado", "El usuario no tiene acceso al negocio seleccionado.");

    public static Error RequiereCambioPassword =>
        Error.Forbidden("Auth_RequiereCambioPassword", "Debe cambiar su contraseña antes de continuar.");

    public static Error PasswordTemporalExpirada =>
        Error.Unauthorized("Auth_PasswordTemporalExpirada", "La contraseña temporal expiró. Solicita una nueva al administrador.");

    public static Error YaAsignadoANegocio =>
        Error.Conflict("Usuario_Negocio", "El usuario ya está asignado al negocio activo.");

    public static Error YaVinculadoANegocio =>
        Error.Conflict("Usuario_YaVinculadoANegocio", "El usuario ya está vinculado a este negocio.");

    public static Error PropietarioSoloSeEditaASiMismo =>
        Error.Forbidden("Usuario_Propietario", "Solo el propietario puede modificar su propia cuenta.");

    public static Error PropietarioNoSePuedeDesactivar =>
        Error.Conflict("Usuario_Propietario_Activo", "El propietario no puede desactivar su propia cuenta.");

    public static Error PropietarioRolNoSePuedeCambiar =>
        Error.Conflict("Usuario_Negocio_Propietario_Rol", "No se puede cambiar el rol del propietario.");

    public static Error PinRequerido =>
        Error.Validation("Usuario_Pin", "El PIN es requerido.");

    public static Error PinFormatoInvalido =>
        Error.Validation("Usuario_Pin", "El PIN debe ser exactamente 6 dígitos numéricos.");

    public static Error PinIncorrecto =>
        Error.Unauthorized("Auth_PinIncorrecto", "El PIN es incorrecto.");

    public static Error PinNoConfigurado =>
        Error.Forbidden("Auth_PinNoConfigurado", "Debe configurar su PIN de seguridad antes de realizar esta acción.");
}

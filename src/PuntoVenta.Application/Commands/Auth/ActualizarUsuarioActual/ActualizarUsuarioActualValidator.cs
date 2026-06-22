using FluentValidation;
using PuntoVenta.Application.Common.Validation;
using PuntoVenta.Domain.Entities.Usuarios;

namespace PuntoVenta.Application.Commands.Auth.ActualizarUsuarioActual;

public sealed class ActualizarUsuarioActualValidator : AbstractValidator<ActualizarUsuarioActualCommand>
{
    public ActualizarUsuarioActualValidator()
    {
        RuleFor(x => x.Nombre)
            .NotEmpty()
            .WithError(UsuarioErrors.NombreRequerido)
            .MaximumLength(Usuario.NombreMaxLength)
            .WithError(UsuarioErrors.NombreExcedeLongitud);

        // Identificación opcional e informativa (puede repetirse): solo se
        // valida longitud cuando viene informada.
        RuleFor(x => x.Identificacion)
            .MaximumLength(Usuario.IdentificacionMaxLength)
            .WithError(UsuarioErrors.IdentificacionExcedeLongitud)
            .When(x => !string.IsNullOrWhiteSpace(x.Identificacion));

        RuleFor(x => x.Correo)
            .MaximumLength(Usuario.CorreoMaxLength)
            .WithError(UsuarioErrors.CorreoExcedeLongitud)
            .When(x => !string.IsNullOrWhiteSpace(x.Correo));

        RuleFor(x => x.Telefono)
            .MaximumLength(Usuario.TelefonoMaxLength)
            .WithError(UsuarioErrors.TelefonoExcedeLongitud)
            .When(x => !string.IsNullOrWhiteSpace(x.Telefono));
    }
}

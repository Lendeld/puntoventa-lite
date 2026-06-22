using FluentValidation;
using PuntoVenta.Application.Common.Validation;
using PuntoVenta.Domain.Entities.Usuarios;

namespace PuntoVenta.Application.Commands.Usuarios;

public sealed class CrearUsuarioValidator : AbstractValidator<CrearUsuarioCommand>
{
    public CrearUsuarioValidator()
    {
        // La contraseña temporal exige el mismo formato que el cambio de
        // contraseña (ver CambiarPasswordUsuarioActualValidator).
        RuleFor(x => x.Password)
            .NotEmpty()
            .WithError(UsuarioErrors.PasswordRequerido)
            .MinimumLength(8)
            .WithError(UsuarioErrors.PasswordNuevaDemasiadoCorta)
            .Matches("[A-Z]")
            .WithError(UsuarioErrors.PasswordNuevaRequiereMayuscula)
            .Matches("[a-z]")
            .WithError(UsuarioErrors.PasswordNuevaRequiereMinuscula)
            .Matches("[0-9]")
            .WithError(UsuarioErrors.PasswordNuevaRequiereDigito)
            .Matches(@"[$&+,:;=?@#|'<>.^*()%!\-]")
            .WithError(UsuarioErrors.PasswordNuevaRequiereSimbolo);
    }
}

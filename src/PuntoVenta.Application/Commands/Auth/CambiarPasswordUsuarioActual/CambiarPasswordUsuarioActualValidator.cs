using FluentValidation;
using PuntoVenta.Application.Common.Validation;
using PuntoVenta.Domain.Entities.Usuarios;

namespace PuntoVenta.Application.Commands.Auth.CambiarPasswordUsuarioActual;

public sealed class CambiarPasswordUsuarioActualValidator : AbstractValidator<CambiarPasswordUsuarioActualCommand>
{
    public CambiarPasswordUsuarioActualValidator()
    {
        RuleFor(x => x.PasswordActual)
            .NotEmpty()
            .WithError(UsuarioErrors.PasswordActualRequerido);

        RuleFor(x => x.PasswordNueva)
            .NotEmpty()
            .WithError(UsuarioErrors.PasswordNuevaRequerida)
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

using FluentValidation;
using PuntoVenta.Application.Common.Validation;
using PuntoVenta.Domain.Entities.Usuarios;

namespace PuntoVenta.Application.Commands.Auth.EstablecerPinUsuarioActual;

public sealed class EstablecerPinUsuarioActualValidator : AbstractValidator<EstablecerPinUsuarioActualCommand>
{
    public EstablecerPinUsuarioActualValidator()
    {
        RuleFor(x => x.PasswordActual)
            .NotEmpty()
            .WithError(UsuarioErrors.PasswordActualRequerido);

        RuleFor(x => x.PinNuevo)
            .NotEmpty()
            .WithError(UsuarioErrors.PinRequerido)
            .Matches(@"^\d{6}$")
            .WithError(UsuarioErrors.PinFormatoInvalido);
    }
}

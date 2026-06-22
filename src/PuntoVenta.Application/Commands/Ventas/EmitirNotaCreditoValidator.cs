using FluentValidation;
using PuntoVenta.Domain.Entities.Ventas;

namespace PuntoVenta.Application.Commands.Ventas;

public sealed class EmitirNotaCreditoValidator : AbstractValidator<EmitirNotaCreditoCommand>
{
    public EmitirNotaCreditoValidator()
    {
        RuleFor(x => x.DocumentoOrigenId).NotEmpty();

        When(x => !string.IsNullOrWhiteSpace(x.Razon), () =>
        {
            RuleFor(x => x.Razon)
                .MaximumLength(DocumentoVentaReferencia.RazonMaxLength);
        });

        When(x => x.Modo != ModoNotaCredito.Anulacion, () =>
        {
            RuleFor(x => x.Lineas)
                .NotEmpty()
                .WithMessage("Debe indicar al menos una línea.");
        });

        RuleForEach(x => x.Lineas).ChildRules(linea =>
        {
            linea.RuleFor(l => l.ProductoId).NotEmpty();
            linea.RuleFor(l => l.Cantidad).GreaterThan(0);
        });
    }
}

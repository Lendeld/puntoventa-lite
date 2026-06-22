using ErrorOr;
using FluentValidation;
using Mediator;

namespace PuntoVenta.Application.Common.Validation;

public sealed class ValidationBehavior<TMessage, TResponse>(IEnumerable<IValidator<TMessage>> validators) : IPipelineBehavior<TMessage, TResponse>
    where TMessage : IMessage
{
    private readonly IEnumerable<IValidator<TMessage>> _validators = validators;

    public async ValueTask<TResponse> Handle(
        TMessage message,
        MessageHandlerDelegate<TMessage, TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
        {
            return await next(message, cancellationToken);
        }

        var context = new ValidationContext<TMessage>(message);
        var results = _validators.Select(v => v.Validate(context)).ToList();
        var hasFailures = results.Any(r => !r.IsValid);

        if (!hasFailures)
        {
            return await next(message, cancellationToken);
        }

        var errors = results
            .Where(r => !r.IsValid)
            .SelectMany(r => r.ToErrorOrErrors())
            .ToList();
        return (TResponse)(dynamic)errors;
    }
}

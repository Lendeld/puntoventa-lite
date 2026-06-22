using ErrorOr;
using FluentValidation;
using FluentValidation.Results;

namespace PuntoVenta.Application.Common.Validation;

public static class FluentValidationExtensions
{
    public static IRuleBuilderOptions<T, TProperty> WithError<T, TProperty>(
        this IRuleBuilderOptions<T, TProperty> rule,
        Error error)
    {
        return rule
            .WithErrorCode(error.Code)
            .WithMessage(error.Description);
    }

    public static List<Error> ToErrorOrErrors(this ValidationResult validationResult)
    {
        return [.. validationResult.Errors.Select(error => Error.Validation(error.ErrorCode, error.ErrorMessage))];
    }
}

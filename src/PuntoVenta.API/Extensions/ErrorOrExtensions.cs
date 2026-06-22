using ErrorOr;
using Microsoft.AspNetCore.Http;

namespace PuntoVenta.API.Extensions;

public sealed record ApiErrorResponse(
    int Status,
    string Type,
    string Title,
    Dictionary<string, string> Errors,
    string Severity);

public static class ErrorOrExtensions
{
    public static IResult ToProblem(this List<Error> errors)
    {
        var firstError = errors[0];

        var (status, type, title) = firstError.Type switch
        {
            ErrorType.Validation => (StatusCodes.Status400BadRequest, "Validation", "Error de validación"),
            ErrorType.Conflict => (StatusCodes.Status409Conflict, "Conflict", "Conflicto"),
            ErrorType.NotFound => (StatusCodes.Status404NotFound, "NotFound", "No encontrado"),
            ErrorType.Unauthorized => (StatusCodes.Status401Unauthorized, "Unauthorized", "No autorizado"),
            ErrorType.Forbidden => (StatusCodes.Status403Forbidden, "Forbidden", "Acceso denegado"),
            _ => (StatusCodes.Status500InternalServerError, "ServerError", "Error interno")
        };

        var apiErrors = errors.ToDictionary(e => e.Code, e => e.Description);

        var severity = ResolveSeverity(errors);
        if (severity == "warning")
        {
            title = "Advertencia";
        }

        var response = new ApiErrorResponse(status, type, title, apiErrors, severity);

        return Results.Json(response, statusCode: status);
    }

    private static string ResolveSeverity(List<Error> errors)
    {
        foreach (var error in errors)
        {
            if (error.Metadata is { } metadata
                && metadata.TryGetValue("severity", out var value)
                && value is string severity
                && !string.IsNullOrWhiteSpace(severity))
            {
                return severity;
            }
        }
        return "error";
    }
}

using Microsoft.AspNetCore.Mvc;

namespace Sgip.WebApi.Common;

/// <summary>
/// Único lugar que arma un ProblemDetails con forma consistente. Para que el frontend
/// reciba siempre la misma estructura sin importar de dónde vino el error.
/// </summary>
public static class ApiProblemDetailsFactory
{
    private static IHttpContextAccessor? _accessor;

    public static void Configure(IHttpContextAccessor accessor)
    {
        _accessor = accessor;
    }

    public static ProblemDetails Build(
        int statusCode,
        string title,
        string? detail = null,
        IDictionary<string, string[]>? errors = null)
    {
        var httpContext = _accessor?.HttpContext;

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = httpContext?.Request.Path,
        };

        if (httpContext != null)
            problem.Extensions["traceId"] = httpContext.TraceIdentifier;

        if (errors is { Count: > 0 })
            problem.Extensions["errors"] = errors;

        return problem;
    }

    /// <summary>
    /// Traduce el mensaje técnico de System.Text.Json a algo legible para
    /// el cliente de la API, sin perder de vista el campo que falló.
    /// </summary>
    public static string HumanizeBindingError(string rawMessage, string field)
    {
        var cleanField = field.TrimStart('$', '.');

        return rawMessage.Contains("could not be converted")
            ? $"El valor enviado para '{cleanField}' no tiene el tipo correcto."
            : rawMessage.Contains("is required")
                ? $"El campo '{cleanField}' es requerido."
                : rawMessage;
    }
}
using Microsoft.AspNetCore.Mvc;
using Sgip.Domain.Exceptions;
using Sgip.WebApi.Common;

namespace Sgip.WebApi.Middleware;

public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            var (statusCode, title) = ex switch
            {
                ConflictException => (StatusCodes.Status409Conflict, "Conflicto de estado"),
                BusinessRuleException => (StatusCodes.Status422UnprocessableEntity, "Regla de negocio violada"),
                _ => (StatusCodes.Status500InternalServerError, "Error interno"),
            };

            if (statusCode == StatusCodes.Status500InternalServerError)
                _logger.LogError(ex, "Error no controlado en {Method} {Path}", context.Request.Method, context.Request.Path);
            else
                _logger.LogWarning("{Title}: {Message} ({Method} {Path})", title, ex.Message, context.Request.Method, context.Request.Path);

            var problem = ApiProblemDetailsFactory.Build(statusCode, title, detail: ex.Message);

            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/problem+json";
            await context.Response.WriteAsJsonAsync(problem);
        }
    }
}
using Microsoft.AspNetCore.Mvc;
using Sgip.Domain.Common;

namespace Sgip.WebApi.Common;

public static class ResultExtensions
{
    public static IActionResult ToActionResult<T>(this Result<T> result, Func<T, IActionResult>? onSuccess = null)
    {
        if (result.IsSuccess)
            return onSuccess != null ? onSuccess(result.Value!) : new OkObjectResult(result.Value);

        var (status, title) = result.Error!.Type switch
        {
            ErrorType.NotFound => (StatusCodes.Status404NotFound, "Recurso no encontrado"),
            ErrorType.Conflict => (StatusCodes.Status409Conflict, "Conflicto de estado"),
            ErrorType.Validation => (StatusCodes.Status422UnprocessableEntity, "Regla de negocio violada"),
            _ => (StatusCodes.Status500InternalServerError, "Error interno"),
        };

        var problem = ApiProblemDetailsFactory.Build(status, title, detail: result.Error.Message);
        problem.Extensions["code"] = result.Error.Code;

        return new ObjectResult(problem) { StatusCode = status };
    }
}
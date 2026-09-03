using Microsoft.AspNetCore.Mvc;
using Sgip.WebApi.Common;

namespace Sgip.WebApi.Extensions;

public static class ApiBehaviorExtensions
{
    /// <summary>
    /// Reemplaza la respuesta automática de [ApiController] ante JSON
    /// malformado / tipos inválidos / campos faltantes en el binding, para
    /// que use el mismo formato de ProblemDetails que el resto de la API
    /// </summary>
    public static IMvcBuilder ConfigureInvalidModelStateResponse(this IMvcBuilder builder)
    {
        builder.Services.Configure<ApiBehaviorOptions>(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var errors = context.ModelState
                    .Where(kvp => kvp.Value?.Errors.Count > 0)
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value!.Errors
                            .Select(e => ApiProblemDetailsFactory.HumanizeBindingError(e.ErrorMessage, kvp.Key))
                            .ToArray());

                var problem = ApiProblemDetailsFactory.Build(
                    StatusCodes.Status400BadRequest,
                    title: "Solicitud inválida",
                    detail: "El cuerpo de la solicitud no es JSON válido o falta algún campo requerido.",
                    errors: errors);

                return new BadRequestObjectResult(problem)
                {
                    ContentTypes = { "application/problem+json" }
                };
            };
        });

        return builder;
    }
}
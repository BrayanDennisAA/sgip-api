using System.Reflection;
using FluentValidation;
using Microsoft.OpenApi;
using Sgip.Application.Services;
using Sgip.Application.Services.Interfaces;
using Sgip.Domain.Strategies;
using Sgip.Infrastructure;
using Sgip.WebApi.Extensions;
using Sgip.WebApi.Validations;

namespace Sgip.WebApi;

public static class DependencyInjection
{
    public static IServiceCollection AddWebServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddInfrastructure(configuration);

        // --- Servicios ---
        services.AddScoped<ILoanService, LoanService>();
        services.AddScoped<ITransactionService, TransactionService>();

        // --- Strategies ---
        services.AddScoped<IInstallmentStrategy, FixedInstallmentStrategy>();
        services.AddScoped<IInstallmentStrategy, DecreasingInstallmentStrategy>();
        services.AddScoped<IInstallmentStrategyFactory, InstallmentStrategyFactory>();


        services.AddOpenApi();

        services.AddControllers(options =>
        {
            options.Filters.Add<ValidationFilter>();
        })
        .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter()))
        .ConfigureInvalidModelStateResponse();
        
        services.AddValidatorsFromAssemblyContaining<Program>();

        services.AddEndpointsApiExplorer();
        
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "SGIP API - Sistema de Gestión de Inversiones y Préstamos",
                Version = "v1",
                Description = "API de simulación, solicitud y aprobación de préstamos, y gestión de transacciones con idempotencia."
            });

            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            c.IncludeXmlComments(xmlPath);

        });

        services.AddCors(options =>
        {
            options.AddPolicy("AllowFrontend", policy =>
                policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
        });

        services.AddHttpContextAccessor();

        return services;
    }
}
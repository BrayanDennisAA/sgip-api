using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sgip.Application.Repositories.Interfaces;
using Sgip.Infrastructure.Data;
using Sgip.Infrastructure.Repositories;

namespace Sgip.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = BuildConnectionString(configuration);

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString));

        // --- Repositorios ---
        services.AddScoped<ILoanRepository, LoanRepository>();
        services.AddScoped<ITransactionRepository, TransactionRepository>();

        return services;
    }

    private static string BuildConnectionString(IConfiguration configuration)
    {
        var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
        if (string.IsNullOrWhiteSpace(databaseUrl))
            return configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("No se encontró cadena de conexión.");

        var uri = new Uri(databaseUrl);
        var userInfo = uri.UserInfo.Split(':');

        return $"Host={uri.Host};Port={uri.Port};Database={uri.AbsolutePath.TrimStart('/')};" +
               $"Username={userInfo[0]};Password={userInfo[1]};SSL Mode=Require;Trust Server Certificate=true";
    }
}
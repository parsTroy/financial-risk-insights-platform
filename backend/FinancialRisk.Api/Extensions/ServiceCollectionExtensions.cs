using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using FinancialRisk.Api.Data;
using FinancialRisk.Api.Services;

namespace FinancialRisk.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFinancialRiskDatabase(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        // Validate connection string
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        }

        // Add Entity Framework Core with PostgreSQL
        services.AddDbContext<FinancialRiskDbContext>(options =>
        {
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                // Enable retry on failure for better resilience
                npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorCodesToAdd: null);
                
                // Enable detailed logging in development
                if (environment.IsDevelopment())
                {
                    // Note: EnableDetailedErrors is not available on NpgsqlDbContextOptionsBuilder in EF Core 9.0
                    // Detailed errors are enabled at the DbContext level below
                }
            });
            
            // Enable sensitive data logging only in development
            if (environment.IsDevelopment())
            {
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
            }
        });

        // Add health checks including database health check
        services.AddHealthChecks()
            .AddNpgSql(connectionString, name: "postgresql");

        // Register database services
        services.AddScoped<IDatabaseMigrationService, DatabaseMigrationService>();
        services.AddScoped<DataSeederService>();

        return services;
    }
}

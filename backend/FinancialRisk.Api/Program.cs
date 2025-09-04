using dotenv.net;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using FinancialRisk.Api.Data;
using FinancialRisk.Api.Services;
using FinancialRisk.Api.Extensions;

// Load environment variables from .env file
DotEnv.Load();

var builder = WebApplication.CreateBuilder(args);

// Add environment variables to configuration
builder.Configuration.AddEnvironmentVariables();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddControllers(); // Add this line to register controllers
builder.Services.AddOpenApi();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5000", "https://localhost:5000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Add Financial Risk Database with EF Core and PostgreSQL
// Temporarily disabled for testing financial API without database
// builder.Services.AddFinancialRiskDatabase(builder.Configuration, builder.Environment);

// Configure financial API settings with environment variable support
builder.Services.Configure<FinancialRisk.Api.Models.FinancialApiConfig>(options =>
{
    options.ApiKey = Environment.GetEnvironmentVariable("ALPHA_VANTAGE_API_KEY") ?? "";
    options.BaseUrl = Environment.GetEnvironmentVariable("FINANCIAL_API_BASE_URL") ?? "https://www.alphavantage.co/";
    options.RequestTimeoutSeconds = int.TryParse(Environment.GetEnvironmentVariable("FINANCIAL_API_TIMEOUT_SECONDS"), out var timeout) ? timeout : 30;
    options.MaxRequestsPerMinute = int.TryParse(Environment.GetEnvironmentVariable("FINANCIAL_API_MAX_REQUESTS_PER_MINUTE"), out var rateLimit) ? rateLimit : 5;
    options.Provider = Environment.GetEnvironmentVariable("FINANCIAL_API_PROVIDER") ?? "AlphaVantage";
});

// Register HTTP client for financial API
// builder.Services.AddHttpClient<FinancialRisk.Api.Services.IFinancialDataService, FinancialRisk.Api.Services.AlphaVantageService>();

// Register financial data service
// builder.Services.AddScoped<FinancialRisk.Api.Services.IFinancialDataService, FinancialRisk.Api.Services.AlphaVantageService>();

// Register data persistence service (temporarily disabled for testing)
// builder.Services.AddScoped<FinancialRisk.Api.Services.IDataPersistenceService, FinancialRisk.Api.Services.DataPersistenceService>();

// Register risk metrics service (temporarily disabled for testing)
// builder.Services.AddScoped<FinancialRisk.Api.Services.IRiskMetricsService, FinancialRisk.Api.Services.RiskMetricsService>();

// Register VaR calculation service (temporarily disabled for testing)
// builder.Services.AddScoped<FinancialRisk.Api.Services.IVaRCalculationService, FinancialRisk.Api.Services.VaRCalculationService>();

// Register portfolio optimization service (temporarily disabled for testing)
// builder.Services.AddScoped<FinancialRisk.Api.Services.IPortfolioOptimizationService, FinancialRisk.Api.Services.PortfolioOptimizationService>();

// Register HTTP client factory
builder.Services.AddHttpClient();

// Register health checks
builder.Services.AddHealthChecks();

// Register portfolio builder service
builder.Services.AddScoped<FinancialRisk.Api.Services.IPortfolioBuilderService, FinancialRisk.Api.Services.PortfolioBuilderService>();

// Register Python/C++ interop services (temporarily disabled for testing)
/*
builder.Services.Configure<FinancialRisk.Api.Services.InteropConfiguration>(options =>
{
    options.EnablePythonNet = true;
    options.EnableGrpc = true;
    options.EnableCpp = true;
    options.PreferredMethod = "auto";
    options.EnableFallback = true;
    options.EnableMetricsAggregation = true;
});

builder.Services.AddScoped<FinancialRisk.Api.Services.PythonInteropService>();
builder.Services.AddScoped<FinancialRisk.Api.Services.GrpcPythonService>();
builder.Services.AddScoped<FinancialRisk.Api.Services.CppInteropService>();
builder.Services.AddScoped<FinancialRisk.Api.Services.IPythonInteropService, FinancialRisk.Api.Services.UnifiedInteropService>();
*/

var app = builder.Build();

// Log configuration to verify environment variables are loaded
var logger = app.Services.GetRequiredService<ILogger<Program>>();
var config = app.Services.GetRequiredService<IOptions<FinancialRisk.Api.Models.FinancialApiConfig>>();
logger.LogInformation("Financial API Configuration:");
logger.LogInformation("  Provider: {Provider}", config.Value.Provider);
logger.LogInformation("  Base URL: {BaseUrl}", config.Value.BaseUrl);
logger.LogInformation("  Timeout: {Timeout}s", config.Value.RequestTimeoutSeconds);
logger.LogInformation("  Rate Limit: {RateLimit}/min", config.Value.MaxRequestsPerMinute);
logger.LogInformation("  API Key: {ApiKey}", string.IsNullOrEmpty(config.Value.ApiKey) ? "NOT SET" : "SET (length: " + config.Value.ApiKey.Length + ")");

// Log database connection information
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
logger.LogInformation("Database Configuration:");
logger.LogInformation("  Connection String: {ConnectionString}", 
    connectionString?.Replace("Password=postgres", "Password=***") ?? "NOT SET");

// Ensure database is created and seeded (only in non-testing environments)
if (!builder.Environment.IsEnvironment("Test"))
{
    // Temporarily comment out database initialization to test financial API
    /*
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<FinancialRiskDbContext>();
        var seeder = scope.ServiceProvider.GetRequiredService<DataSeederService>();
        
        try
        {
            // Use migration service to ensure database is properly initialized
            var migrationService = scope.ServiceProvider.GetRequiredService<IDatabaseMigrationService>();
            await migrationService.MigrateAsync();
            logger.LogInformation("Database migration completed successfully");
            
            // Seed data
            await seeder.SeedDataAsync();
            logger.LogInformation("Database seeded successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while creating/seeding the PostgreSQL database");
            throw; // Re-throw to prevent app from starting with database issues
        }
    }
    */
    logger.LogInformation("Database initialization temporarily disabled for testing");
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// app.UseHttpsRedirection();

// Add CORS middleware
app.UseCors("AllowFrontend");

// Add health check endpoint
app.MapHealthChecks("/health");

// Add this line to enable controller routing
// Temporarily disable all controllers to test financial API without database
app.MapControllers();

// Map controllers for testing
app.MapControllerRoute(
    name: "financialData",
    pattern: "api/financialdata/{action}/{id?}",
    defaults: new { controller = "FinancialData" });

app.MapControllerRoute(
    name: "riskMetrics",
    pattern: "api/riskmetrics/{action}/{id?}",
    defaults: new { controller = "RiskMetrics" });

app.MapControllerRoute(
    name: "var",
    pattern: "api/var/{action}/{id?}",
    defaults: new { controller = "VaR" });

app.MapControllerRoute(
    name: "monteCarlo",
    pattern: "api/montecarlo/{action}/{id?}",
    defaults: new { controller = "MonteCarlo" });

app.MapControllerRoute(
    name: "portfolioOptimization",
    pattern: "api/portfolio/{action}/{id?}",
    defaults: new { controller = "PortfolioOptimization" });

app.MapControllerRoute(
    name: "interop",
    pattern: "api/interop/{action}/{id?}",
    defaults: new { controller = "Interop" });

app.MapControllerRoute(
    name: "portfolioBuilder",
    pattern: "api/portfoliobuilder/{action}/{id?}",
    defaults: new { controller = "PortfolioBuilder" });

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

// Start the application
app.Run();

// Make Program accessible for integration testing
public partial class Program { }

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

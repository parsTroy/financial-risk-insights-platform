using dotenv.net;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using FinancialRisk.Api.Data;
using FinancialRisk.Api.Services;

// Load environment variables from .env file
DotEnv.Load();

var builder = WebApplication.CreateBuilder(args);

// Add environment variables to configuration
builder.Configuration.AddEnvironmentVariables();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddControllers(); // Add this line to register controllers
builder.Services.AddOpenApi();

// Add Entity Framework Core with PostgreSQL
builder.Services.AddDbContext<FinancialRiskDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

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
builder.Services.AddHttpClient<FinancialRisk.Api.Services.IFinancialDataService, FinancialRisk.Api.Services.AlphaVantageService>();

// Register financial data service
builder.Services.AddScoped<FinancialRisk.Api.Services.IFinancialDataService, FinancialRisk.Api.Services.AlphaVantageService>();

// Register data seeder service
builder.Services.AddScoped<DataSeederService>();

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

// Ensure database is created and seeded
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<FinancialRiskDbContext>();
    var seeder = scope.ServiceProvider.GetRequiredService<DataSeederService>();
    
    try
    {
        // Seed data (this will also ensure database is created)
        await seeder.SeedDataAsync();
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while creating/seeding the PostgreSQL database");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// app.UseHttpsRedirection();

// Add this line to enable controller routing
app.MapControllers();

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

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

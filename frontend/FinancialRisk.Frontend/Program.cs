using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using FinancialRisk.Frontend;
using FinancialRisk.Frontend.Services;
using System.Text.Json;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Configure API settings
var apiSettings = builder.Configuration.GetSection("ApiSettings").Get<ApiConfiguration>() ?? new ApiConfiguration();
if (string.IsNullOrEmpty(apiSettings.BaseUrl))
{
    apiSettings.BaseUrl = "http://localhost:7001/api";
}

// Configure HTTP client
builder.Services.AddHttpClient("ApiClient", client =>
{
    client.BaseAddress = new Uri(apiSettings.BaseUrl);
    client.Timeout = TimeSpan.FromMilliseconds(apiSettings.Timeout);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.DefaultRequestHeaders.Add("User-Agent", "FinancialRisk-Frontend/1.0");
});

// Register services
builder.Services.AddScoped<ApiService>(provider =>
{
    var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();
    var httpClient = httpClientFactory.CreateClient("ApiClient");
    var logger = provider.GetRequiredService<ILogger<ApiService>>();
    return new ApiService(httpClient, logger);
});
builder.Services.AddScoped<VaRApiService>();
builder.Services.AddScoped<PortfolioApiService>();
builder.Services.AddScoped<PortfolioBuilderApiService>();
builder.Services.AddScoped<RiskMetricsApiService>();

// Configure JSON serialization
builder.Services.Configure<JsonSerializerOptions>(options =>
{
    options.PropertyNameCaseInsensitive = true;
    options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.WriteIndented = true;
});

// Configure logging
builder.Services.AddLogging(logging =>
{
    logging.SetMinimumLevel(LogLevel.Information);
});

await builder.Build().RunAsync();

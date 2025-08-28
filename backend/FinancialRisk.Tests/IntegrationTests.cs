using FinancialRisk.Api;
using FinancialRisk.Api.Data;
using FinancialRisk.Api.Services;
using FinancialRisk.Api.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System.Net;
using System.Text;
using System.Text.Json;
using Xunit;

namespace FinancialRisk.Tests
{
    public class IntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly Mock<IFinancialDataService> _mockFinancialDataService;

        public IntegrationTests(WebApplicationFactory<Program> factory)
        {
            _mockFinancialDataService = new Mock<IFinancialDataService>();
            
            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Replace the real service with mock
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(IFinancialDataService));
                    if (descriptor != null)
                    {
                        services.Remove(descriptor);
                    }
                    services.AddSingleton(_mockFinancialDataService.Object);

                    // Use in-memory database for testing
                    var dbContextDescriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<FinancialRiskDbContext>));
                    if (dbContextDescriptor != null)
                    {
                        services.Remove(dbContextDescriptor);
                    }

                    services.AddDbContext<FinancialRiskDbContext>(options =>
                    {
                        options.UseInMemoryDatabase("TestDb_" + Guid.NewGuid().ToString());
                    });

                    // Remove the real seeder service
                    var seederDescriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DataSeederService));
                    if (seederDescriptor != null)
                    {
                        services.Remove(seederDescriptor);
                    }
                });

                builder.ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddConsole();
                });
            });
        }

        [Fact]
        public async Task HealthCheck_ReturnsOk()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("/health");

            // Assert
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("Healthy", content);
        }

        [Fact]
        public async Task GetAssets_ReturnsOkWithData()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("/api/Assets");

            // Assert
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("AAPL", content);
            Assert.Contains("MSFT", content);
        }

        [Fact]
        public async Task GetAsset_WithValidId_ReturnsOk()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("/api/Assets/1");

            // Assert
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("AAPL", content);
        }

        [Fact]
        public async Task GetAsset_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("/api/Assets/999");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetAssetBySymbol_WithValidSymbol_ReturnsOk()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("/api/Assets/symbol/AAPL");

            // Assert
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("AAPL", content);
        }

        [Fact]
        public async Task GetAssetBySymbol_WithInvalidSymbol_ReturnsNotFound()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("/api/Assets/symbol/INVALID");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetPortfolios_ReturnsOkWithData()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("/api/Portfolios");

            // Assert
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("Conservative", content);
            Assert.Contains("Balanced", content);
            Assert.Contains("Aggressive", content);
        }

        [Fact]
        public async Task GetPortfolio_WithValidId_ReturnsOk()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("/api/Portfolios/1");

            // Assert
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("Conservative", content);
        }

        [Fact]
        public async Task GetPortfolio_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("/api/Portfolios/999");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetPortfolioPerformance_WithValidId_ReturnsOk()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("/api/Portfolios/1/performance");

            // Assert
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("totalValue", content);
        }

        [Fact]
        public async Task GetPortfolioPerformance_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("/api/Portfolios/999/performance");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetStockQuote_WithMockedService_ReturnsExpectedData()
        {
            // Arrange
            var expectedResponse = new ApiResponse<StockQuote>
            {
                Success = true,
                Data = new StockQuote
                {
                    Symbol = "AAPL",
                    Open = 150.00m,
                    High = 155.00m,
                    Low = 149.00m,
                    Close = 152.50m,
                    Volume = 1000000,
                    Change = 2.50m,
                    ChangePercent = 1.67m,
                    Timestamp = DateTime.UtcNow
                },
                StatusCode = 200
            };

            _mockFinancialDataService
                .Setup(x => x.GetStockQuoteAsync("AAPL"))
                .ReturnsAsync(expectedResponse);

            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("/api/FinancialData/stock/AAPL");

            // Assert
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ApiResponse<StockQuote>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal("AAPL", result.Data!.Symbol);
            Assert.Equal(152.50m, result.Data.Close);
        }

        [Fact]
        public async Task GetStockQuote_WithMockedServiceFailure_ReturnsError()
        {
            // Arrange
            var failedResponse = new ApiResponse<StockQuote>
            {
                Success = false,
                Data = null,
                ErrorMessage = "API request failed",
                StatusCode = 500
            };

            _mockFinancialDataService
                .Setup(x => x.GetStockQuoteAsync("INVALID"))
                .ReturnsAsync(failedResponse);

            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("/api/FinancialData/stock/INVALID");

            // Assert
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ApiResponse<StockQuote>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains("API request failed", result.ErrorMessage);
        }

        [Fact]
        public async Task GetCurrentPrice_WithMockedService_ReturnsExpectedData()
        {
            // Arrange
            var expectedResponse = new ApiResponse<decimal>
            {
                Success = true,
                Data = 152.50m,
                StatusCode = 200
            };

            _mockFinancialDataService
                .Setup(x => x.GetCurrentPriceAsync("AAPL"))
                .ReturnsAsync(expectedResponse);

            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("/api/FinancialData/stock/AAPL/price");

            // Assert
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ApiResponse<decimal>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(152.50m, result.Data);
        }

        [Fact]
        public async Task GetForexQuote_WithMockedService_ReturnsExpectedData()
        {
            // Arrange
            var expectedResponse = new ApiResponse<ForexQuote>
            {
                Success = true,
                Data = new ForexQuote
                {
                    FromCurrency = "USD",
                    ToCurrency = "EUR",
                    ExchangeRate = 0.85m,
                    Timestamp = DateTime.UtcNow
                },
                StatusCode = 200
            };

            _mockFinancialDataService
                .Setup(x => x.GetForexQuoteAsync("USD", "EUR"))
                .ReturnsAsync(expectedResponse);

            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("/api/FinancialData/forex/USD/EUR");

            // Assert
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ApiResponse<ForexQuote>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal("USD", result.Data!.FromCurrency);
            Assert.Equal("EUR", result.Data.ToCurrency);
            Assert.Equal(0.85m, result.Data.ExchangeRate);
        }

        [Fact]
        public async Task GetStockHistory_WithMockedService_ReturnsExpectedData()
        {
            // Arrange
            var expectedResponse = new ApiResponse<List<StockQuote>>
            {
                Success = true,
                Data = new List<StockQuote>
                {
                    new StockQuote { Symbol = "AAPL", Close = 150.00m, Timestamp = DateTime.Today.AddDays(-1) },
                    new StockQuote { Symbol = "AAPL", Close = 152.50m, Timestamp = DateTime.Today }
                },
                StatusCode = 200
            };

            _mockFinancialDataService
                .Setup(x => x.GetStockHistoryAsync("AAPL", 5))
                .ReturnsAsync(expectedResponse);

            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("/api/FinancialData/stock/AAPL/history?days=5");

            // Assert
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ApiResponse<List<StockQuote>>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data!.Count);
            Assert.Equal("AAPL", result.Data[0].Symbol);
        }

        [Fact]
        public async Task GetStockQuote_WithNullSymbol_ReturnsBadRequest()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("/api/FinancialData/stock/");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetForexQuote_WithInvalidCurrencies_ReturnsBadRequest()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("/api/FinancialData/forex//EUR");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}

using FinancialRisk.Api.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Text;
using System.Text.Json;
using Xunit;

namespace FinancialRisk.Tests
{
    public class RiskMetricsApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public RiskMetricsApiIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task GetAssetRiskMetrics_WithValidSymbol_ReturnsOk()
        {
            // Arrange
            var symbol = "AAPL";
            var url = $"/api/riskmetrics/asset/{symbol}?days=30";

            // Act
            var response = await _client.GetAsync(url);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            
            var content = await response.Content.ReadAsStringAsync();
            Assert.NotEmpty(content);
            
            var riskMetrics = JsonSerializer.Deserialize<RiskMetrics>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            
            Assert.NotNull(riskMetrics);
            Assert.Equal(symbol, riskMetrics.Symbol);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task GetAssetRiskMetrics_WithInvalidSymbol_ReturnsBadRequest()
        {
            // Arrange
            var symbol = "";
            var url = $"/api/riskmetrics/asset/{symbol}";

            // Act
            var response = await _client.GetAsync(url);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task GetAssetRiskMetrics_WithInvalidDays_ReturnsBadRequest()
        {
            // Arrange
            var symbol = "AAPL";
            var url = $"/api/riskmetrics/asset/{symbol}?days=10"; // Too few days

            // Act
            var response = await _client.GetAsync(url);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task GetMultipleAssetRiskMetrics_WithValidSymbols_ReturnsOk()
        {
            // Arrange
            var symbols = new List<string> { "AAPL", "MSFT", "GOOGL" };
            var url = "/api/riskmetrics/assets/batch?days=30";
            var json = JsonSerializer.Serialize(symbols);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync(url, content);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            
            var responseContent = await response.Content.ReadAsStringAsync();
            Assert.NotEmpty(responseContent);
            
            var riskMetricsList = JsonSerializer.Deserialize<List<RiskMetrics>>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            
            Assert.NotNull(riskMetricsList);
            Assert.Equal(symbols.Count, riskMetricsList.Count);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task GetMultipleAssetRiskMetrics_WithEmptySymbols_ReturnsBadRequest()
        {
            // Arrange
            var symbols = new List<string>();
            var url = "/api/riskmetrics/assets/batch";
            var json = JsonSerializer.Serialize(symbols);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync(url, content);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task GetMultipleAssetRiskMetrics_WithTooManySymbols_ReturnsBadRequest()
        {
            // Arrange
            var symbols = Enumerable.Range(1, 51).Select(i => $"SYMBOL{i}").ToList(); // 51 symbols
            var url = "/api/riskmetrics/assets/batch";
            var json = JsonSerializer.Serialize(symbols);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync(url, content);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task GetPortfolioRiskMetrics_WithValidRequest_ReturnsOk()
        {
            // Arrange
            var request = new PortfolioRiskMetricsRequest
            {
                Symbols = new List<string> { "AAPL", "MSFT" },
                Weights = new List<decimal> { 0.6m, 0.4m },
                Days = 30
            };
            var url = "/api/riskmetrics/portfolio";
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync(url, content);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            
            var responseContent = await response.Content.ReadAsStringAsync();
            Assert.NotEmpty(responseContent);
            
            var portfolioMetrics = JsonSerializer.Deserialize<PortfolioRiskMetrics>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            
            Assert.NotNull(portfolioMetrics);
            Assert.Equal(request.Symbols, portfolioMetrics.Symbols);
            Assert.Equal(request.Weights, portfolioMetrics.Weights);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task GetPortfolioRiskMetrics_WithMismatchedSymbolsAndWeights_ReturnsBadRequest()
        {
            // Arrange
            var request = new PortfolioRiskMetricsRequest
            {
                Symbols = new List<string> { "AAPL", "MSFT" },
                Weights = new List<decimal> { 0.6m }, // Mismatched count
                Days = 30
            };
            var url = "/api/riskmetrics/portfolio";
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync(url, content);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task GetPortfolioRiskMetrics_WithInvalidWeights_ReturnsBadRequest()
        {
            // Arrange
            var request = new PortfolioRiskMetricsRequest
            {
                Symbols = new List<string> { "AAPL", "MSFT" },
                Weights = new List<decimal> { 0.6m, 0.6m }, // Weights don't sum to 1.0
                Days = 30
            };
            var url = "/api/riskmetrics/portfolio";
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync(url, content);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task CompareRiskMetrics_WithValidSymbols_ReturnsOk()
        {
            // Arrange
            var symbols = new List<string> { "AAPL", "MSFT", "GOOGL" };
            var url = $"/api/riskmetrics/compare?symbols={string.Join("&symbols=", symbols)}&days=30";

            // Act
            var response = await _client.GetAsync(url);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            
            var responseContent = await response.Content.ReadAsStringAsync();
            Assert.NotEmpty(responseContent);
            
            var comparison = JsonSerializer.Deserialize<object>(responseContent);
            Assert.NotNull(comparison);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task CompareRiskMetrics_WithTooManySymbols_ReturnsBadRequest()
        {
            // Arrange
            var symbols = Enumerable.Range(1, 21).Select(i => $"SYMBOL{i}").ToList(); // 21 symbols
            var url = $"/api/riskmetrics/compare?symbols={string.Join("&symbols=", symbols)}&days=30";

            // Act
            var response = await _client.GetAsync(url);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task CompareRiskMetrics_WithEmptySymbols_ReturnsBadRequest()
        {
            // Arrange
            var url = "/api/riskmetrics/compare?days=30";

            // Act
            var response = await _client.GetAsync(url);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task GetAssetRiskMetrics_WithTooManyDays_ReturnsBadRequest()
        {
            // Arrange
            var symbol = "AAPL";
            var url = $"/api/riskmetrics/asset/{symbol}?days=1001"; // Too many days

            // Act
            var response = await _client.GetAsync(url);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task GetPortfolioRiskMetrics_WithTooManyAssets_ReturnsBadRequest()
        {
            // Arrange
            var request = new PortfolioRiskMetricsRequest
            {
                Symbols = Enumerable.Range(1, 51).Select(i => $"SYMBOL{i}").ToList(), // 51 assets
                Weights = Enumerable.Range(1, 51).Select(i => 1m / 51).ToList(),
                Days = 30
            };
            var url = "/api/riskmetrics/portfolio";
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync(url, content);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}

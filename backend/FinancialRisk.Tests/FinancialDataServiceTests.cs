using FinancialRisk.Api.Models;
using FinancialRisk.Api.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using System.Net;
using Xunit;

namespace FinancialRisk.Tests
{
    public class FinancialDataServiceTests
    {
        private readonly Mock<ILogger<AlphaVantageService>> _mockLogger;
        private readonly FinancialApiConfig _config;
        private readonly Mock<HttpMessageHandler> _mockHttpHandler;

        public FinancialDataServiceTests()
        {
            _mockLogger = new Mock<ILogger<AlphaVantageService>>();
            _config = new FinancialApiConfig
            {
                ApiKey = "test-key",
                BaseUrl = "https://test.api.com/",
                RequestTimeoutSeconds = 30,
                MaxRequestsPerMinute = 5,
                Provider = "AlphaVantage"
            };
            _mockHttpHandler = new Mock<HttpMessageHandler>();
        }

        [Fact]
        public void Constructor_InitializesCorrectly()
        {
            // Arrange & Act
            var httpClient = new HttpClient(_mockHttpHandler.Object);
            var service = new AlphaVantageService(httpClient, _mockLogger.Object, Options.Create(_config));

            // Assert
            Assert.NotNull(service);
        }

        [Fact]
        public async Task GetStockQuoteAsync_WithValidResponse_ReturnsSuccess()
        {
            // Arrange
            var expectedResponse = @"{""Global Quote"":{""01. symbol"":""AAPL"",""02. open"":""150.00"",""03. high"":""155.00"",""04. low"":""149.00"",""05. price"":""152.50"",""06. volume"":""1000000"",""09. change"":""2.50"",""10. change percent"":""1.67%""}}";
            
            _mockHttpHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(expectedResponse)
                });

            var httpClient = new HttpClient(_mockHttpHandler.Object);
            var service = new AlphaVantageService(httpClient, _mockLogger.Object, Options.Create(_config));

            // Act
            var result = await service.GetStockQuoteAsync("AAPL");

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal("AAPL", result.Data.Symbol);
            Assert.Equal(152.50m, result.Data.Close);
            Assert.Equal(200, result.StatusCode);
        }

        [Fact]
        public async Task GetStockQuoteAsync_WithApiError_ReturnsFailure()
        {
            // Arrange
            _mockHttpHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.TooManyRequests,
                    Content = new StringContent("Rate limit exceeded")
                });

            var httpClient = new HttpClient(_mockHttpHandler.Object);
            var service = new AlphaVantageService(httpClient, _mockLogger.Object, Options.Create(_config));

            // Act
            var result = await service.GetStockQuoteAsync("AAPL");

            // Assert
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.Equal(429, result.StatusCode);
            Assert.Contains("API request failed", result.ErrorMessage);
        }

        [Fact]
        public async Task GetForexQuoteAsync_WithValidResponse_ReturnsSuccess()
        {
            // Arrange
            var expectedResponse = @"{""Realtime Currency Exchange Rate"":{""1. From_Currency Code"":""USD"",""2. From_Currency Name"":""United States Dollar"",""3. To_Currency Code"":""EUR"",""4. To_Currency Name"":""Euro"",""5. Exchange Rate"":""0.85"",""6. Last Refreshed"":""2024-01-01 00:00:00"",""7. Time Zone"":""UTC"",""8. Bid Price"":""0.85"",""9. Ask Price"":""0.85""}}";
            
            _mockHttpHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(expectedResponse)
                });

            var httpClient = new HttpClient(_mockHttpHandler.Object);
            var service = new AlphaVantageService(httpClient, _mockLogger.Object, Options.Create(_config));

            // Act
            var result = await service.GetForexQuoteAsync("USD", "EUR");

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal("USD", result.Data.FromCurrency);
            Assert.Equal("EUR", result.Data.ToCurrency);
            Assert.Equal(0.85m, result.Data.ExchangeRate);
            Assert.Equal(200, result.StatusCode);
        }

        [Fact]
        public async Task GetCurrentPriceAsync_WithValidStockQuote_ReturnsPrice()
        {
            // Arrange
            var expectedResponse = @"{""Global Quote"":{""01. symbol"":""MSFT"",""02. open"":""300.00"",""03. high"":""305.00"",""04. low"":""298.00"",""05. price"":""302.50"",""06. volume"":""2000000"",""09. change"":""2.50"",""10. change percent"":""0.83%""}}";
            
            _mockHttpHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(expectedResponse)
                });

            var httpClient = new HttpClient(_mockHttpHandler.Object);
            var service = new AlphaVantageService(httpClient, _mockLogger.Object, Options.Create(_config));

            // Act
            var result = await service.GetCurrentPriceAsync("MSFT");

            // Assert
            Assert.True(result.Success);
            Assert.Equal(302.50m, result.Data);
            Assert.Equal(200, result.StatusCode);
        }
    }
}

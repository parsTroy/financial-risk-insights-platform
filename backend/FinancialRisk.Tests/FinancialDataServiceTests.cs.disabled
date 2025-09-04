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
        private readonly Mock<IDataPersistenceService> _mockDataPersistenceService;

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
            _mockDataPersistenceService = new Mock<IDataPersistenceService>();
        }

        [Fact]
        public void Constructor_InitializesCorrectly()
        {
            // Arrange & Act
            var httpClient = new HttpClient(_mockHttpHandler.Object);
            var service = new AlphaVantageService(httpClient, _mockLogger.Object, Options.Create(_config), _mockDataPersistenceService.Object);

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
            var service = new AlphaVantageService(httpClient, _mockLogger.Object, Options.Create(_config), _mockDataPersistenceService.Object);

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
            var service = new AlphaVantageService(httpClient, _mockLogger.Object, Options.Create(_config), _mockDataPersistenceService.Object);

            // Act
            var result = await service.GetStockQuoteAsync("AAPL");

            // Assert
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.Equal(429, result.StatusCode);
            Assert.Contains("API request failed", result.ErrorMessage);
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
            var service = new AlphaVantageService(httpClient, _mockLogger.Object, Options.Create(_config), _mockDataPersistenceService.Object);

            // Act
            var result = await service.GetCurrentPriceAsync("MSFT");

            // Assert
            Assert.True(result.Success);
            Assert.Equal(302.50m, result.Data);
            Assert.Equal(200, result.StatusCode);
        }

        // NEW COMPREHENSIVE API CONNECTION TESTS

        [Fact]
        public async Task GetStockQuoteAsync_WithNetworkFailure_ReturnsFailure()
        {
            // Arrange - Simulate network failure
            _mockHttpHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new HttpRequestException("Network connection failed"));

            var httpClient = new HttpClient(_mockHttpHandler.Object);
            var service = new AlphaVantageService(httpClient, _mockLogger.Object, Options.Create(_config), _mockDataPersistenceService.Object);

            // Act
            var result = await service.GetStockQuoteAsync("AAPL");

            // Assert
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.Equal(500, result.StatusCode);
            Assert.Contains("Network error occurred while fetching data", result.ErrorMessage);
        }

        [Fact]
        public async Task GetStockQuoteAsync_WithTimeout_ReturnsFailure()
        {
            // Arrange - Simulate timeout
            _mockHttpHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new TaskCanceledException("Request timeout"));

            var httpClient = new HttpClient(_mockHttpHandler.Object);
            var service = new AlphaVantageService(httpClient, _mockLogger.Object, Options.Create(_config), _mockDataPersistenceService.Object);

            // Act
            var result = await service.GetStockQuoteAsync("AAPL");

            // Assert
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.Equal(408, result.StatusCode); // Request Timeout
            Assert.Contains("Request timed out", result.ErrorMessage);
        }

        [Fact]
        public async Task GetStockQuoteAsync_WithMalformedJson_ReturnsFailure()
        {
            // Arrange - Malformed JSON response
            var malformedResponse = @"{""Global Quote"":{""01. symbol"":""AAPL"",""02. open"":""150.00"",""03. high"":""155.00"",""04. low"":""149.00"",""05. price"":""152.50"",""06. volume"":""1000000"",""09. change"":""2.50"",""10. change percent"":""1.67%"""; // Missing closing brace
            
            _mockHttpHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(malformedResponse)
                });

            var httpClient = new HttpClient(_mockHttpHandler.Object);
            var service = new AlphaVantageService(httpClient, _mockLogger.Object, Options.Create(_config), _mockDataPersistenceService.Object);

            // Act
            var result = await service.GetStockQuoteAsync("AAPL");

            // Assert
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.Equal(500, result.StatusCode);
            Assert.Contains("Failed to parse API response", result.ErrorMessage);
        }

        [Fact]
        public async Task GetStockQuoteAsync_WithEmptyResponse_ReturnsFailure()
        {
            // Arrange - Empty response
            _mockHttpHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("")
                });

            var httpClient = new HttpClient(_mockHttpHandler.Object);
            var service = new AlphaVantageService(httpClient, _mockLogger.Object, Options.Create(_config), _mockDataPersistenceService.Object);

            // Act
            var result = await service.GetStockQuoteAsync("AAPL");

            // Assert
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.Equal(500, result.StatusCode);
            Assert.Contains("Failed to parse API response", result.ErrorMessage);
        }

        [Fact]
        public async Task GetStockQuoteAsync_WithUnauthorized_ReturnsFailure()
        {
            // Arrange - Unauthorized (invalid API key)
            _mockHttpHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.Unauthorized,
                    Content = new StringContent("Invalid API key")
                });

            var httpClient = new HttpClient(_mockHttpHandler.Object);
            var service = new AlphaVantageService(httpClient, _mockLogger.Object, Options.Create(_config), _mockDataPersistenceService.Object);

            // Act
            var result = await service.GetStockQuoteAsync("AAPL");

            // Assert
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.Equal(401, result.StatusCode);
            Assert.Contains("API request failed", result.ErrorMessage);
        }

        [Fact]
        public async Task GetStockQuoteAsync_WithServerError_ReturnsFailure()
        {
            // Arrange - Server error
            _mockHttpHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Content = new StringContent("Internal server error")
                });

            var httpClient = new HttpClient(_mockHttpHandler.Object);
            var service = new AlphaVantageService(httpClient, _mockLogger.Object, Options.Create(_config), _mockDataPersistenceService.Object);

            // Act
            var result = await service.GetStockQuoteAsync("AAPL");

            // Assert
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.Equal(500, result.StatusCode);
            Assert.Contains("API request failed", result.ErrorMessage);
        }

        [Fact]
        public async Task GetStockQuoteAsync_WithRateLimitExceeded_ReturnsFailure()
        {
            // Arrange - Rate limit exceeded
            _mockHttpHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.TooManyRequests,
                    Content = new StringContent("Rate limit exceeded. Please try again later.")
                });

            var httpClient = new HttpClient(_mockHttpHandler.Object);
            var service = new AlphaVantageService(httpClient, _mockLogger.Object, Options.Create(_config), _mockDataPersistenceService.Object);

            // Act
            var result = await service.GetStockQuoteAsync("AAPL");

            // Assert
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.Equal(429, result.StatusCode);
            Assert.Contains("API request failed", result.ErrorMessage);
        }

        [Fact]
        public async Task GetStockQuoteAsync_WithInvalidSymbol_ReturnsFailure()
        {
            // Arrange - Invalid symbol response
            var invalidResponse = @"{""Error Message"":""Invalid API call. Please retry or visit the documentation (https://www.alphavantage.co/documentation/) for TIME_SERIES_DAILY.""}";
            
            _mockHttpHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(invalidResponse)
                });

            var httpClient = new HttpClient(_mockHttpHandler.Object);
            var service = new AlphaVantageService(httpClient, _mockLogger.Object, Options.Create(_config), _mockDataPersistenceService.Object);

            // Act
            var result = await service.GetStockQuoteAsync("INVALID");

            // Assert
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.Equal(500, result.StatusCode);
            Assert.Contains("Failed to parse API response", result.ErrorMessage);
        }



        [Fact]
        public async Task GetCurrentPriceAsync_WithNetworkFailure_ReturnsFailure()
        {
            // Arrange - Simulate network failure for current price
            _mockHttpHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new HttpRequestException("Network connection failed"));

            var httpClient = new HttpClient(_mockHttpHandler.Object);
            var service = new AlphaVantageService(httpClient, _mockLogger.Object, Options.Create(_config), _mockDataPersistenceService.Object);

            // Act
            var result = await service.GetCurrentPriceAsync("MSFT");

            // Assert
            Assert.False(result.Success);
            Assert.Equal(0, result.Data);
            Assert.Equal(500, result.StatusCode);
            Assert.Contains("Network error occurred while fetching data", result.ErrorMessage);
        }
    }
}

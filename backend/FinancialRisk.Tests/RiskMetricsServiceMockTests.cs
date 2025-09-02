using FinancialRisk.Api.Models;
using FinancialRisk.Api.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace FinancialRisk.Tests
{
    public class RiskMetricsServiceMockTests
    {
        private readonly Mock<ILogger<RiskMetricsService>> _mockLogger;
        private readonly Mock<IFinancialDataService> _mockFinancialDataService;
        private readonly Mock<IDataPersistenceService> _mockDataPersistenceService;

        public RiskMetricsServiceMockTests()
        {
            _mockLogger = new Mock<ILogger<RiskMetricsService>>();
            _mockFinancialDataService = new Mock<IFinancialDataService>();
            _mockDataPersistenceService = new Mock<IDataPersistenceService>();
        }

        [Fact]
        public async Task CalculateRiskMetricsAsync_WithNoData_ReturnsError()
        {
            // Arrange
            var symbol = "INVALID";
            var mockApiResponse = new ApiResponse<List<StockQuote>>
            {
                Success = false,
                Data = null
            };

            _mockFinancialDataService
                .Setup(x => x.GetStockHistoryAsync(symbol, It.IsAny<int>()))
                .ReturnsAsync(mockApiResponse);

            var service = new RiskMetricsService(_mockLogger.Object, _mockFinancialDataService.Object, _mockDataPersistenceService.Object);

            // Act
            var result = await service.CalculateRiskMetricsAsync(symbol);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(symbol, result.Symbol);
            Assert.NotNull(result.Error);
            Assert.Equal("No historical data available", result.Error);
        }

        [Fact]
        public async Task CalculateRiskMetricsAsync_WithInsufficientData_ReturnsError()
        {
            // Arrange
            var symbol = "AAPL";
            var mockStockQuotes = new List<StockQuote> { new StockQuote { Symbol = symbol, Close = 100, Timestamp = DateTime.UtcNow } };
            var mockApiResponse = new ApiResponse<List<StockQuote>>
            {
                Success = true,
                Data = mockStockQuotes
            };

            _mockFinancialDataService
                .Setup(x => x.GetStockHistoryAsync(symbol, It.IsAny<int>()))
                .ReturnsAsync(mockApiResponse);

            var service = new RiskMetricsService(_mockLogger.Object, _mockFinancialDataService.Object, _mockDataPersistenceService.Object);

            // Act
            var result = await service.CalculateRiskMetricsAsync(symbol);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(symbol, result.Symbol);
            Assert.NotNull(result.Error);
            Assert.Equal("Insufficient data for calculations", result.Error);
        }

        [Fact]
        public async Task CalculatePortfolioRiskMetricsAsync_WithMismatchedSymbolsAndWeights_ReturnsError()
        {
            // Arrange
            var symbols = new List<string> { "AAPL", "MSFT" };
            var weights = new List<decimal> { 0.6m }; // Mismatched count

            var service = new RiskMetricsService(_mockLogger.Object, _mockFinancialDataService.Object, _mockDataPersistenceService.Object);

            // Act
            var result = await service.CalculatePortfolioRiskMetricsAsync(symbols, weights);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Error);
            Assert.Equal("Number of symbols must match number of weights", result.Error);
        }

        [Fact]
        public async Task CalculatePortfolioRiskMetricsAsync_WithInsufficientData_ReturnsError()
        {
            // Arrange
            var symbols = new List<string> { "AAPL", "MSFT" };
            var weights = new List<decimal> { 0.6m, 0.4m };

            _mockFinancialDataService
                .Setup(x => x.GetStockHistoryAsync(It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(new ApiResponse<List<StockQuote>> { Success = false, Data = null });

            var service = new RiskMetricsService(_mockLogger.Object, _mockFinancialDataService.Object, _mockDataPersistenceService.Object);

            // Act
            var result = await service.CalculatePortfolioRiskMetricsAsync(symbols, weights);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Error);
            Assert.Equal("Insufficient asset data", result.Error);
        }

        [Fact]
        public async Task CalculateMultipleAssetRiskMetricsAsync_WithEmptyList_ReturnsEmptyList()
        {
            // Arrange
            var symbols = new List<string>();
            var service = new RiskMetricsService(_mockLogger.Object, _mockFinancialDataService.Object, _mockDataPersistenceService.Object);

            // Act
            var result = await service.CalculateMultipleAssetRiskMetricsAsync(symbols);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task CalculateRiskMetricsAsync_WithException_ReturnsError()
        {
            // Arrange
            var symbol = "AAPL";
            _mockFinancialDataService
                .Setup(x => x.GetStockHistoryAsync(symbol, It.IsAny<int>()))
                .ThrowsAsync(new Exception("Test exception"));

            var service = new RiskMetricsService(_mockLogger.Object, _mockFinancialDataService.Object, _mockDataPersistenceService.Object);

            // Act
            var result = await service.CalculateRiskMetricsAsync(symbol);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(symbol, result.Symbol);
            Assert.NotNull(result.Error);
            Assert.Equal("Test exception", result.Error);
        }

        [Fact]
        public async Task CalculatePortfolioRiskMetricsAsync_WithException_ReturnsError()
        {
            // Arrange
            var symbols = new List<string> { "AAPL", "MSFT" };
            var weights = new List<decimal> { 0.6m, 0.4m };

            _mockFinancialDataService
                .Setup(x => x.GetStockHistoryAsync(It.IsAny<string>(), It.IsAny<int>()))
                .ThrowsAsync(new Exception("Test exception"));

            var service = new RiskMetricsService(_mockLogger.Object, _mockFinancialDataService.Object, _mockDataPersistenceService.Object);

            // Act
            var result = await service.CalculatePortfolioRiskMetricsAsync(symbols, weights);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Error);
            Assert.Equal("Test exception", result.Error);
        }
    }
}

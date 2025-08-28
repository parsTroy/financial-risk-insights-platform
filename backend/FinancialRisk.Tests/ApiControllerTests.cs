using FinancialRisk.Api.Controllers;
using FinancialRisk.Api.controllers;
using FinancialRisk.Api.Models;
using FinancialRisk.Api.Services;
using FinancialRisk.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace FinancialRisk.Tests
{
    public class ApiControllerTests : IDisposable
    {
        private readonly Mock<IFinancialDataService> _mockFinancialDataService;
        private readonly Mock<ILogger<FinancialDataController>> _mockLogger;
        private readonly Mock<ILogger<AssetsController>> _mockAssetsLogger;
        private readonly Mock<ILogger<PortfoliosController>> _mockPortfoliosLogger;
        private readonly FinancialRiskDbContext _dbContext; // Use real DbContext with in-memory database

        public ApiControllerTests()
        {
            _mockFinancialDataService = new Mock<IFinancialDataService>();
            _mockLogger = new Mock<ILogger<FinancialDataController>>();
            _mockAssetsLogger = new Mock<ILogger<AssetsController>>();
            _mockPortfoliosLogger = new Mock<ILogger<PortfoliosController>>();
            
            // Create in-memory database for testing
            var options = new DbContextOptionsBuilder<FinancialRiskDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _dbContext = new FinancialRiskDbContext(options);
            
            // Setup test data
            SetupTestData();
        }
        
        private void SetupTestData()
        {
            // Add test assets
            var asset1 = new Asset { Id = 1, Symbol = "AAPL", Name = "Apple Inc.", Sector = "Technology", Industry = "Consumer Electronics", AssetType = "Stock", CreatedAt = DateTime.UtcNow };
            var asset2 = new Asset { Id = 2, Symbol = "MSFT", Name = "Microsoft Corporation", Sector = "Technology", Industry = "Software", AssetType = "Stock", CreatedAt = DateTime.UtcNow };
            
            _dbContext.Assets.AddRange(asset1, asset2);
            
            // Add test portfolio
            var portfolio = new Portfolio { Id = 1, Name = "Test Portfolio", Description = "Test portfolio for unit tests", Strategy = "Growth", CreatedAt = DateTime.UtcNow, IsActive = true };
            _dbContext.Portfolios.Add(portfolio);
            
            _dbContext.SaveChanges();
        }
        
        public void Dispose()
        {
            _dbContext?.Dispose();
        }

        #region FinancialDataController Tests

        [Fact]
        public async Task GetStockQuote_WithValidSymbol_ReturnsOkResult()
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

            var controller = new FinancialDataController(_mockFinancialDataService.Object, _mockLogger.Object);

            // Act
            var result = await controller.GetStockQuote("AAPL");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ApiResponse<StockQuote>>(okResult.Value);
            Assert.True(response.Success);
            Assert.Equal("AAPL", response.Data!.Symbol);
            Assert.Equal(152.50m, response.Data.Close);
        }

        [Fact]
        public async Task GetStockQuote_WithServiceFailure_ReturnsInternalServerError()
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

            var controller = new FinancialDataController(_mockFinancialDataService.Object, _mockLogger.Object);

            // Act
            var result = await controller.GetStockQuote("INVALID");

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, objectResult.StatusCode);
            var response = Assert.IsType<ApiResponse<StockQuote>>(objectResult.Value);
            Assert.False(response.Success);
            Assert.Contains("API request failed", response.ErrorMessage);
        }

        [Fact]
        public async Task GetStockQuote_WithNullSymbol_ReturnsBadRequest()
        {
            // Arrange
            var controller = new FinancialDataController(_mockFinancialDataService.Object, _mockLogger.Object);

            // Act
            var result = await controller.GetStockQuote("");

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Symbol is required", badRequestResult.Value);
        }

        [Fact]
        public async Task GetStockQuote_WithEmptySymbol_ReturnsBadRequest()
        {
            // Arrange
            var controller = new FinancialDataController(_mockFinancialDataService.Object, _mockLogger.Object);

            // Act
            var result = await controller.GetStockQuote("   ");

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Symbol is required", badRequestResult.Value);
        }

        [Fact]
        public async Task GetCurrentPrice_WithValidSymbol_ReturnsOkResult()
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

            var controller = new FinancialDataController(_mockFinancialDataService.Object, _mockLogger.Object);

            // Act
            var result = await controller.GetCurrentPrice("AAPL");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ApiResponse<decimal>>(okResult.Value);
            Assert.True(response.Success);
            Assert.Equal(152.50m, response.Data);
        }

        [Fact]
        public async Task GetCurrentPrice_WithServiceFailure_ReturnsInternalServerError()
        {
            // Arrange
            var failedResponse = new ApiResponse<decimal>
            {
                Success = false,
                Data = 0,
                ErrorMessage = "Network failure",
                StatusCode = 500
            };

            _mockFinancialDataService
                .Setup(x => x.GetCurrentPriceAsync("INVALID"))
                .ReturnsAsync(failedResponse);

            var controller = new FinancialDataController(_mockFinancialDataService.Object, _mockLogger.Object);

            // Act
            var result = await controller.GetCurrentPrice("INVALID");

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, objectResult.StatusCode);
            var response = Assert.IsType<ApiResponse<decimal>>(objectResult.Value);
            Assert.False(response.Success);
            Assert.Contains("Network failure", response.ErrorMessage);
        }

        [Fact]
        public async Task GetForexQuote_WithValidCurrencies_ReturnsOkResult()
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

            var controller = new FinancialDataController(_mockFinancialDataService.Object, _mockLogger.Object);

            // Act
            var result = await controller.GetForexQuote("USD", "EUR");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ApiResponse<ForexQuote>>(okResult.Value);
            Assert.True(response.Success);
            Assert.Equal("USD", response.Data!.FromCurrency);
            Assert.Equal("EUR", response.Data.ToCurrency);
            Assert.Equal(0.85m, response.Data.ExchangeRate);
        }

        [Fact]
        public async Task GetForexQuote_WithInvalidFromCurrency_ReturnsBadRequest()
        {
            // Arrange
            var controller = new FinancialDataController(_mockFinancialDataService.Object, _mockLogger.Object);

            // Act
            var result = await controller.GetForexQuote("", "EUR");

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Both fromCurrency and toCurrency are required", badRequestResult.Value);
        }

        [Fact]
        public async Task GetForexQuote_WithInvalidToCurrency_ReturnsBadRequest()
        {
            // Arrange
            var controller = new FinancialDataController(_mockFinancialDataService.Object, _mockLogger.Object);

            // Act
            var result = await controller.GetForexQuote("USD", "");

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Both fromCurrency and toCurrency are required", badRequestResult.Value);
        }

        [Fact]
        public async Task GetStockHistory_WithValidSymbol_ReturnsOkResult()
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

            var controller = new FinancialDataController(_mockFinancialDataService.Object, _mockLogger.Object);

            // Act
            var result = await controller.GetStockHistory("AAPL", 5);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ApiResponse<List<StockQuote>>>(okResult.Value);
            Assert.True(response.Success);
            Assert.Equal(2, response.Data!.Count);
            Assert.Equal("AAPL", response.Data[0].Symbol);
        }

        [Fact]
        public async Task GetStockHistory_WithInvalidDays_ReturnsBadRequest()
        {
            // Arrange
            var controller = new FinancialDataController(_mockFinancialDataService.Object, _mockLogger.Object);

            // Act
            var result = await controller.GetStockHistory("AAPL", 0);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Days must be between 1 and 365", badRequestResult.Value);
        }

        #endregion

        #region AssetsController Tests

        [Fact]
        public async Task GetAssets_ReturnsOkResult()
        {
            // Arrange
            var controller = new AssetsController(_dbContext, _mockAssetsLogger.Object);

            // Act
            var result = await controller.GetAssets();

            // Assert
            Assert.IsType<OkObjectResult>(result.Result);
        }

        [Fact]
        public async Task GetAsset_WithValidId_ReturnsOkResult()
        {
            // Arrange
            var controller = new AssetsController(_dbContext, _mockAssetsLogger.Object);

            // Act
            var result = await controller.GetAsset(1);

            // Assert
            Assert.IsType<OkObjectResult>(result.Result);
        }

        [Fact]
        public async Task GetAsset_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var controller = new AssetsController(_dbContext, _mockAssetsLogger.Object);

            // Act
            var result = await controller.GetAsset(999);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task GetAssetBySymbol_WithValidSymbol_ReturnsOkResult()
        {
            // Arrange
            var controller = new AssetsController(_dbContext, _mockAssetsLogger.Object);

            // Act
            var result = await controller.GetAssetBySymbol("AAPL");

            // Assert
            Assert.IsType<OkObjectResult>(result.Result);
        }

        [Fact]
        public async Task GetAssetBySymbol_WithInvalidSymbol_ReturnsNotFound()
        {
            // Arrange
            var controller = new AssetsController(_dbContext, _mockAssetsLogger.Object);

            // Act
            var result = await controller.GetAssetBySymbol("INVALID");

            // Assert
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        #endregion

        #region PortfoliosController Tests

        [Fact]
        public async Task GetPortfolios_ReturnsOkResult()
        {
            // Arrange
            var controller = new PortfoliosController(_dbContext, _mockPortfoliosLogger.Object);

            // Act
            var result = await controller.GetPortfolios();

            // Assert
            Assert.IsType<OkObjectResult>(result.Result);
        }

        [Fact]
        public async Task GetPortfolio_WithValidId_ReturnsOkResult()
        {
            // Arrange
            var controller = new PortfoliosController(_dbContext, _mockPortfoliosLogger.Object);

            // Act
            var result = await controller.GetPortfolio(1);

            // Assert
            Assert.IsType<OkObjectResult>(result.Result);
        }

        [Fact]
        public async Task GetPortfolio_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var controller = new PortfoliosController(_dbContext, _mockPortfoliosLogger.Object);

            // Act
            var result = await controller.GetPortfolio(999);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task GetPortfolioPerformance_WithValidId_ReturnsOkResult()
        {
            // Arrange
            var controller = new PortfoliosController(_dbContext, _mockPortfoliosLogger.Object);

            // Act
            var result = await controller.GetPortfolioPerformance(1, null, null);

            // Assert
            Assert.IsType<OkObjectResult>(result.Result);
        }

        #endregion
    }
}

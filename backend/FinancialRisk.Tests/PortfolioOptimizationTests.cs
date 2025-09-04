using Microsoft.Extensions.Logging;
using Moq;
using FinancialRisk.Api.Services;
using FinancialRisk.Api.Models;
using System.Text.Json;

namespace FinancialRisk.Tests
{
    public class PortfolioOptimizationTests
    {
        private readonly Mock<ILogger<PortfolioOptimizationService>> _mockLogger;
        private readonly Mock<IFinancialDataService> _mockFinancialDataService;
        private readonly Mock<IDataPersistenceService> _mockDataPersistenceService;
        private readonly PortfolioOptimizationService _optimizationService;

        public PortfolioOptimizationTests()
        {
            _mockLogger = new Mock<ILogger<PortfolioOptimizationService>>();
            _mockFinancialDataService = new Mock<IFinancialDataService>();
            _mockDataPersistenceService = new Mock<IDataPersistenceService>();
            
            _optimizationService = new PortfolioOptimizationService(
                _mockLogger.Object,
                _mockFinancialDataService.Object,
                _mockDataPersistenceService.Object
            );
        }

        [Fact]
        public async Task OptimizePortfolio_WithValidData_ReturnsSuccess()
        {
            // Arrange
            var request = new PortfolioOptimizationRequest
            {
                PortfolioName = "Test Portfolio",
                Symbols = new List<string> { "AAPL", "GOOGL", "MSFT" },
                OptimizationMethod = "MeanVariance",
                RiskAversion = 1.0,
                MaxWeight = 1.0,
                MinWeight = 0.0,
                LookbackPeriod = 252
            };

            // Mock financial data for each asset
            foreach (var symbol in request.Symbols)
            {
                var returns = GenerateSampleReturns(252);
                var mockHistoryResult = new ApiResponse<List<StockQuote>>
                {
                    Success = true,
                    Data = GenerateStockQuotes(returns)
                };

                _mockFinancialDataService
                    .Setup(x => x.GetStockHistoryAsync(symbol, request.LookbackPeriod))
                    .ReturnsAsync(mockHistoryResult);
            }

            // Act
            var result = await _optimizationService.OptimizePortfolioAsync(request);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.OptimizationResult);
            Assert.Equal("Test Portfolio", result.OptimizationResult.PortfolioName);
            Assert.Equal("MeanVariance", result.OptimizationResult.OptimizationMethod);
            Assert.True(result.OptimizationResult.OptimalWeights.Count > 0);
            Assert.True(result.OptimizationResult.ExpectedReturn >= 0);
            Assert.True(result.OptimizationResult.ExpectedVolatility >= 0);
        }

        [Fact]
        public async Task OptimizePortfolio_WithInsufficientAssets_ReturnsError()
        {
            // Arrange
            var request = new PortfolioOptimizationRequest
            {
                PortfolioName = "Test Portfolio",
                Symbols = new List<string> { "AAPL" }, // Only one asset
                OptimizationMethod = "MeanVariance",
                RiskAversion = 1.0
            };

            // Act
            var result = await _optimizationService.OptimizePortfolioAsync(request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("At least 2 assets required for portfolio optimization", result.Error);
        }

        [Fact]
        public async Task OptimizePortfolio_WithInsufficientData_ReturnsError()
        {
            // Arrange
            var request = new PortfolioOptimizationRequest
            {
                PortfolioName = "Test Portfolio",
                Symbols = new List<string> { "AAPL", "GOOGL" },
                OptimizationMethod = "MeanVariance",
                RiskAversion = 1.0,
                LookbackPeriod = 252
            };

            // Mock failed data fetch
            var mockHistoryResult = new ApiResponse<List<StockQuote>>
            {
                Success = false,
                Data = null
            };

            _mockFinancialDataService
                .Setup(x => x.GetStockHistoryAsync(It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(mockHistoryResult);

            // Act
            var result = await _optimizationService.OptimizePortfolioAsync(request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Insufficient asset data for optimization", result.Error);
        }

        [Fact]
        public async Task CalculateEfficientFrontier_WithValidData_ReturnsSuccess()
        {
            // Arrange
            var request = new PortfolioOptimizationRequest
            {
                PortfolioName = "Test Portfolio",
                Symbols = new List<string> { "AAPL", "GOOGL", "MSFT" },
                OptimizationMethod = "MeanVariance",
                RiskAversion = 1.0,
                EfficientFrontierPoints = 20
            };

            // Mock financial data for each asset
            foreach (var symbol in request.Symbols)
            {
                var returns = GenerateSampleReturns(252);
                var mockHistoryResult = new ApiResponse<List<StockQuote>>
                {
                    Success = true,
                    Data = GenerateStockQuotes(returns)
                };

                _mockFinancialDataService
                    .Setup(x => x.GetStockHistoryAsync(symbol, request.LookbackPeriod))
                    .ReturnsAsync(mockHistoryResult);
            }

            // Act
            var result = await _optimizationService.CalculateEfficientFrontierAsync(request);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Points);
            Assert.True(result.Points.Count > 0);
            Assert.NotNull(result.MinVolatilityPoint);
            Assert.NotNull(result.MaxSharpePoint);
            Assert.NotNull(result.MaxReturnPoint);
        }

        [Fact]
        public async Task CalculateEfficientFrontier_WithInsufficientAssets_ReturnsError()
        {
            // Arrange
            var request = new PortfolioOptimizationRequest
            {
                PortfolioName = "Test Portfolio",
                Symbols = new List<string> { "AAPL" }, // Only one asset
                OptimizationMethod = "MeanVariance",
                RiskAversion = 1.0
            };

            // Act
            var result = await _optimizationService.CalculateEfficientFrontierAsync(request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("At least 2 assets are required for efficient frontier calculation", result.Error);
        }

        [Theory]
        [InlineData("MeanVariance")]
        [InlineData("MinimumVariance")]
        [InlineData("MaximumSharpe")]
        [InlineData("EqualWeight")]
        [InlineData("RiskParity")]
        public async Task OptimizePortfolio_WithDifferentMethods_ReturnsSuccess(string method)
        {
            // Arrange
            var request = new PortfolioOptimizationRequest
            {
                PortfolioName = "Test Portfolio",
                Symbols = new List<string> { "AAPL", "GOOGL", "MSFT" },
                OptimizationMethod = method,
                RiskAversion = 1.0,
                LookbackPeriod = 252
            };

            // Mock financial data for each asset
            foreach (var symbol in request.Symbols)
            {
                var returns = GenerateSampleReturns(252);
                var mockHistoryResult = new ApiResponse<List<StockQuote>>
                {
                    Success = true,
                    Data = GenerateStockQuotes(returns)
                };

                _mockFinancialDataService
                    .Setup(x => x.GetStockHistoryAsync(symbol, request.LookbackPeriod))
                    .ReturnsAsync(mockHistoryResult);
            }

            // Act
            var result = await _optimizationService.OptimizePortfolioAsync(request);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.OptimizationResult);
            Assert.Equal(method, result.OptimizationResult.OptimizationMethod);
        }

        [Fact]
        public async Task OptimizeRiskBudgeting_ReturnsNotImplemented()
        {
            // Arrange
            var request = new RiskBudgetingRequest
            {
                PortfolioName = "Test Portfolio",
                Symbols = new List<string> { "AAPL", "GOOGL" },
                RiskBudgets = new List<double> { 0.5, 0.5 }
            };

            // Act
            var result = await _optimizationService.OptimizeRiskBudgetingAsync(request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Risk budgeting optimization not yet implemented", result.Error);
        }

        [Fact]
        public async Task OptimizeBlackLitterman_ReturnsNotImplemented()
        {
            // Arrange
            var request = new BlackLittermanRequest
            {
                PortfolioName = "Test Portfolio",
                Symbols = new List<string> { "AAPL", "GOOGL" },
                MarketCapWeights = new List<double> { 0.6, 0.4 },
                Views = new List<double> { 0.1 },
                PickMatrix = new List<List<double>> { new List<double> { 1, -1 } },
                ViewUncertainties = new List<double> { 0.05 }
            };

            // Act
            var result = await _optimizationService.OptimizeBlackLittermanAsync(request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Black-Litterman optimization not yet implemented", result.Error);
        }

        [Fact]
        public async Task OptimizeTransactionCosts_ReturnsNotImplemented()
        {
            // Arrange
            var request = new TransactionCostOptimizationRequest
            {
                PortfolioName = "Test Portfolio",
                Symbols = new List<string> { "AAPL", "GOOGL" },
                CurrentWeights = new List<double> { 0.6, 0.4 },
                TargetWeights = new List<double> { 0.5, 0.5 },
                TransactionCosts = new List<double> { 0.001, 0.001 }
            };

            // Act
            var result = await _optimizationService.OptimizeTransactionCostsAsync(request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Transaction cost optimization not yet implemented", result.Error);
        }

        [Fact]
        public async Task GetOptimizationHistory_ReturnsEmptyList()
        {
            // Arrange
            var portfolioName = "Test Portfolio";

            // Act
            var result = await _optimizationService.GetOptimizationHistoryAsync(portfolioName);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetOptimizationById_ReturnsNull()
        {
            // Arrange
            var id = 1;

            // Act
            var result = await _optimizationService.GetOptimizationByIdAsync(id);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task SaveOptimizationResult_ReturnsTrue()
        {
            // Arrange
            var result = new PortfolioOptimizationResult
            {
                Success = true,
                PortfolioName = "Test Portfolio",
                OptimizationMethod = "MeanVariance",
                OptimalWeights = new List<double> { 0.4, 0.3, 0.3 },
                ExpectedReturn = 0.12,
                ExpectedVolatility = 0.15,
                SharpeRatio = 0.8,
                VaR = 0.05,
                CVaR = 0.07,
                DiversificationRatio = 1.2,
                ConcentrationRatio = 0.34,
                AssetWeights = new List<AssetWeight>(),
                OptimizationMetadata = new Dictionary<string, object>(),
                CalculationDate = DateTime.UtcNow
            };

            // Act
            var saveResult = await _optimizationService.SaveOptimizationResultAsync(result);

            // Assert
            Assert.True(saveResult);
        }

        [Fact]
        public async Task GetAvailableOptimizationMethods_ReturnsMethods()
        {
            // Act
            var methods = await _optimizationService.GetAvailableOptimizationMethodsAsync();

            // Assert
            Assert.NotNull(methods);
            Assert.NotEmpty(methods);
            Assert.Contains("MeanVariance", methods);
            Assert.Contains("MinimumVariance", methods);
            Assert.Contains("MaximumSharpe", methods);
            Assert.Contains("EqualWeight", methods);
            Assert.Contains("RiskParity", methods);
            Assert.Contains("BlackLitterman", methods);
            Assert.Contains("MeanCVaR", methods);
        }

        [Fact]
        public async Task GetOptimizationConstraints_ReturnsConstraints()
        {
            // Act
            var constraints = await _optimizationService.GetOptimizationConstraintsAsync();

            // Assert
            Assert.NotNull(constraints);
            Assert.NotEmpty(constraints);
            Assert.Contains("max_weight", constraints.Keys);
            Assert.Contains("min_weight", constraints.Keys);
            Assert.Contains("max_leverage", constraints.Keys);
            Assert.Contains("max_turnover", constraints.Keys);
            Assert.Contains("max_concentration", constraints.Keys);
            Assert.Contains("transaction_costs", constraints.Keys);
        }

        [Theory]
        [InlineData(0.5)]
        [InlineData(1.0)]
        [InlineData(2.0)]
        [InlineData(5.0)]
        public async Task OptimizePortfolio_WithDifferentRiskAversion_ReturnsSuccess(double riskAversion)
        {
            // Arrange
            var request = new PortfolioOptimizationRequest
            {
                PortfolioName = "Test Portfolio",
                Symbols = new List<string> { "AAPL", "GOOGL", "MSFT" },
                OptimizationMethod = "MeanVariance",
                RiskAversion = riskAversion,
                LookbackPeriod = 252
            };

            // Mock financial data for each asset
            foreach (var symbol in request.Symbols)
            {
                var returns = GenerateSampleReturns(252);
                var mockHistoryResult = new ApiResponse<List<StockQuote>>
                {
                    Success = true,
                    Data = GenerateStockQuotes(returns)
                };

                _mockFinancialDataService
                    .Setup(x => x.GetStockHistoryAsync(symbol, request.LookbackPeriod))
                    .ReturnsAsync(mockHistoryResult);
            }

            // Act
            var result = await _optimizationService.OptimizePortfolioAsync(request);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.OptimizationResult);
            Assert.Equal(riskAversion, result.OptimizationResult.OptimizationMetadata.GetValueOrDefault("risk_aversion", 0.0));
        }

        [Theory]
        [InlineData(0.1, 0.9)]
        [InlineData(0.0, 0.5)]
        [InlineData(0.2, 1.0)]
        public async Task OptimizePortfolio_WithWeightConstraints_ReturnsSuccess(double minWeight, double maxWeight)
        {
            // Arrange
            var request = new PortfolioOptimizationRequest
            {
                PortfolioName = "Test Portfolio",
                Symbols = new List<string> { "AAPL", "GOOGL", "MSFT" },
                OptimizationMethod = "MeanVariance",
                RiskAversion = 1.0,
                MinWeight = minWeight,
                MaxWeight = maxWeight,
                LookbackPeriod = 252
            };

            // Mock financial data for each asset
            foreach (var symbol in request.Symbols)
            {
                var returns = GenerateSampleReturns(252);
                var mockHistoryResult = new ApiResponse<List<StockQuote>>
                {
                    Success = true,
                    Data = GenerateStockQuotes(returns)
                };

                _mockFinancialDataService
                    .Setup(x => x.GetStockHistoryAsync(symbol, request.LookbackPeriod))
                    .ReturnsAsync(mockHistoryResult);
            }

            // Act
            var result = await _optimizationService.OptimizePortfolioAsync(request);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.OptimizationResult);
            
            // Verify weights are within constraints
            foreach (var weight in result.OptimizationResult.OptimalWeights)
            {
                Assert.True(weight >= minWeight - 0.001); // Allow small floating point errors
                Assert.True(weight <= maxWeight + 0.001);
            }
        }

        private double[] GenerateSampleReturns(int count)
        {
            var random = new Random(42); // Fixed seed for reproducible tests
            var returns = new double[count];
            
            for (int i = 0; i < count; i++)
            {
                // Generate normal distribution with mean 0.001 and std 0.02
                var u1 = random.NextDouble();
                var u2 = random.NextDouble();
                var z0 = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Cos(2.0 * Math.PI * u2);
                returns[i] = 0.001 + 0.02 * z0;
            }
            
            return returns;
        }

        private List<StockQuote> GenerateStockQuotes(double[] returns)
        {
            var quotes = new List<StockQuote>();
            var basePrice = 100.0;
            var currentPrice = basePrice;
            
            quotes.Add(new StockQuote
            {
                Symbol = "TEST",
                Timestamp = DateTime.Today.AddDays(-returns.Length),
                Open = (decimal)basePrice,
                High = (decimal)(basePrice * 1.01),
                Low = (decimal)(basePrice * 0.99),
                Close = (decimal)basePrice,
                Volume = 1000000
            });
            
            for (int i = 0; i < returns.Length; i++)
            {
                currentPrice *= (1 + returns[i]);
                quotes.Add(new StockQuote
                {
                    Symbol = "TEST",
                    Timestamp = DateTime.Today.AddDays(-returns.Length + i + 1),
                    Open = quotes.Last().Close,
                    High = (decimal)(currentPrice * 1.01),
                    Low = (decimal)(currentPrice * 0.99),
                    Close = (decimal)currentPrice,
                    Volume = 1000000
                });
            }
            
            return quotes;
        }
    }
}

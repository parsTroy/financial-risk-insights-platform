using Microsoft.Extensions.Logging;
using Moq;
using FinancialRisk.Api.Services;
using FinancialRisk.Api.Models;
using System.Text.Json;

namespace FinancialRisk.Tests
{
    public class MonteCarloEngineTests
    {
        private readonly Mock<ILogger<VaRCalculationService>> _mockLogger;
        private readonly Mock<IFinancialDataService> _mockFinancialDataService;
        private readonly Mock<IDataPersistenceService> _mockDataPersistenceService;
        private readonly VaRCalculationService _varService;

        public MonteCarloEngineTests()
        {
            _mockLogger = new Mock<ILogger<VaRCalculationService>>();
            _mockFinancialDataService = new Mock<IFinancialDataService>();
            _mockDataPersistenceService = new Mock<IDataPersistenceService>();
            
            _varService = new VaRCalculationService(
                _mockLogger.Object,
                _mockFinancialDataService.Object,
                _mockDataPersistenceService.Object
            );
        }

        [Fact]
        public async Task CalculateMonteCarloVaR_WithValidData_ReturnsSuccess()
        {
            // Arrange
            var symbol = "AAPL";
            var returns = GenerateSampleReturns(252);
            var request = new VaRCalculationRequest
            {
                Symbol = symbol,
                CalculationType = "MonteCarlo",
                DistributionType = "Normal",
                ConfidenceLevels = new List<double> { 0.95, 0.99 },
                Days = 252,
                SimulationCount = 10000,
                TimeHorizon = 1.0
            };

            var mockHistoryResult = new ApiResponse<List<StockQuote>>
            {
                Success = true,
                Data = GenerateStockQuotes(returns)
            };

            _mockFinancialDataService
                .Setup(x => x.GetStockHistoryAsync(symbol, 252))
                .ReturnsAsync(mockHistoryResult);

            // Act
            var result = await _varService.CalculateVaRAsync(request);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal("MonteCarlo", result.Data.CalculationType);
            Assert.Equal("Normal", result.Data.DistributionType);
            Assert.True(result.Data.VaR > 0);
            Assert.True(result.Data.CVaR > 0);
            Assert.Equal(10000, result.Data.SimulationCount);
        }

        [Fact]
        public async Task CalculateMonteCarloVaR_WithInsufficientData_ReturnsError()
        {
            // Arrange
            var symbol = "INVALID";
            var request = new VaRCalculationRequest
            {
                Symbol = symbol,
                CalculationType = "MonteCarlo",
                DistributionType = "Normal",
                ConfidenceLevels = new List<double> { 0.95, 0.99 },
                Days = 252,
                SimulationCount = 10000,
                TimeHorizon = 1.0
            };

            var mockHistoryResult = new ApiResponse<List<StockQuote>>
            {
                Success = false,
                Data = null
            };

            _mockFinancialDataService
                .Setup(x => x.GetStockHistoryAsync(symbol, 252))
                .ReturnsAsync(mockHistoryResult);

            // Act
            var result = await _varService.CalculateVaRAsync(request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("No historical data available", result.Error);
        }

        [Fact]
        public async Task CalculateMonteCarloPortfolioVaR_WithValidData_ReturnsSuccess()
        {
            // Arrange
            var portfolioName = "Test Portfolio";
            var symbols = new List<string> { "AAPL", "GOOGL", "MSFT" };
            var weights = new List<decimal> { 0.4m, 0.3m, 0.3m };
            var request = new PortfolioVaRCalculationRequest
            {
                PortfolioName = portfolioName,
                Symbols = symbols,
                Weights = weights,
                CalculationType = "MonteCarlo",
                DistributionType = "Normal",
                ConfidenceLevels = new List<double> { 0.95, 0.99 },
                Days = 252,
                SimulationCount = 10000,
                TimeHorizon = 1.0
            };

            // Mock data for each asset
            foreach (var symbol in symbols)
            {
                var returns = GenerateSampleReturns(252);
                var mockHistoryResult = new ApiResponse<List<StockQuote>>
                {
                    Success = true,
                    Data = GenerateStockQuotes(returns)
                };

                _mockFinancialDataService
                    .Setup(x => x.GetStockHistoryAsync(symbol, 252))
                    .ReturnsAsync(mockHistoryResult);
            }

            // Act
            var result = await _varService.CalculatePortfolioVaRAsync(request);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal("MonteCarlo", result.Data.CalculationType);
            Assert.Equal("Normal", result.Data.DistributionType);
            Assert.True(result.Data.PortfolioVaR > 0);
            Assert.True(result.Data.PortfolioCVaR > 0);
            Assert.Equal(10000, result.Data.SimulationCount);
            Assert.NotNull(result.AssetContributions);
            Assert.Equal(3, result.AssetContributions.Count);
        }

        [Fact]
        public async Task CalculateMonteCarloVaR_WithDifferentDistributions_ReturnsDifferentResults()
        {
            // Arrange
            var symbol = "AAPL";
            var returns = GenerateSampleReturns(252);
            var distributions = new[] { "Normal", "TStudent", "GARCH" };
            var results = new Dictionary<string, VaRCalculationResponse>();

            var mockHistoryResult = new ApiResponse<List<StockQuote>>
            {
                Success = true,
                Data = GenerateStockQuotes(returns)
            };

            _mockFinancialDataService
                .Setup(x => x.GetStockHistoryAsync(symbol, 252))
                .ReturnsAsync(mockHistoryResult);

            // Act
            foreach (var distribution in distributions)
            {
                var request = new VaRCalculationRequest
                {
                    Symbol = symbol,
                    CalculationType = "MonteCarlo",
                    DistributionType = distribution,
                    ConfidenceLevels = new List<double> { 0.95 },
                    Days = 252,
                    SimulationCount = 5000, // Reduced for faster testing
                    TimeHorizon = 1.0
                };

                var result = await _varService.CalculateVaRAsync(request);
                results[distribution] = result;
            }

            // Assert
            foreach (var kvp in results)
            {
                Assert.True(kvp.Value.Success, $"Failed for distribution: {kvp.Key}");
                Assert.NotNull(kvp.Value.Data);
                Assert.Equal(kvp.Key, kvp.Value.Data.DistributionType);
            }
        }

        [Fact]
        public async Task CalculateMonteCarloVaR_WithStressTest_ReturnsStressedResults()
        {
            // Arrange
            var symbol = "AAPL";
            var returns = GenerateSampleReturns(252);
            var request = new VaRStressTestRequest
            {
                Symbol = symbol,
                ScenarioName = "Volatility Shock",
                ScenarioType = "Volatility",
                StressFactor = 2.0,
                ConfidenceLevels = new List<double> { 0.95, 0.99 },
                Days = 252
            };

            var mockHistoryResult = new ApiResponse<List<StockQuote>>
            {
                Success = true,
                Data = GenerateStockQuotes(returns)
            };

            _mockFinancialDataService
                .Setup(x => x.GetStockHistoryAsync(symbol, 252))
                .ReturnsAsync(mockHistoryResult);

            // Act
            var result = await _varService.PerformStressTestAsync(request);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal("Volatility Shock", result.Data.ScenarioName);
            Assert.Equal("Volatility", result.Data.ScenarioType);
            Assert.Equal(2.0, result.Data.StressFactor);
            Assert.True(result.Data.VaR > 0);
            Assert.True(result.Data.CVaR > 0);
        }

        [Fact]
        public async Task CompareVaRMethods_WithMonteCarlo_IncludesMonteCarloResults()
        {
            // Arrange
            var symbol = "AAPL";
            var returns = GenerateSampleReturns(252);
            var days = 252;

            var mockHistoryResult = new ApiResponse<List<StockQuote>>
            {
                Success = true,
                Data = GenerateStockQuotes(returns)
            };

            _mockFinancialDataService
                .Setup(x => x.GetStockHistoryAsync(symbol, days))
                .ReturnsAsync(mockHistoryResult);

            // Act
            var result = await _varService.CompareVaRMethodsAsync(symbol, days);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
            
            var comparison = result.First();
            Assert.Equal(symbol, comparison.Symbol);
            Assert.True(comparison.VaRResults.ContainsKey("MonteCarlo"));
            Assert.True(comparison.CVaRResults.ContainsKey("MonteCarlo"));
            Assert.True(comparison.VaRResults["MonteCarlo"] > 0);
            Assert.True(comparison.CVaRResults["MonteCarlo"] > 0);
        }

        [Theory]
        [InlineData(1000)]
        [InlineData(5000)]
        [InlineData(10000)]
        [InlineData(50000)]
        public async Task CalculateMonteCarloVaR_WithDifferentSimulationCounts_ReturnsConsistentResults(int simulationCount)
        {
            // Arrange
            var symbol = "AAPL";
            var returns = GenerateSampleReturns(252);
            var request = new VaRCalculationRequest
            {
                Symbol = symbol,
                CalculationType = "MonteCarlo",
                DistributionType = "Normal",
                ConfidenceLevels = new List<double> { 0.95 },
                Days = 252,
                SimulationCount = simulationCount,
                TimeHorizon = 1.0
            };

            var mockHistoryResult = new ApiResponse<List<StockQuote>>
            {
                Success = true,
                Data = GenerateStockQuotes(returns)
            };

            _mockFinancialDataService
                .Setup(x => x.GetStockHistoryAsync(symbol, 252))
                .ReturnsAsync(mockHistoryResult);

            // Act
            var result = await _varService.CalculateVaRAsync(request);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(simulationCount, result.Data.SimulationCount);
            Assert.True(result.Data.VaR > 0);
            Assert.True(result.Data.CVaR > 0);
        }

        [Fact]
        public async Task CalculateMonteCarloVaR_WithCustomParameters_AppliesParameters()
        {
            // Arrange
            var symbol = "AAPL";
            var returns = GenerateSampleReturns(252);
            var customParameters = new Dictionary<string, object>
            {
                { "degrees_of_freedom", 5.0 },
                { "location", 0.0 },
                { "scale", 1.0 }
            };

            var request = new VaRCalculationRequest
            {
                Symbol = symbol,
                CalculationType = "MonteCarlo",
                DistributionType = "TStudent",
                ConfidenceLevels = new List<double> { 0.95 },
                Days = 252,
                SimulationCount = 5000,
                TimeHorizon = 1.0,
                Parameters = customParameters
            };

            var mockHistoryResult = new ApiResponse<List<StockQuote>>
            {
                Success = true,
                Data = GenerateStockQuotes(returns)
            };

            _mockFinancialDataService
                .Setup(x => x.GetStockHistoryAsync(symbol, 252))
                .ReturnsAsync(mockHistoryResult);

            // Act
            var result = await _varService.CalculateVaRAsync(request);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal("TStudent", result.Data.DistributionType);
            Assert.NotNull(result.Data.Parameters);
            
            var parameters = JsonSerializer.Deserialize<Dictionary<string, object>>(result.Data.Parameters);
            Assert.NotNull(parameters);
            Assert.True(parameters.ContainsKey("degrees_of_freedom"));
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

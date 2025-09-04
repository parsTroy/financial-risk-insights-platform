using Microsoft.VisualStudio.TestTools.UnitTesting;
using FinancialRisk.Api.Services;
using FinancialRisk.Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FinancialRisk.Tests
{
    [TestClass]
    public class RiskModelTests
    {
        private VaRCalculationService _varService;
        private MonteCarloController _monteCarloController;

        [TestInitialize]
        public void Setup()
        {
            _varService = new VaRCalculationService();
            _monteCarloController = new MonteCarloController(_varService);
        }

        [TestMethod]
        public async Task VaR_Historical_WithKnownDataset_ReturnsExpectedResult()
        {
            // Arrange - Known dataset with expected VaR at 95% confidence
            var returns = new List<double>
            {
                -0.05, -0.03, -0.02, -0.01, 0.00, 0.01, 0.02, 0.03, 0.04, 0.05,
                -0.04, -0.02, -0.01, 0.00, 0.01, 0.02, 0.03, 0.04, 0.05, 0.06,
                -0.06, -0.04, -0.02, -0.01, 0.00, 0.01, 0.02, 0.03, 0.04, 0.05
            };

            var request = new MonteCarloSimulationRequest
            {
                Returns = returns,
                ConfidenceLevel = 0.95,
                TimeHorizon = 1,
                DistributionType = "normal"
            };

            // Act
            var result = await _monteCarloController.CalculateMonteCarloVaR(request);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.IsNotNull(result.Data);
            
            // For 95% confidence with 30 observations, VaR should be around the 5th percentile
            // Expected VaR should be approximately -0.04 (4th worst return)
            var expectedVaR = -0.04; // 5th percentile of sorted returns
            Assert.IsTrue(Math.Abs(result.Data.VaR - expectedVaR) < 0.01, 
                $"Expected VaR around {expectedVaR}, got {result.Data.VaR}");
        }

        [TestMethod]
        public async Task VaR_Parametric_WithNormalDistribution_ReturnsExpectedResult()
        {
            // Arrange - Generate normal distribution with known parameters
            var mean = 0.001; // 0.1% daily return
            var std = 0.02;   // 2% daily volatility
            var confidenceLevel = 0.95;
            
            // Generate 1000 returns from normal distribution
            var random = new Random(42); // Fixed seed for reproducibility
            var returns = new List<double>();
            for (int i = 0; i < 1000; i++)
            {
                var u1 = random.NextDouble();
                var u2 = random.NextDouble();
                var z = Math.Sqrt(-2 * Math.Log(u1)) * Math.Cos(2 * Math.PI * u2); // Box-Muller transform
                returns.Add(mean + std * z);
            }

            var request = new MonteCarloSimulationRequest
            {
                Returns = returns,
                ConfidenceLevel = confidenceLevel,
                TimeHorizon = 1,
                DistributionType = "normal"
            };

            // Act
            var result = await _monteCarloController.CalculateMonteCarloVaR(request);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.IsNotNull(result.Data);
            
            // Expected parametric VaR = mean - 1.645 * std (for 95% confidence)
            var expectedVaR = mean - 1.645 * std;
            var tolerance = 0.01; // Allow 1% tolerance due to sampling
            Assert.IsTrue(Math.Abs(result.Data.VaR - expectedVaR) < tolerance,
                $"Expected VaR around {expectedVaR:F4}, got {result.Data.VaR:F4}");
        }

        [TestMethod]
        public async Task VaR_MonteCarlo_WithKnownParameters_ReturnsConsistentResult()
        {
            // Arrange - Use known parameters for Monte Carlo simulation
            var mean = 0.0005; // 0.05% daily return
            var std = 0.015;   // 1.5% daily volatility
            var confidenceLevel = 0.99;
            var numSimulations = 10000;

            var request = new MonteCarloSimulationRequest
            {
                Returns = new List<double>(), // Empty for Monte Carlo
                ConfidenceLevel = confidenceLevel,
                TimeHorizon = 1,
                DistributionType = "normal",
                NumSimulations = numSimulations,
                MeanReturn = mean,
                Volatility = std
            };

            // Act
            var result = await _monteCarloController.CalculateMonteCarloVaR(request);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.IsNotNull(result.Data);
            
            // Expected VaR = mean - 2.326 * std (for 99% confidence)
            var expectedVaR = mean - 2.326 * std;
            var tolerance = 0.005; // Allow 0.5% tolerance for Monte Carlo simulation
            Assert.IsTrue(Math.Abs(result.Data.VaR - expectedVaR) < tolerance,
                $"Expected VaR around {expectedVaR:F4}, got {result.Data.VaR:F4}");
        }

        [TestMethod]
        public async Task CVaR_WithKnownDataset_ReturnsExpectedResult()
        {
            // Arrange - Known dataset where CVaR should be the mean of worst 5% returns
            var returns = new List<double>
            {
                -0.10, -0.08, -0.06, -0.04, -0.02, 0.00, 0.02, 0.04, 0.06, 0.08,
                0.10, 0.12, 0.14, 0.16, 0.18, 0.20, 0.22, 0.24, 0.26, 0.28
            };

            var request = new MonteCarloSimulationRequest
            {
                Returns = returns,
                ConfidenceLevel = 0.95,
                TimeHorizon = 1,
                DistributionType = "normal"
            };

            // Act
            var result = await _monteCarloController.CalculateMonteCarloVaR(request);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.IsNotNull(result.Data);
            
            // For 95% confidence, CVaR should be the mean of the worst 5% (1 observation)
            // The worst return is -0.10, so CVaR should be approximately -0.10
            var expectedCVaR = -0.10;
            Assert.IsTrue(Math.Abs(result.Data.CVaR - expectedCVaR) < 0.01,
                $"Expected CVaR around {expectedCVaR}, got {result.Data.CVaR}");
        }

        [TestMethod]
        public async Task VaR_WithZeroReturns_ReturnsZeroVaR()
        {
            // Arrange
            var returns = new List<double>(new double[100]); // All zeros
            var request = new MonteCarloSimulationRequest
            {
                Returns = returns,
                ConfidenceLevel = 0.95,
                TimeHorizon = 1,
                DistributionType = "normal"
            };

            // Act
            var result = await _monteCarloController.CalculateMonteCarloVaR(request);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.IsNotNull(result.Data);
            Assert.AreEqual(0.0, result.Data.VaR, 0.001);
            Assert.AreEqual(0.0, result.Data.CVaR, 0.001);
        }

        [TestMethod]
        public async Task VaR_WithSingleReturn_ReturnsThatReturn()
        {
            // Arrange
            var returns = new List<double> { -0.05 };
            var request = new MonteCarloSimulationRequest
            {
                Returns = returns,
                ConfidenceLevel = 0.95,
                TimeHorizon = 1,
                DistributionType = "normal"
            };

            // Act
            var result = await _monteCarloController.CalculateMonteCarloVaR(request);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.IsNotNull(result.Data);
            Assert.AreEqual(0.05, result.Data.VaR, 0.001);
        }

        [TestMethod]
        public async Task VaR_WithDifferentConfidenceLevels_ReturnsAppropriateValues()
        {
            // Arrange - Use same dataset for different confidence levels
            var returns = new List<double>
            {
                -0.10, -0.08, -0.06, -0.04, -0.02, 0.00, 0.02, 0.04, 0.06, 0.08,
                0.10, 0.12, 0.14, 0.16, 0.18, 0.20, 0.22, 0.24, 0.26, 0.28
            };

            var confidenceLevels = new[] { 0.90, 0.95, 0.99 };
            var expectedVaRs = new[] { 0.08, 0.10, 0.10 }; // Expected VaR values

            for (int i = 0; i < confidenceLevels.Length; i++)
            {
                var request = new MonteCarloSimulationRequest
                {
                    Returns = returns,
                    ConfidenceLevel = confidenceLevels[i],
                    TimeHorizon = 1,
                    DistributionType = "normal"
                };

                // Act
                var result = await _monteCarloController.CalculateMonteCarloVaR(request);

                // Assert
                Assert.IsTrue(result.IsSuccess, $"Failed for confidence level {confidenceLevels[i]}");
                Assert.IsNotNull(result.Data);
                Assert.AreEqual(expectedVaRs[i], result.Data.VaR, 0.01,
                    $"VaR mismatch for confidence level {confidenceLevels[i]}");
            }
        }

        [TestMethod]
        public async Task VaR_WithEmptyReturns_ReturnsError()
        {
            // Arrange
            var request = new MonteCarloSimulationRequest
            {
                Returns = new List<double>(),
                ConfidenceLevel = 0.95,
                TimeHorizon = 1,
                DistributionType = "normal"
            };

            // Act
            var result = await _monteCarloController.CalculateMonteCarloVaR(request);

            // Assert
            Assert.IsFalse(result.IsSuccess);
            Assert.IsNotNull(result.ErrorMessage);
        }

        [TestMethod]
        public async Task VaR_WithInvalidConfidenceLevel_ReturnsError()
        {
            // Arrange
            var returns = new List<double> { -0.01, 0.01, 0.02 };
            var request = new MonteCarloSimulationRequest
            {
                Returns = returns,
                ConfidenceLevel = 1.5, // Invalid confidence level
                TimeHorizon = 1,
                DistributionType = "normal"
            };

            // Act
            var result = await _monteCarloController.CalculateMonteCarloVaR(request);

            // Assert
            Assert.IsFalse(result.IsSuccess);
            Assert.IsNotNull(result.ErrorMessage);
        }

        [TestMethod]
        public async Task VaR_StressTest_WithLargeDataset_CompletesSuccessfully()
        {
            // Arrange - Large dataset to test performance
            var random = new Random(42);
            var returns = new List<double>();
            for (int i = 0; i < 10000; i++)
            {
                returns.Add(random.NextDouble() * 0.1 - 0.05); // Random returns between -5% and 5%
            }

            var request = new MonteCarloSimulationRequest
            {
                Returns = returns,
                ConfidenceLevel = 0.95,
                TimeHorizon = 1,
                DistributionType = "normal"
            };

            // Act
            var startTime = DateTime.Now;
            var result = await _monteCarloController.CalculateMonteCarloVaR(request);
            var executionTime = DateTime.Now - startTime;

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.IsNotNull(result.Data);
            Assert.IsTrue(executionTime.TotalSeconds < 5, "VaR calculation should complete within 5 seconds");
        }
    }
}

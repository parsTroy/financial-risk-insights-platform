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
    public class MonteCarloSimulationTests
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
        public async Task MonteCarloSimulation_WithNormalDistribution_ReturnsValidResults()
        {
            // Arrange
            var request = new MonteCarloSimulationRequest
            {
                Returns = new List<double>(), // Empty for Monte Carlo simulation
                ConfidenceLevel = 0.95,
                TimeHorizon = 1,
                DistributionType = "normal",
                NumSimulations = 10000,
                MeanReturn = 0.001, // 0.1% daily return
                Volatility = 0.02   // 2% daily volatility
            };

            // Act
            var result = await _monteCarloController.CalculateMonteCarloVaR(request);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.IsNotNull(result.Data);
            Assert.IsTrue(result.Data.VaR > 0, "VaR should be positive");
            Assert.IsTrue(result.Data.CVaR > 0, "CVaR should be positive");
            Assert.IsTrue(result.Data.CVaR >= result.Data.VaR, "CVaR should be >= VaR");
        }

        [TestMethod]
        public async Task MonteCarloSimulation_WithTDistribution_ReturnsValidResults()
        {
            // Arrange
            var request = new MonteCarloSimulationRequest
            {
                Returns = new List<double>(),
                ConfidenceLevel = 0.95,
                TimeHorizon = 1,
                DistributionType = "t_student",
                NumSimulations = 10000,
                MeanReturn = 0.001,
                Volatility = 0.02,
                DegreesOfFreedom = 5 // Heavy tails
            };

            // Act
            var result = await _monteCarloController.CalculateMonteCarloVaR(request);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.IsNotNull(result.Data);
            Assert.IsTrue(result.Data.VaR > 0, "VaR should be positive");
            Assert.IsTrue(result.Data.CVaR > 0, "CVaR should be positive");
        }

        [TestMethod]
        public async Task MonteCarloSimulation_WithGARCHDistribution_ReturnsValidResults()
        {
            // Arrange
            var request = new MonteCarloSimulationRequest
            {
                Returns = new List<double>(),
                ConfidenceLevel = 0.95,
                TimeHorizon = 1,
                DistributionType = "garch",
                NumSimulations = 10000,
                MeanReturn = 0.001,
                Volatility = 0.02,
                GarchParameters = new Dictionary<string, double>
                {
                    ["alpha"] = 0.1,
                    ["beta"] = 0.85,
                    ["omega"] = 0.0001
                }
            };

            // Act
            var result = await _monteCarloController.CalculateMonteCarloVaR(request);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.IsNotNull(result.Data);
            Assert.IsTrue(result.Data.VaR > 0, "VaR should be positive");
        }

        [TestMethod]
        public async Task MonteCarloSimulation_WithCopulaDistribution_ReturnsValidResults()
        {
            // Arrange
            var request = new MonteCarloSimulationRequest
            {
                Returns = new List<double>(),
                ConfidenceLevel = 0.95,
                TimeHorizon = 1,
                DistributionType = "copula",
                NumSimulations = 10000,
                MeanReturn = 0.001,
                Volatility = 0.02,
                CopulaParameters = new Dictionary<string, object>
                {
                    ["type"] = "gaussian",
                    ["correlation"] = 0.3
                }
            };

            // Act
            var result = await _monteCarloController.CalculateMonteCarloVaR(request);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.IsNotNull(result.Data);
            Assert.IsTrue(result.Data.VaR > 0, "VaR should be positive");
        }

        [TestMethod]
        public async Task MonteCarloSimulation_WithSkewedTDistribution_ReturnsValidResults()
        {
            // Arrange
            var request = new MonteCarloSimulationRequest
            {
                Returns = new List<double>(),
                ConfidenceLevel = 0.95,
                TimeHorizon = 1,
                DistributionType = "skewed_t",
                NumSimulations = 10000,
                MeanReturn = 0.001,
                Volatility = 0.02,
                Skewness = -0.5, // Negative skewness
                DegreesOfFreedom = 5
            };

            // Act
            var result = await _monteCarloController.CalculateMonteCarloVaR(request);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.IsNotNull(result.Data);
            Assert.IsTrue(result.Data.VaR > 0, "VaR should be positive");
        }

        [TestMethod]
        public async Task MonteCarloSimulation_WithMixtureDistribution_ReturnsValidResults()
        {
            // Arrange
            var request = new MonteCarloSimulationRequest
            {
                Returns = new List<double>(),
                ConfidenceLevel = 0.95,
                TimeHorizon = 1,
                DistributionType = "mixture",
                NumSimulations = 10000,
                MeanReturn = 0.001,
                Volatility = 0.02,
                MixtureParameters = new Dictionary<string, object>
                {
                    ["weights"] = new List<double> { 0.7, 0.3 },
                    ["means"] = new List<double> { 0.001, -0.002 },
                    ["volatilities"] = new List<double> { 0.015, 0.05 }
                }
            };

            // Act
            var result = await _monteCarloController.CalculateMonteCarloVaR(request);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.IsNotNull(result.Data);
            Assert.IsTrue(result.Data.VaR > 0, "VaR should be positive");
        }

        [TestMethod]
        public async Task MonteCarloSimulation_WithDifferentConfidenceLevels_ReturnsAppropriateValues()
        {
            // Arrange
            var confidenceLevels = new[] { 0.90, 0.95, 0.99 };
            var results = new List<MonteCarloSimulationResult>();

            foreach (var confidenceLevel in confidenceLevels)
            {
                var request = new MonteCarloSimulationRequest
                {
                    Returns = new List<double>(),
                    ConfidenceLevel = confidenceLevel,
                    TimeHorizon = 1,
                    DistributionType = "normal",
                    NumSimulations = 10000,
                    MeanReturn = 0.001,
                    Volatility = 0.02
                };

                // Act
                var result = await _monteCarloController.CalculateMonteCarloVaR(request);
                results.Add(result.Data);
            }

            // Assert
            Assert.AreEqual(3, results.Count);
            
            // VaR should increase with confidence level (more negative values)
            for (int i = 1; i < results.Count; i++)
            {
                Assert.IsTrue(results[i].VaR >= results[i-1].VaR, 
                    $"VaR should increase with confidence level. {confidenceLevels[i]}: {results[i].VaR}, {confidenceLevels[i-1]}: {results[i-1].VaR}");
            }
        }

        [TestMethod]
        public async Task MonteCarloSimulation_WithDifferentTimeHorizons_ReturnsScaledResults()
        {
            // Arrange
            var timeHorizons = new[] { 1, 5, 10, 30 }; // 1, 5, 10, 30 days
            var results = new List<MonteCarloSimulationResult>();

            foreach (var timeHorizon in timeHorizons)
            {
                var request = new MonteCarloSimulationRequest
                {
                    Returns = new List<double>(),
                    ConfidenceLevel = 0.95,
                    TimeHorizon = timeHorizon,
                    DistributionType = "normal",
                    NumSimulations = 10000,
                    MeanReturn = 0.001,
                    Volatility = 0.02
                };

                // Act
                var result = await _monteCarloController.CalculateMonteCarloVaR(request);
                results.Add(result.Data);
            }

            // Assert
            Assert.AreEqual(4, results.Count);
            
            // VaR should scale with square root of time horizon
            for (int i = 1; i < results.Count; i++)
            {
                var expectedRatio = Math.Sqrt(timeHorizons[i] / (double)timeHorizons[0]);
                var actualRatio = results[i].VaR / results[0].VaR;
                var tolerance = 0.2; // 20% tolerance for Monte Carlo simulation
                
                Assert.IsTrue(Math.Abs(actualRatio - expectedRatio) < tolerance,
                    $"VaR scaling should follow square root rule. Expected ratio: {expectedRatio:F2}, Actual: {actualRatio:F2}");
            }
        }

        [TestMethod]
        public async Task MonteCarloSimulation_WithDifferentNumSimulations_ReturnsConvergentResults()
        {
            // Arrange
            var numSimulations = new[] { 1000, 5000, 10000, 50000 };
            var results = new List<MonteCarloSimulationResult>();

            foreach (var numSim in numSimulations)
            {
                var request = new MonteCarloSimulationRequest
                {
                    Returns = new List<double>(),
                    ConfidenceLevel = 0.95,
                    TimeHorizon = 1,
                    DistributionType = "normal",
                    NumSimulations = numSim,
                    MeanReturn = 0.001,
                    Volatility = 0.02
                };

                // Act
                var result = await _monteCarloController.CalculateMonteCarloVaR(request);
                results.Add(result.Data);
            }

            // Assert
            Assert.AreEqual(4, results.Count);
            
            // Results should converge as number of simulations increases
            var var1000 = results[0].VaR;
            var var50000 = results[3].VaR;
            var convergenceTolerance = 0.1; // 10% tolerance
            
            Assert.IsTrue(Math.Abs(var50000 - var1000) / var1000 < convergenceTolerance,
                $"VaR should converge with more simulations. 1000 sims: {var1000:F4}, 50000 sims: {var50000:F4}");
        }

        [TestMethod]
        public async Task MonteCarloSimulation_WithZeroVolatility_ReturnsZeroVaR()
        {
            // Arrange
            var request = new MonteCarloSimulationRequest
            {
                Returns = new List<double>(),
                ConfidenceLevel = 0.95,
                TimeHorizon = 1,
                DistributionType = "normal",
                NumSimulations = 10000,
                MeanReturn = 0.001,
                Volatility = 0.0 // Zero volatility
            };

            // Act
            var result = await _monteCarloController.CalculateMonteCarloVaR(request);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.IsNotNull(result.Data);
            Assert.AreEqual(0.0, result.Data.VaR, 0.001, "VaR should be zero with zero volatility");
        }

        [TestMethod]
        public async Task MonteCarloSimulation_WithNegativeMean_ReturnsValidResults()
        {
            // Arrange
            var request = new MonteCarloSimulationRequest
            {
                Returns = new List<double>(),
                ConfidenceLevel = 0.95,
                TimeHorizon = 1,
                DistributionType = "normal",
                NumSimulations = 10000,
                MeanReturn = -0.001, // Negative mean return
                Volatility = 0.02
            };

            // Act
            var result = await _monteCarloController.CalculateMonteCarloVaR(request);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.IsNotNull(result.Data);
            Assert.IsTrue(result.Data.VaR > 0, "VaR should be positive even with negative mean");
        }

        [TestMethod]
        public async Task MonteCarloSimulation_WithHighVolatility_ReturnsHighVaR()
        {
            // Arrange
            var request = new MonteCarloSimulationRequest
            {
                Returns = new List<double>(),
                ConfidenceLevel = 0.95,
                TimeHorizon = 1,
                DistributionType = "normal",
                NumSimulations = 10000,
                MeanReturn = 0.001,
                Volatility = 0.10 // High volatility (10%)
            };

            // Act
            var result = await _monteCarloController.CalculateMonteCarloVaR(request);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.IsNotNull(result.Data);
            Assert.IsTrue(result.Data.VaR > 0.15, "VaR should be high with high volatility");
        }

        [TestMethod]
        public async Task MonteCarloSimulation_WithInvalidParameters_ReturnsError()
        {
            // Arrange - Invalid confidence level
            var request = new MonteCarloSimulationRequest
            {
                Returns = new List<double>(),
                ConfidenceLevel = 1.5, // Invalid confidence level
                TimeHorizon = 1,
                DistributionType = "normal",
                NumSimulations = 10000,
                MeanReturn = 0.001,
                Volatility = 0.02
            };

            // Act
            var result = await _monteCarloController.CalculateMonteCarloVaR(request);

            // Assert
            Assert.IsFalse(result.IsSuccess);
            Assert.IsNotNull(result.ErrorMessage);
        }

        [TestMethod]
        public async Task MonteCarloSimulation_WithInvalidDistribution_ReturnsError()
        {
            // Arrange - Invalid distribution type
            var request = new MonteCarloSimulationRequest
            {
                Returns = new List<double>(),
                ConfidenceLevel = 0.95,
                TimeHorizon = 1,
                DistributionType = "invalid_distribution",
                NumSimulations = 10000,
                MeanReturn = 0.001,
                Volatility = 0.02
            };

            // Act
            var result = await _monteCarloController.CalculateMonteCarloVaR(request);

            // Assert
            Assert.IsFalse(result.IsSuccess);
            Assert.IsNotNull(result.ErrorMessage);
        }

        [TestMethod]
        public async Task MonteCarloSimulation_Performance_WithLargeSimulations_CompletesInReasonableTime()
        {
            // Arrange - Large number of simulations
            var request = new MonteCarloSimulationRequest
            {
                Returns = new List<double>(),
                ConfidenceLevel = 0.95,
                TimeHorizon = 1,
                DistributionType = "normal",
                NumSimulations = 100000, // Large number of simulations
                MeanReturn = 0.001,
                Volatility = 0.02
            };

            // Act
            var startTime = DateTime.Now;
            var result = await _monteCarloController.CalculateMonteCarloVaR(request);
            var executionTime = DateTime.Now - startTime;

            // Assert
            Assert.IsTrue(result.IsSuccess, "Simulation should succeed with large number of simulations");
            Assert.IsTrue(executionTime.TotalSeconds < 30, 
                "Simulation should complete within 30 seconds for 100,000 simulations");
        }

        [TestMethod]
        public async Task MonteCarloSimulation_WithReproducibleSeed_ReturnsConsistentResults()
        {
            // Arrange - Two simulations with same parameters
            var request1 = new MonteCarloSimulationRequest
            {
                Returns = new List<double>(),
                ConfidenceLevel = 0.95,
                TimeHorizon = 1,
                DistributionType = "normal",
                NumSimulations = 10000,
                MeanReturn = 0.001,
                Volatility = 0.02,
                RandomSeed = 42 // Fixed seed
            };

            var request2 = new MonteCarloSimulationRequest
            {
                Returns = new List<double>(),
                ConfidenceLevel = 0.95,
                TimeHorizon = 1,
                DistributionType = "normal",
                NumSimulations = 10000,
                MeanReturn = 0.001,
                Volatility = 0.02,
                RandomSeed = 42 // Same fixed seed
            };

            // Act
            var result1 = await _monteCarloController.CalculateMonteCarloVaR(request1);
            var result2 = await _monteCarloController.CalculateMonteCarloVaR(request2);

            // Assert
            Assert.IsTrue(result1.IsSuccess);
            Assert.IsTrue(result2.IsSuccess);
            Assert.AreEqual(result1.Data.VaR, result2.Data.VaR, 0.0001, 
                "Results should be identical with same random seed");
        }
    }
}

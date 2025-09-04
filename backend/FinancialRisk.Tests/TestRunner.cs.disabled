using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FinancialRisk.Tests
{
    /// <summary>
    /// Comprehensive test runner for risk and optimization models
    /// Provides regression testing with known datasets and validates VaR, Sharpe ratio, and optimization outputs
    /// </summary>
    [TestClass]
    public class TestRunner
    {
        [TestMethod]
        public async Task RunAllRiskModelTests()
        {
            Console.WriteLine("Running Risk Model Tests...");
            Console.WriteLine("==========================");
            
            var riskTests = new RiskModelTests();
            riskTests.Setup();
            
            // Run VaR tests
            await riskTests.VaR_Historical_WithKnownDataset_ReturnsExpectedResult();
            Console.WriteLine("✓ VaR Historical test passed");
            
            await riskTests.VaR_Parametric_WithNormalDistribution_ReturnsExpectedResult();
            Console.WriteLine("✓ VaR Parametric test passed");
            
            await riskTests.VaR_MonteCarlo_WithKnownParameters_ReturnsConsistentResult();
            Console.WriteLine("✓ VaR Monte Carlo test passed");
            
            await riskTests.CVaR_WithKnownDataset_ReturnsExpectedResult();
            Console.WriteLine("✓ CVaR test passed");
            
            await riskTests.VaR_WithZeroReturns_ReturnsZeroVaR();
            Console.WriteLine("✓ VaR Zero Returns test passed");
            
            await riskTests.VaR_WithSingleReturn_ReturnsThatReturn();
            Console.WriteLine("✓ VaR Single Return test passed");
            
            await riskTests.VaR_WithDifferentConfidenceLevels_ReturnsAppropriateValues();
            Console.WriteLine("✓ VaR Different Confidence Levels test passed");
            
            await riskTests.VaR_WithEmptyReturns_ReturnsError();
            Console.WriteLine("✓ VaR Empty Returns Error test passed");
            
            await riskTests.VaR_WithInvalidConfidenceLevel_ReturnsError();
            Console.WriteLine("✓ VaR Invalid Confidence Level Error test passed");
            
            await riskTests.VaR_StressTest_WithLargeDataset_CompletesSuccessfully();
            Console.WriteLine("✓ VaR Stress Test passed");
            
            Console.WriteLine("All Risk Model Tests Completed Successfully!");
        }

        [TestMethod]
        public async Task RunAllPortfolioOptimizationTests()
        {
            Console.WriteLine("Running Portfolio Optimization Tests...");
            Console.WriteLine("======================================");
            
            var optimizationTests = new PortfolioOptimizationTests();
            optimizationTests.Setup();
            
            // Run optimization tests
            await optimizationTests.MarkowitzOptimization_WithKnownDataset_ReturnsOptimalWeights();
            Console.WriteLine("✓ Markowitz Optimization test passed");
            
            await optimizationTests.SharpeRatio_WithKnownParameters_ReturnsExpectedValue();
            Console.WriteLine("✓ Sharpe Ratio test passed");
            
            await optimizationTests.EfficientFrontier_WithKnownDataset_ReturnsValidFrontier();
            Console.WriteLine("✓ Efficient Frontier test passed");
            
            await optimizationTests.MinimumVariance_WithKnownDataset_ReturnsMinimumVariancePortfolio();
            Console.WriteLine("✓ Minimum Variance test passed");
            
            await optimizationTests.RiskParity_WithKnownDataset_ReturnsEqualRiskContribution();
            Console.WriteLine("✓ Risk Parity test passed");
            
            await optimizationTests.EqualWeight_WithKnownDataset_ReturnsEqualWeights();
            Console.WriteLine("✓ Equal Weight test passed");
            
            await optimizationTests.BlackLitterman_WithKnownDataset_ReturnsValidWeights();
            Console.WriteLine("✓ Black-Litterman test passed");
            
            await optimizationTests.Optimization_WithInvalidInput_ReturnsError();
            Console.WriteLine("✓ Optimization Invalid Input Error test passed");
            
            await optimizationTests.Optimization_WithMismatchedDimensions_ReturnsError();
            Console.WriteLine("✓ Optimization Mismatched Dimensions Error test passed");
            
            await optimizationTests.Optimization_WithNegativeReturns_HandlesCorrectly();
            Console.WriteLine("✓ Optimization Negative Returns test passed");
            
            await optimizationTests.Optimization_Performance_WithLargeDataset_CompletesInReasonableTime();
            Console.WriteLine("✓ Optimization Performance test passed");
            
            Console.WriteLine("All Portfolio Optimization Tests Completed Successfully!");
        }

        [TestMethod]
        public async Task RunAllMonteCarloSimulationTests()
        {
            Console.WriteLine("Running Monte Carlo Simulation Tests...");
            Console.WriteLine("======================================");
            
            var monteCarloTests = new MonteCarloSimulationTests();
            monteCarloTests.Setup();
            
            // Run Monte Carlo tests
            await monteCarloTests.MonteCarloSimulation_WithNormalDistribution_ReturnsValidResults();
            Console.WriteLine("✓ Monte Carlo Normal Distribution test passed");
            
            await monteCarloTests.MonteCarloSimulation_WithTDistribution_ReturnsValidResults();
            Console.WriteLine("✓ Monte Carlo T-Distribution test passed");
            
            await monteCarloTests.MonteCarloSimulation_WithGARCHDistribution_ReturnsValidResults();
            Console.WriteLine("✓ Monte Carlo GARCH Distribution test passed");
            
            await monteCarloTests.MonteCarloSimulation_WithCopulaDistribution_ReturnsValidResults();
            Console.WriteLine("✓ Monte Carlo Copula Distribution test passed");
            
            await monteCarloTests.MonteCarloSimulation_WithSkewedTDistribution_ReturnsValidResults();
            Console.WriteLine("✓ Monte Carlo Skewed-T Distribution test passed");
            
            await monteCarloTests.MonteCarloSimulation_WithMixtureDistribution_ReturnsValidResults();
            Console.WriteLine("✓ Monte Carlo Mixture Distribution test passed");
            
            await monteCarloTests.MonteCarloSimulation_WithDifferentConfidenceLevels_ReturnsAppropriateValues();
            Console.WriteLine("✓ Monte Carlo Different Confidence Levels test passed");
            
            await monteCarloTests.MonteCarloSimulation_WithDifferentTimeHorizons_ReturnsScaledResults();
            Console.WriteLine("✓ Monte Carlo Different Time Horizons test passed");
            
            await monteCarloTests.MonteCarloSimulation_WithDifferentNumSimulations_ReturnsConvergentResults();
            Console.WriteLine("✓ Monte Carlo Different Simulations test passed");
            
            await monteCarloTests.MonteCarloSimulation_WithZeroVolatility_ReturnsZeroVaR();
            Console.WriteLine("✓ Monte Carlo Zero Volatility test passed");
            
            await monteCarloTests.MonteCarloSimulation_WithNegativeMean_ReturnsValidResults();
            Console.WriteLine("✓ Monte Carlo Negative Mean test passed");
            
            await monteCarloTests.MonteCarloSimulation_WithHighVolatility_ReturnsHighVaR();
            Console.WriteLine("✓ Monte Carlo High Volatility test passed");
            
            await monteCarloTests.MonteCarloSimulation_WithInvalidParameters_ReturnsError();
            Console.WriteLine("✓ Monte Carlo Invalid Parameters Error test passed");
            
            await monteCarloTests.MonteCarloSimulation_WithInvalidDistribution_ReturnsError();
            Console.WriteLine("✓ Monte Carlo Invalid Distribution Error test passed");
            
            await monteCarloTests.MonteCarloSimulation_Performance_WithLargeSimulations_CompletesInReasonableTime();
            Console.WriteLine("✓ Monte Carlo Performance test passed");
            
            await monteCarloTests.MonteCarloSimulation_WithReproducibleSeed_ReturnsConsistentResults();
            Console.WriteLine("✓ Monte Carlo Reproducible Seed test passed");
            
            Console.WriteLine("All Monte Carlo Simulation Tests Completed Successfully!");
        }

        [TestMethod]
        public async Task RunAllStatisticalModelTests()
        {
            Console.WriteLine("Running Statistical Model Tests...");
            Console.WriteLine("==================================");
            
            var statisticalTests = new StatisticalModelTests();
            statisticalTests.Setup();
            
            // Run statistical model tests
            await statisticalTests.GARCHModel_WithKnownDataset_ReturnsValidVolatility();
            Console.WriteLine("✓ GARCH Model test passed");
            
            await statisticalTests.CopulaModel_WithKnownDataset_ReturnsValidCorrelationMatrix();
            Console.WriteLine("✓ Copula Model test passed");
            
            await statisticalTests.RegimeSwitching_WithKnownDataset_ReturnsValidRegimes();
            Console.WriteLine("✓ Regime Switching test passed");
            
            await statisticalTests.GARCHModel_WithDifferentParameters_ReturnsDifferentVolatilities();
            Console.WriteLine("✓ GARCH Different Parameters test passed");
            
            await statisticalTests.CopulaModel_WithDifferentTypes_ReturnsValidResults();
            Console.WriteLine("✓ Copula Different Types test passed");
            
            await statisticalTests.RegimeSwitching_WithDifferentNumRegimes_ReturnsAppropriateResults();
            Console.WriteLine("✓ Regime Switching Different Regimes test passed");
            
            await statisticalTests.GARCHModel_WithEmptyData_ReturnsError();
            Console.WriteLine("✓ GARCH Empty Data Error test passed");
            
            await statisticalTests.CopulaModel_WithSingleAsset_ReturnsError();
            Console.WriteLine("✓ Copula Single Asset Error test passed");
            
            await statisticalTests.StatisticalModels_Performance_WithLargeDataset_CompletesInReasonableTime();
            Console.WriteLine("✓ Statistical Models Performance test passed");
            
            await statisticalTests.StatisticalModels_WithInvalidParameters_ReturnsError();
            Console.WriteLine("✓ Statistical Models Invalid Parameters Error test passed");
            
            await statisticalTests.StatisticalModels_WithMissingParameters_ReturnsError();
            Console.WriteLine("✓ Statistical Models Missing Parameters Error test passed");
            
            Console.WriteLine("All Statistical Model Tests Completed Successfully!");
        }

        [TestMethod]
        public async Task RunAllPricingModelTests()
        {
            Console.WriteLine("Running Pricing Model Tests...");
            Console.WriteLine("=============================");
            
            var pricingTests = new PricingModelTests();
            pricingTests.Setup();
            
            // Run pricing model tests
            await pricingTests.BlackScholes_CallOption_WithKnownParameters_ReturnsExpectedPrice();
            Console.WriteLine("✓ Black-Scholes Call Option test passed");
            
            await pricingTests.BlackScholes_PutOption_WithKnownParameters_ReturnsExpectedPrice();
            Console.WriteLine("✓ Black-Scholes Put Option test passed");
            
            await pricingTests.BlackScholes_AtTheMoney_ReturnsReasonablePrice();
            Console.WriteLine("✓ Black-Scholes At-The-Money test passed");
            
            await pricingTests.BlackScholes_OutOfTheMoney_ReturnsLowerPrice();
            Console.WriteLine("✓ Black-Scholes Out-Of-The-Money test passed");
            
            await pricingTests.MonteCarloPricing_WithKnownParameters_ReturnsValidPrice();
            Console.WriteLine("✓ Monte Carlo Pricing test passed");
            
            await pricingTests.MonteCarloPricing_WithDifferentSimulations_ReturnsConvergentResults();
            Console.WriteLine("✓ Monte Carlo Pricing Convergence test passed");
            
            await pricingTests.BinomialTree_WithKnownParameters_ReturnsValidPrice();
            Console.WriteLine("✓ Binomial Tree test passed");
            
            await pricingTests.BinomialTree_WithDifferentSteps_ReturnsConvergentResults();
            Console.WriteLine("✓ Binomial Tree Convergence test passed");
            
            await pricingTests.PricingModels_WithZeroVolatility_ReturnsIntrinsicValue();
            Console.WriteLine("✓ Pricing Models Zero Volatility test passed");
            
            await pricingTests.PricingModels_WithZeroTimeToMaturity_ReturnsIntrinsicValue();
            Console.WriteLine("✓ Pricing Models Zero Time to Maturity test passed");
            
            await pricingTests.PricingModels_WithNegativeParameters_ReturnsError();
            Console.WriteLine("✓ Pricing Models Negative Parameters Error test passed");
            
            await pricingTests.PricingModels_WithMissingParameters_ReturnsError();
            Console.WriteLine("✓ Pricing Models Missing Parameters Error test passed");
            
            await pricingTests.PricingModels_PutCallParity_IsSatisfied();
            Console.WriteLine("✓ Pricing Models Put-Call Parity test passed");
            
            await pricingTests.PricingModels_Performance_WithLargeSimulations_CompletesInReasonableTime();
            Console.WriteLine("✓ Pricing Models Performance test passed");
            
            await pricingTests.PricingModels_WithDifferentOptionTypes_ReturnsAppropriatePrices();
            Console.WriteLine("✓ Pricing Models Different Option Types test passed");
            
            Console.WriteLine("All Pricing Model Tests Completed Successfully!");
        }

        [TestMethod]
        public async Task RunCompleteTestSuite()
        {
            Console.WriteLine("Running Complete Test Suite for Risk and Optimization Models");
            Console.WriteLine("==========================================================");
            Console.WriteLine($"Test execution started at: {DateTime.Now}");
            Console.WriteLine();
            
            var startTime = DateTime.Now;
            
            try
            {
                // Run all test categories
                await RunAllRiskModelTests();
                Console.WriteLine();
                
                await RunAllPortfolioOptimizationTests();
                Console.WriteLine();
                
                await RunAllMonteCarloSimulationTests();
                Console.WriteLine();
                
                await RunAllStatisticalModelTests();
                Console.WriteLine();
                
                await RunAllPricingModelTests();
                Console.WriteLine();
                
                var endTime = DateTime.Now;
                var totalTime = endTime - startTime;
                
                Console.WriteLine("==========================================================");
                Console.WriteLine("ALL TESTS COMPLETED SUCCESSFULLY!");
                Console.WriteLine($"Total execution time: {totalTime.TotalSeconds:F2} seconds");
                Console.WriteLine($"Test execution completed at: {endTime}");
                Console.WriteLine("==========================================================");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Test suite failed with error: {ex.Message}");
                throw;
            }
        }

        [TestMethod]
        public void GenerateTestReport()
        {
            Console.WriteLine("Risk and Optimization Models Test Report");
            Console.WriteLine("=========================================");
            Console.WriteLine();
            
            Console.WriteLine("Test Categories:");
            Console.WriteLine("1. Risk Model Tests (11 tests)");
            Console.WriteLine("   - VaR Historical with known datasets");
            Console.WriteLine("   - VaR Parametric with normal distribution");
            Console.WriteLine("   - VaR Monte Carlo with known parameters");
            Console.WriteLine("   - CVaR calculations");
            Console.WriteLine("   - Edge cases and error handling");
            Console.WriteLine("   - Performance stress tests");
            Console.WriteLine();
            
            Console.WriteLine("2. Portfolio Optimization Tests (11 tests)");
            Console.WriteLine("   - Markowitz mean-variance optimization");
            Console.WriteLine("   - Sharpe ratio validation");
            Console.WriteLine("   - Efficient frontier calculation");
            Console.WriteLine("   - Minimum variance optimization");
            Console.WriteLine("   - Risk parity optimization");
            Console.WriteLine("   - Black-Litterman optimization");
            Console.WriteLine("   - Equal weight portfolios");
            Console.WriteLine("   - Error handling and performance tests");
            Console.WriteLine();
            
            Console.WriteLine("3. Monte Carlo Simulation Tests (16 tests)");
            Console.WriteLine("   - Normal distribution simulations");
            Console.WriteLine("   - T-distribution simulations");
            Console.WriteLine("   - GARCH distribution simulations");
            Console.WriteLine("   - Copula distribution simulations");
            Console.WriteLine("   - Skewed-T distribution simulations");
            Console.WriteLine("   - Mixture distribution simulations");
            Console.WriteLine("   - Different confidence levels and time horizons");
            Console.WriteLine("   - Convergence and reproducibility tests");
            Console.WriteLine();
            
            Console.WriteLine("4. Statistical Model Tests (11 tests)");
            Console.WriteLine("   - GARCH volatility modeling");
            Console.WriteLine("   - Copula dependence modeling");
            Console.WriteLine("   - Regime switching models");
            Console.WriteLine("   - Parameter sensitivity tests");
            Console.WriteLine("   - Error handling and performance tests");
            Console.WriteLine();
            
            Console.WriteLine("5. Pricing Model Tests (16 tests)");
            Console.WriteLine("   - Black-Scholes option pricing");
            Console.WriteLine("   - Monte Carlo option pricing");
            Console.WriteLine("   - Binomial tree option pricing");
            Console.WriteLine("   - Put-call parity validation");
            Console.WriteLine("   - Intrinsic value calculations");
            Console.WriteLine("   - Convergence and performance tests");
            Console.WriteLine();
            
            Console.WriteLine("Total Tests: 65 comprehensive regression tests");
            Console.WriteLine("Coverage: VaR, CVaR, Sharpe ratio, optimization outputs, pricing models");
            Console.WriteLine("Validation: Known datasets, edge cases, error handling, performance");
        }
    }
}

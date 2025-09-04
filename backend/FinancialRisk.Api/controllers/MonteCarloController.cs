using FinancialRisk.Api.Models;
using FinancialRisk.Api.Services;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace FinancialRisk.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MonteCarloController : ControllerBase
    {
        private readonly ILogger<MonteCarloController> _logger;
        private readonly IVaRCalculationService _varService;
        private readonly IFinancialDataService _financialDataService;

        public MonteCarloController(
            ILogger<MonteCarloController> logger,
            IVaRCalculationService varService,
            IFinancialDataService financialDataService)
        {
            _logger = logger;
            _varService = varService;
            _financialDataService = financialDataService;
        }

        /// <summary>
        /// Run Monte Carlo simulation for a single asset
        /// </summary>
        /// <param name="request">Monte Carlo simulation parameters</param>
        /// <returns>Monte Carlo simulation results</returns>
        [HttpPost("simulate")]
        public async Task<ActionResult<MonteCarloSimulationResult>> RunSimulation([FromBody] MonteCarloSimulationRequest request)
        {
            try
            {
                _logger.LogInformation("Starting Monte Carlo simulation for {Symbol}", request.Symbol);

                // Fetch historical data
                var historyResult = await _financialDataService.GetStockHistoryAsync(request.Symbol, 252);
                if (!historyResult.Success || historyResult.Data == null || !historyResult.Data.Any())
                {
                    return BadRequest(new MonteCarloSimulationResult
                    {
                        Success = false,
                        Error = "No historical data available for the specified symbol"
                    });
                }

                // Calculate returns
                var returns = CalculateReturns(historyResult.Data);
                if (returns.Length < 2)
                {
                    return BadRequest(new MonteCarloSimulationResult
                    {
                        Success = false,
                        Error = "Insufficient data for Monte Carlo simulation"
                    });
                }

                // Convert to VaR request format
                var varRequest = new VaRCalculationRequest
                {
                    Symbol = request.Symbol,
                    CalculationType = "MonteCarlo",
                    DistributionType = request.DistributionType,
                    ConfidenceLevels = request.ConfidenceLevels,
                    Days = 252,
                    SimulationCount = request.NumSimulations,
                    TimeHorizon = request.TimeHorizon,
                    Parameters = request.CustomParameters
                };

                // Run VaR calculation
                var varResult = await _varService.CalculateVaRAsync(varRequest);
                if (!varResult.Success)
                {
                    return BadRequest(new MonteCarloSimulationResult
                    {
                        Success = false,
                        Error = varResult.Error ?? "Monte Carlo simulation failed"
                    });
                }

                // Convert to Monte Carlo result format
                var result = new MonteCarloSimulationResult
                {
                    Success = true,
                    Symbol = request.Symbol,
                    DistributionType = request.DistributionType,
                    NumSimulations = request.NumSimulations,
                    TimeHorizon = request.TimeHorizon,
                    VaRValues = new Dictionary<double, double>
                    {
                        { 0.95, varResult.Data?.VaR ?? 0.0 },
                        { 0.99, varResult.Data?.VaR ?? 0.0 } // Would need separate calculation for 99%
                    },
                    CVaRValues = new Dictionary<double, double>
                    {
                        { 0.95, varResult.Data?.CVaR ?? 0.0 },
                        { 0.99, varResult.Data?.CVaR ?? 0.0 } // Would need separate calculation for 99%
                    },
                    ExpectedValue = 0.0, // Would be calculated in full implementation
                    StandardDeviation = 0.0, // Would be calculated in full implementation
                    Skewness = 0.0,
                    Kurtosis = 0.0,
                    Percentiles = new Dictionary<double, double>(),
                    SimulatedReturns = new List<double>(),
                    SimulatedPrices = new List<double>(),
                    SimulationMetadata = new Dictionary<string, object>
                    {
                        { "calculation_type", "MonteCarlo" },
                        { "distribution_type", request.DistributionType },
                        { "num_simulations", request.NumSimulations },
                        { "time_horizon", request.TimeHorizon },
                        { "sample_size", returns.Length }
                    },
                    CalculationDate = DateTime.UtcNow
                };

                _logger.LogInformation("Monte Carlo simulation completed for {Symbol}", request.Symbol);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error running Monte Carlo simulation for {Symbol}", request.Symbol);
                return StatusCode(500, new MonteCarloSimulationResult
                {
                    Success = false,
                    Error = ex.Message
                });
            }
        }

        /// <summary>
        /// Run Monte Carlo simulation for a portfolio
        /// </summary>
        /// <param name="request">Portfolio Monte Carlo simulation parameters</param>
        /// <returns>Portfolio Monte Carlo simulation results</returns>
        [HttpPost("portfolio/simulate")]
        public async Task<ActionResult<MonteCarloPortfolioSimulationResult>> RunPortfolioSimulation([FromBody] MonteCarloPortfolioSimulationRequest request)
        {
            try
            {
                _logger.LogInformation("Starting portfolio Monte Carlo simulation for {PortfolioName}", request.PortfolioName);

                if (request.Symbols.Count != request.Weights.Count)
                {
                    return BadRequest(new MonteCarloPortfolioSimulationResult
                    {
                        Success = false,
                        Error = "Number of symbols must match number of weights"
                    });
                }

                // Convert to VaR request format
                var varRequest = new PortfolioVaRCalculationRequest
                {
                    PortfolioName = request.PortfolioName,
                    Symbols = request.Symbols,
                    Weights = request.Weights,
                    CalculationType = "MonteCarlo",
                    DistributionType = request.DistributionType,
                    ConfidenceLevels = request.ConfidenceLevels,
                    Days = 252,
                    SimulationCount = request.NumSimulations,
                    TimeHorizon = request.TimeHorizon,
                    Parameters = request.CustomParameters
                };

                // Run portfolio VaR calculation
                var varResult = await _varService.CalculatePortfolioVaRAsync(varRequest);
                if (!varResult.Success)
                {
                    return BadRequest(new MonteCarloPortfolioSimulationResult
                    {
                        Success = false,
                        Error = varResult.Error ?? "Portfolio Monte Carlo simulation failed"
                    });
                }

                // Convert to Monte Carlo result format
                var result = new MonteCarloPortfolioSimulationResult
                {
                    Success = true,
                    PortfolioName = request.PortfolioName,
                    DistributionType = request.DistributionType,
                    NumSimulations = request.NumSimulations,
                    TimeHorizon = request.TimeHorizon,
                    PortfolioVaRValues = new Dictionary<double, double>
                    {
                        { 0.95, varResult.Data?.PortfolioVaR ?? 0.0 },
                        { 0.99, varResult.Data?.PortfolioVaR ?? 0.0 } // Would need separate calculation for 99%
                    },
                    PortfolioCVaRValues = new Dictionary<double, double>
                    {
                        { 0.95, varResult.Data?.PortfolioCVaR ?? 0.0 },
                        { 0.99, varResult.Data?.PortfolioCVaR ?? 0.0 } // Would need separate calculation for 99%
                    },
                    ExpectedReturn = 0.0, // Would be calculated in full implementation
                    PortfolioVolatility = 0.0, // Would be calculated in full implementation
                    AssetResults = new List<MonteCarloSimulationResult>(),
                    VaRContributions = varResult.AssetContributions?.Select(c => c.VaRContribution).ToList() ?? new List<double>(),
                    MarginalVaR = varResult.AssetContributions?.Select(c => c.MarginalVaR).ToList() ?? new List<double>(),
                    ComponentVaR = varResult.AssetContributions?.Select(c => c.ComponentVaR).ToList() ?? new List<double>(),
                    DiversificationRatio = 1.0, // Would be calculated in full implementation
                    PortfolioReturns = new List<double>(),
                    PortfolioValues = new List<double>(),
                    SimulationMetadata = new Dictionary<string, object>
                    {
                        { "calculation_type", "MonteCarlo" },
                        { "distribution_type", request.DistributionType },
                        { "num_simulations", request.NumSimulations },
                        { "time_horizon", request.TimeHorizon },
                        { "num_assets", request.Symbols.Count },
                        { "use_correlation", request.UseCorrelation }
                    },
                    CalculationDate = DateTime.UtcNow
                };

                _logger.LogInformation("Portfolio Monte Carlo simulation completed for {PortfolioName}", request.PortfolioName);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error running portfolio Monte Carlo simulation for {PortfolioName}", request.PortfolioName);
                return StatusCode(500, new MonteCarloPortfolioSimulationResult
                {
                    Success = false,
                    Error = ex.Message
                });
            }
        }

        /// <summary>
        /// Run Monte Carlo stress test scenarios
        /// </summary>
        /// <param name="request">Stress test parameters</param>
        /// <returns>Stress test results</returns>
        [HttpPost("stress-test")]
        public async Task<ActionResult<MonteCarloStressTestResult>> RunStressTest([FromBody] MonteCarloStressTestRequest request)
        {
            try
            {
                _logger.LogInformation("Starting Monte Carlo stress test for {Symbol} - {ScenarioName}", 
                    request.Symbol, request.ScenarioName);

                // Run base simulation
                var baseRequest = new MonteCarloSimulationRequest
                {
                    Symbol = request.Symbol,
                    DistributionType = request.DistributionType,
                    NumSimulations = request.NumSimulations,
                    ConfidenceLevels = request.ConfidenceLevels,
                    CustomParameters = request.CustomParameters
                };

                var baseResult = await RunSimulation(baseRequest);
                if (baseResult.Value?.Success != true)
                {
                    return BadRequest(new MonteCarloStressTestResult
                    {
                        Success = false,
                        Error = "Base simulation failed"
                    });
                }

                // Run stressed simulation
                var stressedRequest = new MonteCarloSimulationRequest
                {
                    Symbol = request.Symbol,
                    DistributionType = request.DistributionType,
                    NumSimulations = request.NumSimulations,
                    ConfidenceLevels = request.ConfidenceLevels,
                    CustomParameters = ApplyStressFactor(request.CustomParameters, request.StressFactor, request.ScenarioType)
                };

                var stressedResult = await RunSimulation(stressedRequest);
                if (stressedResult.Value?.Success != true)
                {
                    return BadRequest(new MonteCarloStressTestResult
                    {
                        Success = false,
                        Error = "Stressed simulation failed"
                    });
                }

                // Create stress test result
                var result = new MonteCarloStressTestResult
                {
                    Success = true,
                    Symbol = request.Symbol,
                    ScenarioName = request.ScenarioName,
                    ScenarioType = request.ScenarioType,
                    StressFactor = request.StressFactor,
                    ScenarioResults = new Dictionary<string, MonteCarloSimulationResult>
                    {
                        { "base", baseResult.Value! },
                        { "stressed", stressedResult.Value! }
                    },
                    VaRComparison = new Dictionary<string, double>
                    {
                        { "base_95", baseResult.Value!.VaRValues.GetValueOrDefault(0.95, 0.0) },
                        { "stressed_95", stressedResult.Value!.VaRValues.GetValueOrDefault(0.95, 0.0) }
                    },
                    CVaRComparison = new Dictionary<string, double>
                    {
                        { "base_95", baseResult.Value!.CVaRValues.GetValueOrDefault(0.95, 0.0) },
                        { "stressed_95", stressedResult.Value!.CVaRValues.GetValueOrDefault(0.95, 0.0) }
                    },
                    CalculationDate = DateTime.UtcNow
                };

                _logger.LogInformation("Monte Carlo stress test completed for {Symbol} - {ScenarioName}", 
                    request.Symbol, request.ScenarioName);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error running Monte Carlo stress test for {Symbol}", request.Symbol);
                return StatusCode(500, new MonteCarloStressTestResult
                {
                    Success = false,
                    Error = ex.Message
                });
            }
        }

        /// <summary>
        /// Compare different Monte Carlo methods and distributions
        /// </summary>
        /// <param name="symbol">Asset symbol</param>
        /// <param name="numSimulations">Number of simulations</param>
        /// <returns>Comparison results</returns>
        [HttpGet("compare/{symbol}")]
        public async Task<ActionResult<MonteCarloComparisonResult>> CompareMethods(string symbol, int numSimulations = 10000)
        {
            try
            {
                _logger.LogInformation("Starting Monte Carlo method comparison for {Symbol}", symbol);

                var distributions = new[] { "Normal", "TStudent", "GARCH" };
                var results = new Dictionary<string, MonteCarloSimulationResult>();
                var executionTimes = new Dictionary<string, double>();

                foreach (var distribution in distributions)
                {
                    var stopwatch = Stopwatch.StartNew();
                    
                    var request = new MonteCarloSimulationRequest
                    {
                        Symbol = symbol,
                        DistributionType = distribution,
                        NumSimulations = numSimulations,
                        ConfidenceLevels = new List<double> { 0.95, 0.99 }
                    };

                    var result = await RunSimulation(request);
                    stopwatch.Stop();

                    if (result.Value?.Success == true)
                    {
                        results[distribution] = result.Value;
                        executionTimes[distribution] = stopwatch.Elapsed.TotalMilliseconds;
                    }
                }

                if (!results.Any())
                {
                    return BadRequest(new MonteCarloComparisonResult
                    {
                        Symbol = symbol,
                        VaRResults = new Dictionary<string, double>(),
                        CVaRResults = new Dictionary<string, double>(),
                        ExecutionTimes = new Dictionary<string, double>(),
                        AccuracyMetrics = new Dictionary<string, double>(),
                        BestMethod = "None",
                        BestDistribution = "None",
                        ComparisonDate = DateTime.UtcNow
                    });
                }

                // Find best method based on VaR (lowest is typically better)
                var bestDistribution = results
                    .OrderBy(r => r.Value.VaRValues.GetValueOrDefault(0.95, double.MaxValue))
                    .First().Key;

                var comparisonResult = new MonteCarloComparisonResult
                {
                    Symbol = symbol,
                    VaRResults = results.ToDictionary(r => r.Key, r => r.Value.VaRValues.GetValueOrDefault(0.95, 0.0)),
                    CVaRResults = results.ToDictionary(r => r.Key, r => r.Value.CVaRValues.GetValueOrDefault(0.95, 0.0)),
                    ExecutionTimes = executionTimes,
                    AccuracyMetrics = new Dictionary<string, double>(), // Would calculate accuracy metrics
                    BestMethod = "MonteCarlo",
                    BestDistribution = bestDistribution,
                    ComparisonDate = DateTime.UtcNow
                };

                _logger.LogInformation("Monte Carlo method comparison completed for {Symbol}", symbol);
                return Ok(comparisonResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error comparing Monte Carlo methods for {Symbol}", symbol);
                return StatusCode(500, new MonteCarloComparisonResult
                {
                    Symbol = symbol,
                    VaRResults = new Dictionary<string, double>(),
                    CVaRResults = new Dictionary<string, double>(),
                    ExecutionTimes = new Dictionary<string, double>(),
                    AccuracyMetrics = new Dictionary<string, double>(),
                    BestMethod = "Error",
                    BestDistribution = "Error",
                    ComparisonDate = DateTime.UtcNow
                });
            }
        }

        /// <summary>
        /// Get available distribution types for Monte Carlo simulation
        /// </summary>
        /// <returns>List of available distribution types</returns>
        [HttpGet("distributions")]
        public ActionResult<Dictionary<string, object>> GetAvailableDistributions()
        {
            var distributions = new Dictionary<string, object>
            {
                ["Normal"] = new
                {
                    Name = "Normal Distribution",
                    Description = "Standard normal distribution for returns",
                    Parameters = new[] { "mean", "volatility" },
                    UseCase = "Standard market conditions"
                },
                ["TStudent"] = new
                {
                    Name = "Student's t-Distribution",
                    Description = "Heavy-tailed distribution for fat tails",
                    Parameters = new[] { "degrees_of_freedom", "location", "scale" },
                    UseCase = "Market stress periods"
                },
                ["GARCH"] = new
                {
                    Name = "GARCH Process",
                    Description = "Time-varying volatility model",
                    Parameters = new[] { "omega", "alpha", "beta" },
                    UseCase = "Volatility clustering"
                },
                ["Copula"] = new
                {
                    Name = "Copula-based",
                    Description = "Dependency structure modeling",
                    Parameters = new[] { "correlation_matrix" },
                    UseCase = "Portfolio dependencies"
                },
                ["Mixture"] = new
                {
                    Name = "Mixture Distribution",
                    Description = "Mixture of multiple distributions",
                    Parameters = new[] { "weights", "means", "volatilities" },
                    UseCase = "Regime switching"
                }
            };

            return Ok(distributions);
        }

        private double[] CalculateReturns(List<StockQuote> prices)
        {
            if (prices.Count < 2) return new double[0];

            var returns = new double[prices.Count - 1];
            for (int i = 1; i < prices.Count; i++)
            {
                returns[i - 1] = (double)((prices[i].Close - prices[i - 1].Close) / prices[i - 1].Close);
            }
            return returns;
        }

        private Dictionary<string, object>? ApplyStressFactor(Dictionary<string, object>? parameters, 
            double stressFactor, string scenarioType)
        {
            if (parameters == null) return null;

            var stressedParams = new Dictionary<string, object>(parameters);

            switch (scenarioType.ToLower())
            {
                case "volatilityshock":
                    if (stressedParams.ContainsKey("volatility"))
                    {
                        stressedParams["volatility"] = (double)stressedParams["volatility"] * stressFactor;
                    }
                    break;
                case "returnshock":
                    if (stressedParams.ContainsKey("mean"))
                    {
                        stressedParams["mean"] = (double)stressedParams["mean"] * stressFactor;
                    }
                    break;
                case "correlationshock":
                    if (stressedParams.ContainsKey("correlation"))
                    {
                        stressedParams["correlation"] = (double)stressedParams["correlation"] * stressFactor;
                    }
                    break;
            }

            return stressedParams;
        }
    }
}

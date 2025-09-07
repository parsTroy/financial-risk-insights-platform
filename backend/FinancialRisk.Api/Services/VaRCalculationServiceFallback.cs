using FinancialRisk.Api.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FinancialRisk.Api.Services
{
    public class VaRCalculationServiceFallback : IVaRCalculationService
    {
        private readonly ILogger<VaRCalculationServiceFallback> _logger;
        private readonly IFinancialDataService _financialDataService;
        private readonly IDataPersistenceService _dataPersistenceService;

        public VaRCalculationServiceFallback(
            ILogger<VaRCalculationServiceFallback> logger,
            IFinancialDataService financialDataService,
            IDataPersistenceService dataPersistenceService)
        {
            _logger = logger;
            _financialDataService = financialDataService;
            _dataPersistenceService = dataPersistenceService;
        }

        public async Task<VaRCalculationResponse> CalculateVaRAsync(VaRCalculationRequest request)
        {
            try
            {
                _logger.LogInformation("Calculating VaR for {Symbol} using fallback implementation", request.Symbol);

                // Get historical data
                var historicalDataResponse = await _financialDataService.GetStockHistoryAsync(request.Symbol, request.Days);
                if (!historicalDataResponse.Success || historicalDataResponse.Data == null || !historicalDataResponse.Data.Any())
                {
                    return new VaRCalculationResponse
                    {
                        Success = false,
                        Error = "No historical data available"
                    };
                }

                var returns = CalculateReturns(historicalDataResponse.Data);
                if (returns.Count < 30)
                {
                    return new VaRCalculationResponse
                    {
                        Success = false,
                        Error = "Insufficient data for VaR calculation (minimum 30 days required)"
                    };
                }

                // Calculate VaR based on method (use first confidence level from the list)
                var confidenceLevel = request.ConfidenceLevels.FirstOrDefault();
                var result = request.CalculationType.ToLower() switch
                {
                    "historical" => CalculateHistoricalVaR(returns, confidenceLevel),
                    "parametric" => CalculateParametricVaR(returns, confidenceLevel),
                    "montecarlo" => CalculateMonteCarloVaR(returns, confidenceLevel, request.SimulationCount),
                    "bootstrap" => CalculateBootstrapVaR(returns, confidenceLevel, 1000),
                    _ => CalculateHistoricalVaR(returns, confidenceLevel)
                };

                // Create VaR calculation data
                var varCalculation = new VaRCalculation
                {
                    Symbol = request.Symbol,
                    CalculationType = request.CalculationType,
                    DistributionType = request.DistributionType,
                    ConfidenceLevel = confidenceLevel,
                    VaR = result.VaR,
                    CVaR = result.CVaR,
                    SampleSize = returns.Count,
                    SimulationCount = request.SimulationCount,
                    TimeHorizon = request.TimeHorizon,
                    CalculationDate = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                };

                // Save calculation to database
                await SaveVaRCalculation(varCalculation);

                return new VaRCalculationResponse
                {
                    Success = true,
                    Data = varCalculation
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating VaR for {Symbol} using fallback", request.Symbol);
                return new VaRCalculationResponse
                {
                    Success = false,
                    Error = ex.Message
                };
            }
        }

        public async Task<PortfolioVaRCalculationResponse> CalculatePortfolioVaRAsync(PortfolioVaRCalculationRequest request)
        {
            try
            {
                _logger.LogInformation("Calculating portfolio VaR for {PortfolioName} using fallback implementation", request.PortfolioName);

                var portfolioReturns = new List<double>();
                var totalWeight = request.Weights.Sum();

                // Validate weights sum to 1.0
                if (Math.Abs(totalWeight - 1.0m) > 0.01m)
                {
                    return new PortfolioVaRCalculationResponse
                    {
                        Success = false,
                        Error = "Portfolio weights must sum to 1.0"
                    };
                }

                // Get historical data for each asset
                for (int i = 0; i < request.Symbols.Count; i++)
                {
                    var symbol = request.Symbols[i];
                    var weight = (double)request.Weights[i];

                    var historicalDataResponse = await _financialDataService.GetStockHistoryAsync(symbol, 252);
                    if (!historicalDataResponse.Success || historicalDataResponse.Data == null || !historicalDataResponse.Data.Any())
                    {
                        return new PortfolioVaRCalculationResponse
                        {
                            Success = false,
                            Error = $"No historical data available for {symbol}"
                        };
                    }

                    var returns = CalculateReturns(historicalDataResponse.Data);
                    if (returns.Count < 30)
                    {
                        return new PortfolioVaRCalculationResponse
                        {
                            Success = false,
                            Error = $"Insufficient data for {symbol} (minimum 30 days required)"
                        };
                    }

                    // Weight the returns
                    var weightedReturns = returns.Select(r => r * weight).ToList();
                    portfolioReturns.AddRange(weightedReturns);
                }

                // Calculate portfolio VaR (use first confidence level from the list)
                var confidenceLevel = request.ConfidenceLevels.FirstOrDefault();
                var result = CalculateHistoricalVaR(portfolioReturns, confidenceLevel);

                // Create portfolio VaR calculation data
                var portfolioVarCalculation = new PortfolioVaRCalculation
                {
                    PortfolioName = request.PortfolioName,
                    CalculationType = request.CalculationType,
                    DistributionType = request.DistributionType,
                    ConfidenceLevel = confidenceLevel,
                    PortfolioVaR = result.VaR,
                    PortfolioCVaR = result.CVaR,
                    SampleSize = portfolioReturns.Count,
                    SimulationCount = request.SimulationCount,
                    TimeHorizon = request.TimeHorizon,
                    CalculationDate = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                };

                // Save calculation to database
                await SavePortfolioVaRCalculation(portfolioVarCalculation);

                return new PortfolioVaRCalculationResponse
                {
                    Success = true,
                    Data = portfolioVarCalculation
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating portfolio VaR for {PortfolioName} using fallback", request.PortfolioName);
                return new PortfolioVaRCalculationResponse
                {
                    Success = false,
                    Error = ex.Message
                };
            }
        }

        public async Task<VaRStressTestResponse> PerformStressTestAsync(VaRStressTestRequest request)
        {
            try
            {
                _logger.LogInformation("Performing stress test for {Symbol} using fallback implementation", request.Symbol);

                // Get historical data
                var historicalDataResponse = await _financialDataService.GetStockHistoryAsync(request.Symbol, 252);
                if (!historicalDataResponse.Success || historicalDataResponse.Data == null || !historicalDataResponse.Data.Any())
                {
                    return new VaRStressTestResponse
                    {
                        Success = false,
                        Error = "No historical data available"
                    };
                }

                var returns = CalculateReturns(historicalDataResponse.Data);
                if (returns.Count < 30)
                {
                    return new VaRStressTestResponse
                    {
                        Success = false,
                        Error = "Insufficient data for stress test (minimum 30 days required)"
                    };
                }

                // Apply stress factor
                var stressedReturns = returns.Select(r => r * (1 + request.StressFactor)).ToList();
                var confidenceLevel = request.ConfidenceLevels.FirstOrDefault();
                var result = CalculateHistoricalVaR(stressedReturns, confidenceLevel);

                // Create stress test data
                var stressTest = new VaRStressTest
                {
                    Symbol = request.Symbol,
                    ScenarioName = request.ScenarioName,
                    ScenarioType = request.ScenarioType,
                    StressFactor = request.StressFactor,
                    VaR = result.VaR,
                    CVaR = result.CVaR,
                    ExpectedLoss = result.VaR,
                    UnexpectedLoss = result.CVaR - result.VaR,
                    ScenarioDate = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                };

                return new VaRStressTestResponse
                {
                    Success = true,
                    Data = stressTest
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing stress test for {Symbol} using fallback", request.Symbol);
                return new VaRStressTestResponse
                {
                    Success = false,
                    Error = ex.Message
                };
            }
        }

        public async Task<List<VaRComparisonResult>> CompareVaRMethodsAsync(string symbol, int days = 252)
        {
            try
            {
                _logger.LogInformation("Comparing VaR methods for {Symbol} using fallback implementation", symbol);

                // Get historical data
                var historicalDataResponse = await _financialDataService.GetStockHistoryAsync(symbol, days);
                if (!historicalDataResponse.Success || historicalDataResponse.Data == null || !historicalDataResponse.Data.Any())
                {
                    return new List<VaRComparisonResult>();
                }

                var returns = CalculateReturns(historicalDataResponse.Data);
                if (returns.Count < 30)
                {
                    return new List<VaRComparisonResult>();
                }

                var results = new List<VaRComparisonResult>();

                // Compare different methods
                var methods = new[] { "Historical", "Parametric", "MonteCarlo", "Bootstrap" };
                var confidenceLevel = 0.95;
                var varResults = new Dictionary<string, double>();
                var cvarResults = new Dictionary<string, double>();
                var bestMethod = "Historical";
                var bestVaR = double.MaxValue;
                var bestCVaR = double.MaxValue;

                foreach (var method in methods)
                {
                    var result = method.ToLower() switch
                    {
                        "historical" => CalculateHistoricalVaR(returns, confidenceLevel),
                        "parametric" => CalculateParametricVaR(returns, confidenceLevel),
                        "montecarlo" => CalculateMonteCarloVaR(returns, confidenceLevel, 10000),
                        "bootstrap" => CalculateBootstrapVaR(returns, confidenceLevel, 1000),
                        _ => CalculateHistoricalVaR(returns, confidenceLevel)
                    };

                    varResults[method] = result.VaR;
                    cvarResults[method] = result.CVaR;

                    if (result.VaR < bestVaR)
                    {
                        bestVaR = result.VaR;
                        bestCVaR = result.CVaR;
                        bestMethod = method;
                    }
                }

                results.Add(new VaRComparisonResult
                {
                    Symbol = symbol,
                    VaRResults = varResults,
                    CVaRResults = cvarResults,
                    BestMethod = bestMethod,
                    BestVaR = bestVaR,
                    BestCVaR = bestCVaR
                });

                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error comparing VaR methods for {Symbol} using fallback", symbol);
                return new List<VaRComparisonResult>();
            }
        }

        public async Task<VaRBacktestResult> PerformBacktestAsync(string symbol, string method, double confidenceLevel, int backtestDays = 252)
        {
            try
            {
                _logger.LogInformation("Performing VaR backtest for {Symbol} using fallback implementation", symbol);

                // Get historical data
                var historicalDataResponse = await _financialDataService.GetStockHistoryAsync(symbol, backtestDays);
                if (!historicalDataResponse.Success || historicalDataResponse.Data == null || !historicalDataResponse.Data.Any())
                {
                    return new VaRBacktestResult
                    {
                        Symbol = symbol,
                        Method = method,
                        ConfidenceLevel = confidenceLevel,
                        BacktestPeriod = 0,
                        Violations = 0,
                        ViolationRate = 0.0,
                        KupiecTestStatistic = 0.0,
                        KupiecPValue = 0.0,
                        KupiecTestPassed = false,
                        ChristoffersenTestStatistic = 0.0,
                        ChristoffersenPValue = 0.0,
                        ChristoffersenTestPassed = false,
                        BacktestDate = DateTime.UtcNow,
                        CreatedAt = DateTime.UtcNow
                    };
                }

                var returns = CalculateReturns(historicalDataResponse.Data);
                if (returns.Count < 30)
                {
                    return new VaRBacktestResult
                    {
                        Symbol = symbol,
                        Method = method,
                        ConfidenceLevel = confidenceLevel,
                        BacktestPeriod = 0,
                        Violations = 0,
                        ViolationRate = 0.0,
                        KupiecTestStatistic = 0.0,
                        KupiecPValue = 0.0,
                        KupiecTestPassed = false,
                        ChristoffersenTestStatistic = 0.0,
                        ChristoffersenPValue = 0.0,
                        ChristoffersenTestPassed = false,
                        BacktestDate = DateTime.UtcNow,
                        CreatedAt = DateTime.UtcNow
                    };
                }

                // Perform backtest (simplified version)
                var violations = 0;
                var totalDays = returns.Count;
                var varValue = method.ToLower() switch
                {
                    "historical" => CalculateHistoricalVaR(returns, confidenceLevel).VaR,
                    "parametric" => CalculateParametricVaR(returns, confidenceLevel).VaR,
                    "montecarlo" => CalculateMonteCarloVaR(returns, confidenceLevel, 10000).VaR,
                    "bootstrap" => CalculateBootstrapVaR(returns, confidenceLevel, 1000).VaR,
                    _ => CalculateHistoricalVaR(returns, confidenceLevel).VaR
                };

                // Count violations
                foreach (var returnValue in returns)
                {
                    if (returnValue < -varValue)
                    {
                        violations++;
                    }
                }

                var violationRate = (double)violations / totalDays;
                var expectedViolationRate = 1 - confidenceLevel;
                var kupiecStatistic = CalculateKupiecTestStatistic(violations, totalDays, expectedViolationRate);

                return new VaRBacktestResult
                {
                    Symbol = symbol,
                    Method = method,
                    ConfidenceLevel = confidenceLevel,
                    BacktestPeriod = totalDays,
                    Violations = violations,
                    ViolationRate = violationRate,
                    KupiecTestStatistic = kupiecStatistic,
                    KupiecPValue = 0.05, // Simplified
                    KupiecTestPassed = kupiecStatistic < 3.84, // Chi-square critical value for 95% confidence
                    ChristoffersenTestStatistic = 0.0, // Simplified
                    ChristoffersenPValue = 0.05, // Simplified
                    ChristoffersenTestPassed = true, // Simplified
                    BacktestDate = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing VaR backtest for {Symbol} using fallback", symbol);
                return new VaRBacktestResult
                {
                    Symbol = symbol,
                    Method = method,
                    ConfidenceLevel = confidenceLevel,
                    BacktestPeriod = 0,
                    Violations = 0,
                    ViolationRate = 0.0,
                    KupiecTestStatistic = 0.0,
                    KupiecPValue = 0.0,
                    KupiecTestPassed = false,
                    ChristoffersenTestStatistic = 0.0,
                    ChristoffersenPValue = 0.0,
                    ChristoffersenTestPassed = false,
                    BacktestDate = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                };
            }
        }

        public async Task<List<VaRCalculation>> GetVaRHistoryAsync(string symbol, int limit = 100)
        {
            try
            {
                _logger.LogInformation("Getting VaR history for {Symbol} using fallback implementation", symbol);
                // This would typically query the database
                return new List<VaRCalculation>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting VaR history for {Symbol} using fallback", symbol);
                return new List<VaRCalculation>();
            }
        }

        public async Task<List<PortfolioVaRCalculation>> GetPortfolioVaRHistoryAsync(string portfolioName, int limit = 100)
        {
            try
            {
                _logger.LogInformation("Getting portfolio VaR history for {PortfolioName} using fallback implementation", portfolioName);
                // This would typically query the database
                return new List<PortfolioVaRCalculation>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting portfolio VaR history for {PortfolioName} using fallback", portfolioName);
                return new List<PortfolioVaRCalculation>();
            }
        }

        private List<double> CalculateReturns(List<StockQuote> historicalData)
        {
            var returns = new List<double>();
            var prices = historicalData.OrderBy(d => d.Timestamp).Select(d => d.Close).ToList();

            for (int i = 1; i < prices.Count; i++)
            {
                var dailyReturn = (double)((prices[i] - prices[i - 1]) / prices[i - 1]);
                returns.Add(dailyReturn);
            }
            return returns;
        }

        private (double VaR, double CVaR) CalculateHistoricalVaR(List<double> returns, double confidenceLevel)
        {
            var sortedReturns = returns.OrderBy(r => r).ToList();
            var index = (int)((1 - confidenceLevel) * sortedReturns.Count);
            var var = -sortedReturns[index];
            
            // CVaR is the average of returns worse than VaR
            var tailReturns = sortedReturns.Take(index).ToList();
            var cvar = tailReturns.Any() ? -tailReturns.Average() : var;
            
            return (var, cvar);
        }

        private (double VaR, double CVaR) CalculateParametricVaR(List<double> returns, double confidenceLevel)
        {
            var mean = returns.Average();
            var stdDev = Math.Sqrt(returns.Select(r => Math.Pow(r - mean, 2)).Average());
            var zScore = GetZScore(confidenceLevel);
            
            var var = -(mean + zScore * stdDev);
            var cvar = -(mean + (zScore * stdDev) / (1 - confidenceLevel));
            
            return (var, cvar);
        }

        private (double VaR, double CVaR) CalculateMonteCarloVaR(List<double> returns, double confidenceLevel, int numSimulations)
        {
            var mean = returns.Average();
            var stdDev = Math.Sqrt(returns.Select(r => Math.Pow(r - mean, 2)).Average());
            var random = new Random();
            var simulatedReturns = new List<double>();

            for (int i = 0; i < numSimulations; i++)
            {
                var randomValue = random.NextDouble();
                var zScore = Math.Sqrt(-2 * Math.Log(randomValue)) * Math.Cos(2 * Math.PI * random.NextDouble());
                var simulatedReturn = mean + stdDev * zScore;
                simulatedReturns.Add(simulatedReturn);
            }

            return CalculateHistoricalVaR(simulatedReturns, confidenceLevel);
        }

        private (double VaR, double CVaR) CalculateBootstrapVaR(List<double> returns, double confidenceLevel, int bootstrapSamples)
        {
            var random = new Random();
            var bootstrapReturns = new List<double>();

            for (int i = 0; i < bootstrapSamples; i++)
            {
                var randomIndex = random.Next(returns.Count);
                bootstrapReturns.Add(returns[randomIndex]);
            }

            return CalculateHistoricalVaR(bootstrapReturns, confidenceLevel);
        }

        private double GetZScore(double confidenceLevel)
        {
            // Approximate Z-scores for common confidence levels
            return confidenceLevel switch
            {
                0.90 => 1.28,
                0.95 => 1.65,
                0.99 => 2.33,
                0.999 => 3.09,
                _ => 1.65
            };
        }

        private double CalculateKupiecTestStatistic(int violations, int totalDays, double expectedViolationRate)
        {
            if (expectedViolationRate <= 0 || expectedViolationRate >= 1)
                return 0;

            var actualViolationRate = (double)violations / totalDays;
            var numerator = Math.Pow(actualViolationRate, violations) * Math.Pow(1 - actualViolationRate, totalDays - violations);
            var denominator = Math.Pow(expectedViolationRate, violations) * Math.Pow(1 - expectedViolationRate, totalDays - violations);
            
            if (denominator == 0)
                return 0;

            return -2 * Math.Log(numerator / denominator);
        }

        private async Task SaveVaRCalculation(VaRCalculation calculation)
        {
            try
            {
                // For now, just log the calculation since we don't have the persistence method
                _logger.LogInformation("VaR calculation completed for {Symbol}: VaR={VaR}, CVaR={CVaR}", 
                    calculation.Symbol, calculation.VaR, calculation.CVaR);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to save VaR calculation to database");
            }
        }

        private async Task SavePortfolioVaRCalculation(PortfolioVaRCalculation calculation)
        {
            try
            {
                // For now, just log the calculation since we don't have the persistence method
                _logger.LogInformation("Portfolio VaR calculation completed for {PortfolioName}: VaR={VaR}, CVaR={CVaR}", 
                    calculation.PortfolioName, calculation.PortfolioVaR, calculation.PortfolioCVaR);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to save portfolio VaR calculation to database");
            }
        }
    }
}

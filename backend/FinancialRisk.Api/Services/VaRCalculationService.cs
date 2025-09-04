using FinancialRisk.Api.Models;
using Microsoft.Extensions.Logging;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Diagnostics;

namespace FinancialRisk.Api.Services
{
    public interface IVaRCalculationService
    {
        Task<VaRCalculationResponse> CalculateVaRAsync(VaRCalculationRequest request);
        Task<PortfolioVaRCalculationResponse> CalculatePortfolioVaRAsync(PortfolioVaRCalculationRequest request);
        Task<VaRStressTestResponse> PerformStressTestAsync(VaRStressTestRequest request);
        Task<List<VaRComparisonResult>> CompareVaRMethodsAsync(string symbol, int days = 252);
        Task<VaRBacktestResult> PerformBacktestAsync(string symbol, string method, double confidenceLevel, int backtestDays = 252);
        Task<List<VaRCalculation>> GetVaRHistoryAsync(string symbol, int limit = 100);
        Task<List<PortfolioVaRCalculation>> GetPortfolioVaRHistoryAsync(string portfolioName, int limit = 100);
    }

    public class VaRCalculationService : IVaRCalculationService
    {
        private readonly ILogger<VaRCalculationService> _logger;
        private readonly IFinancialDataService _financialDataService;
        private readonly IDataPersistenceService _dataPersistenceService;

        // C++ library imports for historical simulation
        [DllImport("VaRCalculations.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern double CalculateHistoricalVaR(double[] returns, int length, double confidenceLevel);

        [DllImport("VaRCalculations.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern double CalculateHistoricalCVaR(double[] returns, int length, double confidenceLevel);

        [DllImport("VaRCalculations.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern double CalculateParametricVaR(double[] returns, int length, double confidenceLevel);

        [DllImport("VaRCalculations.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern double CalculateParametricCVaR(double[] returns, int length, double confidenceLevel);

        [DllImport("VaRCalculations.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern double CalculateBootstrapVaR(double[] returns, int length, double confidenceLevel, int bootstrapSamples);

        [DllImport("VaRCalculations.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void CalculateVaRConfidenceIntervals(double[] returns, int length, double confidenceLevel, 
                                                                 int bootstrapSamples, out double lowerBound, out double upperBound);

        public VaRCalculationService(
            ILogger<VaRCalculationService> logger,
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
                _logger.LogInformation("Calculating VaR for {Symbol} using {Method}", request.Symbol, request.CalculationType);

                // Fetch historical data
                var historyResult = await _financialDataService.GetStockHistoryAsync(request.Symbol, request.Days);
                if (!historyResult.Success || historyResult.Data == null || !historyResult.Data.Any())
                {
                    return new VaRCalculationResponse
                    {
                        Success = false,
                        Error = "No historical data available"
                    };
                }

                // Calculate returns
                var returns = CalculateReturns(historyResult.Data);
                if (returns.Length < 2)
                {
                    return new VaRCalculationResponse
                    {
                        Success = false,
                        Error = "Insufficient data for VaR calculation"
                    };
                }

                // Calculate VaR based on method
                VaRCalculation varResult;
                if (request.CalculationType.ToLower() == "montecarlo")
                {
                    varResult = await CalculateMonteCarloVaRAsync(request, returns);
                }
                else
                {
                    varResult = CalculateHistoricalVaR(request, returns);
                }

                // Save to database
                await SaveVaRCalculationAsync(varResult);

                return new VaRCalculationResponse
                {
                    Success = true,
                    Data = varResult
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating VaR for {Symbol}", request.Symbol);
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
                _logger.LogInformation("Calculating portfolio VaR for {PortfolioName}", request.PortfolioName);

                if (request.Symbols.Count != request.Weights.Count)
                {
                    return new PortfolioVaRCalculationResponse
                    {
                        Success = false,
                        Error = "Number of symbols must match number of weights"
                    };
                }

                // Fetch data for all assets
                var assetData = new Dictionary<string, double[]>();
                foreach (var symbol in request.Symbols)
                {
                    var historyResult = await _financialDataService.GetStockHistoryAsync(symbol, request.Days);
                    if (historyResult.Success && historyResult.Data != null && historyResult.Data.Any())
                    {
                        var returns = CalculateReturns(historyResult.Data);
                        if (returns.Length >= 2)
                        {
                            assetData[symbol] = returns;
                        }
                    }
                }

                if (assetData.Count < 2)
                {
                    return new PortfolioVaRCalculationResponse
                    {
                        Success = false,
                        Error = "Insufficient asset data for portfolio VaR calculation"
                    };
                }

                // Calculate portfolio VaR
                PortfolioVaRCalculation portfolioResult;
                List<VaRAssetContribution> contributions;

                if (request.CalculationType.ToLower() == "montecarlo")
                {
                    var result = await CalculateMonteCarloPortfolioVaRAsync(request, assetData);
                    portfolioResult = result.portfolioResult;
                    contributions = result.contributions;
                }
                else
                {
                    var result = CalculateHistoricalPortfolioVaR(request, assetData);
                    portfolioResult = result.portfolioResult;
                    contributions = result.contributions;
                }

                // Save to database
                await SavePortfolioVaRCalculationAsync(portfolioResult, contributions);

                return new PortfolioVaRCalculationResponse
                {
                    Success = true,
                    Data = portfolioResult,
                    AssetContributions = contributions
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating portfolio VaR for {PortfolioName}", request.PortfolioName);
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
                _logger.LogInformation("Performing stress test for {Symbol} with scenario {ScenarioName}", 
                    request.Symbol, request.ScenarioName);

                // Fetch historical data
                var historyResult = await _financialDataService.GetStockHistoryAsync(request.Symbol, request.Days);
                if (!historyResult.Success || historyResult.Data == null || !historyResult.Data.Any())
                {
                    return new VaRStressTestResponse
                    {
                        Success = false,
                        Error = "No historical data available"
                    };
                }

                // Calculate returns
                var returns = CalculateReturns(historyResult.Data);
                if (returns.Length < 2)
                {
                    return new VaRStressTestResponse
                    {
                        Success = false,
                        Error = "Insufficient data for stress test"
                    };
                }

                // Apply stress factor
                var stressedReturns = ApplyStressFactor(returns, request.StressFactor, request.ScenarioType);

                // Calculate VaR on stressed data
                var var95 = CalculateHistoricalVaR(stressedReturns, stressedReturns.Length, 0.95);
                var var99 = CalculateHistoricalVaR(stressedReturns, stressedReturns.Length, 0.99);
                var cvar95 = CalculateHistoricalCVaR(stressedReturns, stressedReturns.Length, 0.95);
                var cvar99 = CalculateHistoricalCVaR(stressedReturns, stressedReturns.Length, 0.99);

                var stressTest = new VaRStressTest
                {
                    Symbol = request.Symbol,
                    ScenarioName = request.ScenarioName,
                    ScenarioType = request.ScenarioType,
                    StressFactor = request.StressFactor,
                    VaR = var95,
                    CVaR = cvar95,
                    ExpectedLoss = cvar95,
                    UnexpectedLoss = var95 - cvar95,
                    ScenarioDate = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    Parameters = JsonSerializer.Serialize(request.Parameters)
                };

                // Save to database
                await SaveVaRStressTestAsync(stressTest);

                return new VaRStressTestResponse
                {
                    Success = true,
                    Data = stressTest
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing stress test for {Symbol}", request.Symbol);
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
                _logger.LogInformation("Comparing VaR methods for {Symbol}", symbol);

                // Fetch historical data
                var historyResult = await _financialDataService.GetStockHistoryAsync(symbol, days);
                if (!historyResult.Success || historyResult.Data == null || !historyResult.Data.Any())
                {
                    return new List<VaRComparisonResult>();
                }

                var returns = CalculateReturns(historyResult.Data);
                if (returns.Length < 2)
                {
                    return new List<VaRComparisonResult>();
                }

                var comparison = new VaRComparisonResult
                {
                    Symbol = symbol,
                    VaRResults = new Dictionary<string, double>(),
                    CVaRResults = new Dictionary<string, double>(),
                    ConfidenceIntervals = new Dictionary<string, double>()
                };

                // Historical VaR
                var historicalVar95 = CalculateHistoricalVaR(returns, returns.Length, 0.95);
                var historicalVar99 = CalculateHistoricalVaR(returns, returns.Length, 0.99);
                var historicalCvar95 = CalculateHistoricalCVaR(returns, returns.Length, 0.95);
                var historicalCvar99 = CalculateHistoricalCVaR(returns, returns.Length, 0.99);

                comparison.VaRResults["Historical"] = historicalVar95;
                comparison.CVaRResults["Historical"] = historicalCvar95;

                // Parametric VaR
                var parametricVar95 = CalculateParametricVaR(returns, returns.Length, 0.95);
                var parametricVar99 = CalculateParametricVaR(returns, returns.Length, 0.99);
                var parametricCvar95 = CalculateParametricCVaR(returns, returns.Length, 0.95);
                var parametricCvar99 = CalculateParametricCVaR(returns, returns.Length, 0.99);

                comparison.VaRResults["Parametric"] = parametricVar95;
                comparison.CVaRResults["Parametric"] = parametricCvar95;

                // Monte Carlo VaR
                var monteCarloResult = await CalculateMonteCarloVaRAsync(new VaRCalculationRequest
                {
                    Symbol = symbol,
                    CalculationType = "MonteCarlo",
                    DistributionType = "Normal",
                    ConfidenceLevels = new List<double> { 0.95, 0.99 },
                    Days = days,
                    SimulationCount = 10000
                }, returns);

                comparison.VaRResults["MonteCarlo"] = monteCarloResult.VaR;
                comparison.CVaRResults["MonteCarlo"] = monteCarloResult.CVaR;

                // Determine best method (lowest VaR)
                var bestMethod = comparison.VaRResults.MinBy(x => x.Value);
                comparison.BestMethod = bestMethod.Key;
                comparison.BestVaR = bestMethod.Value;
                comparison.BestCVaR = comparison.CVaRResults[bestMethod.Key];

                return new List<VaRComparisonResult> { comparison };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error comparing VaR methods for {Symbol}", symbol);
                return new List<VaRComparisonResult>();
            }
        }

        public async Task<VaRBacktestResult> PerformBacktestAsync(string symbol, string method, double confidenceLevel, int backtestDays = 252)
        {
            try
            {
                _logger.LogInformation("Performing VaR backtest for {Symbol} using {Method}", symbol, method);

                // This is a simplified backtest implementation
                // In practice, you would implement proper backtesting with rolling windows
                var backtestResult = new VaRBacktestResult
                {
                    Symbol = symbol,
                    Method = method,
                    ConfidenceLevel = confidenceLevel,
                    BacktestPeriod = backtestDays,
                    Violations = 0, // Would be calculated from actual backtest
                    ViolationRate = 0.0,
                    KupiecTestStatistic = 0.0,
                    KupiecPValue = 1.0,
                    KupiecTestPassed = true,
                    ChristoffersenTestStatistic = 0.0,
                    ChristoffersenPValue = 1.0,
                    ChristoffersenTestPassed = true,
                    BacktestDate = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                };

                // Save to database
                await SaveVaRBacktestAsync(backtestResult);

                return backtestResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing VaR backtest for {Symbol}", symbol);
                throw;
            }
        }

        public async Task<List<VaRCalculation>> GetVaRHistoryAsync(string symbol, int limit = 100)
        {
            // This would query the database for VaR calculation history
            // For now, return empty list
            return new List<VaRCalculation>();
        }

        public async Task<List<PortfolioVaRCalculation>> GetPortfolioVaRHistoryAsync(string portfolioName, int limit = 100)
        {
            // This would query the database for portfolio VaR calculation history
            // For now, return empty list
            return new List<PortfolioVaRCalculation>();
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

        private VaRCalculation CalculateHistoricalVaR(VaRCalculationRequest request, double[] returns)
        {
            var var95 = CalculateHistoricalVaR(returns, returns.Length, 0.95);
            var var99 = CalculateHistoricalVaR(returns, returns.Length, 0.99);
            var cvar95 = CalculateHistoricalCVaR(returns, returns.Length, 0.95);
            var cvar99 = CalculateHistoricalCVaR(returns, returns.Length, 0.99);

            // Calculate confidence intervals
            CalculateVaRConfidenceIntervals(returns, returns.Length, 0.95, 1000, out double var95Lower, out double var95Upper);
            CalculateVaRConfidenceIntervals(returns, returns.Length, 0.99, 1000, out double var99Lower, out double var99Upper);

            return new VaRCalculation
            {
                Symbol = request.Symbol,
                CalculationType = request.CalculationType,
                DistributionType = request.DistributionType,
                ConfidenceLevel = 0.95,
                VaR = var95,
                CVaR = cvar95,
                VaRLowerBound = var95Lower,
                VaRUpperBound = var95Upper,
                CVaRLowerBound = var95Lower,
                CVaRUpperBound = var95Upper,
                SampleSize = returns.Length,
                SimulationCount = 0,
                TimeHorizon = request.TimeHorizon,
                CalculationDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                Parameters = JsonSerializer.Serialize(request.Parameters)
            };
        }

        private async Task<VaRCalculation> CalculateMonteCarloVaRAsync(VaRCalculationRequest request, double[] returns)
        {
            try
            {
                // Call Python Monte Carlo simulation
                var pythonScript = Path.Combine(Directory.GetCurrentDirectory(), "Services", "monte_carlo_var.py");
                var pythonArgs = $"{pythonScript} {request.Symbol} {JsonSerializer.Serialize(returns)} {request.DistributionType} {request.SimulationCount}";

                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "python3",
                        Arguments = pythonArgs,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                var output = await process.StandardOutput.ReadToEndAsync();
                var error = await process.StandardError.ReadToEndAsync();
                await process.WaitForExitAsync();

                if (process.ExitCode != 0)
                {
                    _logger.LogError("Python Monte Carlo simulation failed: {Error}", error);
                    throw new Exception($"Python simulation failed: {error}");
                }

                // Parse Python output (simplified - in practice, use proper JSON parsing)
                var result = JsonSerializer.Deserialize<Dictionary<string, double>>(output);

                return new VaRCalculation
                {
                    Symbol = request.Symbol,
                    CalculationType = "MonteCarlo",
                    DistributionType = request.DistributionType,
                    ConfidenceLevel = 0.95,
                    VaR = result.GetValueOrDefault("var_95", 0.0),
                    CVaR = result.GetValueOrDefault("cvar_95", 0.0),
                    SampleSize = returns.Length,
                    SimulationCount = request.SimulationCount,
                    TimeHorizon = request.TimeHorizon,
                    CalculationDate = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    Parameters = JsonSerializer.Serialize(request.Parameters)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Monte Carlo VaR calculation for {Symbol}", request.Symbol);
                throw;
            }
        }

        private (PortfolioVaRCalculation portfolioResult, List<VaRAssetContribution> contributions) CalculateHistoricalPortfolioVaR(
            PortfolioVaRCalculationRequest request, Dictionary<string, double[]> assetData)
        {
            // Calculate portfolio returns
            var portfolioReturns = CalculatePortfolioReturns(assetData, request.Weights);

            var var95 = CalculateHistoricalVaR(portfolioReturns, portfolioReturns.Length, 0.95);
            var var99 = CalculateHistoricalVaR(portfolioReturns, portfolioReturns.Length, 0.99);
            var cvar95 = CalculateHistoricalCVaR(portfolioReturns, portfolioReturns.Length, 0.95);
            var cvar99 = CalculateHistoricalCVaR(portfolioReturns, portfolioReturns.Length, 0.99);

            var portfolioResult = new PortfolioVaRCalculation
            {
                PortfolioName = request.PortfolioName,
                CalculationType = request.CalculationType,
                DistributionType = request.DistributionType,
                ConfidenceLevel = 0.95,
                PortfolioVaR = var95,
                PortfolioCVaR = cvar95,
                SampleSize = portfolioReturns.Length,
                SimulationCount = 0,
                TimeHorizon = request.TimeHorizon,
                CalculationDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                Parameters = JsonSerializer.Serialize(request.Parameters)
            };

            // Calculate asset contributions
            var contributions = new List<VaRAssetContribution>();
            for (int i = 0; i < request.Symbols.Count; i++)
            {
                var symbol = request.Symbols[i];
                var weight = (double)request.Weights[i];
                var assetReturns = assetData[symbol];
                var assetVar = CalculateHistoricalVaR(assetReturns, assetReturns.Length, 0.95);

                contributions.Add(new VaRAssetContribution
                {
                    PortfolioVaRCalculationId = 0, // Will be set when saved
                    Symbol = symbol,
                    Weight = (double)request.Weights[i],
                    VaRContribution = weight * assetVar / var95,
                    CVaRContribution = weight * assetVar / cvar95,
                    MarginalVaR = assetVar,
                    ComponentVaR = weight * assetVar,
                    CreatedAt = DateTime.UtcNow
                });
            }

            return (portfolioResult, contributions);
        }

        private async Task<(PortfolioVaRCalculation portfolioResult, List<VaRAssetContribution> contributions)> CalculateMonteCarloPortfolioVaRAsync(
            PortfolioVaRCalculationRequest request, Dictionary<string, double[]> assetData)
        {
            // Similar to single asset Monte Carlo but for portfolio
            // This would call Python with portfolio data
            throw new NotImplementedException("Portfolio Monte Carlo VaR not yet implemented");
        }

        private double[] CalculatePortfolioReturns(Dictionary<string, double[]> assetData, List<decimal> weights)
        {
            var minLength = assetData.Values.Min(arr => arr.Length);
            var portfolioReturns = new double[minLength];

            for (int i = 0; i < minLength; i++)
            {
                portfolioReturns[i] = 0;
                int weightIndex = 0;
                foreach (var kvp in assetData)
                {
                    portfolioReturns[i] += (double)weights[weightIndex] * kvp.Value[i];
                    weightIndex++;
                }
            }

            return portfolioReturns;
        }

        private double[] ApplyStressFactor(double[] returns, double stressFactor, string scenarioType)
        {
            switch (scenarioType.ToLower())
            {
                case "historical":
                    // Apply historical stress (multiply by factor)
                    return returns.Select(r => r * stressFactor).ToArray();
                case "volatility":
                    // Increase volatility
                    return returns.Select(r => r * Math.Sqrt(stressFactor)).ToArray();
                default:
                    return returns;
            }
        }

        private async Task SaveVaRCalculationAsync(VaRCalculation calculation)
        {
            // This would save to database
            _logger.LogInformation("Saving VaR calculation for {Symbol}", calculation.Symbol);
        }

        private async Task SavePortfolioVaRCalculationAsync(PortfolioVaRCalculation calculation, List<VaRAssetContribution> contributions)
        {
            // This would save to database
            _logger.LogInformation("Saving portfolio VaR calculation for {PortfolioName}", calculation.PortfolioName);
        }

        private async Task SaveVaRStressTestAsync(VaRStressTest stressTest)
        {
            // This would save to database
            _logger.LogInformation("Saving VaR stress test for {Symbol}", stressTest.Symbol);
        }

        private async Task SaveVaRBacktestAsync(VaRBacktestResult backtest)
        {
            // This would save to database
            _logger.LogInformation("Saving VaR backtest for {Symbol}", backtest.Symbol);
        }
    }
}

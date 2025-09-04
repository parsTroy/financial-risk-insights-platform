using FinancialRisk.Api.Models;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text.Json;

namespace FinancialRisk.Api.Services
{
    public class PortfolioOptimizationService : IPortfolioOptimizationService
    {
        private readonly ILogger<PortfolioOptimizationService> _logger;
        private readonly IFinancialDataService _financialDataService;
        private readonly IDataPersistenceService _dataPersistenceService;

        public PortfolioOptimizationService(
            ILogger<PortfolioOptimizationService> logger,
            IFinancialDataService financialDataService,
            IDataPersistenceService dataPersistenceService)
        {
            _logger = logger;
            _financialDataService = financialDataService;
            _dataPersistenceService = dataPersistenceService;
        }

        public async Task<PortfolioOptimizationResponse> OptimizePortfolioAsync(PortfolioOptimizationRequest request)
        {
            try
            {
                _logger.LogInformation("Starting portfolio optimization for {PortfolioName} using {Method}", 
                    request.PortfolioName, request.OptimizationMethod);

                // Validate request
                if (request.Symbols.Count < 2)
                {
                    return new PortfolioOptimizationResponse
                    {
                        Success = false,
                        Error = "At least 2 assets required for portfolio optimization"
                    };
                }

                // Fetch historical data for all assets
                var assetData = await FetchAssetDataAsync(request.Symbols, request.LookbackPeriod);
                if (assetData.Count < 2)
                {
                    return new PortfolioOptimizationResponse
                    {
                        Success = false,
                        Error = "Insufficient asset data for optimization"
                    };
                }

                // Prepare optimization data for Python
                var optimizationData = PrepareOptimizationData(request, assetData);

                // Run Python optimization
                var optimizationResult = await RunPythonOptimizationAsync(optimizationData);
                if (!optimizationResult.Success)
                {
                    return new PortfolioOptimizationResponse
                    {
                        Success = false,
                        Error = optimizationResult.Error ?? "Portfolio optimization failed"
                    };
                }

                // Convert to C# result format
                var result = ConvertToOptimizationResult(request, optimizationResult, assetData);

                // Calculate efficient frontier if requested
                EfficientFrontier? efficientFrontier = null;
                if (request.CalculateEfficientFrontier)
                {
                    efficientFrontier = await CalculateEfficientFrontierAsync(request);
                }

                // Save to database
                await SaveOptimizationResultAsync(result);

                _logger.LogInformation("Portfolio optimization completed for {PortfolioName}", request.PortfolioName);

                return new PortfolioOptimizationResponse
                {
                    Success = true,
                    OptimizationResult = result,
                    EfficientFrontier = efficientFrontier
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in portfolio optimization for {PortfolioName}", request.PortfolioName);
                return new PortfolioOptimizationResponse
                {
                    Success = false,
                    Error = ex.Message
                };
            }
        }

        public async Task<EfficientFrontier> CalculateEfficientFrontierAsync(PortfolioOptimizationRequest request)
        {
            try
            {
                _logger.LogInformation("Calculating efficient frontier for {PortfolioName}", request.PortfolioName);

                // Fetch historical data for all assets
                var assetData = await FetchAssetDataAsync(request.Symbols, request.LookbackPeriod);
                if (assetData.Count < 2)
                {
                    return new EfficientFrontier
                    {
                        Success = false,
                        Error = "Insufficient asset data for efficient frontier calculation"
                    };
                }

                // Prepare optimization data for Python
                var optimizationData = PrepareOptimizationData(request, assetData);

                // Run Python efficient frontier calculation
                var frontierResult = await RunPythonEfficientFrontierAsync(optimizationData, request.EfficientFrontierPoints);
                if (!frontierResult.Success)
                {
                    return new EfficientFrontier
                    {
                        Success = false,
                        Error = frontierResult.Error ?? "Efficient frontier calculation failed"
                    };
                }

                // Convert to C# result format
                var frontier = ConvertToEfficientFrontier(frontierResult, request.PortfolioName);

                _logger.LogInformation("Efficient frontier calculated for {PortfolioName} with {Points} points", 
                    request.PortfolioName, frontier.Points.Count);

                return frontier;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating efficient frontier for {PortfolioName}", request.PortfolioName);
                return new EfficientFrontier
                {
                    Success = false,
                    Error = ex.Message
                };
            }
        }

        public async Task<RiskBudgetingResult> OptimizeRiskBudgetingAsync(RiskBudgetingRequest request)
        {
            try
            {
                _logger.LogInformation("Starting risk budgeting optimization for {PortfolioName}", request.PortfolioName);

                // This would implement risk budgeting optimization
                // For now, return a placeholder implementation
                return new RiskBudgetingResult
                {
                    Success = false,
                    Error = "Risk budgeting optimization not yet implemented",
                    PortfolioName = request.PortfolioName,
                    OptimalWeights = new List<double>(),
                    RiskBudgets = request.RiskBudgets,
                    ActualRiskContributions = new List<double>(),
                    PortfolioVolatility = 0.0,
                    AssetWeights = new List<AssetWeight>(),
                    OptimizationMetadata = new Dictionary<string, object>(),
                    CalculationDate = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in risk budgeting optimization for {PortfolioName}", request.PortfolioName);
                return new RiskBudgetingResult
                {
                    Success = false,
                    Error = ex.Message,
                    PortfolioName = request.PortfolioName,
                    OptimalWeights = new List<double>(),
                    RiskBudgets = new List<double>(),
                    ActualRiskContributions = new List<double>(),
                    PortfolioVolatility = 0.0,
                    AssetWeights = new List<AssetWeight>(),
                    OptimizationMetadata = new Dictionary<string, object>(),
                    CalculationDate = DateTime.UtcNow
                };
            }
        }

        public async Task<BlackLittermanResult> OptimizeBlackLittermanAsync(BlackLittermanRequest request)
        {
            try
            {
                _logger.LogInformation("Starting Black-Litterman optimization for {PortfolioName}", request.PortfolioName);

                // This would implement Black-Litterman optimization
                // For now, return a placeholder implementation
                return new BlackLittermanResult
                {
                    Success = false,
                    Error = "Black-Litterman optimization not yet implemented",
                    PortfolioName = request.PortfolioName,
                    OptimalWeights = new List<double>(),
                    ImpliedReturns = new List<double>(),
                    AdjustedReturns = new List<double>(),
                    ExpectedReturn = 0.0,
                    ExpectedVolatility = 0.0,
                    SharpeRatio = 0.0,
                    AssetWeights = new List<AssetWeight>(),
                    OptimizationMetadata = new Dictionary<string, object>(),
                    CalculationDate = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Black-Litterman optimization for {PortfolioName}", request.PortfolioName);
                return new BlackLittermanResult
                {
                    Success = false,
                    Error = ex.Message,
                    PortfolioName = request.PortfolioName,
                    OptimalWeights = new List<double>(),
                    ImpliedReturns = new List<double>(),
                    AdjustedReturns = new List<double>(),
                    ExpectedReturn = 0.0,
                    ExpectedVolatility = 0.0,
                    SharpeRatio = 0.0,
                    AssetWeights = new List<AssetWeight>(),
                    OptimizationMetadata = new Dictionary<string, object>(),
                    CalculationDate = DateTime.UtcNow
                };
            }
        }

        public async Task<TransactionCostOptimizationResult> OptimizeTransactionCostsAsync(TransactionCostOptimizationRequest request)
        {
            try
            {
                _logger.LogInformation("Starting transaction cost optimization for {PortfolioName}", request.PortfolioName);

                // This would implement transaction cost optimization
                // For now, return a placeholder implementation
                return new TransactionCostOptimizationResult
                {
                    Success = false,
                    Error = "Transaction cost optimization not yet implemented",
                    PortfolioName = request.PortfolioName,
                    OptimalWeights = new List<double>(),
                    RebalancingWeights = new List<double>(),
                    TotalTransactionCosts = 0.0,
                    Turnover = 0.0,
                    AssetWeights = new List<AssetWeight>(),
                    OptimizationMetadata = new Dictionary<string, object>(),
                    CalculationDate = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in transaction cost optimization for {PortfolioName}", request.PortfolioName);
                return new TransactionCostOptimizationResult
                {
                    Success = false,
                    Error = ex.Message,
                    PortfolioName = request.PortfolioName,
                    OptimalWeights = new List<double>(),
                    RebalancingWeights = new List<double>(),
                    TotalTransactionCosts = 0.0,
                    Turnover = 0.0,
                    AssetWeights = new List<AssetWeight>(),
                    OptimizationMetadata = new Dictionary<string, object>(),
                    CalculationDate = DateTime.UtcNow
                };
            }
        }

        public async Task<List<PortfolioOptimizationResult>> GetOptimizationHistoryAsync(string portfolioName, int limit = 100)
        {
            try
            {
                _logger.LogInformation("Retrieving optimization history for {PortfolioName}", portfolioName);
                
                // This would query the database for optimization history
                // For now, return empty list
                return new List<PortfolioOptimizationResult>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving optimization history for {PortfolioName}", portfolioName);
                return new List<PortfolioOptimizationResult>();
            }
        }

        public async Task<PortfolioOptimizationResult> GetOptimizationByIdAsync(int id)
        {
            try
            {
                _logger.LogInformation("Retrieving optimization result with ID {Id}", id);
                
                // This would query the database for the specific optimization result
                // For now, return null
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving optimization result with ID {Id}", id);
                return null;
            }
        }

        public async Task<bool> SaveOptimizationResultAsync(PortfolioOptimizationResult result)
        {
            try
            {
                _logger.LogInformation("Saving optimization result for {PortfolioName}", result.PortfolioName);
                
                // This would save to the database
                // For now, just log the action
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving optimization result for {PortfolioName}", result.PortfolioName);
                return false;
            }
        }

        public async Task<List<string>> GetAvailableOptimizationMethodsAsync()
        {
            return new List<string>
            {
                "MeanVariance",
                "MinimumVariance",
                "MaximumSharpe",
                "EqualWeight",
                "RiskParity",
                "BlackLitterman",
                "MeanCVaR"
            };
        }

        public async Task<Dictionary<string, object>> GetOptimizationConstraintsAsync()
        {
            return new Dictionary<string, object>
            {
                ["max_weight"] = 1.0,
                ["min_weight"] = 0.0,
                ["max_leverage"] = 1.0,
                ["max_turnover"] = 1.0,
                ["max_concentration"] = 0.4,
                ["sector_limits"] = new Dictionary<string, double>(),
                ["transaction_costs"] = 0.0
            };
        }

        private async Task<Dictionary<string, AssetOptimizationData>> FetchAssetDataAsync(List<string> symbols, int lookbackPeriod)
        {
            var assetData = new Dictionary<string, AssetOptimizationData>();

            foreach (var symbol in symbols)
            {
                try
                {
                    var historyResult = await _financialDataService.GetStockHistoryAsync(symbol, lookbackPeriod);
                    if (historyResult.Success && historyResult.Data != null && historyResult.Data.Any())
                    {
                        var returns = CalculateReturns(historyResult.Data);
                        if (returns.Length >= 2)
                        {
                            var expectedReturn = returns.Average();
                            var volatility = Math.Sqrt(returns.Select(r => Math.Pow(r - expectedReturn, 2)).Average());

                            assetData[symbol] = new AssetOptimizationData
                            {
                                Symbol = symbol,
                                ExpectedReturn = expectedReturn,
                                Volatility = volatility,
                                HistoricalReturns = returns.ToList()
                            };
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to fetch data for symbol {Symbol}", symbol);
                }
            }

            return assetData;
        }

        private Dictionary<string, object> PrepareOptimizationData(PortfolioOptimizationRequest request, Dictionary<string, AssetOptimizationData> assetData)
        {
            var optimizationData = new Dictionary<string, object>
            {
                ["method"] = request.OptimizationMethod.ToLower(),
                ["risk_aversion"] = request.RiskAversion,
                ["target_return"] = request.TargetReturn,
                ["max_weight"] = request.MaxWeight,
                ["min_weight"] = request.MinWeight,
                ["max_leverage"] = request.MaxLeverage,
                ["transaction_costs"] = request.TransactionCosts,
                ["confidence_level"] = request.ConfidenceLevel,
                ["custom_constraints"] = request.CustomConstraints ?? new Dictionary<string, object>(),
                ["assets"] = assetData.Values.Select(asset => new Dictionary<string, object>
                {
                    ["symbol"] = asset.Symbol,
                    ["expected_return"] = asset.ExpectedReturn,
                    ["volatility"] = asset.Volatility,
                    ["historical_returns"] = asset.HistoricalReturns,
                    ["sector"] = asset.Sector,
                    ["market_cap"] = asset.MarketCap,
                    ["beta"] = asset.Beta
                }).ToList()
            };

            return optimizationData;
        }

        private async Task<PortfolioOptimizationResult> RunPythonOptimizationAsync(Dictionary<string, object> optimizationData)
        {
            try
            {
                var pythonScript = Path.Combine(Directory.GetCurrentDirectory(), "Services", "portfolio_optimizer.py");
                var optimizationJson = JsonSerializer.Serialize(optimizationData);
                var pythonArgs = $"{pythonScript} optimize \"{optimizationJson}\"";

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
                    _logger.LogError("Python portfolio optimization failed: {Error}", error);
                    return new PortfolioOptimizationResult
                    {
                        Success = false,
                        Error = $"Python optimization failed: {error}"
                    };
                }

                // Parse Python output
                var result = JsonSerializer.Deserialize<PortfolioOptimizationResult>(output);
                return result ?? new PortfolioOptimizationResult
                {
                    Success = false,
                    Error = "Failed to parse optimization result"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error running Python portfolio optimization");
                return new PortfolioOptimizationResult
                {
                    Success = false,
                    Error = ex.Message
                };
            }
        }

        private async Task<EfficientFrontier> RunPythonEfficientFrontierAsync(Dictionary<string, object> optimizationData, int numPoints)
        {
            try
            {
                var pythonScript = Path.Combine(Directory.GetCurrentDirectory(), "Services", "portfolio_optimizer.py");
                var optimizationJson = JsonSerializer.Serialize(optimizationData);
                var pythonArgs = $"{pythonScript} frontier \"{optimizationJson}\" {numPoints}";

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
                    _logger.LogError("Python efficient frontier calculation failed: {Error}", error);
                    return new EfficientFrontier
                    {
                        Success = false,
                        Error = $"Python frontier calculation failed: {error}"
                    };
                }

                // Parse Python output
                var result = JsonSerializer.Deserialize<EfficientFrontier>(output);
                return result ?? new EfficientFrontier
                {
                    Success = false,
                    Error = "Failed to parse efficient frontier result"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error running Python efficient frontier calculation");
                return new EfficientFrontier
                {
                    Success = false,
                    Error = ex.Message
                };
            }
        }

        private PortfolioOptimizationResult ConvertToOptimizationResult(
            PortfolioOptimizationRequest request, 
            PortfolioOptimizationResult pythonResult, 
            Dictionary<string, AssetOptimizationData> assetData)
        {
            var result = new PortfolioOptimizationResult
            {
                Success = pythonResult.Success,
                Error = pythonResult.Error,
                PortfolioName = request.PortfolioName,
                OptimizationMethod = request.OptimizationMethod,
                OptimalWeights = pythonResult.OptimalWeights,
                ExpectedReturn = pythonResult.ExpectedReturn,
                ExpectedVolatility = pythonResult.ExpectedVolatility,
                SharpeRatio = pythonResult.SharpeRatio,
                VaR = pythonResult.VaR,
                CVaR = pythonResult.CVaR,
                DiversificationRatio = pythonResult.DiversificationRatio,
                ConcentrationRatio = pythonResult.ConcentrationRatio,
                OptimizationMetadata = pythonResult.OptimizationMetadata,
                CalculationDate = DateTime.UtcNow
            };

            // Create asset weights
            result.AssetWeights = new List<AssetWeight>();
            for (int i = 0; i < request.Symbols.Count && i < pythonResult.OptimalWeights.Count; i++)
            {
                var symbol = request.Symbols[i];
                var weight = pythonResult.OptimalWeights[i];
                var asset = assetData.ContainsKey(symbol) ? assetData[symbol] : new AssetOptimizationData();

                result.AssetWeights.Add(new AssetWeight
                {
                    Symbol = symbol,
                    Weight = weight,
                    ExpectedReturn = asset.ExpectedReturn,
                    Volatility = asset.Volatility,
                    RiskContribution = weight * asset.Volatility / result.ExpectedVolatility,
                    ReturnContribution = weight * asset.ExpectedReturn
                });
            }

            return result;
        }

        private EfficientFrontier ConvertToEfficientFrontier(EfficientFrontier pythonFrontier, string portfolioName)
        {
            var frontier = new EfficientFrontier
            {
                Success = pythonFrontier.Success,
                Error = pythonFrontier.Error,
                Points = pythonFrontier.Points,
                MinVolatilityPoint = pythonFrontier.MinVolatilityPoint,
                MaxSharpePoint = pythonFrontier.MaxSharpePoint,
                MaxReturnPoint = pythonFrontier.MaxReturnPoint,
                FrontierMetadata = pythonFrontier.FrontierMetadata,
                CalculationDate = DateTime.UtcNow
            };

            return frontier;
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
    }
}

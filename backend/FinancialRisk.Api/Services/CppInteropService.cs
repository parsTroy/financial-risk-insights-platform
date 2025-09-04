using Microsoft.Extensions.Logging;
using System.Runtime.InteropServices;
using FinancialRisk.Api.Models;

namespace FinancialRisk.Api.Services
{
    /// <summary>
    /// C++/CLI interop service for high-performance quantitative models
    /// Uses P/Invoke to call native C++ functions for performance-critical operations
    /// </summary>
    public class CppInteropService : IPythonInteropService, IDisposable
    {
        private readonly ILogger<CppInteropService> _logger;
        private readonly CppInteropConfiguration _config;
        private bool _isInitialized = false;
        private bool _disposed = false;

        // P/Invoke declarations for C++ functions
        [DllImport("QuantEngine", CallingConvention = CallingConvention.Cdecl)]
        private static extern double CalculateVaRHistorical(double[] returns, int length, double confidenceLevel);

        [DllImport("QuantEngine", CallingConvention = CallingConvention.Cdecl)]
        private static extern double CalculateVaRParametric(double mean, double std, double confidenceLevel);

        [DllImport("QuantEngine", CallingConvention = CallingConvention.Cdecl)]
        private static extern double CalculateCVaR(double[] returns, int length, double confidenceLevel);

        [DllImport("QuantEngine", CallingConvention = CallingConvention.Cdecl)]
        private static extern void CalculateVaRMonteCarlo(double[] returns, int length, double confidenceLevel, 
                                                         int numSimulations, double[] result);

        [DllImport("QuantEngine", CallingConvention = CallingConvention.Cdecl)]
        private static extern void OptimizeMarkowitz(double[] expectedReturns, double[] covarianceMatrix, 
                                                    int numAssets, double riskAversion, double[] optimalWeights);

        [DllImport("QuantEngine", CallingConvention = CallingConvention.Cdecl)]
        private static extern void CalculateEfficientFrontier(double[] expectedReturns, double[] covarianceMatrix,
                                                             int numAssets, int numPoints, double[] frontierPoints);

        [DllImport("QuantEngine", CallingConvention = CallingConvention.Cdecl)]
        private static extern void OptimizeRiskParity(double[] covarianceMatrix, int numAssets, double[] optimalWeights);

        [DllImport("QuantEngine", CallingConvention = CallingConvention.Cdecl)]
        private static extern double BlackScholes(double spot, double strike, double timeToMaturity, 
                                                 double riskFreeRate, double volatility, int optionType);

        [DllImport("QuantEngine", CallingConvention = CallingConvention.Cdecl)]
        private static extern void MonteCarloPricing(double spot, double strike, double timeToMaturity,
                                                   double riskFreeRate, double volatility, int optionType,
                                                   int numSimulations, double[] result);

        [DllImport("QuantEngine", CallingConvention = CallingConvention.Cdecl)]
        private static extern double BinomialTree(double spot, double strike, double timeToMaturity,
                                                double riskFreeRate, double volatility, int optionType, int nSteps);

        [DllImport("QuantEngine", CallingConvention = CallingConvention.Cdecl)]
        private static extern double CalculateSharpeRatio(double[] returns, int length, double riskFreeRate);

        [DllImport("QuantEngine", CallingConvention = CallingConvention.Cdecl)]
        private static extern void CalculateCorrelationMatrix(double[] data, int rows, int cols, double[] correlationMatrix);

        [DllImport("QuantEngine", CallingConvention = CallingConvention.Cdecl)]
        private static extern void CalculateCovarianceMatrix(double[] data, int rows, int cols, double[] covarianceMatrix);

        [DllImport("QuantEngine", CallingConvention = CallingConvention.Cdecl)]
        private static extern double CalculatePortfolioVolatility(double[] weights, double[] covarianceMatrix, int numAssets);

        [DllImport("QuantEngine", CallingConvention = CallingConvention.Cdecl)]
        private static extern double CalculatePortfolioReturn(double[] weights, double[] expectedReturns, int numAssets);

        [DllImport("QuantEngine", CallingConvention = CallingConvention.Cdecl)]
        private static extern int GetMemoryUsage();

        [DllImport("QuantEngine", CallingConvention = CallingConvention.Cdecl)]
        private static extern void ClearCache();

        [DllImport("QuantEngine", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr GetVersion();

        [DllImport("QuantEngine", CallingConvention = CallingConvention.Cdecl)]
        private static extern int GetLastError();

        [DllImport("QuantEngine", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr GetLastErrorMessage();

        public CppInteropService(ILogger<CppInteropService> logger, CppInteropConfiguration config)
        {
            _logger = logger;
            _config = config;
        }

        public async Task<bool> InitializeAsync()
        {
            try
            {
                if (_isInitialized)
                    return true;

                _logger.LogInformation("Initializing C++ interop service");

                // Test C++ library availability
                try
                {
                    var version = Marshal.PtrToStringAnsi(GetVersion());
                    _logger.LogInformation("C++ QuantEngine library version: {Version}", version);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to load C++ QuantEngine library");
                    return false;
                }

                _isInitialized = true;
                _logger.LogInformation("C++ interop service initialized successfully");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize C++ interop service");
                return false;
            }
        }

        public async Task<T> CallPythonFunctionAsync<T>(string moduleName, string functionName, params object[] parameters)
        {
            // C++ service doesn't support Python function calls directly
            throw new NotSupportedException("C++ interop service does not support direct Python function calls");
        }

        public async Task<QuantModelResult> ExecuteQuantModelAsync(QuantModelRequest request)
        {
            try
            {
                _logger.LogInformation("Executing quant model via C++: {ModelName}", request.ModelName);

                if (!_isInitialized)
                {
                    await InitializeAsync();
                }

                var startTime = DateTime.UtcNow;
                var result = new QuantModelResult
                {
                    ModelName = request.ModelName,
                    RequestId = request.RequestId,
                    Success = false
                };

                try
                {
                    // Route to appropriate C++ function based on model name
                    switch (request.ModelName.ToLower())
                    {
                        case "var_historical":
                            result = await ExecuteVaRHistoricalAsync(request);
                            break;
                        case "var_parametric":
                            result = await ExecuteVaRParametricAsync(request);
                            break;
                        case "var_monte_carlo":
                            result = await ExecuteVaRMonteCarloAsync(request);
                            break;
                        case "cvar_calculation":
                            result = await ExecuteCVaRAsync(request);
                            break;
                        case "markowitz_optimization":
                            result = await ExecuteMarkowitzOptimizationAsync(request);
                            break;
                        case "efficient_frontier":
                            result = await ExecuteEfficientFrontierAsync(request);
                            break;
                        case "risk_parity":
                            result = await ExecuteRiskParityAsync(request);
                            break;
                        case "black_scholes":
                            result = await ExecuteBlackScholesAsync(request);
                            break;
                        case "monte_carlo_pricing":
                            result = await ExecuteMonteCarloPricingAsync(request);
                            break;
                        case "binomial_tree":
                            result = await ExecuteBinomialTreeAsync(request);
                            break;
                        default:
                            throw new NotSupportedException($"Model {request.ModelName} not supported by C++ interop service");
                    }

                    result.Success = true;
                }
                catch (Exception ex)
                {
                    result.Success = false;
                    result.Error = ex.Message;
                    _logger.LogError(ex, "Error executing C++ model: {ModelName}", request.ModelName);
                }

                result.ExecutionTime = DateTime.UtcNow - startTime;
                result.CompletionTime = DateTime.UtcNow;
                result.MemoryUsageMB = GetMemoryUsage();
                result.PythonVersion = "C++ Native";

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing quant model via C++: {ModelName}", request.ModelName);
                return new QuantModelResult
                {
                    Success = false,
                    Error = ex.Message,
                    ModelName = request.ModelName,
                    ExecutionTime = TimeSpan.Zero
                };
            }
        }

        public async Task<List<string>> GetAvailableModelsAsync()
        {
            return new List<string>
            {
                "var_historical",
                "var_parametric", 
                "var_monte_carlo",
                "cvar_calculation",
                "markowitz_optimization",
                "efficient_frontier",
                "risk_parity",
                "black_scholes",
                "monte_carlo_pricing",
                "binomial_tree"
            };
        }

        public async Task<ModelMetadata> GetModelMetadataAsync(string modelName)
        {
            var metadata = new ModelMetadata
            {
                ModelName = modelName,
                Success = true,
                Author = "C++ QuantEngine",
                Version = "1.0.0",
                Category = "Performance",
                LastModified = DateTime.UtcNow
            };

            switch (modelName.ToLower())
            {
                case "var_historical":
                    metadata.Description = "Historical Value at Risk calculation";
                    metadata.RequiredParameters.Add("returns");
                    metadata.RequiredParameters.Add("confidence_level");
                    break;
                case "var_parametric":
                    metadata.Description = "Parametric Value at Risk calculation";
                    metadata.RequiredParameters.Add("mean");
                    metadata.RequiredParameters.Add("std");
                    metadata.RequiredParameters.Add("confidence_level");
                    break;
                case "markowitz_optimization":
                    metadata.Description = "Markowitz portfolio optimization";
                    metadata.RequiredParameters.Add("expected_returns");
                    metadata.RequiredParameters.Add("covariance_matrix");
                    metadata.RequiredParameters.Add("risk_aversion");
                    break;
                case "black_scholes":
                    metadata.Description = "Black-Scholes option pricing";
                    metadata.RequiredParameters.Add("spot");
                    metadata.RequiredParameters.Add("strike");
                    metadata.RequiredParameters.Add("time_to_maturity");
                    metadata.RequiredParameters.Add("risk_free_rate");
                    metadata.RequiredParameters.Add("volatility");
                    metadata.RequiredParameters.Add("option_type");
                    break;
            }

            return metadata;
        }

        public async Task<bool> ValidateModelAsync(string modelName, Dictionary<string, object> parameters)
        {
            try
            {
                // Basic validation - check required parameters
                var metadata = await GetModelMetadataAsync(modelName);
                if (!metadata.Success)
                {
                    return false;
                }

                foreach (var requiredParam in metadata.RequiredParameters)
                {
                    if (!parameters.ContainsKey(requiredParam))
                    {
                        return false;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating C++ model: {ModelName}", modelName);
                return false;
            }
        }

        public async Task<PerformanceMetrics> GetPerformanceMetricsAsync()
        {
            return new PerformanceMetrics
            {
                TotalRequests = 0, // Would be tracked in a real implementation
                SuccessfulRequests = 0,
                FailedRequests = 0,
                SuccessRate = 0.0,
                AverageExecutionTime = TimeSpan.Zero,
                MinExecutionTime = TimeSpan.Zero,
                MaxExecutionTime = TimeSpan.Zero,
                TotalMemoryUsageMB = GetMemoryUsage(),
                AverageMemoryUsageMB = GetMemoryUsage(),
                ActiveConnections = 1,
                QueuedRequests = 0,
                LastReset = DateTime.UtcNow
            };
        }

        private async Task<QuantModelResult> ExecuteVaRHistoricalAsync(QuantModelRequest request)
        {
            var returns = GetParameterAsDoubleArray(request.Parameters, "returns");
            var confidenceLevel = GetParameterAsDouble(request.Parameters, "confidence_level", 0.95);

            var var = CalculateVaRHistorical(returns, returns.Length, confidenceLevel);

            return new QuantModelResult
            {
                Results = new Dictionary<string, object>
                {
                    ["var"] = var,
                    ["confidence_level"] = confidenceLevel,
                    ["method"] = "historical"
                }
            };
        }

        private async Task<QuantModelResult> ExecuteVaRParametricAsync(QuantModelRequest request)
        {
            var mean = GetParameterAsDouble(request.Parameters, "mean");
            var std = GetParameterAsDouble(request.Parameters, "std");
            var confidenceLevel = GetParameterAsDouble(request.Parameters, "confidence_level", 0.95);

            var var = CalculateVaRParametric(mean, std, confidenceLevel);

            return new QuantModelResult
            {
                Results = new Dictionary<string, object>
                {
                    ["var"] = var,
                    ["confidence_level"] = confidenceLevel,
                    ["method"] = "parametric",
                    ["mean"] = mean,
                    ["std"] = std
                }
            };
        }

        private async Task<QuantModelResult> ExecuteVaRMonteCarloAsync(QuantModelRequest request)
        {
            var returns = GetParameterAsDoubleArray(request.Parameters, "returns");
            var confidenceLevel = GetParameterAsDouble(request.Parameters, "confidence_level", 0.95);
            var numSimulations = GetParameterAsInt(request.Parameters, "num_simulations", 10000);

            var result = new double[3];
            CalculateVaRMonteCarlo(returns, returns.Length, confidenceLevel, numSimulations, result);

            return new QuantModelResult
            {
                Results = new Dictionary<string, object>
                {
                    ["var"] = result[0],
                    ["mean_return"] = result[1],
                    ["std_return"] = result[2],
                    ["confidence_level"] = confidenceLevel,
                    ["num_simulations"] = numSimulations,
                    ["method"] = "monte_carlo"
                }
            };
        }

        private async Task<QuantModelResult> ExecuteCVaRAsync(QuantModelRequest request)
        {
            var returns = GetParameterAsDoubleArray(request.Parameters, "returns");
            var confidenceLevel = GetParameterAsDouble(request.Parameters, "confidence_level", 0.95);

            var cvar = CalculateCVaR(returns, returns.Length, confidenceLevel);

            return new QuantModelResult
            {
                Results = new Dictionary<string, object>
                {
                    ["cvar"] = cvar,
                    ["confidence_level"] = confidenceLevel,
                    ["method"] = "historical"
                }
            };
        }

        private async Task<QuantModelResult> ExecuteMarkowitzOptimizationAsync(QuantModelRequest request)
        {
            var expectedReturns = GetParameterAsDoubleArray(request.Parameters, "expected_returns");
            var covarianceMatrix = GetParameterAsDoubleArray(request.Parameters, "covariance_matrix");
            var riskAversion = GetParameterAsDouble(request.Parameters, "risk_aversion", 1.0);

            var numAssets = expectedReturns.Length;
            var optimalWeights = new double[numAssets];
            OptimizeMarkowitz(expectedReturns, covarianceMatrix, numAssets, riskAversion, optimalWeights);

            return new QuantModelResult
            {
                Results = new Dictionary<string, object>
                {
                    ["optimal_weights"] = optimalWeights,
                    ["risk_aversion"] = riskAversion,
                    ["method"] = "markowitz"
                }
            };
        }

        private async Task<QuantModelResult> ExecuteEfficientFrontierAsync(QuantModelRequest request)
        {
            var expectedReturns = GetParameterAsDoubleArray(request.Parameters, "expected_returns");
            var covarianceMatrix = GetParameterAsDoubleArray(request.Parameters, "covariance_matrix");
            var numPoints = GetParameterAsInt(request.Parameters, "num_points", 50);

            var numAssets = expectedReturns.Length;
            var frontierPoints = new double[numPoints * 2];
            CalculateEfficientFrontier(expectedReturns, covarianceMatrix, numAssets, numPoints, frontierPoints);

            var points = new List<object>();
            for (int i = 0; i < numPoints; i++)
            {
                points.Add(new
                {
                    expected_return = frontierPoints[i * 2],
                    expected_volatility = frontierPoints[i * 2 + 1]
                });
            }

            return new QuantModelResult
            {
                Results = new Dictionary<string, object>
                {
                    ["frontier_points"] = points,
                    ["num_points"] = numPoints,
                    ["method"] = "efficient_frontier"
                }
            };
        }

        private async Task<QuantModelResult> ExecuteRiskParityAsync(QuantModelRequest request)
        {
            var covarianceMatrix = GetParameterAsDoubleArray(request.Parameters, "covariance_matrix");
            var numAssets = (int)Math.Sqrt(covarianceMatrix.Length);

            var optimalWeights = new double[numAssets];
            OptimizeRiskParity(covarianceMatrix, numAssets, optimalWeights);

            return new QuantModelResult
            {
                Results = new Dictionary<string, object>
                {
                    ["optimal_weights"] = optimalWeights,
                    ["method"] = "risk_parity"
                }
            };
        }

        private async Task<QuantModelResult> ExecuteBlackScholesAsync(QuantModelRequest request)
        {
            var spot = GetParameterAsDouble(request.Parameters, "spot");
            var strike = GetParameterAsDouble(request.Parameters, "strike");
            var timeToMaturity = GetParameterAsDouble(request.Parameters, "time_to_maturity");
            var riskFreeRate = GetParameterAsDouble(request.Parameters, "risk_free_rate");
            var volatility = GetParameterAsDouble(request.Parameters, "volatility");
            var optionType = GetParameterAsInt(request.Parameters, "option_type", 1);

            var price = BlackScholes(spot, strike, timeToMaturity, riskFreeRate, volatility, optionType);

            return new QuantModelResult
            {
                Results = new Dictionary<string, object>
                {
                    ["option_price"] = price,
                    ["option_type"] = optionType == 1 ? "call" : "put",
                    ["method"] = "black_scholes"
                }
            };
        }

        private async Task<QuantModelResult> ExecuteMonteCarloPricingAsync(QuantModelRequest request)
        {
            var spot = GetParameterAsDouble(request.Parameters, "spot");
            var strike = GetParameterAsDouble(request.Parameters, "strike");
            var timeToMaturity = GetParameterAsDouble(request.Parameters, "time_to_maturity");
            var riskFreeRate = GetParameterAsDouble(request.Parameters, "risk_free_rate");
            var volatility = GetParameterAsDouble(request.Parameters, "volatility");
            var optionType = GetParameterAsInt(request.Parameters, "option_type", 1);
            var numSimulations = GetParameterAsInt(request.Parameters, "num_simulations", 10000);

            var result = new double[2];
            MonteCarloPricing(spot, strike, timeToMaturity, riskFreeRate, volatility, optionType, numSimulations, result);

            return new QuantModelResult
            {
                Results = new Dictionary<string, object>
                {
                    ["option_price"] = result[0],
                    ["standard_error"] = result[1],
                    ["option_type"] = optionType == 1 ? "call" : "put",
                    ["num_simulations"] = numSimulations,
                    ["method"] = "monte_carlo"
                }
            };
        }

        private async Task<QuantModelResult> ExecuteBinomialTreeAsync(QuantModelRequest request)
        {
            var spot = GetParameterAsDouble(request.Parameters, "spot");
            var strike = GetParameterAsDouble(request.Parameters, "strike");
            var timeToMaturity = GetParameterAsDouble(request.Parameters, "time_to_maturity");
            var riskFreeRate = GetParameterAsDouble(request.Parameters, "risk_free_rate");
            var volatility = GetParameterAsDouble(request.Parameters, "volatility");
            var optionType = GetParameterAsInt(request.Parameters, "option_type", 1);
            var nSteps = GetParameterAsInt(request.Parameters, "n_steps", 100);

            var price = BinomialTree(spot, strike, timeToMaturity, riskFreeRate, volatility, optionType, nSteps);

            return new QuantModelResult
            {
                Results = new Dictionary<string, object>
                {
                    ["option_price"] = price,
                    ["option_type"] = optionType == 1 ? "call" : "put",
                    ["n_steps"] = nSteps,
                    ["method"] = "binomial_tree"
                }
            };
        }

        private double[] GetParameterAsDoubleArray(Dictionary<string, object> parameters, string key)
        {
            if (parameters.TryGetValue(key, out var value) && value is List<object> list)
            {
                return list.Cast<double>().ToArray();
            }
            throw new ArgumentException($"Parameter {key} not found or not a double array");
        }

        private double GetParameterAsDouble(Dictionary<string, object> parameters, string key, double defaultValue = 0.0)
        {
            if (parameters.TryGetValue(key, out var value))
            {
                return Convert.ToDouble(value);
            }
            return defaultValue;
        }

        private int GetParameterAsInt(Dictionary<string, object> parameters, string key, int defaultValue = 0)
        {
            if (parameters.TryGetValue(key, out var value))
            {
                return Convert.ToInt32(value);
            }
            return defaultValue;
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                try
                {
                    ClearCache();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error during C++ interop service cleanup");
                }
                finally
                {
                    _disposed = true;
                }
            }
        }
    }

    public class CppInteropConfiguration
    {
        public string LibraryPath { get; set; } = "QuantEngine";
        public bool EnableCaching { get; set; } = true;
        public int CacheSize { get; set; } = 1000;
        public TimeSpan CacheTimeout { get; set; } = TimeSpan.FromMinutes(30);
        public bool EnableMemoryOptimization { get; set; } = true;
        public int MaxMemoryUsageMB { get; set; } = 1024;
    }
}

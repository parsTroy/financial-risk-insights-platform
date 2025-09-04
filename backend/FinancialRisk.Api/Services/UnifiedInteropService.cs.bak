using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using FinancialRisk.Api.Models;

namespace FinancialRisk.Api.Services
{
    /// <summary>
    /// Unified interop service that can use Python.NET, gRPC, or C++/CLI
    /// Automatically selects the best available method for each operation
    /// </summary>
    public class UnifiedInteropService : IPythonInteropService, IDisposable
    {
        private readonly ILogger<UnifiedInteropService> _logger;
        private readonly InteropConfiguration _config;
        private readonly PythonInteropService _pythonService;
        private readonly GrpcPythonService _grpcService;
        private readonly CppInteropService _cppService;
        private bool _disposed = false;

        public UnifiedInteropService(
            ILogger<UnifiedInteropService> logger,
            IOptions<InteropConfiguration> config,
            PythonInteropService pythonService,
            GrpcPythonService grpcService,
            CppInteropService cppService)
        {
            _logger = logger;
            _config = config.Value;
            _pythonService = pythonService;
            _grpcService = grpcService;
            _cppService = cppService;
        }

        public async Task<bool> InitializeAsync()
        {
            try
            {
                _logger.LogInformation("Initializing unified interop service");

                bool pythonInitialized = false;
                bool grpcInitialized = false;
                bool cppInitialized = false;

                // Try to initialize each service
                if (_config.EnablePythonNet)
                {
                    try
                    {
                        pythonInitialized = await _pythonService.InitializeAsync();
                        if (pythonInitialized)
                        {
                            _logger.LogInformation("Python.NET service initialized successfully");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to initialize Python.NET service");
                    }
                }

                if (_config.EnableGrpc)
                {
                    try
                    {
                        grpcInitialized = await _grpcService.InitializeAsync();
                        if (grpcInitialized)
                        {
                            _logger.LogInformation("gRPC service initialized successfully");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to initialize gRPC service");
                    }
                }

                if (_config.EnableCpp)
                {
                    try
                    {
                        cppInitialized = await _cppService.InitializeAsync();
                        if (cppInitialized)
                        {
                            _logger.LogInformation("C++ service initialized successfully");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to initialize C++ service");
                    }
                }

                bool anyInitialized = pythonInitialized || grpcInitialized || cppInitialized;
                
                if (anyInitialized)
                {
                    _logger.LogInformation("Unified interop service initialized with {Count} active services", 
                        (pythonInitialized ? 1 : 0) + (grpcInitialized ? 1 : 0) + (cppInitialized ? 1 : 0));
                }
                else
                {
                    _logger.LogError("No interop services could be initialized");
                }

                return anyInitialized;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize unified interop service");
                return false;
            }
        }

        public async Task<T> CallPythonFunctionAsync<T>(string moduleName, string functionName, params object[] parameters)
        {
            try
            {
                // Try services in order of preference
                if (_config.EnablePythonNet)
                {
                    try
                    {
                        return await _pythonService.CallPythonFunctionAsync<T>(moduleName, functionName, parameters);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Python.NET call failed, trying gRPC");
                    }
                }

                if (_config.EnableGrpc)
                {
                    try
                    {
                        return await _grpcService.CallPythonFunctionAsync<T>(moduleName, functionName, parameters);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "gRPC call failed, trying C++");
                    }
                }

                if (_config.EnableCpp)
                {
                    try
                    {
                        return await _cppService.CallPythonFunctionAsync<T>(moduleName, functionName, parameters);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "All interop methods failed");
                        throw;
                    }
                }

                throw new InvalidOperationException("No interop services available");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling Python function {Module}.{Function}", moduleName, functionName);
                throw;
            }
        }

        public async Task<QuantModelResult> ExecuteQuantModelAsync(QuantModelRequest request)
        {
            try
            {
                _logger.LogInformation("Executing quant model: {ModelName}", request.ModelName);

                // Determine best service based on model type and configuration
                var service = SelectBestService(request);

                if (service == null)
                {
                    throw new InvalidOperationException("No suitable interop service available");
                }

                var result = await service.ExecuteQuantModelAsync(request);

                // Add interop metadata
                result.Metadata["interop_method"] = GetServiceName(service);
                result.Metadata["execution_timestamp"] = DateTime.UtcNow;

                _logger.LogInformation("Quant model {ModelName} executed using {Service}", 
                    request.ModelName, GetServiceName(service));

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing quant model: {ModelName}", request.ModelName);
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
            try
            {
                var allModels = new HashSet<string>();

                // Collect models from all available services
                if (_config.EnablePythonNet)
                {
                    try
                    {
                        var pythonModels = await _pythonService.GetAvailableModelsAsync();
                        foreach (var model in pythonModels)
                        {
                            allModels.Add($"python:{model}");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to get Python.NET models");
                    }
                }

                if (_config.EnableGrpc)
                {
                    try
                    {
                        var grpcModels = await _grpcService.GetAvailableModelsAsync();
                        foreach (var model in grpcModels)
                        {
                            allModels.Add($"grpc:{model}");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to get gRPC models");
                    }
                }

                if (_config.EnableCpp)
                {
                    try
                    {
                        var cppModels = await _cppService.GetAvailableModelsAsync();
                        foreach (var model in cppModels)
                        {
                            allModels.Add($"cpp:{model}");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to get C++ models");
                    }
                }

                return allModels.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available models");
                return new List<string>();
            }
        }

        public async Task<ModelMetadata> GetModelMetadataAsync(string modelName)
        {
            try
            {
                // Try to determine service from model name prefix
                var service = GetServiceFromModelName(modelName);
                
                if (service != null)
                {
                    return await service.GetModelMetadataAsync(modelName);
                }

                // Try all services if no prefix
                if (_config.EnablePythonNet)
                {
                    try
                    {
                        var metadata = await _pythonService.GetModelMetadataAsync(modelName);
                        if (metadata.Success)
                        {
                            return metadata;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to get Python.NET metadata for {ModelName}", modelName);
                    }
                }

                if (_config.EnableGrpc)
                {
                    try
                    {
                        var metadata = await _grpcService.GetModelMetadataAsync(modelName);
                        if (metadata.Success)
                        {
                            return metadata;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to get gRPC metadata for {ModelName}", modelName);
                    }
                }

                if (_config.EnableCpp)
                {
                    try
                    {
                        var metadata = await _cppService.GetModelMetadataAsync(modelName);
                        if (metadata.Success)
                        {
                            return metadata;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to get C++ metadata for {ModelName}", modelName);
                    }
                }

                return new ModelMetadata { ModelName = modelName, Success = false, Error = "Model not found" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting metadata for model: {ModelName}", modelName);
                return new ModelMetadata { ModelName = modelName, Success = false, Error = ex.Message };
            }
        }

        public async Task<bool> ValidateModelAsync(string modelName, Dictionary<string, object> parameters)
        {
            try
            {
                var service = GetServiceFromModelName(modelName) ?? SelectBestService(new QuantModelRequest { ModelName = modelName });
                
                if (service == null)
                {
                    return false;
                }

                return await service.ValidateModelAsync(modelName, parameters);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating model: {ModelName}", modelName);
                return false;
            }
        }

        public async Task<PerformanceMetrics> GetPerformanceMetricsAsync()
        {
            try
            {
                var metrics = new PerformanceMetrics();

                // Aggregate metrics from all services
                if (_config.EnablePythonNet)
                {
                    try
                    {
                        var pythonMetrics = await _pythonService.GetPerformanceMetricsAsync();
                        AggregateMetrics(metrics, pythonMetrics, "Python.NET");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to get Python.NET metrics");
                    }
                }

                if (_config.EnableGrpc)
                {
                    try
                    {
                        var grpcMetrics = await _grpcService.GetPerformanceMetricsAsync();
                        AggregateMetrics(metrics, grpcMetrics, "gRPC");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to get gRPC metrics");
                    }
                }

                if (_config.EnableCpp)
                {
                    try
                    {
                        var cppMetrics = await _cppService.GetPerformanceMetricsAsync();
                        AggregateMetrics(metrics, cppMetrics, "C++");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to get C++ metrics");
                    }
                }

                return metrics;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting performance metrics");
                return new PerformanceMetrics();
            }
        }

        private IPythonInteropService? SelectBestService(QuantModelRequest request)
        {
            // Selection logic based on model type, performance requirements, etc.
            var modelType = request.ModelType?.ToLower();
            var priority = request.Priority;

            // High priority requests use C++ for speed
            if (priority > 5 && _config.EnableCpp)
            {
                return _cppService;
            }

            // Statistical models prefer Python for flexibility
            if (modelType == "statistical" && _config.EnablePythonNet)
            {
                return _pythonService;
            }

            // Real-time models prefer gRPC for performance
            if (modelType == "realtime" && _config.EnableGrpc)
            {
                return _grpcService;
            }

            // Default fallback order
            if (_config.EnablePythonNet) return _pythonService;
            if (_config.EnableGrpc) return _grpcService;
            if (_config.EnableCpp) return _cppService;

            return null;
        }

        private IPythonInteropService? GetServiceFromModelName(string modelName)
        {
            if (modelName.StartsWith("python:"))
            {
                return _config.EnablePythonNet ? _pythonService : null;
            }
            if (modelName.StartsWith("grpc:"))
            {
                return _config.EnableGrpc ? _grpcService : null;
            }
            if (modelName.StartsWith("cpp:"))
            {
                return _config.EnableCpp ? _cppService : null;
            }
            return null;
        }

        private string GetServiceName(IPythonInteropService service)
        {
            if (service == _pythonService) return "Python.NET";
            if (service == _grpcService) return "gRPC";
            if (service == _cppService) return "C++";
            return "Unknown";
        }

        private void AggregateMetrics(PerformanceMetrics target, PerformanceMetrics source, string serviceName)
        {
            target.TotalRequests += source.TotalRequests;
            target.SuccessfulRequests += source.SuccessfulRequests;
            target.FailedRequests += source.FailedRequests;
            target.TotalMemoryUsageMB += source.TotalMemoryUsageMB;
            target.ActiveConnections += source.ActiveConnections;
            target.QueuedRequests += source.QueuedRequests;

            // Update success rate
            if (target.TotalRequests > 0)
            {
                target.SuccessRate = (double)target.SuccessfulRequests / target.TotalRequests;
            }

            // Update average execution time
            if (source.TotalRequests > 0)
            {
                var weightedTime = source.AverageExecutionTime.TotalMilliseconds * source.TotalRequests;
                target.AverageExecutionTime = TimeSpan.FromMilliseconds(weightedTime / target.TotalRequests);
            }

            // Update average memory usage
            if (source.TotalRequests > 0)
            {
                target.AverageMemoryUsageMB = target.TotalMemoryUsageMB / target.TotalRequests;
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                try
                {
                    _pythonService?.Dispose();
                    _grpcService?.Dispose();
                    _cppService?.Dispose();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error during unified interop service cleanup");
                }
                finally
                {
                    _disposed = true;
                }
            }
        }
    }

    public class InteropConfiguration
    {
        public bool EnablePythonNet { get; set; } = true;
        public bool EnableGrpc { get; set; } = true;
        public bool EnableCpp { get; set; } = true;
        public string PreferredMethod { get; set; } = "auto"; // auto, python, grpc, cpp
        public Dictionary<string, string> ModelServiceMapping { get; set; } = new();
        public bool EnableFallback { get; set; } = true;
        public bool EnableMetricsAggregation { get; set; } = true;
        public TimeSpan ServiceTimeout { get; set; } = TimeSpan.FromMinutes(5);
        public int MaxRetries { get; set; } = 3;
    }
}

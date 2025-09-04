using Microsoft.Extensions.Logging;
using Grpc.Core;
using FinancialRisk.Api.Models;

namespace FinancialRisk.Api.Services
{
    /// <summary>
    /// gRPC service for Python model execution
    /// Provides high-performance communication with Python quant models
    /// </summary>
    public class GrpcPythonService : IPythonInteropService, IDisposable
    {
        private readonly ILogger<GrpcPythonService> _logger;
        private readonly GrpcPythonConfiguration _config;
        private Channel? _channel;
        private QuantModelService.QuantModelServiceClient? _client;
        private bool _disposed = false;

        public GrpcPythonService(ILogger<GrpcPythonService> logger, GrpcPythonConfiguration config)
        {
            _logger = logger;
            _config = config;
        }

        public async Task<bool> InitializeAsync()
        {
            try
            {
                _logger.LogInformation("Initializing gRPC Python service");

                // Create gRPC channel
                _channel = new Channel(_config.ServerAddress, _config.Port, ChannelCredentials.Insecure);
                _client = new QuantModelService.QuantModelServiceClient(_channel);

                // Test connection
                var healthRequest = new HealthCheckRequest();
                var healthResponse = await _client.HealthCheckAsync(healthRequest);

                if (healthResponse.IsHealthy)
                {
                    _logger.LogInformation("gRPC Python service initialized successfully");
                    return true;
                }
                else
                {
                    _logger.LogError("gRPC Python service health check failed");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize gRPC Python service");
                return false;
            }
        }

        public async Task<T> CallPythonFunctionAsync<T>(string moduleName, string functionName, params object[] parameters)
        {
            try
            {
                if (_client == null)
                {
                    await InitializeAsync();
                }

                _logger.LogDebug("Calling Python function {Module}.{Function} via gRPC", moduleName, functionName);

                var request = new FunctionCallRequest
                {
                    ModuleName = moduleName,
                    FunctionName = functionName
                };

                // Add parameters
                foreach (var param in parameters)
                {
                    var paramValue = new ParameterValue();
                    paramValue.Value = System.Text.Json.JsonSerializer.Serialize(param);
                    request.Parameters.Add(paramValue);
                }

                var response = await _client.CallFunctionAsync(request);

                if (response.Success)
                {
                    var result = System.Text.Json.JsonSerializer.Deserialize<T>(response.Result);
                    return result;
                }
                else
                {
                    throw new InvalidOperationException($"gRPC call failed: {response.Error}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling Python function {Module}.{Function} via gRPC", moduleName, functionName);
                throw;
            }
        }

        public async Task<QuantModelResult> ExecuteQuantModelAsync(QuantModelRequest request)
        {
            try
            {
                if (_client == null)
                {
                    await InitializeAsync();
                }

                _logger.LogInformation("Executing quant model via gRPC: {ModelName}", request.ModelName);

                var grpcRequest = new ModelExecutionRequest
                {
                    ModelName = request.ModelName,
                    ModelType = request.ModelType,
                    RequestId = request.RequestId,
                    Priority = request.Priority,
                    EnableCaching = request.EnableCaching
                };

                // Add parameters
                foreach (var param in request.Parameters)
                {
                    grpcRequest.Parameters[param.Key] = System.Text.Json.JsonSerializer.Serialize(param.Value);
                }

                // Add input data
                foreach (var data in request.InputData)
                {
                    grpcRequest.InputData[data.Key] = System.Text.Json.JsonSerializer.Serialize(data.Value);
                }

                // Add options
                foreach (var option in request.Options)
                {
                    grpcRequest.Options[option.Key] = System.Text.Json.JsonSerializer.Serialize(option.Value);
                }

                var response = await _client.ExecuteModelAsync(grpcRequest);

                var result = new QuantModelResult
                {
                    Success = response.Success,
                    Error = response.Error,
                    ModelName = response.ModelName,
                    RequestId = response.RequestId,
                    ExecutionTime = TimeSpan.FromMilliseconds(response.ExecutionTimeMs),
                    CompletionTime = response.CompletionTime.ToDateTime(),
                    MemoryUsageMB = response.MemoryUsageMb,
                    PythonVersion = response.PythonVersion
                };

                // Deserialize results
                foreach (var resultItem in response.Results)
                {
                    var value = System.Text.Json.JsonSerializer.Deserialize<object>(resultItem.Value);
                    result.Results[resultItem.Key] = value;
                }

                // Deserialize metadata
                foreach (var metadataItem in response.Metadata)
                {
                    var value = System.Text.Json.JsonSerializer.Deserialize<object>(metadataItem.Value);
                    result.Metadata[metadataItem.Key] = value;
                }

                // Add warnings
                result.Warnings.AddRange(response.Warnings);

                _logger.LogInformation("Quant model {ModelName} executed successfully via gRPC", request.ModelName);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing quant model via gRPC: {ModelName}", request.ModelName);
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
                if (_client == null)
                {
                    await InitializeAsync();
                }

                var request = new GetModelsRequest();
                var response = await _client.GetAvailableModelsAsync(request);

                return response.Models.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available models via gRPC");
                return new List<string>();
            }
        }

        public async Task<ModelMetadata> GetModelMetadataAsync(string modelName)
        {
            try
            {
                if (_client == null)
                {
                    await InitializeAsync();
                }

                var request = new GetModelMetadataRequest
                {
                    ModelName = modelName
                };

                var response = await _client.GetModelMetadataAsync(request);

                var metadata = new ModelMetadata
                {
                    Success = response.Success,
                    Error = response.Error,
                    ModelName = response.ModelName,
                    Description = response.Description,
                    Version = response.Version,
                    Author = response.Author,
                    Category = response.Category,
                    LastModified = response.LastModified.ToDateTime(),
                    IsDeprecated = response.IsDeprecated,
                    DeprecationMessage = response.DeprecationMessage
                };

                // Add required parameters
                metadata.RequiredParameters.AddRange(response.RequiredParameters);

                // Add optional parameters
                metadata.OptionalParameters.AddRange(response.OptionalParameters);

                // Add parameter types
                foreach (var paramType in response.ParameterTypes)
                {
                    var value = System.Text.Json.JsonSerializer.Deserialize<object>(paramType.Value);
                    metadata.ParameterTypes[paramType.Key] = value;
                }

                // Add default values
                foreach (var defaultValue in response.DefaultValues)
                {
                    var value = System.Text.Json.JsonSerializer.Deserialize<object>(defaultValue.Value);
                    metadata.DefaultValues[defaultValue.Key] = value;
                }

                // Add dependencies
                metadata.Dependencies.AddRange(response.Dependencies);

                // Add tags
                metadata.Tags.AddRange(response.Tags);

                return metadata;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting metadata for model via gRPC: {ModelName}", modelName);
                return new ModelMetadata { ModelName = modelName, Success = false, Error = ex.Message };
            }
        }

        public async Task<bool> ValidateModelAsync(string modelName, Dictionary<string, object> parameters)
        {
            try
            {
                if (_client == null)
                {
                    await InitializeAsync();
                }

                var request = new ValidateModelRequest
                {
                    ModelName = modelName
                };

                // Add parameters
                foreach (var param in parameters)
                {
                    request.Parameters[param.Key] = System.Text.Json.JsonSerializer.Serialize(param.Value);
                }

                var response = await _client.ValidateModelAsync(request);
                return response.IsValid;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating model via gRPC: {ModelName}", modelName);
                return false;
            }
        }

        public async Task<PerformanceMetrics> GetPerformanceMetricsAsync()
        {
            try
            {
                if (_client == null)
                {
                    await InitializeAsync();
                }

                var request = new GetPerformanceMetricsRequest();
                var response = await _client.GetPerformanceMetricsAsync(request);

                var metrics = new PerformanceMetrics
                {
                    TotalRequests = response.TotalRequests,
                    SuccessfulRequests = response.SuccessfulRequests,
                    FailedRequests = response.FailedRequests,
                    SuccessRate = response.SuccessRate,
                    AverageExecutionTime = TimeSpan.FromMilliseconds(response.AverageExecutionTimeMs),
                    MinExecutionTime = TimeSpan.FromMilliseconds(response.MinExecutionTimeMs),
                    MaxExecutionTime = TimeSpan.FromMilliseconds(response.MaxExecutionTimeMs),
                    TotalMemoryUsageMB = response.TotalMemoryUsageMb,
                    AverageMemoryUsageMB = response.AverageMemoryUsageMb,
                    ActiveConnections = response.ActiveConnections,
                    QueuedRequests = response.QueuedRequests,
                    LastReset = response.LastReset.ToDateTime()
                };

                // Add model usage counts
                foreach (var usage in response.ModelUsageCounts)
                {
                    metrics.ModelUsageCounts[usage.Key] = usage.Value;
                }

                // Add model average execution times
                foreach (var execTime in response.ModelAverageExecutionTimes)
                {
                    metrics.ModelAverageExecutionTimes[execTime.Key] = TimeSpan.FromMilliseconds(execTime.Value);
                }

                return metrics;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting performance metrics via gRPC");
                return new PerformanceMetrics();
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                try
                {
                    _channel?.ShutdownAsync().Wait();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error during gRPC channel cleanup");
                }
                finally
                {
                    _disposed = true;
                }
            }
        }
    }

    public class GrpcPythonConfiguration
    {
        public string ServerAddress { get; set; } = "localhost";
        public int Port { get; set; } = 50051;
        public TimeSpan Timeout { get; set; } = TimeSpan.FromMinutes(5);
        public int MaxRetries { get; set; } = 3;
        public bool EnableCompression { get; set; } = true;
    }
}

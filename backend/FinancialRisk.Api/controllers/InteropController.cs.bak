using Microsoft.AspNetCore.Mvc;
using FinancialRisk.Api.Services;
using FinancialRisk.Api.Models;

namespace FinancialRisk.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InteropController : ControllerBase
    {
        private readonly ILogger<InteropController> _logger;
        private readonly IPythonInteropService _interopService;

        public InteropController(ILogger<InteropController> logger, IPythonInteropService interopService)
        {
            _logger = logger;
            _interopService = interopService;
        }

        /// <summary>
        /// Execute a quantitative model
        /// </summary>
        /// <param name="request">Model execution request</param>
        /// <returns>Model execution result</returns>
        [HttpPost("execute")]
        public async Task<ActionResult<ModelExecutionResponse>> ExecuteModel([FromBody] ModelExecutionRequest request)
        {
            try
            {
                _logger.LogInformation("Model execution requested: {ModelName}", request.ModelName);

                if (string.IsNullOrEmpty(request.ModelName))
                {
                    return BadRequest(new ModelExecutionResponse
                    {
                        Success = false,
                        Error = "Model name is required"
                    });
                }

                var quantRequest = new QuantModelRequest
                {
                    ModelName = request.ModelName,
                    Parameters = request.Parameters,
                    InputData = request.InputData,
                    EnableCaching = request.EnableCaching,
                    Priority = request.Priority,
                    RequestId = Guid.NewGuid().ToString(),
                    RequestTime = DateTime.UtcNow
                };

                var result = await _interopService.ExecuteQuantModelAsync(quantRequest);

                var response = new ModelExecutionResponse
                {
                    Success = result.Success,
                    Error = result.Error,
                    Result = result,
                    RequestId = result.RequestId,
                    RequestTime = result.RequestTime,
                    ResponseTime = DateTime.UtcNow
                };

                if (result.Success)
                {
                    return Ok(response);
                }
                else
                {
                    return BadRequest(response);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing model: {ModelName}", request.ModelName);
                return StatusCode(500, new ModelExecutionResponse
                {
                    Success = false,
                    Error = ex.Message,
                    RequestId = Guid.NewGuid().ToString(),
                    RequestTime = DateTime.UtcNow,
                    ResponseTime = DateTime.UtcNow
                });
            }
        }

        /// <summary>
        /// Get available quantitative models
        /// </summary>
        /// <returns>List of available models</returns>
        [HttpGet("models")]
        public async Task<ActionResult<List<string>>> GetAvailableModels()
        {
            try
            {
                var models = await _interopService.GetAvailableModelsAsync();
                return Ok(models);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available models");
                return StatusCode(500, new List<string>());
            }
        }

        /// <summary>
        /// Get metadata for a specific model
        /// </summary>
        /// <param name="modelName">Model name</param>
        /// <returns>Model metadata</returns>
        [HttpGet("models/{modelName}/metadata")]
        public async Task<ActionResult<ModelMetadata>> GetModelMetadata(string modelName)
        {
            try
            {
                if (string.IsNullOrEmpty(modelName))
                {
                    return BadRequest(new ModelMetadata
                    {
                        Success = false,
                        Error = "Model name is required"
                    });
                }

                var metadata = await _interopService.GetModelMetadataAsync(modelName);

                if (metadata.Success)
                {
                    return Ok(metadata);
                }
                else
                {
                    return NotFound(metadata);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting metadata for model: {ModelName}", modelName);
                return StatusCode(500, new ModelMetadata
                {
                    Success = false,
                    Error = ex.Message,
                    ModelName = modelName
                });
            }
        }

        /// <summary>
        /// Validate model parameters
        /// </summary>
        /// <param name="modelName">Model name</param>
        /// <param name="parameters">Parameters to validate</param>
        /// <returns>Validation result</returns>
        [HttpPost("models/{modelName}/validate")]
        public async Task<ActionResult<bool>> ValidateModel(string modelName, [FromBody] Dictionary<string, object> parameters)
        {
            try
            {
                if (string.IsNullOrEmpty(modelName))
                {
                    return BadRequest(false);
                }

                var isValid = await _interopService.ValidateModelAsync(modelName, parameters);
                return Ok(isValid);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating model: {ModelName}", modelName);
                return StatusCode(500, false);
            }
        }

        /// <summary>
        /// Get performance metrics for the interop service
        /// </summary>
        /// <returns>Performance metrics</returns>
        [HttpGet("metrics")]
        public async Task<ActionResult<PerformanceMetrics>> GetPerformanceMetrics()
        {
            try
            {
                var metrics = await _interopService.GetPerformanceMetricsAsync();
                return Ok(metrics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting performance metrics");
                return StatusCode(500, new PerformanceMetrics());
            }
        }

        /// <summary>
        /// Get health status of the interop service
        /// </summary>
        /// <returns>Health check result</returns>
        [HttpGet("health")]
        public async Task<ActionResult<InteropHealthCheck>> GetHealth()
        {
            try
            {
                var metrics = await _interopService.GetPerformanceMetricsAsync();
                var models = await _interopService.GetAvailableModelsAsync();

                var healthCheck = new InteropHealthCheck
                {
                    IsHealthy = true,
                    PythonVersion = "Mixed", // Would be determined by the actual service
                    AvailableModules = models,
                    Performance = metrics,
                    CheckTime = DateTime.UtcNow
                };

                return Ok(healthCheck);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting health status");
                return StatusCode(500, new InteropHealthCheck
                {
                    IsHealthy = false,
                    Error = ex.Message,
                    CheckTime = DateTime.UtcNow
                });
            }
        }

        /// <summary>
        /// Call a Python function directly
        /// </summary>
        /// <param name="moduleName">Python module name</param>
        /// <param name="functionName">Function name</param>
        /// <param name="parameters">Function parameters</param>
        /// <returns>Function result</returns>
        [HttpPost("python/{moduleName}/{functionName}")]
        public async Task<ActionResult<object>> CallPythonFunction(string moduleName, string functionName, [FromBody] object[] parameters)
        {
            try
            {
                if (string.IsNullOrEmpty(moduleName) || string.IsNullOrEmpty(functionName))
                {
                    return BadRequest("Module name and function name are required");
                }

                var result = await _interopService.CallPythonFunctionAsync<object>(moduleName, functionName, parameters ?? new object[0]);
                return Ok(result);
            }
            catch (NotSupportedException ex)
            {
                _logger.LogWarning(ex, "Python function call not supported by current interop service");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling Python function {Module}.{Function}", moduleName, functionName);
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// Get model registry with all available models and their metadata
        /// </summary>
        /// <returns>Model registry</returns>
        [HttpGet("registry")]
        public async Task<ActionResult<ModelRegistry>> GetModelRegistry()
        {
            try
            {
                var models = await _interopService.GetAvailableModelsAsync();
                var registry = new ModelRegistry
                {
                    Models = new List<ModelMetadata>(),
                    TotalModels = models.Count,
                    ActiveModels = models.Count,
                    DeprecatedModels = 0,
                    LastUpdated = DateTime.UtcNow
                };

                // Get metadata for each model
                foreach (var modelName in models)
                {
                    try
                    {
                        var metadata = await _interopService.GetModelMetadataAsync(modelName);
                        if (metadata.Success)
                        {
                            registry.Models.Add(metadata);
                            
                            // Categorize models
                            if (!registry.Categories.ContainsKey(metadata.Category))
                            {
                                registry.Categories[metadata.Category] = new List<string>();
                            }
                            registry.Categories[metadata.Category].Add(modelName);

                            // Add tags
                            foreach (var tag in metadata.Tags)
                            {
                                if (!registry.Tags.ContainsKey(tag))
                                {
                                    registry.Tags[tag] = new List<string>();
                                }
                                registry.Tags[tag].Add(modelName);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to get metadata for model: {ModelName}", modelName);
                    }
                }

                return Ok(registry);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting model registry");
                return StatusCode(500, new ModelRegistry());
            }
        }

        /// <summary>
        /// Get statistics about model usage and performance
        /// </summary>
        /// <returns>Model statistics</returns>
        [HttpGet("stats")]
        public async Task<ActionResult<Dictionary<string, object>>> GetModelStats()
        {
            try
            {
                var metrics = await _interopService.GetPerformanceMetricsAsync();
                var models = await _interopService.GetAvailableModelsAsync();

                var stats = new Dictionary<string, object>
                {
                    ["total_models"] = models.Count,
                    ["total_requests"] = metrics.TotalRequests,
                    ["successful_requests"] = metrics.SuccessfulRequests,
                    ["failed_requests"] = metrics.FailedRequests,
                    ["success_rate"] = metrics.SuccessRate,
                    ["average_execution_time_ms"] = metrics.AverageExecutionTime.TotalMilliseconds,
                    ["min_execution_time_ms"] = metrics.MinExecutionTime.TotalMilliseconds,
                    ["max_execution_time_ms"] = metrics.MaxExecutionTime.TotalMilliseconds,
                    ["total_memory_usage_mb"] = metrics.TotalMemoryUsageMB,
                    ["average_memory_usage_mb"] = metrics.AverageMemoryUsageMB,
                    ["active_connections"] = metrics.ActiveConnections,
                    ["queued_requests"] = metrics.QueuedRequests,
                    ["model_usage_counts"] = metrics.ModelUsageCounts,
                    ["model_average_execution_times"] = metrics.ModelAverageExecutionTimes.ToDictionary(
                        kvp => kvp.Key, 
                        kvp => kvp.Value.TotalMilliseconds
                    ),
                    ["last_reset"] = metrics.LastReset,
                    ["available_models"] = models
                };

                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting model statistics");
                return StatusCode(500, new Dictionary<string, object>());
            }
        }
    }
}

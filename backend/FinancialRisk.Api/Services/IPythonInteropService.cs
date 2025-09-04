using FinancialRisk.Api.Models;

namespace FinancialRisk.Api.Services
{
    /// <summary>
    /// Interface for Python/.NET interop service
    /// Provides seamless integration between C# and Python scientific computing libraries
    /// </summary>
    public interface IPythonInteropService : IDisposable
    {
        /// <summary>
        /// Initialize the Python runtime and import required modules
        /// </summary>
        Task<bool> InitializeAsync();

        /// <summary>
        /// Call a Python function with parameters and return typed result
        /// </summary>
        /// <typeparam name="T">Return type</typeparam>
        /// <param name="moduleName">Python module name</param>
        /// <param name="functionName">Function name</param>
        /// <param name="parameters">Function parameters</param>
        /// <returns>Typed result from Python function</returns>
        Task<T> CallPythonFunctionAsync<T>(string moduleName, string functionName, params object[] parameters);

        /// <summary>
        /// Execute a quantitative model with the given request
        /// </summary>
        /// <param name="request">Model execution request</param>
        /// <returns>Model execution result</returns>
        Task<QuantModelResult> ExecuteQuantModelAsync(QuantModelRequest request);

        /// <summary>
        /// Get list of available quantitative models
        /// </summary>
        /// <returns>List of model names</returns>
        Task<List<string>> GetAvailableModelsAsync();

        /// <summary>
        /// Get metadata for a specific model
        /// </summary>
        /// <param name="modelName">Model name</param>
        /// <returns>Model metadata</returns>
        Task<ModelMetadata> GetModelMetadataAsync(string modelName);

        /// <summary>
        /// Validate model parameters before execution
        /// </summary>
        /// <param name="modelName">Model name</param>
        /// <param name="parameters">Parameters to validate</param>
        /// <returns>True if parameters are valid</returns>
        Task<bool> ValidateModelAsync(string modelName, Dictionary<string, object> parameters);

        /// <summary>
        /// Get performance metrics for the interop service
        /// </summary>
        /// <returns>Performance metrics</returns>
        Task<PerformanceMetrics> GetPerformanceMetricsAsync();
    }
}

using Microsoft.Extensions.Logging;
using System.Runtime.InteropServices;
using System.Text.Json;
using FinancialRisk.Api.Models;

namespace FinancialRisk.Api.Services
{
    /// <summary>
    /// Service for Python/.NET interop using Python.NET
    /// Provides seamless integration between C# and Python scientific computing libraries
    /// </summary>
    public class PythonInteropService : IPythonInteropService, IDisposable
    {
        private readonly ILogger<PythonInteropService> _logger;
        private readonly PythonInteropConfiguration _config;
        private bool _isInitialized = false;
        private bool _disposed = false;

        public PythonInteropService(ILogger<PythonInteropService> logger, PythonInteropConfiguration config)
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

                _logger.LogInformation("Initializing Python.NET interop service");

                // Initialize Python.NET runtime
                if (!PythonEngine.IsInitialized)
                {
                    PythonEngine.Initialize();
                }

                // Set Python path to include our quant models
                var pythonPath = Path.Combine(Directory.GetCurrentDirectory(), "Services", "python_models");
                PythonEngine.PythonPath.Add(pythonPath);

                // Import required Python modules
                await ImportPythonModulesAsync();

                _isInitialized = true;
                _logger.LogInformation("Python.NET interop service initialized successfully");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize Python.NET interop service");
                return false;
            }
        }

        public async Task<T> CallPythonFunctionAsync<T>(string moduleName, string functionName, params object[] parameters)
        {
            try
            {
                if (!_isInitialized)
                {
                    await InitializeAsync();
                }

                _logger.LogDebug("Calling Python function {Module}.{Function}", moduleName, functionName);

                using (Py.GIL())
                {
                    // Import the module
                    var module = Py.Import(moduleName);
                    if (module == null)
                    {
                        throw new InvalidOperationException($"Failed to import Python module: {moduleName}");
                    }

                    // Get the function
                    var function = module.GetAttr(functionName);
                    if (function == null)
                    {
                        throw new InvalidOperationException($"Function {functionName} not found in module {moduleName}");
                    }

                    // Convert parameters to Python objects
                    var pyParams = parameters.Select(p => ToPython(p)).ToArray();

                    // Call the function
                    var result = function.Call(pyParams);

                    // Convert result back to C# type
                    var convertedResult = FromPython<T>(result);

                    _logger.LogDebug("Python function {Module}.{Function} completed successfully", moduleName, functionName);
                    return convertedResult;
                }
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

                if (!_isInitialized)
                {
                    await InitializeAsync();
                }

                using (Py.GIL())
                {
                    // Import the quant models module
                    var quantModels = Py.Import("quant_models");
                    if (quantModels == null)
                    {
                        throw new InvalidOperationException("Failed to import quant_models module");
                    }

                    // Get the model executor function
                    var executor = quantModels.GetAttr("execute_model");
                    if (executor == null)
                    {
                        throw new InvalidOperationException("execute_model function not found");
                    }

                    // Convert request to Python dictionary
                    var requestDict = ToPython(request);

                    // Execute the model
                    var result = executor.Call(requestDict);

                    // Convert result back to C# object
                    var modelResult = FromPython<QuantModelResult>(result);

                    _logger.LogInformation("Quant model {ModelName} executed successfully", request.ModelName);
                    return modelResult;
                }
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
                if (!_isInitialized)
                {
                    await InitializeAsync();
                }

                using (Py.GIL())
                {
                    var quantModels = Py.Import("quant_models");
                    if (quantModels == null)
                    {
                        return new List<string>();
                    }

                    var getModels = quantModels.GetAttr("get_available_models");
                    if (getModels == null)
                    {
                        return new List<string>();
                    }

                    var result = getModels.Call();
                    var models = FromPython<List<string>>(result);

                    return models ?? new List<string>();
                }
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
                if (!_isInitialized)
                {
                    await InitializeAsync();
                }

                using (Py.GIL())
                {
                    var quantModels = Py.Import("quant_models");
                    if (quantModels == null)
                    {
                        return new ModelMetadata { ModelName = modelName, Success = false };
                    }

                    var getMetadata = quantModels.GetAttr("get_model_metadata");
                    if (getMetadata == null)
                    {
                        return new ModelMetadata { ModelName = modelName, Success = false };
                    }

                    var result = getMetadata.Call(modelName);
                    var metadata = FromPython<ModelMetadata>(result);

                    return metadata ?? new ModelMetadata { ModelName = modelName, Success = false };
                }
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
                if (!_isInitialized)
                {
                    await InitializeAsync();
                }

                using (Py.GIL())
                {
                    var quantModels = Py.Import("quant_models");
                    if (quantModels == null)
                    {
                        return false;
                    }

                    var validateModel = quantModels.GetAttr("validate_model");
                    if (validateModel == null)
                    {
                        return false;
                    }

                    var result = validateModel.Call(modelName, ToPython(parameters));
                    var isValid = FromPython<bool>(result);

                    return isValid;
                }
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
                if (!_isInitialized)
                {
                    await InitializeAsync();
                }

                using (Py.GIL())
                {
                    var quantModels = Py.Import("quant_models");
                    if (quantModels == null)
                    {
                        return new PerformanceMetrics();
                    }

                    var getMetrics = quantModels.GetAttr("get_performance_metrics");
                    if (getMetrics == null)
                    {
                        return new PerformanceMetrics();
                    }

                    var result = getMetrics.Call();
                    var metrics = FromPython<PerformanceMetrics>(result);

                    return metrics ?? new PerformanceMetrics();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting performance metrics");
                return new PerformanceMetrics();
            }
        }

        private async Task ImportPythonModulesAsync()
        {
            using (Py.GIL())
            {
                // Import core scientific computing modules
                Py.Import("numpy");
                Py.Import("scipy");
                Py.Import("pandas");
                Py.Import("sklearn");
                Py.Import("statsmodels");
                
                // Import our custom quant models
                Py.Import("quant_models");
                Py.Import("monte_carlo_engine");
                Py.Import("portfolio_optimizer");
            }
        }

        private PyObject ToPython(object obj)
        {
            if (obj == null)
                return PyObject.FromManagedObject(null);

            switch (obj)
            {
                case string str:
                    return new PyString(str);
                case int i:
                    return new PyInt(i);
                case double d:
                    return new PyFloat(d);
                case float f:
                    return new PyFloat(f);
                case bool b:
                    return new PyBool(b);
                case DateTime dt:
                    return new PyString(dt.ToString("O"));
                case Dictionary<string, object> dict:
                    var pyDict = new PyDict();
                    foreach (var kvp in dict)
                    {
                        pyDict[new PyString(kvp.Key)] = ToPython(kvp.Value);
                    }
                    return pyDict;
                case List<object> list:
                    var pyList = new PyList();
                    foreach (var item in list)
                    {
                        pyList.Append(ToPython(item));
                    }
                    return pyList;
                case Array array:
                    var pyArray = new PyList();
                    foreach (var item in array)
                    {
                        pyArray.Append(ToPython(item));
                    }
                    return pyArray;
                default:
                    // Try to serialize to JSON and then to Python
                    try
                    {
                        var json = JsonSerializer.Serialize(obj);
                        return new PyString(json);
                    }
                    catch
                    {
                        return PyObject.FromManagedObject(obj);
                    }
            }
        }

        private T FromPython<T>(PyObject pyObj)
        {
            if (pyObj == null || pyObj.IsNone())
                return default(T);

            try
            {
                // Handle primitive types
                if (typeof(T) == typeof(string))
                    return (T)(object)pyObj.As<string>();
                if (typeof(T) == typeof(int))
                    return (T)(object)pyObj.As<int>();
                if (typeof(T) == typeof(double))
                    return (T)(object)pyObj.As<double>();
                if (typeof(T) == typeof(float))
                    return (T)(object)pyObj.As<float>();
                if (typeof(T) == typeof(bool))
                    return (T)(object)pyObj.As<bool>();

                // Handle complex types by converting to JSON first
                var json = pyObj.As<string>();
                return JsonSerializer.Deserialize<T>(json);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to convert Python object to {Type}", typeof(T).Name);
                return default(T);
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                try
                {
                    if (PythonEngine.IsInitialized)
                    {
                        PythonEngine.Shutdown();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error during Python.NET cleanup");
                }
                finally
                {
                    _disposed = true;
                }
            }
        }
    }
}

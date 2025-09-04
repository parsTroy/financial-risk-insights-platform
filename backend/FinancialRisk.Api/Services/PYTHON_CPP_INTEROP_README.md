# Python/C++ Interop for Quantitative Models

## Overview

This implementation provides seamless integration between Python scientific computing libraries and the C# backend through multiple interop approaches. It enables quantitative models to be called from .NET while maintaining both performance and flexibility.

## Architecture

The interop system supports three different approaches:

1. **Python.NET Integration** - Direct Python runtime integration
2. **gRPC Service** - High-performance communication with Python services
3. **C++/CLI Wrapper** - Native C++ performance for critical operations

### Unified Interop Service

The `UnifiedInteropService` automatically selects the best available method for each operation based on:
- Model type and requirements
- Performance priorities
- Service availability
- Fallback mechanisms

## Features

### Core Capabilities
- **Multiple Interop Methods**: Python.NET, gRPC, and C++/CLI
- **Automatic Service Selection**: Intelligent routing based on model requirements
- **Fallback Mechanisms**: Graceful degradation when services are unavailable
- **Performance Optimization**: C++ for speed, Python for flexibility
- **Model Registry**: Centralized model discovery and metadata
- **Health Monitoring**: Service health checks and performance metrics

### Supported Model Types
- **Risk Management**: VaR, CVaR, Stress Testing
- **Portfolio Optimization**: Markowitz, Black-Litterman, Risk Parity
- **Statistical Models**: GARCH, Copula, Regime Switching
- **Pricing Models**: Black-Scholes, Monte Carlo, Binomial Tree
- **Machine Learning**: Scikit-learn integration
- **Time Series**: ARIMA, GARCH, State Space Models

## Implementation Details

### 1. Python.NET Integration

**File**: `PythonInteropService.cs`

Uses Python.NET to directly embed Python runtime in .NET process.

**Advantages**:
- Seamless integration
- Direct access to Python objects
- No network overhead
- Easy debugging

**Disadvantages**:
- Memory overhead
- Potential GIL (Global Interpreter Lock) issues
- Platform dependencies

**Usage**:
```csharp
var result = await pythonService.CallPythonFunctionAsync<double[]>(
    "numpy", "random.normal", 0.0, 1.0, 1000);
```

### 2. gRPC Service

**File**: `GrpcPythonService.cs`

Uses gRPC for high-performance communication with Python services.

**Advantages**:
- High performance
- Language agnostic
- Built-in load balancing
- Streaming support

**Disadvantages**:
- Network overhead
- Additional infrastructure
- More complex setup

**Usage**:
```csharp
var result = await grpcService.ExecuteQuantModelAsync(request);
```

### 3. C++/CLI Wrapper

**Files**: `QuantEngine.h`, `QuantEngine.cpp`, `CppInteropService.cs`

Native C++ implementation for performance-critical operations.

**Advantages**:
- Maximum performance
- No Python dependencies
- Memory efficient
- Cross-platform

**Disadvantages**:
- Limited to implemented functions
- No access to Python ecosystem
- Requires C++ compilation

**Usage**:
```csharp
var var = CalculateVaRHistorical(returns, length, 0.95);
```

## API Endpoints

### Model Execution
- `POST /api/interop/execute` - Execute any quantitative model
- `POST /api/interop/python/{module}/{function}` - Call Python function directly

### Model Management
- `GET /api/interop/models` - Get available models
- `GET /api/interop/models/{name}/metadata` - Get model metadata
- `POST /api/interop/models/{name}/validate` - Validate model parameters

### Monitoring
- `GET /api/interop/health` - Service health check
- `GET /api/interop/metrics` - Performance metrics
- `GET /api/interop/registry` - Model registry
- `GET /api/interop/stats` - Usage statistics

## Configuration

### InteropConfiguration
```csharp
public class InteropConfiguration
{
    public bool EnablePythonNet { get; set; } = true;
    public bool EnableGrpc { get; set; } = true;
    public bool EnableCpp { get; set; } = true;
    public string PreferredMethod { get; set; } = "auto";
    public bool EnableFallback { get; set; } = true;
    public bool EnableMetricsAggregation { get; set; } = true;
    public TimeSpan ServiceTimeout { get; set; } = TimeSpan.FromMinutes(5);
    public int MaxRetries { get; set; } = 3;
}
```

### Python.NET Configuration
```csharp
public class PythonInteropConfiguration
{
    public string PythonPath { get; set; } = string.Empty;
    public string PythonExecutable { get; set; } = "python3";
    public List<string> PythonModules { get; set; } = new();
    public bool EnableCaching { get; set; } = true;
    public int CacheTimeoutMinutes { get; set; } = 30;
    public bool EnablePerformanceMetrics { get; set; } = true;
    public int MaxConcurrentRequests { get; set; } = 10;
    public TimeSpan RequestTimeout { get; set; } = TimeSpan.FromMinutes(5);
}
```

### gRPC Configuration
```csharp
public class GrpcPythonConfiguration
{
    public string ServerAddress { get; set; } = "localhost";
    public int Port { get; set; } = 50051;
    public TimeSpan Timeout { get; set; } = TimeSpan.FromMinutes(5);
    public int MaxRetries { get; set; } = 3;
    public bool EnableCompression { get; set; } = true;
}
```

### C++ Configuration
```csharp
public class CppInteropConfiguration
{
    public string LibraryPath { get; set; } = "QuantEngine";
    public bool EnableCaching { get; set; } = true;
    public int CacheSize { get; set; } = 1000;
    public TimeSpan CacheTimeout { get; set; } = TimeSpan.FromMinutes(30);
    public bool EnableMemoryOptimization { get; set; } = true;
    public int MaxMemoryUsageMB { get; set; } = 1024;
}
```

## Usage Examples

### Basic Model Execution

```csharp
var request = new ModelExecutionRequest
{
    ModelName = "var_historical",
    Parameters = new Dictionary<string, object>
    {
        ["returns"] = returnsArray,
        ["confidence_level"] = 0.95
    },
    InputData = new Dictionary<string, object>
    {
        ["market_data"] = marketData
    }
};

var response = await interopService.ExecuteModelAsync(request);
```

### Python Function Call

```csharp
var result = await interopService.CallPythonFunctionAsync<double[]>(
    "numpy", "random.normal", 0.0, 1.0, 1000);
```

### Model Discovery

```csharp
var models = await interopService.GetAvailableModelsAsync();
var metadata = await interopService.GetModelMetadataAsync("var_historical");
```

### Performance Monitoring

```csharp
var metrics = await interopService.GetPerformanceMetricsAsync();
var health = await interopService.GetHealthAsync();
```

## Python Models

### Quant Models Module

**File**: `python_models/quant_models.py`

Centralized Python module containing all quantitative models.

**Features**:
- Model registry and discovery
- Unified execution interface
- Error handling and validation
- Performance monitoring
- Metadata management

**Supported Models**:

#### Risk Management
- `var_historical` - Historical Value at Risk
- `var_parametric` - Parametric Value at Risk
- `var_monte_carlo` - Monte Carlo Value at Risk
- `cvar_calculation` - Conditional Value at Risk
- `stress_test` - Portfolio stress testing

#### Portfolio Optimization
- `markowitz_optimization` - Markowitz mean-variance optimization
- `black_litterman` - Black-Litterman optimization
- `risk_parity` - Risk parity optimization
- `efficient_frontier` - Efficient frontier calculation

#### Statistical Models
- `garch_model` - GARCH volatility modeling
- `copula_model` - Copula dependence modeling
- `regime_switching` - Regime switching models

#### Pricing Models
- `black_scholes` - Black-Scholes option pricing
- `monte_carlo_pricing` - Monte Carlo option pricing
- `binomial_tree` - Binomial tree option pricing

### Model Execution

```python
# Direct Python usage
from quant_models import execute_model

request = {
    'model_name': 'var_historical',
    'parameters': {'confidence_level': 0.95},
    'input_data': {'returns': returns_array}
}

result = execute_model(request)
```

## C++ Implementation

### QuantEngine Library

**Files**: `QuantEngine.h`, `QuantEngine.cpp`

High-performance C++ implementation of quantitative models.

**Features**:
- P/Invoke compatible C interface
- Object-oriented C++ classes
- Memory management
- Error handling
- Performance optimization

**Supported Functions**:

#### Risk Management
```cpp
double CalculateVaRHistorical(const double* returns, int length, double confidenceLevel);
double CalculateVaRParametric(double mean, double std, double confidenceLevel);
double CalculateCVaR(const double* returns, int length, double confidenceLevel);
void CalculateVaRMonteCarlo(const double* returns, int length, double confidenceLevel, 
                           int numSimulations, double* result);
```

#### Portfolio Optimization
```cpp
void OptimizeMarkowitz(const double* expectedReturns, const double* covarianceMatrix, 
                      int numAssets, double riskAversion, double* optimalWeights);
void CalculateEfficientFrontier(const double* expectedReturns, const double* covarianceMatrix,
                               int numAssets, int numPoints, double* frontierPoints);
void OptimizeRiskParity(const double* covarianceMatrix, int numAssets, double* optimalWeights);
```

#### Option Pricing
```cpp
double BlackScholes(double spot, double strike, double timeToMaturity, 
                   double riskFreeRate, double volatility, int optionType);
void MonteCarloPricing(double spot, double strike, double timeToMaturity,
                      double riskFreeRate, double volatility, int optionType,
                      int numSimulations, double* result);
double BinomialTree(double spot, double strike, double timeToMaturity,
                   double riskFreeRate, double volatility, int optionType, int nSteps);
```

### C++ Classes

```cpp
// Portfolio optimization
class MarkowitzOptimizer : public PortfolioOptimizer {
    std::vector<double> optimize(const std::vector<double>& expectedReturns,
                               const std::vector<std::vector<double>>& covarianceMatrix,
                               double riskAversion) override;
};

// VaR calculation
class MonteCarloVaRCalculator : public VaRCalculator {
    double calculate(const std::vector<double>& returns, double confidenceLevel) override;
};

// Option pricing
class BlackScholesPricer : public OptionPricer {
    double price(double spot, double strike, double timeToMaturity,
                double riskFreeRate, double volatility, bool isCall) override;
};
```

## Building and Deployment

### Python Dependencies

```bash
pip install numpy scipy pandas scikit-learn statsmodels
```

### C++ Build

```bash
mkdir build
cd build
cmake ..
make
```

### .NET Dependencies

```xml
<PackageReference Include="Python.Runtime" Version="3.0.3" />
<PackageReference Include="Grpc.Net.Client" Version="2.57.0" />
<PackageReference Include="Grpc.Tools" Version="2.60.0" />
```

## Performance Characteristics

### Python.NET
- **Startup Time**: ~2-3 seconds
- **Memory Usage**: ~100-200 MB base
- **Execution Speed**: Good for complex models
- **Best For**: Research, prototyping, complex statistical models

### gRPC
- **Startup Time**: ~1 second
- **Memory Usage**: ~50-100 MB
- **Execution Speed**: Excellent for repeated calls
- **Best For**: Production, high-throughput scenarios

### C++/CLI
- **Startup Time**: ~100ms
- **Memory Usage**: ~10-50 MB
- **Execution Speed**: Maximum performance
- **Best For**: Performance-critical operations, real-time systems

## Error Handling

### Service-Level Errors
- Service unavailable
- Timeout errors
- Memory exhaustion
- Invalid parameters

### Model-Level Errors
- Convergence failures
- Invalid input data
- Numerical instability
- Resource constraints

### Fallback Mechanisms
1. Try Python.NET first
2. Fall back to gRPC if available
3. Use C++ for basic operations
4. Return error if all fail

## Monitoring and Debugging

### Health Checks
```csharp
var health = await interopService.GetHealthAsync();
if (!health.IsHealthy)
{
    // Handle service issues
}
```

### Performance Metrics
```csharp
var metrics = await interopService.GetPerformanceMetricsAsync();
Console.WriteLine($"Success Rate: {metrics.SuccessRate:P2}");
Console.WriteLine($"Average Execution Time: {metrics.AverageExecutionTime}");
Console.WriteLine($"Memory Usage: {metrics.TotalMemoryUsageMB} MB");
```

### Logging
```csharp
// Configure logging in appsettings.json
{
  "Logging": {
    "LogLevel": {
      "FinancialRisk.Api.Services.PythonInteropService": "Debug",
      "FinancialRisk.Api.Services.GrpcPythonService": "Debug",
      "FinancialRisk.Api.Services.CppInteropService": "Debug"
    }
  }
}
```

## Best Practices

### 1. Model Selection
- Use C++ for performance-critical operations
- Use Python for complex statistical models
- Use gRPC for high-throughput scenarios

### 2. Error Handling
- Always check service health
- Implement proper fallback mechanisms
- Log errors for debugging

### 3. Performance Optimization
- Cache frequently used models
- Use appropriate service for each operation
- Monitor memory usage

### 4. Security
- Validate all input parameters
- Sanitize data before passing to Python
- Use secure communication for gRPC

## Troubleshooting

### Common Issues

1. **Python.NET Not Working**
   - Check Python installation
   - Verify Python.NET package
   - Check Python path configuration

2. **gRPC Connection Failed**
   - Verify gRPC server is running
   - Check network connectivity
   - Verify port configuration

3. **C++ Library Not Found**
   - Check library path
   - Verify platform-specific library
   - Check P/Invoke declarations

4. **Memory Issues**
   - Monitor memory usage
   - Implement proper disposal
   - Use memory-efficient models

### Debug Mode
```csharp
// Enable detailed logging
services.Configure<InteropConfiguration>(options =>
{
    options.EnableMetricsAggregation = true;
    options.ServiceTimeout = TimeSpan.FromMinutes(10);
});
```

## Future Enhancements

### Planned Features
- **Model Versioning**: Support for multiple model versions
- **A/B Testing**: Compare different model implementations
- **Auto-scaling**: Dynamic service scaling based on load
- **Model Caching**: Intelligent model result caching
- **Streaming**: Real-time model execution streaming

### Research Areas
- **GPU Acceleration**: CUDA/OpenCL integration
- **Distributed Computing**: Multi-node model execution
- **Model Compression**: Optimize model size and speed
- **AutoML Integration**: Automated model selection

---

**Note**: This interop system provides a robust foundation for quantitative model integration. Choose the appropriate method based on your specific requirements for performance, flexibility, and complexity.

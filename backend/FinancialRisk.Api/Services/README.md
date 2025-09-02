# Risk Calculations C++ Library

This directory contains the C++ implementation of quantitative risk calculations for the Financial Risk Insights Platform.

## Overview

The C++ library provides high-performance implementations of key financial risk metrics:

- **Volatility**: Daily volatility (annualized)
- **Beta**: Beta coefficient vs benchmark
- **Sharpe Ratio**: Risk-adjusted return metric
- **Sortino Ratio**: Downside risk-adjusted return metric
- **Value at Risk (VaR)**: Historical simulation VaR
- **Expected Shortfall**: Conditional VaR
- **Maximum Drawdown**: Maximum peak-to-trough decline
- **Information Ratio**: Active return vs tracking error

## Architecture

```
┌─────────────────┐    ┌──────────────────┐    ┌─────────────────┐
│   C# Backend    │    │   C++ Library    │    │   Database      │
│                 │    │                  │    │                 │
│ RiskMetrics     │◄──►│ RiskCalculations │    │ Historical      │
│ Service         │    │ (DLL/SO/DYLIB)   │    │ Price Data      │
│                 │    │                  │    │                 │
└─────────────────┘    └──────────────────┘    └─────────────────┘
```

## Files

- `RiskCalculations.cpp` - C++ implementation of risk calculations
- `RiskCalculations.h` - C++ header file with function declarations
- `CMakeLists.txt` - CMake build configuration
- `build-cpp.sh` - Build script for different platforms
- `test_risk_calculations.cpp` - C++ unit tests
- `README.md` - This documentation

## Building the C++ Library

### Prerequisites

- CMake 3.16 or higher
- C++17 compatible compiler (GCC, Clang, MSVC)
- Platform-specific build tools

### Build Commands

```bash
# Make build script executable
chmod +x build-cpp.sh

# Build the library
./build-cpp.sh
```

### Manual Build (Alternative)

```bash
# Create build directory
mkdir build
cd build

# Configure with CMake
cmake .. -DCMAKE_BUILD_TYPE=Release

# Build the library
cmake --build . --config Release
```

## Platform Support

| Platform | Library Extension | Compiler | Notes |
|----------|------------------|----------|-------|
| Windows  | `.dll`           | MSVC     | Visual Studio 2019+ |
| macOS    | `.dylib`         | Clang    | Xcode 12+ |
| Linux    | `.so`            | GCC      | GCC 9+ |

## C# Integration

The C# backend uses P/Invoke to call C++ functions:

```csharp
[DllImport("RiskCalculations.dll", CallingConvention = CallingConvention.Cdecl)]
private static extern double CalculateVolatility(double[] returns, int length);
```

## API Endpoints

### Single Asset Risk Metrics
```
GET /api/riskmetrics/asset/{symbol}?days=252
```

### Multiple Asset Risk Metrics
```
POST /api/riskmetrics/assets/batch
Content-Type: application/json
["AAPL", "MSFT", "GOOGL"]
```

### Portfolio Risk Metrics
```
POST /api/riskmetrics/portfolio
Content-Type: application/json
{
  "symbols": ["AAPL", "MSFT"],
  "weights": [0.6, 0.4],
  "days": 252
}
```

### Risk Metrics Comparison
```
GET /api/riskmetrics/compare?symbols=AAPL&symbols=MSFT&days=252
```

## Testing

### C++ Unit Tests
```bash
# Build and run C++ tests
cd build
make test_risk_calculations
./test_risk_calculations
```

### C# Unit Tests
```bash
# Run C# unit tests
dotnet test --filter "FullyQualifiedName~RiskMetricsServiceTests"
```

### Integration Tests
```bash
# Run integration tests
dotnet test --filter "FullyQualifiedName~RiskMetricsApiIntegrationTests"
```

## Performance Characteristics

| Metric | Data Points | Execution Time | Memory Usage |
|--------|-------------|----------------|--------------|
| Volatility | 10,000 | < 1ms | < 1MB |
| Sharpe Ratio | 10,000 | < 1ms | < 1MB |
| VaR (95%) | 10,000 | < 2ms | < 1MB |
| Portfolio Metrics | 50 assets | < 10ms | < 5MB |

## Mathematical Formulas

### Volatility (Annualized)
```
σ = √(Σ(ri - μ)² / (n-1)) × √252
```

### Sharpe Ratio
```
SR = (μ - rf) / σ
```

### Sortino Ratio
```
Sortino = (μ - rf) / σd
```

### Value at Risk (Historical)
```
VaR(α) = -Percentile(returns, 1-α)
```

### Expected Shortfall
```
ES(α) = -E[returns | returns ≤ VaR(α)]
```

## Error Handling

The C++ library includes comprehensive error handling:

- **Input Validation**: Checks for null pointers and invalid array lengths
- **Mathematical Safety**: Handles division by zero and invalid calculations
- **Memory Safety**: Bounds checking and safe array access
- **Return Values**: Meaningful error codes and NaN/Inf detection

## Future Enhancements

- [ ] Monte Carlo VaR simulation
- [ ] GARCH volatility modeling
- [ ] Copula-based portfolio risk
- [ ] Real-time risk monitoring
- [ ] GPU acceleration (CUDA/OpenCL)
- [ ] Machine learning risk models

## Contributing

1. Follow C++17 standards
2. Maintain backward compatibility
3. Add comprehensive tests
4. Update documentation
5. Ensure cross-platform compatibility

## License

This project is part of the Financial Risk Insights Platform and follows the same licensing terms.

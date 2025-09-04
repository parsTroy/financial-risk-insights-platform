# Monte Carlo Engine for Financial Risk Analysis

## Overview

The Monte Carlo Engine is a comprehensive risk analysis tool that provides advanced Monte Carlo simulation capabilities for Value at Risk (VaR) and Conditional Value at Risk (CVaR) calculations. It supports both single asset and portfolio risk analysis with multiple distribution types and advanced statistical methods.

## Architecture

The Monte Carlo Engine implements a hybrid architecture combining:

- **C++ Engine**: High-performance core for large-scale simulations
- **Python Engine**: Flexible implementation for complex distributions and research
- **C# Integration**: Seamless API integration with existing VaR service

## Features

### Core Capabilities
- Single asset VaR and CVaR calculations
- Portfolio VaR and CVaR calculations with correlation modeling
- Stress testing and scenario analysis
- Multiple distribution types (Normal, Student's t, GARCH, Copula, Mixture)
- Advanced variance reduction techniques
- Risk attribution and contribution analysis

### Distribution Types
1. **Normal Distribution**: Standard normal distribution for returns
2. **Student's t-Distribution**: Heavy-tailed distribution for fat tails
3. **GARCH Process**: Time-varying volatility model
4. **Copula-based**: Dependency structure modeling
5. **Mixture Distribution**: Mixture of multiple distributions

### Advanced Features
- Antithetic variates for variance reduction
- Control variates for improved accuracy
- Quasi-Monte Carlo methods
- Bootstrap confidence intervals
- Risk attribution analysis
- Diversification ratio calculation

## File Structure

```
Services/
├── MonteCarloEngine.h              # C++ header file
├── MonteCarloEngine.cpp            # C++ implementation
├── monte_carlo_engine.py           # Python implementation
├── monte_carlo_var.py              # Legacy Python interface
├── CMakeLists.txt                  # CMake build configuration
├── build-monte-carlo.sh            # Build script
├── test_monte_carlo.sh             # Test script
└── MONTE_CARLO_README.md           # This file
```

## Installation and Setup

### Prerequisites
- C++17 compatible compiler (GCC, Clang, MSVC)
- CMake 3.16 or higher
- Python 3.8 or higher
- Required Python packages: numpy, scipy, pandas

### Building the C++ Engine

1. **Build the Monte Carlo Engine**:
   ```bash
   cd backend/FinancialRisk.Api/Services
   ./build-monte-carlo.sh
   ```

2. **Verify the build**:
   ```bash
   ./test_monte_carlo.sh
   ```

### Python Dependencies

Install required Python packages:
```bash
pip install numpy scipy pandas
```

## Usage

### C# API Integration

The Monte Carlo engine is integrated into the existing VaR service and can be accessed through the standard VaR calculation endpoints:

```csharp
// Single asset Monte Carlo VaR
var request = new VaRCalculationRequest
{
    Symbol = "AAPL",
    CalculationType = "MonteCarlo",
    DistributionType = "Normal",
    ConfidenceLevels = new List<double> { 0.95, 0.99 },
    SimulationCount = 10000,
    TimeHorizon = 1.0
};

var result = await varService.CalculateVaRAsync(request);
```

### Portfolio Monte Carlo VaR

```csharp
// Portfolio Monte Carlo VaR
var portfolioRequest = new PortfolioVaRCalculationRequest
{
    PortfolioName = "My Portfolio",
    Symbols = new List<string> { "AAPL", "GOOGL", "MSFT" },
    Weights = new List<decimal> { 0.4m, 0.3m, 0.3m },
    CalculationType = "MonteCarlo",
    DistributionType = "Normal",
    SimulationCount = 10000
};

var portfolioResult = await varService.CalculatePortfolioVaRAsync(portfolioRequest);
```

### Direct Python Usage

```python
from monte_carlo_engine import run_monte_carlo_simulation, run_portfolio_monte_carlo_simulation

# Single asset simulation
result = run_monte_carlo_simulation(
    symbol="AAPL",
    returns_data=historical_returns,
    distribution_type="normal",
    num_simulations=10000
)

# Portfolio simulation
portfolio_data = {
    'assets': [
        {'symbol': 'AAPL', 'returns': aapl_returns},
        {'symbol': 'GOOGL', 'returns': googl_returns}
    ],
    'weights': [0.6, 0.4],
    'num_simulations': 10000,
    'distribution_type': 'normal'
}

portfolio_result = run_portfolio_monte_carlo_simulation(portfolio_data)
```

## API Endpoints

### Monte Carlo Controller

The Monte Carlo engine provides dedicated API endpoints:

- `POST /api/montecarlo/simulate` - Single asset simulation
- `POST /api/montecarlo/portfolio/simulate` - Portfolio simulation
- `POST /api/montecarlo/stress-test` - Stress testing
- `GET /api/montecarlo/compare/{symbol}` - Method comparison
- `GET /api/montecarlo/distributions` - Available distributions

### Example API Calls

**Single Asset Simulation**:
```json
POST /api/montecarlo/simulate
{
    "symbol": "AAPL",
    "distributionType": "Normal",
    "numSimulations": 10000,
    "confidenceLevels": [0.95, 0.99],
    "timeHorizon": 1
}
```

**Portfolio Simulation**:
```json
POST /api/montecarlo/portfolio/simulate
{
    "portfolioName": "Tech Portfolio",
    "symbols": ["AAPL", "GOOGL", "MSFT"],
    "weights": [0.4, 0.3, 0.3],
    "distributionType": "Normal",
    "numSimulations": 10000,
    "useCorrelation": true
}
```

**Stress Testing**:
```json
POST /api/montecarlo/stress-test
{
    "symbol": "AAPL",
    "scenarioName": "Volatility Shock",
    "scenarioType": "VolatilityShock",
    "stressFactor": 2.0,
    "distributionType": "Normal",
    "numSimulations": 10000
}
```

## Performance Characteristics

### C++ Engine
- **Speed**: 10-100x faster than Python for large simulations
- **Memory**: Efficient memory usage with object pooling
- **Scalability**: Handles millions of simulations efficiently
- **Platforms**: Windows, macOS, Linux

### Python Engine
- **Flexibility**: Easy to extend and modify
- **Libraries**: Rich ecosystem of statistical libraries
- **Research**: Ideal for research and experimentation
- **Integration**: Seamless integration with data science workflows

### Hybrid Approach
- **Fallback**: Automatic fallback from C++ to Python
- **Best of Both**: Performance of C++ with flexibility of Python
- **Reliability**: Multiple implementation paths for robustness

## Configuration

### Simulation Parameters

```csharp
public class SimulationParameters
{
    public int NumSimulations { get; set; } = 10000;
    public int TimeHorizon { get; set; } = 1;
    public List<double> ConfidenceLevels { get; set; } = new() { 0.95, 0.99 };
    public string DistributionType { get; set; } = "Normal";
    public bool UseAntitheticVariates { get; set; } = false;
    public bool UseControlVariates { get; set; } = false;
    public bool UseQuasiMonteCarlo { get; set; } = false;
    public int? Seed { get; set; }
    public Dictionary<string, object>? CustomParameters { get; set; }
}
```

### Distribution-Specific Parameters

**Normal Distribution**:
```json
{
    "mean": 0.001,
    "volatility": 0.02
}
```

**Student's t-Distribution**:
```json
{
    "degrees_of_freedom": 5.0,
    "location": 0.0,
    "scale": 1.0
}
```

**GARCH Process**:
```json
{
    "omega": 0.0001,
    "alpha": 0.1,
    "beta": 0.85
}
```

## Testing

### Unit Tests
```bash
cd backend/FinancialRisk.Tests
dotnet test --filter "MonteCarloEngineTests"
```

### Integration Tests
```bash
cd backend/FinancialRisk.Api/Services
./test_monte_carlo.sh
```

### Performance Tests
The test suite includes performance benchmarks for different simulation counts and distribution types.

## Error Handling

The Monte Carlo engine includes comprehensive error handling:

- **Input Validation**: Validates all input parameters
- **Data Validation**: Checks historical data quality
- **Fallback Mechanisms**: Automatic fallback between C++ and Python
- **Error Reporting**: Detailed error messages and logging
- **Graceful Degradation**: Continues operation even with partial failures

## Logging

The engine provides detailed logging at multiple levels:

- **Info**: Simulation progress and completion
- **Warning**: Fallback operations and performance issues
- **Error**: Calculation failures and data problems
- **Debug**: Detailed simulation parameters and intermediate results

## Future Enhancements

### Planned Features
- GPU acceleration for large-scale simulations
- Real-time risk monitoring
- Machine learning integration
- Advanced copula models
- Regime-switching models
- High-frequency data support

### Research Areas
- Quantum Monte Carlo methods
- Deep learning for distribution modeling
- Alternative risk measures
- Multi-asset correlation modeling

## Troubleshooting

### Common Issues

1. **C++ Library Not Found**:
   - Ensure the build script completed successfully
   - Check that the library is in the correct directory
   - Verify platform-specific library naming

2. **Python Dependencies Missing**:
   - Install required packages: `pip install numpy scipy pandas`
   - Check Python version compatibility

3. **Simulation Failures**:
   - Verify input data quality
   - Check parameter ranges
   - Review error logs for specific issues

4. **Performance Issues**:
   - Reduce simulation count for testing
   - Use C++ engine for large simulations
   - Check system resources

### Debug Mode

Enable debug logging to troubleshoot issues:

```csharp
// In appsettings.Development.json
{
  "Logging": {
    "LogLevel": {
      "FinancialRisk.Api.Services": "Debug"
    }
  }
}
```

## Contributing

### Development Setup
1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests for new functionality
5. Run the test suite
6. Submit a pull request

### Code Style
- Follow C# coding conventions
- Use meaningful variable names
- Add comprehensive documentation
- Include unit tests for new features

## License

This Monte Carlo engine is part of the Financial Risk Insights Platform and follows the same licensing terms.

## Support

For technical support or questions about the Monte Carlo engine:

1. Check the troubleshooting section
2. Review the test cases for usage examples
3. Examine the API documentation
4. Create an issue in the project repository

---

**Note**: This Monte Carlo engine is designed for educational and research purposes. For production use in financial applications, ensure compliance with relevant regulations and perform thorough validation of results.

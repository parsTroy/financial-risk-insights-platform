# Portfolio Optimization Engine

## Overview

The Portfolio Optimization Engine implements Markowitz mean-variance optimization theory to provide optimal portfolio allocation strategies. It supports multiple optimization methods, efficient frontier calculation, and advanced portfolio management features.

## Features

### Core Optimization Methods
- **Mean-Variance Optimization**: Classic Markowitz optimization with risk aversion parameter
- **Minimum Variance**: Portfolio with lowest possible risk
- **Maximum Sharpe Ratio**: Portfolio with highest risk-adjusted returns
- **Equal Weight**: Simple equal-weight allocation
- **Risk Parity**: Equal risk contribution from each asset
- **Black-Litterman**: Incorporates market views and uncertainty
- **Mean-CVaR**: Conditional Value at Risk optimization

### Advanced Features
- **Efficient Frontier**: Complete risk-return trade-off curve
- **Risk Budgeting**: Allocate risk rather than capital
- **Transaction Cost Optimization**: Minimize rebalancing costs
- **Constraint Management**: Flexible weight and sector constraints
- **Multiple Asset Classes**: Support for stocks, bonds, commodities, etc.

## Architecture

The portfolio optimization engine uses a hybrid approach:

- **Python Engine**: Advanced optimization algorithms using NumPy and SciPy
- **C# Integration**: Seamless API integration with existing services
- **RESTful API**: Clean HTTP endpoints for frontend integration

## API Endpoints

### Core Optimization
- `POST /api/portfolio/optimize` - Optimize portfolio weights
- `POST /api/portfolio/efficient-frontier` - Calculate efficient frontier
- `GET /api/portfolio/methods` - Get available optimization methods
- `GET /api/portfolio/constraints` - Get available constraints

### Advanced Optimization
- `POST /api/portfolio/risk-budgeting` - Risk budgeting optimization
- `POST /api/portfolio/black-litterman` - Black-Litterman optimization
- `POST /api/portfolio/transaction-costs` - Transaction cost optimization

### History and Analytics
- `GET /api/portfolio/history/{portfolioName}` - Get optimization history
- `GET /api/portfolio/result/{id}` - Get specific optimization result
- `GET /api/portfolio/stats` - Get optimization statistics

## Usage Examples

### Basic Portfolio Optimization

```csharp
var request = new PortfolioOptimizationRequest
{
    PortfolioName = "Tech Portfolio",
    Symbols = new List<string> { "AAPL", "GOOGL", "MSFT", "AMZN" },
    OptimizationMethod = "MeanVariance",
    RiskAversion = 1.0,
    MaxWeight = 0.4,
    MinWeight = 0.05,
    CalculateEfficientFrontier = true,
    EfficientFrontierPoints = 50
};

var result = await optimizationService.OptimizePortfolioAsync(request);
```

### API Request Example

```json
POST /api/portfolio/optimize
{
    "portfolioName": "Tech Portfolio",
    "symbols": ["AAPL", "GOOGL", "MSFT", "AMZN"],
    "optimizationMethod": "MeanVariance",
    "riskAversion": 1.0,
    "maxWeight": 0.4,
    "minWeight": 0.05,
    "calculateEfficientFrontier": true,
    "efficientFrontierPoints": 50
}
```

### Response Example

```json
{
    "success": true,
    "optimizationResult": {
        "portfolioName": "Tech Portfolio",
        "optimizationMethod": "MeanVariance",
        "optimalWeights": [0.25, 0.30, 0.25, 0.20],
        "expectedReturn": 0.135,
        "expectedVolatility": 0.18,
        "sharpeRatio": 0.75,
        "var": 0.08,
        "cvar": 0.12,
        "diversificationRatio": 1.15,
        "concentrationRatio": 0.25,
        "assetWeights": [
            {
                "symbol": "AAPL",
                "weight": 0.25,
                "expectedReturn": 0.12,
                "volatility": 0.25,
                "riskContribution": 0.22,
                "returnContribution": 0.03
            }
        ]
    },
    "efficientFrontier": {
        "success": true,
        "points": [
            {
                "expectedReturn": 0.10,
                "expectedVolatility": 0.15,
                "sharpeRatio": 0.67,
                "weights": [0.4, 0.2, 0.2, 0.2]
            }
        ],
        "minVolatilityPoint": { ... },
        "maxSharpePoint": { ... },
        "maxReturnPoint": { ... }
    }
}
```

## Optimization Methods

### 1. Mean-Variance Optimization
The classic Markowitz approach that maximizes expected return minus risk aversion times variance.

**Parameters:**
- `riskAversion`: Risk tolerance (higher = more risk averse)
- `targetReturn`: Optional target return constraint
- `targetVolatility`: Optional target volatility constraint

### 2. Minimum Variance
Finds the portfolio with the lowest possible risk (volatility).

**Use Case:** Conservative investors seeking capital preservation

### 3. Maximum Sharpe Ratio
Maximizes the Sharpe ratio (return per unit of risk).

**Use Case:** Risk-adjusted return optimization

### 4. Equal Weight
Simple equal allocation across all assets.

**Use Case:** Naive diversification baseline

### 5. Risk Parity
Equal risk contribution from each asset.

**Use Case:** Risk-balanced portfolios

### 6. Black-Litterman
Incorporates market views and uncertainty into optimization.

**Parameters:**
- `marketCapWeights`: Market capitalization weights
- `views`: Investor views on asset returns
- `pickMatrix`: Matrix linking views to assets
- `viewUncertainties`: Uncertainty in views
- `tau`: Scaling factor for market uncertainty

### 7. Mean-CVaR
Optimizes using Conditional Value at Risk instead of variance.

**Use Case:** Tail risk management

## Constraints

### Weight Constraints
- `maxWeight`: Maximum weight per asset (default: 1.0)
- `minWeight`: Minimum weight per asset (default: 0.0)
- `maxLeverage`: Maximum total leverage (default: 1.0)

### Sector Constraints
```json
{
    "sectorLimits": {
        "Technology": 0.4,
        "Healthcare": 0.3,
        "Financial": 0.2
    }
}
```

### Concentration Constraints
```json
{
    "maxConcentration": 0.3,
    "maxSingleAsset": 0.15
}
```

## Risk Metrics

### Portfolio-Level Metrics
- **Expected Return**: Mean expected return
- **Expected Volatility**: Standard deviation of returns
- **Sharpe Ratio**: Risk-adjusted return metric
- **VaR**: Value at Risk (95% and 99% confidence levels)
- **CVaR**: Conditional Value at Risk
- **Diversification Ratio**: Ratio of weighted average volatility to portfolio volatility
- **Concentration Ratio**: Herfindahl index of weights

### Asset-Level Metrics
- **Risk Contribution**: Asset's contribution to portfolio risk
- **Return Contribution**: Asset's contribution to portfolio return
- **Marginal VaR**: Change in VaR from small change in asset weight
- **Component VaR**: Asset's contribution to portfolio VaR

## Efficient Frontier

The efficient frontier shows the optimal risk-return trade-off for a given set of assets.

### Key Points
- **Minimum Volatility Point**: Lowest risk portfolio
- **Maximum Sharpe Point**: Highest risk-adjusted return
- **Maximum Return Point**: Highest expected return

### Usage
```csharp
var frontier = await optimizationService.CalculateEfficientFrontierAsync(request);
foreach (var point in frontier.Points)
{
    Console.WriteLine($"Return: {point.ExpectedReturn:F3}, Risk: {point.ExpectedVolatility:F3}, Sharpe: {point.SharpeRatio:F3}");
}
```

## Advanced Features

### Risk Budgeting
Allocate risk rather than capital across assets.

```csharp
var request = new RiskBudgetingRequest
{
    PortfolioName = "Risk Budget Portfolio",
    Symbols = new List<string> { "AAPL", "GOOGL", "MSFT" },
    RiskBudgets = new List<double> { 0.4, 0.3, 0.3 } // 40%, 30%, 30% risk allocation
};
```

### Transaction Cost Optimization
Minimize rebalancing costs while maintaining target allocation.

```csharp
var request = new TransactionCostOptimizationRequest
{
    PortfolioName = "Cost Optimized Portfolio",
    Symbols = new List<string> { "AAPL", "GOOGL" },
    CurrentWeights = new List<double> { 0.6, 0.4 },
    TargetWeights = new List<double> { 0.5, 0.5 },
    TransactionCosts = new List<double> { 0.001, 0.001 }, // 0.1% per asset
    MaxTurnover = 0.2 // Maximum 20% turnover
};
```

## Performance Characteristics

### Python Engine
- **Flexibility**: Easy to extend and modify
- **Libraries**: Rich ecosystem of optimization libraries
- **Research**: Ideal for research and experimentation
- **Speed**: Good performance for moderate portfolio sizes

### Optimization Algorithms
- **SLSQP**: Sequential Least Squares Programming (default)
- **COBYLA**: Constrained Optimization BY Linear Approximation
- **Trust-Region**: Trust region methods for large problems

### Computational Complexity
- **Mean-Variance**: O(n³) where n is number of assets
- **Efficient Frontier**: O(m × n³) where m is number of frontier points
- **Risk Parity**: O(n⁴) for iterative optimization

## Error Handling

The optimization engine includes comprehensive error handling:

- **Input Validation**: Validates all input parameters
- **Data Validation**: Checks historical data quality
- **Convergence Issues**: Handles optimization failures gracefully
- **Constraint Violations**: Reports infeasible constraints
- **Numerical Issues**: Handles singular matrices and numerical instability

## Testing

### Unit Tests
```bash
dotnet test --filter "PortfolioOptimizationTests"
```

### Integration Tests
```bash
# Test Python engine directly
python3 portfolio_optimizer.py optimize '{"method": "mean_variance", "assets": [...]}'

# Test efficient frontier
python3 portfolio_optimizer.py frontier '{"method": "mean_variance", "assets": [...]}' 20
```

### Performance Tests
The test suite includes performance benchmarks for:
- Different portfolio sizes (2-100 assets)
- Various optimization methods
- Efficient frontier calculation times
- Memory usage patterns

## Configuration

### Python Dependencies
```bash
pip install numpy scipy pandas
```

### C# Configuration
```csharp
// In Program.cs
builder.Services.AddScoped<IPortfolioOptimizationService, PortfolioOptimizationService>();
```

### Environment Variables
```bash
# Optional: Python path for optimization engine
PYTHON_PATH=/usr/bin/python3

# Optional: Optimization timeout (seconds)
OPTIMIZATION_TIMEOUT=30
```

## Best Practices

### 1. Data Quality
- Use at least 252 days of historical data
- Handle missing data appropriately
- Consider data frequency (daily vs. monthly)

### 2. Parameter Selection
- Choose risk aversion based on investor profile
- Set realistic weight constraints
- Consider transaction costs in real implementations

### 3. Regular Rebalancing
- Rebalance monthly or quarterly
- Monitor drift from target weights
- Consider transaction costs vs. drift costs

### 4. Model Validation
- Backtest optimization results
- Compare with benchmark strategies
- Monitor out-of-sample performance

## Limitations

### Model Assumptions
- Returns are normally distributed
- Historical data predicts future performance
- No transaction costs (unless explicitly modeled)
- Perfect liquidity (no market impact)

### Computational Limits
- Large portfolios (>100 assets) may be slow
- Complex constraints may not converge
- Efficient frontier calculation scales with number of points

### Market Reality
- Models don't account for market regime changes
- Correlation estimates may be unstable
- Expected returns are notoriously difficult to estimate

## Future Enhancements

### Planned Features
- **Multi-Period Optimization**: Dynamic rebalancing strategies
- **Regime Detection**: Adapt to changing market conditions
- **Alternative Risk Measures**: Downside deviation, maximum drawdown
- **Factor Models**: Fama-French, Barra factor models
- **Machine Learning**: ML-based return forecasting

### Research Areas
- **Robust Optimization**: Handle parameter uncertainty
- **Stochastic Programming**: Multi-stage optimization
- **Real Options**: Incorporate optionality in portfolio decisions
- **ESG Integration**: Environmental, Social, Governance factors

## Support and Troubleshooting

### Common Issues

1. **Optimization Fails to Converge**
   - Check constraint feasibility
   - Reduce number of assets
   - Try different optimization method

2. **Unrealistic Weights**
   - Verify input data quality
   - Check constraint parameters
   - Review correlation matrix

3. **Slow Performance**
   - Reduce efficient frontier points
   - Use simpler optimization method
   - Consider C++ implementation for large portfolios

### Debug Mode
Enable detailed logging:
```csharp
// In appsettings.Development.json
{
  "Logging": {
    "LogLevel": {
      "FinancialRisk.Api.Services.PortfolioOptimizationService": "Debug"
    }
  }
}
```

## Contributing

### Development Setup
1. Fork the repository
2. Create a feature branch
3. Add tests for new functionality
4. Run the test suite
5. Submit a pull request

### Code Style
- Follow C# coding conventions
- Use meaningful variable names
- Add comprehensive documentation
- Include unit tests for new features

---

**Note**: This portfolio optimization engine is designed for educational and research purposes. For production use in financial applications, ensure compliance with relevant regulations and perform thorough validation of results.

# Unit Tests for Risk and Optimization Models

This document provides comprehensive documentation for the unit test suite implemented for Kanban issue #22: "Write unit tests for risk/optimization models".

## Overview

The test suite provides **65 comprehensive regression tests** with known datasets to validate VaR, Sharpe ratio, and optimization outputs across all quantitative models in the financial risk insights platform.

## Test Categories

### 1. Risk Model Tests (`RiskModelTests.cs`)
**11 tests** covering Value at Risk (VaR) and Conditional Value at Risk (CVaR) calculations

#### Key Test Cases:
- **VaR Historical**: Tests with known datasets and expected VaR at 95% confidence
- **VaR Parametric**: Validates normal distribution-based VaR calculations
- **VaR Monte Carlo**: Tests Monte Carlo simulation consistency
- **CVaR Calculations**: Validates Conditional Value at Risk computations
- **Edge Cases**: Zero returns, single returns, empty datasets
- **Error Handling**: Invalid confidence levels, missing data
- **Performance**: Large dataset stress testing

#### Known Datasets Used:
```csharp
// Historical VaR test dataset
var returns = new List<double> {
    -0.05, -0.03, -0.02, -0.01, 0.00, 0.01, 0.02, 0.03, 0.04, 0.05,
    -0.04, -0.02, -0.01, 0.00, 0.01, 0.02, 0.03, 0.04, 0.05, 0.06,
    -0.06, -0.04, -0.02, -0.01, 0.00, 0.01, 0.02, 0.03, 0.04, 0.05
};
// Expected VaR at 95% confidence: ~-0.04 (4th worst return)
```

### 2. Portfolio Optimization Tests (`PortfolioOptimizationTests.cs`)
**11 tests** covering portfolio optimization algorithms and Sharpe ratio validation

#### Key Test Cases:
- **Markowitz Optimization**: Mean-variance portfolio optimization with known datasets
- **Sharpe Ratio Validation**: Tests with known parameters (return=12%, risk-free=3%, volatility=15%)
- **Efficient Frontier**: Validates frontier point calculations and ordering
- **Minimum Variance**: Tests minimum variance portfolio optimization
- **Risk Parity**: Validates equal risk contribution optimization
- **Black-Litterman**: Tests Black-Litterman optimization with market views
- **Equal Weight**: Validates equal weight portfolio construction
- **Error Handling**: Invalid inputs, mismatched dimensions, negative returns
- **Performance**: Large dataset optimization testing

#### Known Datasets Used:
```csharp
// Portfolio optimization test dataset
var expectedReturns = new List<double> { 0.08, 0.12, 0.06 }; // 8%, 12%, 6%
var covarianceMatrix = new List<List<double>> {
    new List<double> { 0.04, 0.01, 0.02 }, // 4% variance, 1% cov with asset 2, 2% cov with asset 3
    new List<double> { 0.01, 0.09, 0.03 }, // 1% cov with asset 1, 9% variance, 3% cov with asset 3
    new List<double> { 0.02, 0.03, 0.06 }  // 2% cov with asset 1, 3% cov with asset 2, 6% variance
};
```

### 3. Monte Carlo Simulation Tests (`MonteCarloSimulationTests.cs`)
**16 tests** covering various distribution types and simulation parameters

#### Key Test Cases:
- **Distribution Types**: Normal, T-Student, GARCH, Copula, Skewed-T, Mixture
- **Confidence Levels**: Tests with 90%, 95%, 99% confidence levels
- **Time Horizons**: Validates scaling with 1, 5, 10, 30 day horizons
- **Simulation Counts**: Tests convergence with 1K, 5K, 10K, 50K simulations
- **Edge Cases**: Zero volatility, negative mean returns, high volatility
- **Reproducibility**: Tests with fixed random seeds
- **Performance**: Large simulation stress testing

#### Known Parameters Used:
```csharp
// Monte Carlo test parameters
var mean = 0.001;      // 0.1% daily return
var volatility = 0.02;  // 2% daily volatility
var confidenceLevel = 0.95;
var numSimulations = 10000;
// Expected VaR = mean - 1.645 * volatility = 0.001 - 1.645 * 0.02 = -0.0319
```

### 4. Statistical Model Tests (`StatisticalModelTests.cs`)
**11 tests** covering GARCH, Copula, and Regime Switching models

#### Key Test Cases:
- **GARCH Models**: Volatility forecasting with different parameters
- **Copula Models**: Dependence modeling with Gaussian, T, Clayton copulas
- **Regime Switching**: Multi-regime identification and probability estimation
- **Parameter Sensitivity**: Tests with different model parameters
- **Error Handling**: Empty data, single assets, invalid parameters
- **Performance**: Large dataset processing

#### Test Data Generation:
```csharp
// GARCH data generation
var returns = GenerateGARCHData(1000, 0.0001, 0.1, 0.85, 0.0001);
// Parameters: omega=0.0001, alpha=0.1, beta=0.85, initial variance=0.0001

// Correlated data generation
var data = GenerateCorrelatedData(1000, 3, 0.3);
// 1000 observations, 3 assets, 0.3 correlation
```

### 5. Pricing Model Tests (`PricingModelTests.cs`)
**16 tests** covering Black-Scholes, Monte Carlo, and Binomial Tree option pricing

#### Key Test Cases:
- **Black-Scholes**: Call and put options with known parameters
- **Monte Carlo Pricing**: Convergence testing with different simulation counts
- **Binomial Tree**: Convergence testing with different step counts
- **Put-Call Parity**: Validation of put-call parity relationship
- **Intrinsic Values**: Tests with zero volatility and zero time to maturity
- **Edge Cases**: Negative parameters, missing parameters
- **Performance**: Large simulation testing

#### Known Parameters Used:
```csharp
// Black-Scholes test parameters
var spotPrice = 100.0;
var strikePrice = 100.0;
var timeToMaturity = 0.25; // 3 months
var riskFreeRate = 0.05;   // 5% annual
var volatility = 0.2;      // 20% annual
// Expected call option price: ~2.13
```

## Test Execution

### Running Individual Test Categories
```bash
# Run specific test categories
dotnet test --filter "TestCategory=RiskModels"
dotnet test --filter "TestCategory=PortfolioOptimization"
dotnet test --filter "TestCategory=MonteCarlo"
dotnet test --filter "TestCategory=StatisticalModels"
dotnet test --filter "TestCategory=PricingModels"
```

### Running Complete Test Suite
```bash
# Run all tests
dotnet test FinancialRisk.Tests

# Run with detailed output
dotnet test FinancialRisk.Tests --verbosity normal

# Run specific test class
dotnet test --filter "ClassName=RiskModelTests"
```

### Using Test Runner
```csharp
// Run comprehensive test suite
var testRunner = new TestRunner();
await testRunner.RunCompleteTestSuite();

// Generate test report
testRunner.GenerateTestReport();
```

## Validation Criteria

### VaR Validation
- **Historical VaR**: Must match expected percentile of historical returns
- **Parametric VaR**: Must follow normal distribution formula: `VaR = mean - z_score * std`
- **Monte Carlo VaR**: Must converge to expected value with sufficient simulations
- **Confidence Levels**: VaR must increase with confidence level (more negative values)

### Sharpe Ratio Validation
- **Formula**: `Sharpe = (portfolio_return - risk_free_rate) / portfolio_volatility`
- **Known Values**: With return=12%, risk-free=3%, volatility=15%, Sharpe=0.6
- **Range**: Sharpe ratios should be reasonable (typically 0-2 for test datasets)

### Optimization Validation
- **Weight Constraints**: All weights must sum to 1.0 (±0.001 tolerance)
- **Non-negativity**: All weights must be non-negative
- **Constraint Compliance**: Must respect maximum/minimum weight constraints
- **Optimality**: Must improve objective function compared to equal weights

### Pricing Model Validation
- **Black-Scholes**: Must match analytical formula results
- **Put-Call Parity**: `C - P = S - K * exp(-r*T)`
- **Intrinsic Values**: Must equal intrinsic value with zero volatility/time
- **Convergence**: Monte Carlo and Binomial Tree must converge to Black-Scholes

## Performance Benchmarks

### Execution Time Limits
- **VaR Calculations**: < 5 seconds for 10,000 observations
- **Portfolio Optimization**: < 10 seconds for 50 assets
- **Monte Carlo Simulations**: < 30 seconds for 100,000 simulations
- **Statistical Models**: < 10 seconds for 10,000 observations

### Memory Usage
- **Large Datasets**: Must handle 10,000+ observations without memory issues
- **Concurrent Tests**: Must support parallel test execution
- **Resource Cleanup**: Must properly dispose of resources after tests

## Regression Testing

### Known Dataset Validation
All tests use **deterministic datasets** with **expected outcomes** to ensure:
- **Reproducibility**: Same inputs always produce same outputs
- **Accuracy**: Results match theoretical expectations
- **Consistency**: Models behave consistently across different scenarios

### Edge Case Coverage
- **Empty Data**: Proper error handling for missing inputs
- **Invalid Parameters**: Graceful failure for invalid configurations
- **Boundary Conditions**: Testing at limits of valid parameter ranges
- **Performance Limits**: Stress testing with large datasets

## Test Data Management

### Synthetic Data Generation
- **GARCH Data**: Generated using specified parameters (omega, alpha, beta)
- **Correlated Data**: Generated using correlation matrices
- **Regime Switching**: Generated with known regime probabilities
- **Option Data**: Generated using Black-Scholes parameters

### Reproducibility
- **Fixed Seeds**: All random number generators use fixed seeds (42)
- **Deterministic**: Same inputs always produce same outputs
- **Version Control**: Test data is versioned and documented

## Continuous Integration

### Automated Testing
- **Build Integration**: Tests run on every build
- **Failure Alerts**: Immediate notification of test failures
- **Performance Monitoring**: Track test execution times
- **Coverage Reporting**: Monitor test coverage metrics

### Quality Gates
- **All Tests Pass**: No test failures allowed in main branch
- **Performance Limits**: Tests must complete within time limits
- **Coverage Threshold**: Minimum 90% code coverage required

## Maintenance

### Adding New Tests
1. **Follow Naming Convention**: `MethodName_WithCondition_ReturnsExpectedResult`
2. **Use Known Datasets**: Provide deterministic test data
3. **Validate Expected Outcomes**: Include assertions for expected results
4. **Document Test Purpose**: Add comments explaining test rationale
5. **Update Documentation**: Add new tests to this README

### Updating Existing Tests
1. **Preserve Regression**: Don't change expected outcomes without justification
2. **Update Documentation**: Reflect changes in test documentation
3. **Version Control**: Track changes to test data and expectations
4. **Backward Compatibility**: Ensure changes don't break existing functionality

## Conclusion

This comprehensive test suite provides **robust validation** of all risk and optimization models with:
- **65 regression tests** covering all model types
- **Known datasets** with expected outcomes
- **Edge case coverage** for error handling
- **Performance validation** for large datasets
- **Reproducible results** with fixed seeds

The tests ensure **accuracy**, **reliability**, and **performance** of the financial risk insights platform's quantitative models.

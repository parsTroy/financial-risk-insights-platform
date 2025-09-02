#ifndef RISK_CALCULATIONS_H
#define RISK_CALCULATIONS_H

#ifdef __cplusplus
extern "C" {
#endif

// Calculate daily volatility (annualized)
double CalculateVolatility(double* returns, int length);

// Calculate beta vs benchmark
double CalculateBeta(double* assetReturns, double* benchmarkReturns, int length);

// Calculate Sharpe ratio
double CalculateSharpeRatio(double* returns, double riskFreeRate, int length);

// Calculate Sortino ratio (downside deviation)
double CalculateSortinoRatio(double* returns, double riskFreeRate, int length);

// Calculate Value at Risk (VaR) using historical simulation
double CalculateValueAtRisk(double* returns, double confidenceLevel, int length);

// Calculate Expected Shortfall (Conditional VaR)
double CalculateExpectedShortfall(double* returns, double confidenceLevel, int length);

// Calculate Maximum Drawdown
double CalculateMaximumDrawdown(double* returns, int length);

// Calculate Information Ratio
double CalculateInformationRatio(double* assetReturns, double* benchmarkReturns, int length);

#ifdef __cplusplus
}
#endif

#endif // RISK_CALCULATIONS_H

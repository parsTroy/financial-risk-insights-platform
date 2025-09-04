#ifndef VAR_CALCULATIONS_H
#define VAR_CALCULATIONS_H

#ifdef __cplusplus
extern "C" {
#endif

// Historical VaR using percentile method
double CalculateHistoricalVaR(double* returns, int length, double confidenceLevel);

// Historical CVaR (Expected Shortfall) using percentile method
double CalculateHistoricalCVaR(double* returns, int length, double confidenceLevel);

// Parametric VaR using normal distribution assumption
double CalculateParametricVaR(double* returns, int length, double confidenceLevel);

// Parametric CVaR using normal distribution assumption
double CalculateParametricCVaR(double* returns, int length, double confidenceLevel);

// Bootstrap VaR using resampling
double CalculateBootstrapVaR(double* returns, int length, double confidenceLevel, int bootstrapSamples);

// Calculate VaR confidence intervals using bootstrap
void CalculateVaRConfidenceIntervals(double* returns, int length, double confidenceLevel, 
                                   int bootstrapSamples, double* lowerBound, double* upperBound);

// Calculate portfolio VaR using historical simulation
double CalculatePortfolioHistoricalVaR(double* portfolioReturns, int length, double confidenceLevel);

// Calculate portfolio CVaR using historical simulation
double CalculatePortfolioHistoricalCVaR(double* portfolioReturns, int length, double confidenceLevel);

// Calculate VaR decomposition (contribution of each asset to portfolio VaR)
void CalculateVaRDecomposition(double* assetReturns, double* weights, int numAssets, int length, 
                              double confidenceLevel, double* contributions);

#ifdef __cplusplus
}
#endif

#endif // VAR_CALCULATIONS_H

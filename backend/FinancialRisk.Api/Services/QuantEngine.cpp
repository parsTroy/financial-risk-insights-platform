#include "QuantEngine.h"
#include <algorithm>
#include <numeric>
#include <cmath>
#include <random>
#include <iostream>
#include <memory>
#include <stdexcept>

// Global error tracking
static int lastError = 0;
static std::string lastErrorMessage = "";

// Utility functions
void setError(int errorCode, const std::string& message) {
    lastError = errorCode;
    lastErrorMessage = message;
}

// Risk Management Functions

extern "C" double CalculateVaRHistorical(const double* returns, int length, double confidenceLevel) {
    try {
        if (!returns || length <= 0 || confidenceLevel <= 0 || confidenceLevel >= 1) {
            setError(1, "Invalid parameters for VaR calculation");
            return 0.0;
        }
        
        std::vector<double> returnsVec(returns, returns + length);
        std::sort(returnsVec.begin(), returnsVec.end());
        
        int index = static_cast<int>((1 - confidenceLevel) * length);
        if (index >= length) index = length - 1;
        if (index < 0) index = 0;
        
        return -returnsVec[index];
    }
    catch (const std::exception& e) {
        setError(2, std::string("Exception in VaR calculation: ") + e.what());
        return 0.0;
    }
}

extern "C" double CalculateVaRParametric(double mean, double std, double confidenceLevel) {
    try {
        if (std <= 0 || confidenceLevel <= 0 || confidenceLevel >= 1) {
            setError(3, "Invalid parameters for parametric VaR");
            return 0.0;
        }
        
        // Z-score for given confidence level
        double zScore = std::sqrt(2) * std::erfc(2 * confidenceLevel - 1);
        return -(mean + zScore * std);
    }
    catch (const std::exception& e) {
        setError(4, std::string("Exception in parametric VaR: ") + e.what());
        return 0.0;
    }
}

extern "C" double CalculateCVaR(const double* returns, int length, double confidenceLevel) {
    try {
        if (!returns || length <= 0 || confidenceLevel <= 0 || confidenceLevel >= 1) {
            setError(5, "Invalid parameters for CVaR calculation");
            return 0.0;
        }
        
        double var = CalculateVaRHistorical(returns, length, confidenceLevel);
        
        std::vector<double> returnsVec(returns, returns + length);
        std::vector<double> tailReturns;
        
        for (double ret : returnsVec) {
            if (ret <= -var) {
                tailReturns.push_back(ret);
            }
        }
        
        if (tailReturns.empty()) {
            return var;
        }
        
        double sum = std::accumulate(tailReturns.begin(), tailReturns.end(), 0.0);
        return -sum / tailReturns.size();
    }
    catch (const std::exception& e) {
        setError(6, std::string("Exception in CVaR calculation: ") + e.what());
        return 0.0;
    }
}

extern "C" void CalculateVaRMonteCarlo(const double* returns, int length, double confidenceLevel, 
                                      int numSimulations, double* result) {
    try {
        if (!returns || length <= 0 || !result || numSimulations <= 0) {
            setError(7, "Invalid parameters for Monte Carlo VaR");
            return;
        }
        
        // Calculate mean and std from historical data
        double sum = std::accumulate(returns, returns + length, 0.0);
        double mean = sum / length;
        
        double variance = 0.0;
        for (int i = 0; i < length; ++i) {
            variance += (returns[i] - mean) * (returns[i] - mean);
        }
        double std = std::sqrt(variance / (length - 1));
        
        // Monte Carlo simulation
        std::random_device rd;
        std::mt19937 gen(rd());
        std::normal_distribution<> dist(mean, std);
        
        std::vector<double> simulatedReturns(numSimulations);
        for (int i = 0; i < numSimulations; ++i) {
            simulatedReturns[i] = dist(gen);
        }
        
        std::sort(simulatedReturns.begin(), simulatedReturns.end());
        
        int index = static_cast<int>((1 - confidenceLevel) * numSimulations);
        if (index >= numSimulations) index = numSimulations - 1;
        if (index < 0) index = 0;
        
        result[0] = -simulatedReturns[index]; // VaR
        result[1] = mean; // Mean return
        result[2] = std; // Standard deviation
    }
    catch (const std::exception& e) {
        setError(8, std::string("Exception in Monte Carlo VaR: ") + e.what());
    }
}

// Portfolio Optimization Functions

extern "C" void OptimizeMarkowitz(const double* expectedReturns, const double* covarianceMatrix, 
                                 int numAssets, double riskAversion, double* optimalWeights) {
    try {
        if (!expectedReturns || !covarianceMatrix || !optimalWeights || numAssets <= 0) {
            setError(9, "Invalid parameters for Markowitz optimization");
            return;
        }
        
        // Simplified implementation - in practice, use a proper optimization library
        // This is a basic equal-weight allocation as a placeholder
        
        double weight = 1.0 / numAssets;
        for (int i = 0; i < numAssets; ++i) {
            optimalWeights[i] = weight;
        }
    }
    catch (const std::exception& e) {
        setError(10, std::string("Exception in Markowitz optimization: ") + e.what());
    }
}

extern "C" void CalculateEfficientFrontier(const double* expectedReturns, const double* covarianceMatrix,
                                         int numAssets, int numPoints, double* frontierPoints) {
    try {
        if (!expectedReturns || !covarianceMatrix || !frontierPoints || numAssets <= 0 || numPoints <= 0) {
            setError(11, "Invalid parameters for efficient frontier");
            return;
        }
        
        // Simplified implementation - generate basic frontier points
        double minReturn = *std::min_element(expectedReturns, expectedReturns + numAssets);
        double maxReturn = *std::max_element(expectedReturns, expectedReturns + numAssets);
        
        for (int i = 0; i < numPoints; ++i) {
            double returnLevel = minReturn + (maxReturn - minReturn) * i / (numPoints - 1);
            double volatility = 0.1 + 0.1 * i / (numPoints - 1); // Simplified volatility
            
            frontierPoints[i * 2] = returnLevel;     // Expected return
            frontierPoints[i * 2 + 1] = volatility;  // Expected volatility
        }
    }
    catch (const std::exception& e) {
        setError(12, std::string("Exception in efficient frontier: ") + e.what());
    }
}

extern "C" void OptimizeRiskParity(const double* covarianceMatrix, int numAssets, double* optimalWeights) {
    try {
        if (!covarianceMatrix || !optimalWeights || numAssets <= 0) {
            setError(13, "Invalid parameters for risk parity optimization");
            return;
        }
        
        // Simplified risk parity - equal weights
        double weight = 1.0 / numAssets;
        for (int i = 0; i < numAssets; ++i) {
            optimalWeights[i] = weight;
        }
    }
    catch (const std::exception& e) {
        setError(14, std::string("Exception in risk parity optimization: ") + e.what());
    }
}

// Pricing Functions

extern "C" double BlackScholes(double spot, double strike, double timeToMaturity, 
                              double riskFreeRate, double volatility, int optionType) {
    try {
        if (spot <= 0 || strike <= 0 || timeToMaturity <= 0 || volatility <= 0) {
            setError(15, "Invalid parameters for Black-Scholes");
            return 0.0;
        }
        
        double d1 = (std::log(spot / strike) + (riskFreeRate + 0.5 * volatility * volatility) * timeToMaturity) 
                   / (volatility * std::sqrt(timeToMaturity));
        double d2 = d1 - volatility * std::sqrt(timeToMaturity);
        
        // Normal CDF approximation
        auto normalCDF = [](double x) {
            return 0.5 * (1 + std::erf(x / std::sqrt(2)));
        };
        
        double price;
        if (optionType == 1) { // Call option
            price = spot * normalCDF(d1) - strike * std::exp(-riskFreeRate * timeToMaturity) * normalCDF(d2);
        } else { // Put option
            price = strike * std::exp(-riskFreeRate * timeToMaturity) * normalCDF(-d2) - spot * normalCDF(-d1);
        }
        
        return price;
    }
    catch (const std::exception& e) {
        setError(16, std::string("Exception in Black-Scholes: ") + e.what());
        return 0.0;
    }
}

extern "C" void MonteCarloPricing(double spot, double strike, double timeToMaturity,
                                 double riskFreeRate, double volatility, int optionType,
                                 int numSimulations, double* result) {
    try {
        if (spot <= 0 || strike <= 0 || timeToMaturity <= 0 || volatility <= 0 || !result || numSimulations <= 0) {
            setError(17, "Invalid parameters for Monte Carlo pricing");
            return;
        }
        
        std::random_device rd;
        std::mt19937 gen(rd());
        std::normal_distribution<> dist(0.0, 1.0);
        
        double sumPayoffs = 0.0;
        double sumPayoffsSquared = 0.0;
        
        for (int i = 0; i < numSimulations; ++i) {
            double randomShock = dist(gen);
            double stockPrice = spot * std::exp((riskFreeRate - 0.5 * volatility * volatility) * timeToMaturity 
                                              + volatility * std::sqrt(timeToMaturity) * randomShock);
            
            double payoff;
            if (optionType == 1) { // Call option
                payoff = std::max(stockPrice - strike, 0.0);
            } else { // Put option
                payoff = std::max(strike - stockPrice, 0.0);
            }
            
            sumPayoffs += payoff;
            sumPayoffsSquared += payoff * payoff;
        }
        
        double optionPrice = std::exp(-riskFreeRate * timeToMaturity) * sumPayoffs / numSimulations;
        double variance = (sumPayoffsSquared / numSimulations) - (sumPayoffs / numSimulations) * (sumPayoffs / numSimulations);
        double standardError = std::sqrt(variance / numSimulations);
        
        result[0] = optionPrice;
        result[1] = standardError;
    }
    catch (const std::exception& e) {
        setError(18, std::string("Exception in Monte Carlo pricing: ") + e.what());
    }
}

extern "C" double BinomialTree(double spot, double strike, double timeToMaturity,
                              double riskFreeRate, double volatility, int optionType, int nSteps) {
    try {
        if (spot <= 0 || strike <= 0 || timeToMaturity <= 0 || volatility <= 0 || nSteps <= 0) {
            setError(19, "Invalid parameters for binomial tree");
            return 0.0;
        }
        
        double dt = timeToMaturity / nSteps;
        double u = std::exp(volatility * std::sqrt(dt));
        double d = 1.0 / u;
        double p = (std::exp(riskFreeRate * dt) - d) / (u - d);
        
        std::vector<double> optionValues(nSteps + 1);
        
        // Calculate option values at maturity
        for (int i = 0; i <= nSteps; ++i) {
            double stockPrice = spot * std::pow(u, nSteps - i) * std::pow(d, i);
            if (optionType == 1) { // Call option
                optionValues[i] = std::max(stockPrice - strike, 0.0);
            } else { // Put option
                optionValues[i] = std::max(strike - stockPrice, 0.0);
            }
        }
        
        // Backward induction
        for (int step = nSteps - 1; step >= 0; --step) {
            for (int i = 0; i <= step; ++i) {
                optionValues[i] = std::exp(-riskFreeRate * dt) * (p * optionValues[i] + (1 - p) * optionValues[i + 1]);
            }
        }
        
        return optionValues[0];
    }
    catch (const std::exception& e) {
        setError(20, std::string("Exception in binomial tree: ") + e.what());
        return 0.0;
    }
}

// Utility Functions

extern "C" double CalculateSharpeRatio(const double* returns, int length, double riskFreeRate) {
    try {
        if (!returns || length <= 0) {
            setError(21, "Invalid parameters for Sharpe ratio");
            return 0.0;
        }
        
        double sum = std::accumulate(returns, returns + length, 0.0);
        double mean = sum / length;
        
        double variance = 0.0;
        for (int i = 0; i < length; ++i) {
            variance += (returns[i] - mean) * (returns[i] - mean);
        }
        double std = std::sqrt(variance / (length - 1));
        
        if (std == 0) return 0.0;
        
        return (mean - riskFreeRate) / std;
    }
    catch (const std::exception& e) {
        setError(22, std::string("Exception in Sharpe ratio: ") + e.what());
        return 0.0;
    }
}

extern "C" void CalculateCorrelationMatrix(const double* data, int rows, int cols, double* correlationMatrix) {
    try {
        if (!data || !correlationMatrix || rows <= 0 || cols <= 0) {
            setError(23, "Invalid parameters for correlation matrix");
            return;
        }
        
        // Simplified correlation calculation
        for (int i = 0; i < cols; ++i) {
            for (int j = 0; j < cols; ++j) {
                if (i == j) {
                    correlationMatrix[i * cols + j] = 1.0;
                } else {
                    correlationMatrix[i * cols + j] = 0.3; // Simplified correlation
                }
            }
        }
    }
    catch (const std::exception& e) {
        setError(24, std::string("Exception in correlation matrix: ") + e.what());
    }
}

extern "C" void CalculateCovarianceMatrix(const double* data, int rows, int cols, double* covarianceMatrix) {
    try {
        if (!data || !covarianceMatrix || rows <= 0 || cols <= 0) {
            setError(25, "Invalid parameters for covariance matrix");
            return;
        }
        
        // Simplified covariance calculation
        for (int i = 0; i < cols; ++i) {
            for (int j = 0; j < cols; ++j) {
                if (i == j) {
                    covarianceMatrix[i * cols + j] = 0.04; // 20% volatility
                } else {
                    covarianceMatrix[i * cols + j] = 0.012; // Simplified covariance
                }
            }
        }
    }
    catch (const std::exception& e) {
        setError(26, std::string("Exception in covariance matrix: ") + e.what());
    }
}

extern "C" double CalculatePortfolioVolatility(const double* weights, const double* covarianceMatrix, int numAssets) {
    try {
        if (!weights || !covarianceMatrix || numAssets <= 0) {
            setError(27, "Invalid parameters for portfolio volatility");
            return 0.0;
        }
        
        double variance = 0.0;
        for (int i = 0; i < numAssets; ++i) {
            for (int j = 0; j < numAssets; ++j) {
                variance += weights[i] * weights[j] * covarianceMatrix[i * numAssets + j];
            }
        }
        
        return std::sqrt(variance);
    }
    catch (const std::exception& e) {
        setError(28, std::string("Exception in portfolio volatility: ") + e.what());
        return 0.0;
    }
}

extern "C" double CalculatePortfolioReturn(const double* weights, const double* expectedReturns, int numAssets) {
    try {
        if (!weights || !expectedReturns || numAssets <= 0) {
            setError(29, "Invalid parameters for portfolio return");
            return 0.0;
        }
        
        double portfolioReturn = 0.0;
        for (int i = 0; i < numAssets; ++i) {
            portfolioReturn += weights[i] * expectedReturns[i];
        }
        
        return portfolioReturn;
    }
    catch (const std::exception& e) {
        setError(30, std::string("Exception in portfolio return: ") + e.what());
        return 0.0;
    }
}

// Performance and Memory Management

extern "C" int GetMemoryUsage() {
    // Simplified memory usage - in practice, use platform-specific APIs
    return 1024; // MB
}

extern "C" void ClearCache() {
    // Clear any internal caches
}

extern "C" const char* GetVersion() {
    return "1.0.0";
}

extern "C" int GetLastError() {
    return lastError;
}

extern "C" const char* GetLastErrorMessage() {
    return lastErrorMessage.c_str();
}

// C++ Class Implementations

namespace QuantEngine {

std::vector<double> MarkowitzOptimizer::optimize(const std::vector<double>& expectedReturns,
                                                const std::vector<std::vector<double>>& covarianceMatrix,
                                                double riskAversion) {
    // Simplified implementation
    int numAssets = expectedReturns.size();
    std::vector<double> weights(numAssets, 1.0 / numAssets);
    return weights;
}

std::vector<double> RiskParityOptimizer::optimize(const std::vector<double>& expectedReturns,
                                                 const std::vector<std::vector<double>>& covarianceMatrix,
                                                 double riskAversion) {
    // Simplified implementation
    int numAssets = expectedReturns.size();
    std::vector<double> weights(numAssets, 1.0 / numAssets);
    return weights;
}

double HistoricalVaRCalculator::calculate(const std::vector<double>& returns, double confidenceLevel) {
    std::vector<double> sortedReturns = returns;
    std::sort(sortedReturns.begin(), sortedReturns.end());
    
    int index = static_cast<int>((1 - confidenceLevel) * returns.size());
    if (index >= returns.size()) index = returns.size() - 1;
    if (index < 0) index = 0;
    
    return -sortedReturns[index];
}

double ParametricVaRCalculator::calculate(const std::vector<double>& returns, double confidenceLevel) {
    double sum = std::accumulate(returns.begin(), returns.end(), 0.0);
    double mean = sum / returns.size();
    
    double variance = 0.0;
    for (double ret : returns) {
        variance += (ret - mean) * (ret - mean);
    }
    double std = std::sqrt(variance / (returns.size() - 1));
    
    double zScore = std::sqrt(2) * std::erfc(2 * confidenceLevel - 1);
    return -(mean + zScore * std);
}

double MonteCarloVaRCalculator::calculate(const std::vector<double>& returns, double confidenceLevel) {
    double sum = std::accumulate(returns.begin(), returns.end(), 0.0);
    double mean = sum / returns.size();
    
    double variance = 0.0;
    for (double ret : returns) {
        variance += (ret - mean) * (ret - mean);
    }
    double std = std::sqrt(variance / (returns.size() - 1));
    
    std::random_device rd;
    std::mt19937 gen(rd());
    std::normal_distribution<> dist(mean, std);
    
    std::vector<double> simulatedReturns(numSimulations);
    for (int i = 0; i < numSimulations; ++i) {
        simulatedReturns[i] = dist(gen);
    }
    
    std::sort(simulatedReturns.begin(), simulatedReturns.end());
    
    int index = static_cast<int>((1 - confidenceLevel) * numSimulations);
    if (index >= numSimulations) index = numSimulations - 1;
    if (index < 0) index = 0;
    
    return -simulatedReturns[index];
}

double BlackScholesPricer::price(double spot, double strike, double timeToMaturity,
                               double riskFreeRate, double volatility, bool isCall) {
    return BlackScholes(spot, strike, timeToMaturity, riskFreeRate, volatility, isCall ? 1 : 0);
}

double MonteCarloPricer::price(double spot, double strike, double timeToMaturity,
                              double riskFreeRate, double volatility, bool isCall) {
    double result[2];
    MonteCarloPricing(spot, strike, timeToMaturity, riskFreeRate, volatility, isCall ? 1 : 0, numSimulations, result);
    return result[0];
}

double BinomialTreePricer::price(double spot, double strike, double timeToMaturity,
                                double riskFreeRate, double volatility, bool isCall) {
    return BinomialTree(spot, strike, timeToMaturity, riskFreeRate, volatility, isCall ? 1 : 0, nSteps);
}

std::unique_ptr<PortfolioOptimizer> QuantEngineFactory::createOptimizer(const std::string& type) {
    if (type == "markowitz") {
        return std::make_unique<MarkowitzOptimizer>();
    } else if (type == "risk_parity") {
        return std::make_unique<RiskParityOptimizer>();
    }
    return nullptr;
}

std::unique_ptr<VaRCalculator> QuantEngineFactory::createVaRCalculator(const std::string& type, int simulations) {
    if (type == "historical") {
        return std::make_unique<HistoricalVaRCalculator>();
    } else if (type == "parametric") {
        return std::make_unique<ParametricVaRCalculator>();
    } else if (type == "monte_carlo") {
        return std::make_unique<MonteCarloVaRCalculator>(simulations);
    }
    return nullptr;
}

std::unique_ptr<OptionPricer> QuantEngineFactory::createOptionPricer(const std::string& type, int simulations, int steps) {
    if (type == "black_scholes") {
        return std::make_unique<BlackScholesPricer>();
    } else if (type == "monte_carlo") {
        return std::make_unique<MonteCarloPricer>(simulations);
    } else if (type == "binomial_tree") {
        return std::make_unique<BinomialTreePricer>(steps);
    }
    return nullptr;
}

} // namespace QuantEngine

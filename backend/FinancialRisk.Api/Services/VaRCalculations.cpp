#include <vector>
#include <algorithm>
#include <numeric>
#include <cmath>
#include <stdexcept>
#include <random>

extern "C" {
    // Historical VaR using percentile method
    double CalculateHistoricalVaR(double* returns, int length, double confidenceLevel) {
        if (length < 2) return 0.0;
        if (confidenceLevel <= 0.0 || confidenceLevel >= 1.0) return 0.0;
        
        // Create a copy of returns for sorting
        std::vector<double> sortedReturns(returns, returns + length);
        std::sort(sortedReturns.begin(), sortedReturns.end());
        
        // Calculate the index for the confidence level
        int index = static_cast<int>((1.0 - confidenceLevel) * length);
        if (index >= length) index = length - 1;
        if (index < 0) index = 0;
        
        // Return negative VaR (loss)
        return -sortedReturns[index];
    }
    
    // Historical CVaR (Expected Shortfall) using percentile method
    double CalculateHistoricalCVaR(double* returns, int length, double confidenceLevel) {
        if (length < 2) return 0.0;
        if (confidenceLevel <= 0.0 || confidenceLevel >= 1.0) return 0.0;
        
        // Create a copy of returns for sorting
        std::vector<double> sortedReturns(returns, returns + length);
        std::sort(sortedReturns.begin(), sortedReturns.end());
        
        // Calculate the number of observations in the tail
        int tailCount = static_cast<int>((1.0 - confidenceLevel) * length);
        if (tailCount <= 0) tailCount = 1;
        if (tailCount > length) tailCount = length;
        
        // Calculate average of the worst returns
        double tailSum = 0.0;
        for (int i = 0; i < tailCount; ++i) {
            tailSum += sortedReturns[i];
        }
        
        // Return negative CVaR (average loss)
        return -(tailSum / tailCount);
    }
    
    // Parametric VaR using normal distribution assumption
    double CalculateParametricVaR(double* returns, int length, double confidenceLevel) {
        if (length < 2) return 0.0;
        if (confidenceLevel <= 0.0 || confidenceLevel >= 1.0) return 0.0;
        
        // Calculate mean and standard deviation
        double sum = 0.0;
        for (int i = 0; i < length; ++i) {
            sum += returns[i];
        }
        double mean = sum / length;
        
        double variance = 0.0;
        for (int i = 0; i < length; ++i) {
            double diff = returns[i] - mean;
            variance += diff * diff;
        }
        variance /= (length - 1);
        double stdDev = std::sqrt(variance);
        
        // Calculate z-score for confidence level
        double zScore = 0.0;
        if (confidenceLevel == 0.95) {
            zScore = 1.645;
        } else if (confidenceLevel == 0.99) {
            zScore = 2.326;
        } else if (confidenceLevel == 0.90) {
            zScore = 1.282;
        } else {
            // Approximate z-score using inverse normal CDF approximation
            zScore = std::sqrt(2.0) * std::erf(2.0 * confidenceLevel - 1.0);
        }
        
        // Parametric VaR = mean - zScore * stdDev
        return -(mean - zScore * stdDev);
    }
    
    // Parametric CVaR using normal distribution assumption
    double CalculateParametricCVaR(double* returns, int length, double confidenceLevel) {
        if (length < 2) return 0.0;
        if (confidenceLevel <= 0.0 || confidenceLevel >= 1.0) return 0.0;
        
        // Calculate mean and standard deviation
        double sum = 0.0;
        for (int i = 0; i < length; ++i) {
            sum += returns[i];
        }
        double mean = sum / length;
        
        double variance = 0.0;
        for (int i = 0; i < length; ++i) {
            double diff = returns[i] - mean;
            variance += diff * diff;
        }
        variance /= (length - 1);
        double stdDev = std::sqrt(variance);
        
        // Calculate z-score for confidence level
        double zScore = 0.0;
        if (confidenceLevel == 0.95) {
            zScore = 1.645;
        } else if (confidenceLevel == 0.99) {
            zScore = 2.326;
        } else if (confidenceLevel == 0.90) {
            zScore = 1.282;
        } else {
            zScore = std::sqrt(2.0) * std::erf(2.0 * confidenceLevel - 1.0);
        }
        
        // Parametric CVaR = mean - stdDev * phi(zScore) / (1 - confidenceLevel)
        // where phi is the standard normal PDF
        double phi = (1.0 / std::sqrt(2.0 * M_PI)) * std::exp(-0.5 * zScore * zScore);
        double parametricCVaR = mean - stdDev * phi / (1.0 - confidenceLevel);
        
        return -parametricCVaR;
    }
    
    // Bootstrap VaR using resampling
    double CalculateBootstrapVaR(double* returns, int length, double confidenceLevel, int bootstrapSamples) {
        if (length < 2) return 0.0;
        if (confidenceLevel <= 0.0 || confidenceLevel >= 1.0) return 0.0;
        if (bootstrapSamples <= 0) bootstrapSamples = 1000;
        
        std::random_device rd;
        std::mt19937 gen(rd());
        std::uniform_int_distribution<> dis(0, length - 1);
        
        std::vector<double> bootstrapVaRs;
        bootstrapVaRs.reserve(bootstrapSamples);
        
        for (int i = 0; i < bootstrapSamples; ++i) {
            // Create bootstrap sample
            std::vector<double> bootstrapSample;
            bootstrapSample.reserve(length);
            
            for (int j = 0; j < length; ++j) {
                int randomIndex = dis(gen);
                bootstrapSample.push_back(returns[randomIndex]);
            }
            
            // Calculate VaR for this bootstrap sample
            std::sort(bootstrapSample.begin(), bootstrapSample.end());
            int index = static_cast<int>((1.0 - confidenceLevel) * length);
            if (index >= length) index = length - 1;
            if (index < 0) index = 0;
            
            double var = -bootstrapSample[index];
            bootstrapVaRs.push_back(var);
        }
        
        // Calculate mean of bootstrap VaRs
        double sum = 0.0;
        for (double var : bootstrapVaRs) {
            sum += var;
        }
        
        return sum / bootstrapSamples;
    }
    
    // Calculate VaR confidence intervals using bootstrap
    void CalculateVaRConfidenceIntervals(double* returns, int length, double confidenceLevel, 
                                       int bootstrapSamples, double* lowerBound, double* upperBound) {
        if (length < 2) {
            *lowerBound = 0.0;
            *upperBound = 0.0;
            return;
        }
        
        std::random_device rd;
        std::mt19937 gen(rd());
        std::uniform_int_distribution<> dis(0, length - 1);
        
        std::vector<double> bootstrapVaRs;
        bootstrapVaRs.reserve(bootstrapSamples);
        
        for (int i = 0; i < bootstrapSamples; ++i) {
            std::vector<double> bootstrapSample;
            bootstrapSample.reserve(length);
            
            for (int j = 0; j < length; ++j) {
                int randomIndex = dis(gen);
                bootstrapSample.push_back(returns[randomIndex]);
            }
            
            std::sort(bootstrapSample.begin(), bootstrapSample.end());
            int index = static_cast<int>((1.0 - confidenceLevel) * length);
            if (index >= length) index = length - 1;
            if (index < 0) index = 0;
            
            double var = -bootstrapSample[index];
            bootstrapVaRs.push_back(var);
        }
        
        // Sort bootstrap VaRs for percentile calculation
        std::sort(bootstrapVaRs.begin(), bootstrapVaRs.end());
        
        // Calculate 5th and 95th percentiles for confidence intervals
        int lowerIndex = static_cast<int>(0.05 * bootstrapSamples);
        int upperIndex = static_cast<int>(0.95 * bootstrapSamples);
        
        if (lowerIndex >= bootstrapSamples) lowerIndex = bootstrapSamples - 1;
        if (upperIndex >= bootstrapSamples) upperIndex = bootstrapSamples - 1;
        if (lowerIndex < 0) lowerIndex = 0;
        if (upperIndex < 0) upperIndex = 0;
        
        *lowerBound = bootstrapVaRs[lowerIndex];
        *upperBound = bootstrapVaRs[upperIndex];
    }
    
    // Calculate portfolio VaR using historical simulation
    double CalculatePortfolioHistoricalVaR(double* portfolioReturns, int length, double confidenceLevel) {
        return CalculateHistoricalVaR(portfolioReturns, length, confidenceLevel);
    }
    
    // Calculate portfolio CVaR using historical simulation
    double CalculatePortfolioHistoricalCVaR(double* portfolioReturns, int length, double confidenceLevel) {
        return CalculateHistoricalCVaR(portfolioReturns, length, confidenceLevel);
    }
    
    // Calculate VaR decomposition (contribution of each asset to portfolio VaR)
    void CalculateVaRDecomposition(double* assetReturns, double* weights, int numAssets, int length, 
                                  double confidenceLevel, double* contributions) {
        if (numAssets <= 0 || length <= 0) return;
        
        // Calculate portfolio returns
        std::vector<double> portfolioReturns(length, 0.0);
        for (int i = 0; i < length; ++i) {
            for (int j = 0; j < numAssets; ++j) {
                portfolioReturns[i] += weights[j] * assetReturns[j * length + i];
            }
        }
        
        // Calculate portfolio VaR
        double portfolioVaR = CalculateHistoricalVaR(portfolioReturns.data(), length, confidenceLevel);
        
        // Calculate individual asset VaR contributions
        for (int j = 0; j < numAssets; ++j) {
            std::vector<double> assetReturnSeries(assetReturns + j * length, assetReturns + j * length + length);
            double assetVaR = CalculateHistoricalVaR(assetReturnSeries.data(), length, confidenceLevel);
            contributions[j] = weights[j] * assetVaR / portfolioVaR;
        }
    }
}

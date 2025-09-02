#include <vector>
#include <algorithm>
#include <numeric>
#include <cmath>
#include <stdexcept>

extern "C" {
    // Calculate daily volatility (annualized)
    double CalculateVolatility(double* returns, int length) {
        if (length < 2) return 0.0;
        
        // Calculate mean return
        double sum = 0.0;
        for (int i = 0; i < length; ++i) {
            sum += returns[i];
        }
        double mean = sum / length;
        
        // Calculate variance
        double variance = 0.0;
        for (int i = 0; i < length; ++i) {
            double diff = returns[i] - mean;
            variance += diff * diff;
        }
        variance /= (length - 1); // Sample variance
        
        // Return annualized volatility (assuming daily returns)
        return std::sqrt(variance * 252.0);
    }
    
    // Calculate beta vs benchmark
    double CalculateBeta(double* assetReturns, double* benchmarkReturns, int length) {
        if (length < 2) return 0.0;
        
        // Calculate means
        double assetSum = 0.0, benchmarkSum = 0.0;
        for (int i = 0; i < length; ++i) {
            assetSum += assetReturns[i];
            benchmarkSum += benchmarkReturns[i];
        }
        double assetMean = assetSum / length;
        double benchmarkMean = benchmarkSum / length;
        
        // Calculate covariance and benchmark variance
        double covariance = 0.0, benchmarkVariance = 0.0;
        for (int i = 0; i < length; ++i) {
            double assetDiff = assetReturns[i] - assetMean;
            double benchmarkDiff = benchmarkReturns[i] - benchmarkMean;
            covariance += assetDiff * benchmarkDiff;
            benchmarkVariance += benchmarkDiff * benchmarkDiff;
        }
        
        if (benchmarkVariance == 0.0) return 0.0;
        return covariance / benchmarkVariance;
    }
    
    // Calculate Sharpe ratio
    double CalculateSharpeRatio(double* returns, double riskFreeRate, int length) {
        if (length < 2) return 0.0;
        
        // Calculate mean return
        double sum = 0.0;
        for (int i = 0; i < length; ++i) {
            sum += returns[i];
        }
        double meanReturn = sum / length;
        
        // Calculate volatility
        double variance = 0.0;
        for (int i = 0; i < length; ++i) {
            double diff = returns[i] - meanReturn;
            variance += diff * diff;
        }
        variance /= (length - 1);
        double volatility = std::sqrt(variance * 252.0); // Annualized
        
        if (volatility == 0.0) return 0.0;
        
        // Annualize the mean return
        double annualizedReturn = meanReturn * 252.0;
        return (annualizedReturn - riskFreeRate) / volatility;
    }
    
    // Calculate Sortino ratio (downside deviation)
    double CalculateSortinoRatio(double* returns, double riskFreeRate, int length) {
        if (length < 2) return 0.0;
        
        // Calculate mean return
        double sum = 0.0;
        for (int i = 0; i < length; ++i) {
            sum += returns[i];
        }
        double meanReturn = sum / length;
        
        // Calculate downside deviation
        double downsideSum = 0.0;
        int downsideCount = 0;
        for (int i = 0; i < length; ++i) {
            if (returns[i] < meanReturn) {
                double diff = returns[i] - meanReturn;
                downsideSum += diff * diff;
                downsideCount++;
            }
        }
        
        if (downsideCount == 0) return 0.0;
        
        double downsideVariance = downsideSum / downsideCount;
        double downsideDeviation = std::sqrt(downsideVariance * 252.0); // Annualized
        
        if (downsideDeviation == 0.0) return 0.0;
        
        // Annualize the mean return
        double annualizedReturn = meanReturn * 252.0;
        return (annualizedReturn - riskFreeRate) / downsideDeviation;
    }
    
    // Calculate Value at Risk (VaR) using historical simulation
    double CalculateValueAtRisk(double* returns, double confidenceLevel, int length) {
        if (length < 2) return 0.0;
        
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
    
    // Calculate Expected Shortfall (Conditional VaR)
    double CalculateExpectedShortfall(double* returns, double confidenceLevel, int length) {
        if (length < 2) return 0.0;
        
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
        
        // Return negative expected shortfall (average loss)
        return -(tailSum / tailCount);
    }
    
    // Calculate Maximum Drawdown
    double CalculateMaximumDrawdown(double* returns, int length) {
        if (length < 2) return 0.0;
        
        double peak = 0.0;
        double maxDrawdown = 0.0;
        double cumulative = 0.0;
        
        for (int i = 0; i < length; ++i) {
            cumulative += returns[i];
            if (cumulative > peak) {
                peak = cumulative;
            }
            double drawdown = peak - cumulative;
            if (drawdown > maxDrawdown) {
                maxDrawdown = drawdown;
            }
        }
        
        return maxDrawdown;
    }
    
    // Calculate Information Ratio
    double CalculateInformationRatio(double* assetReturns, double* benchmarkReturns, int length) {
        if (length < 2) return 0.0;
        
        // Calculate tracking error (standard deviation of excess returns)
        double excessSum = 0.0;
        for (int i = 0; i < length; ++i) {
            excessSum += (assetReturns[i] - benchmarkReturns[i]);
        }
        double excessMean = excessSum / length;
        
        double trackingErrorSum = 0.0;
        for (int i = 0; i < length; ++i) {
            double excess = assetReturns[i] - benchmarkReturns[i];
            double diff = excess - excessMean;
            trackingErrorSum += diff * diff;
        }
        double trackingError = std::sqrt(trackingErrorSum / (length - 1)) * std::sqrt(252.0);
        
        if (trackingError == 0.0) return 0.0;
        
        // Annualize excess return
        double annualizedExcess = excessMean * 252.0;
        return annualizedExcess / trackingError;
    }
}

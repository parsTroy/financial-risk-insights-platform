#ifndef QUANTENGINE_H
#define QUANTENGINE_H

#ifdef _WIN32
#define QUANTENGINE_API __declspec(dllexport)
#else
#define QUANTENGINE_API
#endif

#include <vector>
#include <string>
#include <memory>

extern "C" {
    // Risk Management Functions
    QUANTENGINE_API double CalculateVaRHistorical(const double* returns, int length, double confidenceLevel);
    QUANTENGINE_API double CalculateVaRParametric(double mean, double std, double confidenceLevel);
    QUANTENGINE_API double CalculateCVaR(const double* returns, int length, double confidenceLevel);
    QUANTENGINE_API void CalculateVaRMonteCarlo(const double* returns, int length, double confidenceLevel, 
                                               int numSimulations, double* result);
    
    // Portfolio Optimization Functions
    QUANTENGINE_API void OptimizeMarkowitz(const double* expectedReturns, const double* covarianceMatrix, 
                                          int numAssets, double riskAversion, double* optimalWeights);
    QUANTENGINE_API void CalculateEfficientFrontier(const double* expectedReturns, const double* covarianceMatrix,
                                                   int numAssets, int numPoints, double* frontierPoints);
    QUANTENGINE_API void OptimizeRiskParity(const double* covarianceMatrix, int numAssets, double* optimalWeights);
    
    // Statistical Functions
    QUANTENGINE_API void CalculateGARCH(const double* returns, int length, int p, int q, double* parameters);
    QUANTENGINE_API void CalculateCopula(const double* data, int rows, int cols, int copulaType, double* result);
    QUANTENGINE_API void RegimeSwitching(const double* returns, int length, int numRegimes, double* probabilities);
    
    // Pricing Functions
    QUANTENGINE_API double BlackScholes(double spot, double strike, double timeToMaturity, 
                                       double riskFreeRate, double volatility, int optionType);
    QUANTENGINE_API void MonteCarloPricing(double spot, double strike, double timeToMaturity,
                                          double riskFreeRate, double volatility, int optionType,
                                          int numSimulations, double* result);
    QUANTENGINE_API double BinomialTree(double spot, double strike, double timeToMaturity,
                                       double riskFreeRate, double volatility, int optionType, int nSteps);
    
    // Utility Functions
    QUANTENGINE_API double CalculateSharpeRatio(const double* returns, int length, double riskFreeRate);
    QUANTENGINE_API void CalculateCorrelationMatrix(const double* data, int rows, int cols, double* correlationMatrix);
    QUANTENGINE_API void CalculateCovarianceMatrix(const double* data, int rows, int cols, double* covarianceMatrix);
    QUANTENGINE_API double CalculatePortfolioVolatility(const double* weights, const double* covarianceMatrix, int numAssets);
    QUANTENGINE_API double CalculatePortfolioReturn(const double* weights, const double* expectedReturns, int numAssets);
    
    // Performance and Memory Management
    QUANTENGINE_API int GetMemoryUsage();
    QUANTENGINE_API void ClearCache();
    QUANTENGINE_API const char* GetVersion();
    QUANTENGINE_API int GetLastError();
    QUANTENGINE_API const char* GetLastErrorMessage();
}

// C++ Classes for advanced usage
namespace QuantEngine {
    
    class PortfolioOptimizer {
    public:
        virtual ~PortfolioOptimizer() = default;
        virtual std::vector<double> optimize(const std::vector<double>& expectedReturns,
                                           const std::vector<std::vector<double>>& covarianceMatrix,
                                           double riskAversion) = 0;
    };
    
    class MarkowitzOptimizer : public PortfolioOptimizer {
    public:
        std::vector<double> optimize(const std::vector<double>& expectedReturns,
                                   const std::vector<std::vector<double>>& covarianceMatrix,
                                   double riskAversion) override;
    };
    
    class RiskParityOptimizer : public PortfolioOptimizer {
    public:
        std::vector<double> optimize(const std::vector<double>& expectedReturns,
                                   const std::vector<std::vector<double>>& covarianceMatrix,
                                   double riskAversion) override;
    };
    
    class VaRCalculator {
    public:
        virtual ~VaRCalculator() = default;
        virtual double calculate(const std::vector<double>& returns, double confidenceLevel) = 0;
    };
    
    class HistoricalVaRCalculator : public VaRCalculator {
    public:
        double calculate(const std::vector<double>& returns, double confidenceLevel) override;
    };
    
    class ParametricVaRCalculator : public VaRCalculator {
    public:
        double calculate(const std::vector<double>& returns, double confidenceLevel) override;
    };
    
    class MonteCarloVaRCalculator : public VaRCalculator {
    private:
        int numSimulations;
    public:
        MonteCarloVaRCalculator(int simulations = 10000) : numSimulations(simulations) {}
        double calculate(const std::vector<double>& returns, double confidenceLevel) override;
    };
    
    class OptionPricer {
    public:
        virtual ~OptionPricer() = default;
        virtual double price(double spot, double strike, double timeToMaturity,
                           double riskFreeRate, double volatility, bool isCall) = 0;
    };
    
    class BlackScholesPricer : public OptionPricer {
    public:
        double price(double spot, double strike, double timeToMaturity,
                   double riskFreeRate, double volatility, bool isCall) override;
    };
    
    class MonteCarloPricer : public OptionPricer {
    private:
        int numSimulations;
    public:
        MonteCarloPricer(int simulations = 10000) : numSimulations(simulations) {}
        double price(double spot, double strike, double timeToMaturity,
                   double riskFreeRate, double volatility, bool isCall) override;
    };
    
    class BinomialTreePricer : public OptionPricer {
    private:
        int nSteps;
    public:
        BinomialTreePricer(int steps = 100) : nSteps(steps) {}
        double price(double spot, double strike, double timeToMaturity,
                   double riskFreeRate, double volatility, bool isCall) override;
    };
    
    // Factory class for creating optimizers and calculators
    class QuantEngineFactory {
    public:
        static std::unique_ptr<PortfolioOptimizer> createOptimizer(const std::string& type);
        static std::unique_ptr<VaRCalculator> createVaRCalculator(const std::string& type, int simulations = 10000);
        static std::unique_ptr<OptionPricer> createOptionPricer(const std::string& type, int simulations = 10000, int steps = 100);
    };
}

#endif // QUANTENGINE_H

#pragma once

#include <vector>
#include <random>
#include <memory>
#include <functional>
#include <string>

namespace MonteCarlo {

    // Forward declarations
    class RandomNumberGenerator;
    class Distribution;
    class MonteCarloSimulation;
    class PortfolioSimulation;

    // Enums for configuration
    enum class DistributionType {
        Normal,
        TStudent,
        GARCH,
        Copula,
        Custom
    };

    enum class SimulationType {
        SingleAsset,
        Portfolio,
        StressTest
    };

    // Core data structures
    struct SimulationParameters {
        int numSimulations = 10000;
        int timeHorizon = 1;
        double confidenceLevel = 0.95;
        DistributionType distributionType = DistributionType::Normal;
        std::vector<double> customParameters;
        bool useAntitheticVariates = false;
        bool useControlVariates = false;
        int seed = 0; // 0 means random seed
    };

    struct AssetParameters {
        std::string symbol;
        double initialPrice;
        double expectedReturn;
        double volatility;
        std::vector<double> historicalReturns;
        double weight = 1.0;
    };

    struct PortfolioParameters {
        std::vector<AssetParameters> assets;
        std::vector<double> weights;
        std::vector<std::vector<double>> correlationMatrix;
        double totalValue = 1.0;
    };

    struct SimulationResult {
        std::vector<double> simulatedReturns;
        std::vector<double> simulatedPrices;
        double var;
        double cvar;
        double expectedValue;
        double standardDeviation;
        double skewness;
        double kurtosis;
        std::vector<double> percentiles;
        bool success = false;
        std::string errorMessage;
    };

    struct PortfolioSimulationResult {
        std::vector<double> portfolioReturns;
        std::vector<double> portfolioValues;
        double portfolioVar;
        double portfolioCvar;
        double expectedReturn;
        double portfolioVolatility;
        std::vector<SimulationResult> assetResults;
        std::vector<double> varContributions;
        bool success = false;
        std::string errorMessage;
    };

    // Abstract base class for random number generation
    class RandomNumberGenerator {
    public:
        virtual ~RandomNumberGenerator() = default;
        virtual double generate() = 0;
        virtual void setSeed(unsigned int seed) = 0;
        virtual std::unique_ptr<RandomNumberGenerator> clone() const = 0;
    };

    // Mersenne Twister implementation
    class MersenneTwisterRNG : public RandomNumberGenerator {
    private:
        std::mt19937_64 generator;
        std::uniform_real_distribution<double> distribution;

    public:
        MersenneTwisterRNG(unsigned int seed = 0);
        double generate() override;
        void setSeed(unsigned int seed) override;
        std::unique_ptr<RandomNumberGenerator> clone() const override;
    };

    // Abstract base class for distributions
    class Distribution {
    public:
        virtual ~Distribution() = default;
        virtual double sample(RandomNumberGenerator& rng) = 0;
        virtual std::unique_ptr<Distribution> clone() const = 0;
        virtual void updateParameters(const std::vector<double>& params) = 0;
    };

    // Normal distribution implementation
    class NormalDistribution : public Distribution {
    private:
        double mean;
        double stdDev;
        std::normal_distribution<double> dist;

    public:
        NormalDistribution(double mean = 0.0, double stdDev = 1.0);
        double sample(RandomNumberGenerator& rng) override;
        std::unique_ptr<Distribution> clone() const override;
        void updateParameters(const std::vector<double>& params) override;
    };

    // Student's t-distribution implementation
    class TStudentDistribution : public Distribution {
    private:
        double degreesOfFreedom;
        double location;
        double scale;
        std::student_t_distribution<double> dist;

    public:
        TStudentDistribution(double df = 5.0, double location = 0.0, double scale = 1.0);
        double sample(RandomNumberGenerator& rng) override;
        std::unique_ptr<Distribution> clone() const override;
        void updateParameters(const std::vector<double>& params) override;
    };

    // GARCH distribution implementation (simplified)
    class GARCHDistribution : public Distribution {
    private:
        double omega, alpha, beta;
        double currentVariance;
        double lastReturn;
        std::normal_distribution<double> dist;

    public:
        GARCHDistribution(double omega = 0.0001, double alpha = 0.1, double beta = 0.85);
        double sample(RandomNumberGenerator& rng) override;
        std::unique_ptr<Distribution> clone() const override;
        void updateParameters(const std::vector<double>& params) override;
        void updateVariance(double returnValue);
    };

    // Main Monte Carlo simulation class
    class MonteCarloSimulation {
    private:
        std::unique_ptr<RandomNumberGenerator> rng;
        std::unique_ptr<Distribution> distribution;
        SimulationParameters params;

        // Helper methods
        void calculateStatistics(const std::vector<double>& returns, SimulationResult& result);
        std::vector<double> generateCorrelatedReturns(const std::vector<double>& independentReturns, 
                                                     const std::vector<std::vector<double>>& correlationMatrix);

    public:
        MonteCarloSimulation(const SimulationParameters& params);
        ~MonteCarloSimulation() = default;

        // Single asset simulation
        SimulationResult simulateSingleAsset(const AssetParameters& asset);
        
        // Portfolio simulation
        PortfolioSimulationResult simulatePortfolio(const PortfolioParameters& portfolio);
        
        // Stress test simulation
        SimulationResult performStressTest(const AssetParameters& asset, 
                                         const std::vector<double>& stressFactors);
        
        // Utility methods
        void setSeed(unsigned int seed);
        void setDistribution(std::unique_ptr<Distribution> dist);
        void setParameters(const SimulationParameters& params);
        
        // Statistical methods
        static double calculateVaR(const std::vector<double>& returns, double confidenceLevel);
        static double calculateCVaR(const std::vector<double>& returns, double confidenceLevel);
        static std::vector<double> calculatePercentiles(const std::vector<double>& data, 
                                                       const std::vector<double>& percentiles);
    };

    // Factory functions
    std::unique_ptr<Distribution> createDistribution(DistributionType type, 
                                                   const std::vector<double>& parameters = {});
    std::unique_ptr<RandomNumberGenerator> createRNG(const std::string& type = "mt19937");

    // Utility functions
    std::vector<std::vector<double>> calculateCorrelationMatrix(const std::vector<std::vector<double>>& returns);
    std::vector<double> calculateCholeskyDecomposition(const std::vector<std::vector<double>>& matrix);
    bool isValidCorrelationMatrix(const std::vector<std::vector<double>>& matrix);

} // namespace MonteCarlo

// C-style interface for P/Invoke
extern "C" {
    // Single asset Monte Carlo VaR
    double CalculateMonteCarloVaR(double* returns, int length, double confidenceLevel, 
                                 int numSimulations, int distributionType, double* parameters, int paramLength);
    
    // Portfolio Monte Carlo VaR
    double CalculatePortfolioMonteCarloVaR(double** assetReturns, int* lengths, int numAssets,
                                          double* weights, double confidenceLevel, int numSimulations,
                                          double** correlationMatrix, int distributionType);
    
    // Monte Carlo simulation with full results
    void RunMonteCarloSimulation(double* returns, int length, double confidenceLevel,
                                int numSimulations, int distributionType, double* parameters, int paramLength,
                                double* result);
    
    // Portfolio simulation with full results
    void RunPortfolioMonteCarloSimulation(double** assetReturns, int* lengths, int numAssets,
                                         double* weights, double confidenceLevel, int numSimulations,
                                         double** correlationMatrix, int distributionType,
                                         double* result);
}

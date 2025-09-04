#include "MonteCarloEngine.h"
#include <algorithm>
#include <numeric>
#include <cmath>
#include <stdexcept>
#include <iostream>
#include <iomanip>

namespace MonteCarlo {

    // MersenneTwisterRNG Implementation
    MersenneTwisterRNG::MersenneTwisterRNG(unsigned int seed) : generator(seed), distribution(0.0, 1.0) {}

    double MersenneTwisterRNG::generate() {
        return distribution(generator);
    }

    void MersenneTwisterRNG::setSeed(unsigned int seed) {
        generator.seed(seed);
    }

    std::unique_ptr<RandomNumberGenerator> MersenneTwisterRNG::clone() const {
        return std::make_unique<MersenneTwisterRNG>(generator());
    }

    // NormalDistribution Implementation
    NormalDistribution::NormalDistribution(double mean, double stdDev) 
        : mean(mean), stdDev(stdDev), dist(mean, stdDev) {}

    double NormalDistribution::sample(RandomNumberGenerator& rng) {
        // Box-Muller transform for normal distribution
        double u1 = rng.generate();
        double u2 = rng.generate();
        
        if (u1 == 0.0) u1 = 1e-10; // Avoid log(0)
        
        double z0 = std::sqrt(-2.0 * std::log(u1)) * std::cos(2.0 * M_PI * u2);
        return mean + stdDev * z0;
    }

    std::unique_ptr<Distribution> NormalDistribution::clone() const {
        return std::make_unique<NormalDistribution>(mean, stdDev);
    }

    void NormalDistribution::updateParameters(const std::vector<double>& params) {
        if (params.size() >= 2) {
            mean = params[0];
            stdDev = params[1];
            dist = std::normal_distribution<double>(mean, stdDev);
        }
    }

    // TStudentDistribution Implementation
    TStudentDistribution::TStudentDistribution(double df, double location, double scale)
        : degreesOfFreedom(df), location(location), scale(scale), dist(df) {}

    double TStudentDistribution::sample(RandomNumberGenerator& rng) {
        // Generate t-distributed random variable
        double normal = NormalDistribution(0.0, 1.0).sample(rng);
        double chi2 = 0.0;
        
        // Generate chi-squared random variable with df degrees of freedom
        for (int i = 0; i < static_cast<int>(degreesOfFreedom); ++i) {
            double z = NormalDistribution(0.0, 1.0).sample(rng);
            chi2 += z * z;
        }
        
        double t = normal / std::sqrt(chi2 / degreesOfFreedom);
        return location + scale * t;
    }

    std::unique_ptr<Distribution> TStudentDistribution::clone() const {
        return std::make_unique<TStudentDistribution>(degreesOfFreedom, location, scale);
    }

    void TStudentDistribution::updateParameters(const std::vector<double>& params) {
        if (params.size() >= 3) {
            degreesOfFreedom = params[0];
            location = params[1];
            scale = params[2];
            dist = std::student_t_distribution<double>(degreesOfFreedom);
        }
    }

    // GARCHDistribution Implementation
    GARCHDistribution::GARCHDistribution(double omega, double alpha, double beta)
        : omega(omega), alpha(alpha), beta(beta), currentVariance(omega / (1 - alpha - beta)), lastReturn(0.0), dist(0.0, 1.0) {}

    double GARCHDistribution::sample(RandomNumberGenerator& rng) {
        double z = dist(rng);
        double returnValue = std::sqrt(currentVariance) * z;
        updateVariance(returnValue);
        return returnValue;
    }

    std::unique_ptr<Distribution> GARCHDistribution::clone() const {
        return std::make_unique<GARCHDistribution>(omega, alpha, beta);
    }

    void GARCHDistribution::updateParameters(const std::vector<double>& params) {
        if (params.size() >= 3) {
            omega = params[0];
            alpha = params[1];
            beta = params[2];
            currentVariance = omega / (1 - alpha - beta);
        }
    }

    void GARCHDistribution::updateVariance(double returnValue) {
        lastReturn = returnValue;
        currentVariance = omega + alpha * returnValue * returnValue + beta * currentVariance;
    }

    // MonteCarloSimulation Implementation
    MonteCarloSimulation::MonteCarloSimulation(const SimulationParameters& params) 
        : params(params) {
        rng = createRNG("mt19937");
        distribution = createDistribution(params.distributionType, params.customParameters);
        
        if (params.seed != 0) {
            rng->setSeed(params.seed);
        }
    }

    SimulationResult MonteCarloSimulation::simulateSingleAsset(const AssetParameters& asset) {
        SimulationResult result;
        
        try {
            result.simulatedReturns.reserve(params.numSimulations);
            result.simulatedPrices.reserve(params.numSimulations);
            
            // Calculate distribution parameters from historical data
            double mean = 0.0, variance = 0.0;
            if (!asset.historicalReturns.empty()) {
                mean = std::accumulate(asset.historicalReturns.begin(), asset.historicalReturns.end(), 0.0) / asset.historicalReturns.size();
                
                for (double ret : asset.historicalReturns) {
                    variance += (ret - mean) * (ret - mean);
                }
                variance /= (asset.historicalReturns.size() - 1);
            } else {
                mean = asset.expectedReturn;
                variance = asset.volatility * asset.volatility;
            }
            
            // Update distribution parameters
            std::vector<double> distParams = {mean, std::sqrt(variance)};
            distribution->updateParameters(distParams);
            
            // Generate simulations
            for (int i = 0; i < params.numSimulations; ++i) {
                double simulatedReturn = distribution->sample(*rng);
                result.simulatedReturns.push_back(simulatedReturn);
                
                // Calculate simulated price
                double simulatedPrice = asset.initialPrice * std::exp(simulatedReturn);
                result.simulatedPrices.push_back(simulatedPrice);
            }
            
            // Calculate statistics
            calculateStatistics(result.simulatedReturns, result);
            
            // Calculate VaR and CVaR
            result.var = calculateVaR(result.simulatedReturns, params.confidenceLevel);
            result.cvar = calculateCVaR(result.simulatedReturns, params.confidenceLevel);
            
            result.success = true;
            
        } catch (const std::exception& e) {
            result.success = false;
            result.errorMessage = e.what();
        }
        
        return result;
    }

    PortfolioSimulationResult MonteCarloSimulation::simulatePortfolio(const PortfolioParameters& portfolio) {
        PortfolioSimulationResult result;
        
        try {
            if (portfolio.assets.empty()) {
                throw std::invalid_argument("Portfolio must contain at least one asset");
            }
            
            if (portfolio.assets.size() != portfolio.weights.size()) {
                throw std::invalid_argument("Number of assets must match number of weights");
            }
            
            // Normalize weights
            double totalWeight = std::accumulate(portfolio.weights.begin(), portfolio.weights.end(), 0.0);
            std::vector<double> normalizedWeights = portfolio.weights;
            for (double& weight : normalizedWeights) {
                weight /= totalWeight;
            }
            
            // Simulate each asset
            result.assetResults.reserve(portfolio.assets.size());
            for (const auto& asset : portfolio.assets) {
                SimulationResult assetResult = simulateSingleAsset(asset);
                result.assetResults.push_back(assetResult);
            }
            
            // Generate correlated returns
            std::vector<std::vector<double>> independentReturns(portfolio.assets.size());
            for (size_t i = 0; i < portfolio.assets.size(); ++i) {
                independentReturns[i] = result.assetResults[i].simulatedReturns;
            }
            
            // Apply correlation if provided
            std::vector<std::vector<double>> correlatedReturns = independentReturns;
            if (!portfolio.correlationMatrix.empty() && 
                portfolio.correlationMatrix.size() == portfolio.assets.size()) {
                correlatedReturns = generateCorrelatedReturns(independentReturns, portfolio.correlationMatrix);
            }
            
            // Calculate portfolio returns
            result.portfolioReturns.reserve(params.numSimulations);
            result.portfolioValues.reserve(params.numSimulations);
            
            for (int sim = 0; sim < params.numSimulations; ++sim) {
                double portfolioReturn = 0.0;
                double portfolioValue = 0.0;
                
                for (size_t i = 0; i < portfolio.assets.size(); ++i) {
                    double assetReturn = correlatedReturns[i][sim];
                    double weight = normalizedWeights[i];
                    double assetValue = portfolio.assets[i].initialPrice * std::exp(assetReturn);
                    
                    portfolioReturn += weight * assetReturn;
                    portfolioValue += weight * assetValue;
                }
                
                result.portfolioReturns.push_back(portfolioReturn);
                result.portfolioValues.push_back(portfolioValue);
            }
            
            // Calculate portfolio statistics
            calculateStatistics(result.portfolioReturns, 
                reinterpret_cast<SimulationResult&>(result));
            
            // Calculate portfolio VaR and CVaR
            result.portfolioVar = calculateVaR(result.portfolioReturns, params.confidenceLevel);
            result.portfolioCvar = calculateCVaR(result.portfolioReturns, params.confidenceLevel);
            
            // Calculate VaR contributions
            result.varContributions.reserve(portfolio.assets.size());
            for (size_t i = 0; i < portfolio.assets.size(); ++i) {
                double contribution = normalizedWeights[i] * result.assetResults[i].var;
                result.varContributions.push_back(contribution);
            }
            
            result.success = true;
            
        } catch (const std::exception& e) {
            result.success = false;
            result.errorMessage = e.what();
        }
        
        return result;
    }

    SimulationResult MonteCarloSimulation::performStressTest(const AssetParameters& asset, 
                                                           const std::vector<double>& stressFactors) {
        SimulationResult result;
        
        try {
            // Create stressed asset parameters
            AssetParameters stressedAsset = asset;
            stressedAsset.volatility *= stressFactors[0]; // Volatility shock
            if (stressFactors.size() > 1) {
                stressedAsset.expectedReturn *= stressFactors[1]; // Return shock
            }
            
            // Run simulation with stressed parameters
            result = simulateSingleAsset(stressedAsset);
            
        } catch (const std::exception& e) {
            result.success = false;
            result.errorMessage = e.what();
        }
        
        return result;
    }

    void MonteCarloSimulation::calculateStatistics(const std::vector<double>& returns, SimulationResult& result) {
        if (returns.empty()) return;
        
        // Calculate mean
        result.expectedValue = std::accumulate(returns.begin(), returns.end(), 0.0) / returns.size();
        
        // Calculate variance and standard deviation
        double variance = 0.0;
        for (double ret : returns) {
            double diff = ret - result.expectedValue;
            variance += diff * diff;
        }
        variance /= (returns.size() - 1);
        result.standardDeviation = std::sqrt(variance);
        
        // Calculate skewness
        double skewnessSum = 0.0;
        for (double ret : returns) {
            double normalized = (ret - result.expectedValue) / result.standardDeviation;
            skewnessSum += normalized * normalized * normalized;
        }
        result.skewness = skewnessSum / returns.size();
        
        // Calculate kurtosis
        double kurtosisSum = 0.0;
        for (double ret : returns) {
            double normalized = (ret - result.expectedValue) / result.standardDeviation;
            double normalizedSquared = normalized * normalized;
            kurtosisSum += normalizedSquared * normalizedSquared;
        }
        result.kurtosis = (kurtosisSum / returns.size()) - 3.0; // Excess kurtosis
        
        // Calculate percentiles
        std::vector<double> sortedReturns = returns;
        std::sort(sortedReturns.begin(), sortedReturns.end());
        
        std::vector<double> percentiles = {0.01, 0.05, 0.10, 0.25, 0.50, 0.75, 0.90, 0.95, 0.99};
        result.percentiles = calculatePercentiles(sortedReturns, percentiles);
    }

    std::vector<double> MonteCarloSimulation::generateCorrelatedReturns(
        const std::vector<double>& independentReturns, 
        const std::vector<std::vector<double>>& correlationMatrix) {
        
        // This is a simplified implementation
        // In practice, you would use Cholesky decomposition
        return independentReturns; // Placeholder
    }

    void MonteCarloSimulation::setSeed(unsigned int seed) {
        rng->setSeed(seed);
    }

    void MonteCarloSimulation::setDistribution(std::unique_ptr<Distribution> dist) {
        distribution = std::move(dist);
    }

    void MonteCarloSimulation::setParameters(const SimulationParameters& params) {
        this->params = params;
    }

    double MonteCarloSimulation::calculateVaR(const std::vector<double>& returns, double confidenceLevel) {
        if (returns.empty()) return 0.0;
        
        std::vector<double> sortedReturns = returns;
        std::sort(sortedReturns.begin(), sortedReturns.end());
        
        int index = static_cast<int>((1.0 - confidenceLevel) * sortedReturns.size());
        index = std::max(0, std::min(index, static_cast<int>(sortedReturns.size()) - 1));
        
        return -sortedReturns[index]; // VaR is typically reported as positive
    }

    double MonteCarloSimulation::calculateCVaR(const std::vector<double>& returns, double confidenceLevel) {
        if (returns.empty()) return 0.0;
        
        std::vector<double> sortedReturns = returns;
        std::sort(sortedReturns.begin(), sortedReturns.end());
        
        int varIndex = static_cast<int>((1.0 - confidenceLevel) * sortedReturns.size());
        varIndex = std::max(0, std::min(varIndex, static_cast<int>(sortedReturns.size()) - 1));
        
        double cvarSum = 0.0;
        int count = 0;
        for (int i = 0; i <= varIndex; ++i) {
            cvarSum += sortedReturns[i];
            count++;
        }
        
        return count > 0 ? -cvarSum / count : 0.0; // CVaR is typically reported as positive
    }

    std::vector<double> MonteCarloSimulation::calculatePercentiles(const std::vector<double>& data, 
                                                                 const std::vector<double>& percentiles) {
        std::vector<double> result;
        result.reserve(percentiles.size());
        
        for (double p : percentiles) {
            int index = static_cast<int>(p * (data.size() - 1));
            index = std::max(0, std::min(index, static_cast<int>(data.size()) - 1));
            result.push_back(data[index]);
        }
        
        return result;
    }

    // Factory functions
    std::unique_ptr<Distribution> createDistribution(DistributionType type, const std::vector<double>& parameters) {
        switch (type) {
            case DistributionType::Normal:
                return std::make_unique<NormalDistribution>(
                    parameters.size() > 0 ? parameters[0] : 0.0,
                    parameters.size() > 1 ? parameters[1] : 1.0
                );
            case DistributionType::TStudent:
                return std::make_unique<TStudentDistribution>(
                    parameters.size() > 0 ? parameters[0] : 5.0,
                    parameters.size() > 1 ? parameters[1] : 0.0,
                    parameters.size() > 2 ? parameters[2] : 1.0
                );
            case DistributionType::GARCH:
                return std::make_unique<GARCHDistribution>(
                    parameters.size() > 0 ? parameters[0] : 0.0001,
                    parameters.size() > 1 ? parameters[1] : 0.1,
                    parameters.size() > 2 ? parameters[2] : 0.85
                );
            default:
                return std::make_unique<NormalDistribution>();
        }
    }

    std::unique_ptr<RandomNumberGenerator> createRNG(const std::string& type) {
        if (type == "mt19937") {
            return std::make_unique<MersenneTwisterRNG>();
        }
        return std::make_unique<MersenneTwisterRNG>(); // Default
    }

    // Utility functions
    std::vector<std::vector<double>> calculateCorrelationMatrix(const std::vector<std::vector<double>>& returns) {
        // Implementation for correlation matrix calculation
        // This is a placeholder - implement proper correlation calculation
        return {};
    }

    std::vector<double> calculateCholeskyDecomposition(const std::vector<std::vector<double>>& matrix) {
        // Implementation for Cholesky decomposition
        // This is a placeholder - implement proper Cholesky decomposition
        return {};
    }

    bool isValidCorrelationMatrix(const std::vector<std::vector<double>>& matrix) {
        // Implementation for correlation matrix validation
        // This is a placeholder - implement proper validation
        return true;
    }

} // namespace MonteCarlo

// C-style interface implementation
extern "C" {
    double CalculateMonteCarloVaR(double* returns, int length, double confidenceLevel, 
                                 int numSimulations, int distributionType, double* parameters, int paramLength) {
        try {
            std::vector<double> returnsVec(returns, returns + length);
            std::vector<double> paramsVec(parameters, parameters + paramLength);
            
            MonteCarlo::SimulationParameters simParams;
            simParams.numSimulations = numSimulations;
            simParams.confidenceLevel = confidenceLevel;
            simParams.distributionType = static_cast<MonteCarlo::DistributionType>(distributionType);
            simParams.customParameters = paramsVec;
            
            MonteCarlo::MonteCarloSimulation simulation(simParams);
            
            MonteCarlo::AssetParameters asset;
            asset.historicalReturns = returnsVec;
            
            auto result = simulation.simulateSingleAsset(asset);
            return result.var;
            
        } catch (...) {
            return -1.0; // Error indicator
        }
    }
    
    double CalculatePortfolioMonteCarloVaR(double** assetReturns, int* lengths, int numAssets,
                                          double* weights, double confidenceLevel, int numSimulations,
                                          double** correlationMatrix, int distributionType) {
        try {
            MonteCarlo::SimulationParameters simParams;
            simParams.numSimulations = numSimulations;
            simParams.confidenceLevel = confidenceLevel;
            simParams.distributionType = static_cast<MonteCarlo::DistributionType>(distributionType);
            
            MonteCarlo::MonteCarloSimulation simulation(simParams);
            
            MonteCarlo::PortfolioParameters portfolio;
            portfolio.weights = std::vector<double>(weights, weights + numAssets);
            
            for (int i = 0; i < numAssets; ++i) {
                MonteCarlo::AssetParameters asset;
                asset.historicalReturns = std::vector<double>(assetReturns[i], assetReturns[i] + lengths[i]);
                portfolio.assets.push_back(asset);
            }
            
            auto result = simulation.simulatePortfolio(portfolio);
            return result.portfolioVar;
            
        } catch (...) {
            return -1.0; // Error indicator
        }
    }
    
    void RunMonteCarloSimulation(double* returns, int length, double confidenceLevel,
                                int numSimulations, int distributionType, double* parameters, int paramLength,
                                double* result) {
        try {
            std::vector<double> returnsVec(returns, returns + length);
            std::vector<double> paramsVec(parameters, parameters + paramLength);
            
            MonteCarlo::SimulationParameters simParams;
            simParams.numSimulations = numSimulations;
            simParams.confidenceLevel = confidenceLevel;
            simParams.distributionType = static_cast<MonteCarlo::DistributionType>(distributionType);
            simParams.customParameters = paramsVec;
            
            MonteCarlo::MonteCarloSimulation simulation(simParams);
            
            MonteCarlo::AssetParameters asset;
            asset.historicalReturns = returnsVec;
            
            auto simResult = simulation.simulateSingleAsset(asset);
            
            // Pack results into output array
            result[0] = simResult.var;
            result[1] = simResult.cvar;
            result[2] = simResult.expectedValue;
            result[3] = simResult.standardDeviation;
            result[4] = simResult.skewness;
            result[5] = simResult.kurtosis;
            result[6] = simResult.success ? 1.0 : 0.0;
            
        } catch (...) {
            // Set error indicators
            for (int i = 0; i < 7; ++i) {
                result[i] = -1.0;
            }
        }
    }
    
    void RunPortfolioMonteCarloSimulation(double** assetReturns, int* lengths, int numAssets,
                                         double* weights, double confidenceLevel, int numSimulations,
                                         double** correlationMatrix, int distributionType,
                                         double* result) {
        try {
            MonteCarlo::SimulationParameters simParams;
            simParams.numSimulations = numSimulations;
            simParams.confidenceLevel = confidenceLevel;
            simParams.distributionType = static_cast<MonteCarlo::DistributionType>(distributionType);
            
            MonteCarlo::MonteCarloSimulation simulation(simParams);
            
            MonteCarlo::PortfolioParameters portfolio;
            portfolio.weights = std::vector<double>(weights, weights + numAssets);
            
            for (int i = 0; i < numAssets; ++i) {
                MonteCarlo::AssetParameters asset;
                asset.historicalReturns = std::vector<double>(assetReturns[i], assetReturns[i] + lengths[i]);
                portfolio.assets.push_back(asset);
            }
            
            auto simResult = simulation.simulatePortfolio(portfolio);
            
            // Pack results into output array
            result[0] = simResult.portfolioVar;
            result[1] = simResult.portfolioCvar;
            result[2] = simResult.expectedReturn;
            result[3] = simResult.portfolioVolatility;
            result[4] = simResult.success ? 1.0 : 0.0;
            
        } catch (...) {
            // Set error indicators
            for (int i = 0; i < 5; ++i) {
                result[i] = -1.0;
            }
        }
    }
}

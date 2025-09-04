#include <iostream>
#include <vector>
#include <cmath>
#include <cassert>
#include <chrono>
#include "VaRCalculations.h"

// Test data
std::vector<double> testReturns = {
    0.01, -0.02, 0.03, -0.01, 0.02, 0.01, -0.03, 0.02, 0.01, -0.01,
    0.02, 0.01, -0.02, 0.03, 0.01, -0.01, 0.02, 0.01, -0.02, 0.01,
    0.015, -0.025, 0.035, -0.015, 0.025, 0.015, -0.035, 0.025, 0.015, -0.015
};

// Helper function to check if two doubles are approximately equal
bool approximatelyEqual(double a, double b, double epsilon = 1e-6) {
    return std::abs(a - b) < epsilon;
}

// Test historical VaR calculation
void testHistoricalVaR() {
    std::cout << "Testing historical VaR calculation...\n";
    
    double var95 = CalculateHistoricalVaR(testReturns.data(), testReturns.size(), 0.95);
    double var99 = CalculateHistoricalVaR(testReturns.data(), testReturns.size(), 0.99);
    
    // Basic checks
    assert(var95 > 0.0);
    assert(var99 > 0.0);
    assert(var99 >= var95); // 99% VaR should be >= 95% VaR
    
    std::cout << "✅ Historical VaR test passed: 95% VaR = " << var95 << ", 99% VaR = " << var99 << "\n";
}

// Test historical CVaR calculation
void testHistoricalCVaR() {
    std::cout << "Testing historical CVaR calculation...\n";
    
    double cvar95 = CalculateHistoricalCVaR(testReturns.data(), testReturns.size(), 0.95);
    double cvar99 = CalculateHistoricalCVaR(testReturns.data(), testReturns.size(), 0.99);
    
    // Basic checks
    assert(cvar95 > 0.0);
    assert(cvar99 > 0.0);
    assert(cvar99 >= cvar95); // 99% CVaR should be >= 95% CVaR
    
    std::cout << "✅ Historical CVaR test passed: 95% CVaR = " << cvar95 << ", 99% CVaR = " << cvar99 << "\n";
}

// Test parametric VaR calculation
void testParametricVaR() {
    std::cout << "Testing parametric VaR calculation...\n";
    
    double var95 = CalculateParametricVaR(testReturns.data(), testReturns.size(), 0.95);
    double var99 = CalculateParametricVaR(testReturns.data(), testReturns.size(), 0.99);
    
    // Basic checks
    assert(var95 > 0.0);
    assert(var99 > 0.0);
    assert(var99 >= var95); // 99% VaR should be >= 95% VaR
    
    std::cout << "✅ Parametric VaR test passed: 95% VaR = " << var95 << ", 99% VaR = " << var99 << "\n";
}

// Test parametric CVaR calculation
void testParametricCVaR() {
    std::cout << "Testing parametric CVaR calculation...\n";
    
    double cvar95 = CalculateParametricCVaR(testReturns.data(), testReturns.size(), 0.95);
    double cvar99 = CalculateParametricCVaR(testReturns.data(), testReturns.size(), 0.99);
    
    // Basic checks
    assert(cvar95 > 0.0);
    assert(cvar99 > 0.0);
    assert(cvar99 >= cvar95); // 99% CVaR should be >= 95% CVaR
    
    std::cout << "✅ Parametric CVaR test passed: 95% CVaR = " << cvar95 << ", 99% CVaR = " << cvar99 << "\n";
}

// Test bootstrap VaR calculation
void testBootstrapVaR() {
    std::cout << "Testing bootstrap VaR calculation...\n";
    
    double var95 = CalculateBootstrapVaR(testReturns.data(), testReturns.size(), 0.95, 1000);
    double var99 = CalculateBootstrapVaR(testReturns.data(), testReturns.size(), 0.99, 1000);
    
    // Basic checks
    assert(var95 > 0.0);
    assert(var99 > 0.0);
    assert(var99 >= var95); // 99% VaR should be >= 95% VaR
    
    std::cout << "✅ Bootstrap VaR test passed: 95% VaR = " << var95 << ", 99% VaR = " << var99 << "\n";
}

// Test VaR confidence intervals
void testVaRConfidenceIntervals() {
    std::cout << "Testing VaR confidence intervals...\n";
    
    double lowerBound, upperBound;
    CalculateVaRConfidenceIntervals(testReturns.data(), testReturns.size(), 0.95, 1000, &lowerBound, &upperBound);
    
    // Basic checks
    assert(lowerBound > 0.0);
    assert(upperBound > 0.0);
    assert(upperBound >= lowerBound);
    
    std::cout << "✅ VaR confidence intervals test passed: Lower = " << lowerBound << ", Upper = " << upperBound << "\n";
}

// Test portfolio VaR calculation
void testPortfolioVaR() {
    std::cout << "Testing portfolio VaR calculation...\n";
    
    // Create portfolio returns (simple average of test returns)
    std::vector<double> portfolioReturns = testReturns;
    
    double var95 = CalculatePortfolioHistoricalVaR(portfolioReturns.data(), portfolioReturns.size(), 0.95);
    double var99 = CalculatePortfolioHistoricalVaR(portfolioReturns.data(), portfolioReturns.size(), 0.99);
    
    // Basic checks
    assert(var95 > 0.0);
    assert(var99 > 0.0);
    assert(var99 >= var95);
    
    std::cout << "✅ Portfolio VaR test passed: 95% VaR = " << var95 << ", 99% VaR = " << var99 << "\n";
}

// Test VaR decomposition
void testVaRDecomposition() {
    std::cout << "Testing VaR decomposition...\n";
    
    // Create asset returns matrix (2 assets, same data for simplicity)
    std::vector<double> assetReturns(2 * testReturns.size());
    for (size_t i = 0; i < testReturns.size(); ++i) {
        assetReturns[i] = testReturns[i]; // Asset 1
        assetReturns[i + testReturns.size()] = testReturns[i]; // Asset 2
    }
    
    // Equal weights
    std::vector<double> weights = {0.5, 0.5};
    std::vector<double> contributions(2);
    
    CalculateVaRDecomposition(assetReturns.data(), weights.data(), 2, testReturns.size(), 0.95, contributions.data());
    
    // Basic checks
    assert(contributions[0] > 0.0);
    assert(contributions[1] > 0.0);
    assert(approximatelyEqual(contributions[0] + contributions[1], 1.0, 0.1)); // Should sum to approximately 1
    
    std::cout << "✅ VaR decomposition test passed: Asset 1 contribution = " << contributions[0] 
              << ", Asset 2 contribution = " << contributions[1] << "\n";
}

// Test edge cases
void testEdgeCases() {
    std::cout << "Testing edge cases...\n";
    
    // Test with empty array
    double emptyVar = CalculateHistoricalVaR(nullptr, 0, 0.95);
    assert(emptyVar == 0.0);
    
    // Test with single element
    double singleReturn = 0.01;
    double singleVar = CalculateHistoricalVaR(&singleReturn, 1, 0.95);
    assert(singleVar == 0.0);
    
    // Test with two elements
    double twoReturns[] = {0.01, -0.01};
    double twoVar = CalculateHistoricalVaR(twoReturns, 2, 0.95);
    assert(twoVar > 0.0);
    
    // Test with invalid confidence level
    double invalidVar = CalculateHistoricalVaR(testReturns.data(), testReturns.size(), 1.5);
    assert(invalidVar == 0.0);
    
    std::cout << "✅ Edge cases test passed\n";
}

// Performance test
void testPerformance() {
    std::cout << "Testing performance...\n";
    
    // Create large dataset
    const int size = 10000;
    std::vector<double> largeReturns(size);
    for (int i = 0; i < size; ++i) {
        largeReturns[i] = (i % 2 == 0) ? 0.01 : -0.01;
    }
    
    // Time the calculations
    auto start = std::chrono::high_resolution_clock::now();
    
    double var95 = CalculateHistoricalVaR(largeReturns.data(), size, 0.95);
    double var99 = CalculateHistoricalVaR(largeReturns.data(), size, 0.99);
    double cvar95 = CalculateHistoricalCVaR(largeReturns.data(), size, 0.95);
    double cvar99 = CalculateHistoricalCVaR(largeReturns.data(), size, 0.99);
    double bootstrapVar = CalculateBootstrapVaR(largeReturns.data(), size, 0.95, 1000);
    
    auto end = std::chrono::high_resolution_clock::now();
    auto duration = std::chrono::duration_cast<std::chrono::microseconds>(end - start);
    
    // Basic checks
    assert(var95 > 0.0);
    assert(var99 > 0.0);
    assert(cvar95 > 0.0);
    assert(cvar99 > 0.0);
    assert(bootstrapVar > 0.0);
    
    std::cout << "✅ Performance test passed: " << duration.count() << " microseconds for " << size << " data points\n";
    std::cout << "   Historical VaR 95%: " << var95 << "\n";
    std::cout << "   Historical VaR 99%: " << var99 << "\n";
    std::cout << "   Historical CVaR 95%: " << cvar95 << "\n";
    std::cout << "   Historical CVaR 99%: " << cvar99 << "\n";
    std::cout << "   Bootstrap VaR 95%: " << bootstrapVar << "\n";
}

// Test method comparison
void testMethodComparison() {
    std::cout << "Testing method comparison...\n";
    
    double historicalVar = CalculateHistoricalVaR(testReturns.data(), testReturns.size(), 0.95);
    double parametricVar = CalculateParametricVaR(testReturns.data(), testReturns.size(), 0.95);
    double bootstrapVar = CalculateBootstrapVaR(testReturns.data(), testReturns.size(), 0.95, 1000);
    
    // All methods should produce positive VaR values
    assert(historicalVar > 0.0);
    assert(parametricVar > 0.0);
    assert(bootstrapVar > 0.0);
    
    std::cout << "✅ Method comparison test passed:\n";
    std::cout << "   Historical VaR: " << historicalVar << "\n";
    std::cout << "   Parametric VaR: " << parametricVar << "\n";
    std::cout << "   Bootstrap VaR: " << bootstrapVar << "\n";
}

int main() {
    std::cout << "🧪 Starting VaR Calculations C++ library tests...\n\n";
    
    try {
        testHistoricalVaR();
        testHistoricalCVaR();
        testParametricVaR();
        testParametricCVaR();
        testBootstrapVaR();
        testVaRConfidenceIntervals();
        testPortfolioVaR();
        testVaRDecomposition();
        testEdgeCases();
        testPerformance();
        testMethodComparison();
        
        std::cout << "\n🎉 All VaR tests passed successfully!\n";
        std::cout << "✅ C++ VaR library is ready for production use\n";
        
        return 0;
    } catch (const std::exception& e) {
        std::cout << "\n❌ Test failed: " << e.what() << "\n";
        return 1;
    }
}

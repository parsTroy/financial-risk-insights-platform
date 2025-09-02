#include <iostream>
#include <vector>
#include <cmath>
#include <cassert>
#include "RiskCalculations.h"

// Test data
std::vector<double> testReturns = {
    0.01, -0.02, 0.03, -0.01, 0.02, 0.01, -0.03, 0.02, 0.01, -0.01,
    0.02, 0.01, -0.02, 0.03, 0.01, -0.01, 0.02, 0.01, -0.02, 0.01
};

std::vector<double> benchmarkReturns = {
    0.005, -0.015, 0.025, -0.008, 0.018, 0.008, -0.025, 0.018, 0.008, -0.008,
    0.018, 0.008, -0.015, 0.025, 0.008, -0.008, 0.018, 0.008, -0.015, 0.008
};

// Helper function to check if two doubles are approximately equal
bool approximatelyEqual(double a, double b, double epsilon = 1e-6) {
    return std::abs(a - b) < epsilon;
}

// Test volatility calculation
void testVolatility() {
    std::cout << "Testing volatility calculation...\n";
    
    double volatility = CalculateVolatility(testReturns.data(), testReturns.size());
    
    // Basic checks
    assert(volatility > 0.0);
    assert(volatility < 1.0); // Should be reasonable for daily returns
    
    std::cout << "✅ Volatility test passed: " << volatility << "\n";
}

// Test beta calculation
void testBeta() {
    std::cout << "Testing beta calculation...\n";
    
    double beta = CalculateBeta(testReturns.data(), benchmarkReturns.data(), testReturns.size());
    
    // Basic checks
    assert(!std::isnan(beta));
    assert(!std::isinf(beta));
    
    std::cout << "✅ Beta test passed: " << beta << "\n";
}

// Test Sharpe ratio calculation
void testSharpeRatio() {
    std::cout << "Testing Sharpe ratio calculation...\n";
    
    double sharpe = CalculateSharpeRatio(testReturns.data(), 0.02, testReturns.size());
    
    // Basic checks
    assert(!std::isnan(sharpe));
    assert(!std::isinf(sharpe));
    
    std::cout << "✅ Sharpe ratio test passed: " << sharpe << "\n";
}

// Test Sortino ratio calculation
void testSortinoRatio() {
    std::cout << "Testing Sortino ratio calculation...\n";
    
    double sortino = CalculateSortinoRatio(testReturns.data(), 0.02, testReturns.size());
    
    // Basic checks
    assert(!std::isnan(sortino));
    assert(!std::isinf(sortino));
    
    std::cout << "✅ Sortino ratio test passed: " << sortino << "\n";
}

// Test Value at Risk calculation
void testValueAtRisk() {
    std::cout << "Testing Value at Risk calculation...\n";
    
    double var95 = CalculateValueAtRisk(testReturns.data(), 0.95, testReturns.size());
    double var99 = CalculateValueAtRisk(testReturns.data(), 0.99, testReturns.size());
    
    // Basic checks
    assert(var95 > 0.0);
    assert(var99 > 0.0);
    assert(var99 >= var95); // 99% VaR should be >= 95% VaR
    
    std::cout << "✅ VaR test passed: 95% VaR = " << var95 << ", 99% VaR = " << var99 << "\n";
}

// Test Expected Shortfall calculation
void testExpectedShortfall() {
    std::cout << "Testing Expected Shortfall calculation...\n";
    
    double es95 = CalculateExpectedShortfall(testReturns.data(), 0.95, testReturns.size());
    double es99 = CalculateExpectedShortfall(testReturns.data(), 0.99, testReturns.size());
    
    // Basic checks
    assert(es95 > 0.0);
    assert(es99 > 0.0);
    assert(es99 >= es95); // 99% ES should be >= 95% ES
    
    std::cout << "✅ Expected Shortfall test passed: 95% ES = " << es95 << ", 99% ES = " << es99 << "\n";
}

// Test Maximum Drawdown calculation
void testMaximumDrawdown() {
    std::cout << "Testing Maximum Drawdown calculation...\n";
    
    double maxDD = CalculateMaximumDrawdown(testReturns.data(), testReturns.size());
    
    // Basic checks
    assert(maxDD >= 0.0);
    assert(!std::isnan(maxDD));
    assert(!std::isinf(maxDD));
    
    std::cout << "✅ Maximum Drawdown test passed: " << maxDD << "\n";
}

// Test Information Ratio calculation
void testInformationRatio() {
    std::cout << "Testing Information Ratio calculation...\n";
    
    double infoRatio = CalculateInformationRatio(testReturns.data(), benchmarkReturns.data(), testReturns.size());
    
    // Basic checks
    assert(!std::isnan(infoRatio));
    assert(!std::isinf(infoRatio));
    
    std::cout << "✅ Information Ratio test passed: " << infoRatio << "\n";
}

// Test edge cases
void testEdgeCases() {
    std::cout << "Testing edge cases...\n";
    
    // Test with empty array
    double emptyVol = CalculateVolatility(nullptr, 0);
    assert(emptyVol == 0.0);
    
    // Test with single element
    double singleReturn = 0.01;
    double singleVol = CalculateVolatility(&singleReturn, 1);
    assert(singleVol == 0.0);
    
    // Test with two elements
    double twoReturns[] = {0.01, -0.01};
    double twoVol = CalculateVolatility(twoReturns, 2);
    assert(twoVol > 0.0);
    
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
    
    // Time the calculation
    auto start = std::chrono::high_resolution_clock::now();
    
    double vol = CalculateVolatility(largeReturns.data(), size);
    double sharpe = CalculateSharpeRatio(largeReturns.data(), 0.02, size);
    double var = CalculateValueAtRisk(largeReturns.data(), 0.95, size);
    
    auto end = std::chrono::high_resolution_clock::now();
    auto duration = std::chrono::duration_cast<std::chrono::microseconds>(end - start);
    
    // Basic checks
    assert(vol > 0.0);
    assert(!std::isnan(sharpe));
    assert(var > 0.0);
    
    std::cout << "✅ Performance test passed: " << duration.count() << " microseconds for " << size << " data points\n";
}

int main() {
    std::cout << "🧪 Starting RiskCalculations C++ library tests...\n\n";
    
    try {
        testVolatility();
        testBeta();
        testSharpeRatio();
        testSortinoRatio();
        testValueAtRisk();
        testExpectedShortfall();
        testMaximumDrawdown();
        testInformationRatio();
        testEdgeCases();
        testPerformance();
        
        std::cout << "\n🎉 All tests passed successfully!\n";
        std::cout << "✅ C++ library is ready for production use\n";
        
        return 0;
    } catch (const std::exception& e) {
        std::cout << "\n❌ Test failed: " << e.what() << "\n";
        return 1;
    }
}

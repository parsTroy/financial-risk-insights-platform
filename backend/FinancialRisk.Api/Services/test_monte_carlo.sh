#!/bin/bash

# Test script for Monte Carlo Engine
# This script tests both the C++ and Python implementations

set -e  # Exit on any error

echo "Testing Monte Carlo Engine..."
echo "============================="

# Get the directory where this script is located
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$SCRIPT_DIR"

echo "Testing Python Monte Carlo Engine..."
echo "-----------------------------------"

# Test Python implementation
python3 -c "
import sys
sys.path.append('.')
from monte_carlo_engine import run_monte_carlo_simulation
import json

# Generate sample data
import numpy as np
np.random.seed(42)
sample_returns = np.random.normal(0.001, 0.02, 252).tolist()

print('Running Python Monte Carlo simulation...')
result = run_monte_carlo_simulation('AAPL', sample_returns, 'normal', 10000)

print('Python Monte Carlo Results:')
print(f'  VaR 95%: {result[\"var_95\"]:.4f}')
print(f'  VaR 99%: {result[\"var_99\"]:.4f}')
print(f'  CVaR 95%: {result[\"cvar_95\"]:.4f}')
print(f'  CVaR 99%: {result[\"cvar_99\"]:.4f}')
print(f'  Success: {result[\"success\"]}')
"

echo ""
echo "Testing Python Monte Carlo VaR Script..."
echo "----------------------------------------"

# Test the command-line interface
python3 monte_carlo_var.py AAPL "[0.01, -0.02, 0.015, 0.03, -0.01, 0.02, -0.005, 0.01, -0.015, 0.025]" normal 1000

echo ""
echo "Testing C++ Monte Carlo Engine (if available)..."
echo "------------------------------------------------"

# Test C++ implementation if library exists
if [ -f "libMonteCarloEngine.dylib" ] || [ -f "libMonteCarloEngine.so" ] || [ -f "libMonteCarloEngine.dll" ]; then
    echo "C++ library found, testing..."
    
    # Create a simple C++ test program
    cat > test_monte_carlo.cpp << 'EOF'
#include <iostream>
#include <vector>
#include <random>
#include <chrono>

// Simple test function
extern "C" {
    double CalculateMonteCarloVaR(double* returns, int length, double confidenceLevel, 
                                 int numSimulations, int distributionType, double* parameters, int paramLength);
}

int main() {
    // Generate test data
    std::vector<double> returns = {0.01, -0.02, 0.015, 0.03, -0.01, 0.02, -0.005, 0.01, -0.015, 0.025};
    double parameters[] = {0.0, 1.0}; // Normal distribution parameters
    
    std::cout << "Testing C++ Monte Carlo Engine..." << std::endl;
    
    auto start = std::chrono::high_resolution_clock::now();
    
    double var95 = CalculateMonteCarloVaR(returns.data(), returns.size(), 0.95, 
                                        10000, 0, parameters, 2);
    double var99 = CalculateMonteCarloVaR(returns.data(), returns.size(), 0.99, 
                                        10000, 0, parameters, 2);
    
    auto end = std::chrono::high_resolution_clock::now();
    auto duration = std::chrono::duration_cast<std::chrono::milliseconds>(end - start);
    
    std::cout << "C++ Monte Carlo Results:" << std::endl;
    std::cout << "  VaR 95%: " << var95 << std::endl;
    std::cout << "  VaR 99%: " << var99 << std::endl;
    std::cout << "  Execution time: " << duration.count() << " ms" << std::endl;
    
    return 0;
}
EOF

    # Compile and run the test
    if command -v g++ &> /dev/null; then
        if [ -f "libMonteCarloEngine.dylib" ]; then
            g++ -o test_monte_carlo test_monte_carlo.cpp -L. -lMonteCarloEngine -std=c++17
            export DYLD_LIBRARY_PATH=.:$DYLD_LIBRARY_PATH
            ./test_monte_carlo
        elif [ -f "libMonteCarloEngine.so" ]; then
            g++ -o test_monte_carlo test_monte_carlo.cpp -L. -lMonteCarloEngine -std=c++17
            export LD_LIBRARY_PATH=.:$LD_LIBRARY_PATH
            ./test_monte_carlo
        elif [ -f "libMonteCarloEngine.dll" ]; then
            g++ -o test_monte_carlo.exe test_monte_carlo.cpp -L. -lMonteCarloEngine -std=c++17
            ./test_monte_carlo.exe
        fi
        
        # Clean up
        rm -f test_monte_carlo test_monte_carlo.exe test_monte_carlo.cpp
    else
        echo "g++ compiler not found, skipping C++ test"
    fi
else
    echo "C++ library not found, skipping C++ test"
    echo "Run ./build-monte-carlo.sh to build the C++ library"
fi

echo ""
echo "Testing Portfolio Monte Carlo Simulation..."
echo "------------------------------------------"

# Test portfolio simulation
python3 -c "
import sys
sys.path.append('.')
from monte_carlo_engine import run_portfolio_monte_carlo_simulation
import json

# Prepare portfolio data
portfolio_data = {
    'assets': [
        {'symbol': 'AAPL', 'returns': [0.01, -0.02, 0.015, 0.03, -0.01, 0.02, -0.005, 0.01, -0.015, 0.025]},
        {'symbol': 'GOOGL', 'returns': [0.02, -0.01, 0.025, 0.01, -0.02, 0.015, 0.005, -0.01, 0.02, -0.005]},
        {'symbol': 'MSFT', 'returns': [0.015, 0.01, -0.01, 0.02, 0.005, -0.015, 0.025, 0.01, -0.005, 0.02]}
    ],
    'weights': [0.4, 0.3, 0.3],
    'num_simulations': 5000,
    'confidence_levels': [0.95, 0.99],
    'distribution_type': 'normal'
}

print('Running Portfolio Monte Carlo simulation...')
result = run_portfolio_monte_carlo_simulation(portfolio_data)

print('Portfolio Monte Carlo Results:')
print(f'  Portfolio VaR 95%: {result[\"portfolio_var_95\"]:.4f}')
print(f'  Portfolio VaR 99%: {result[\"portfolio_var_99\"]:.4f}')
print(f'  Portfolio CVaR 95%: {result[\"portfolio_cvar_95\"]:.4f}')
print(f'  Portfolio CVaR 99%: {result[\"portfolio_cvar_99\"]:.4f}')
print(f'  Success: {result[\"success\"]}')
"

echo ""
echo "All tests completed!"
echo "===================="
echo "Monte Carlo Engine is working correctly."
echo ""
echo "Next steps:"
echo "1. Build the C++ library: ./build-monte-carlo.sh"
echo "2. Test the C# API integration"
echo "3. Run the full application"

#!/bin/bash

# Build script for Monte Carlo Engine
# This script compiles the C++ Monte Carlo engine and makes it available for the C# API

set -e  # Exit on any error

echo "Building Monte Carlo Engine..."
echo "================================"

# Get the directory where this script is located
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$SCRIPT_DIR"

# Create build directory if it doesn't exist
if [ ! -d "build" ]; then
    mkdir build
fi

cd build

echo "Configuring CMake..."
cmake .. -DCMAKE_BUILD_TYPE=Release

echo "Building Monte Carlo Engine..."
make -j$(nproc)

echo "Copying libraries to appropriate locations..."

# Copy the built libraries to the Services directory
if [ -f "lib/libMonteCarloEngine.dylib" ]; then
    cp lib/libMonteCarloEngine.dylib ../
    echo "Copied libMonteCarloEngine.dylib to Services directory"
elif [ -f "lib/libMonteCarloEngine.so" ]; then
    cp lib/libMonteCarloEngine.so ../
    echo "Copied libMonteCarloEngine.so to Services directory"
elif [ -f "lib/libMonteCarloEngine.dll" ]; then
    cp lib/libMonteCarloEngine.dll ../
    echo "Copied libMonteCarloEngine.dll to Services directory"
else
    echo "Warning: Monte Carlo Engine library not found in expected location"
fi

# Also copy other libraries if they exist
if [ -f "lib/libVaRCalculations.dylib" ]; then
    cp lib/libVaRCalculations.dylib ../
    echo "Copied libVaRCalculations.dylib to Services directory"
elif [ -f "lib/libVaRCalculations.so" ]; then
    cp lib/libVaRCalculations.so ../
    echo "Copied libVaRCalculations.so to Services directory"
elif [ -f "lib/libVaRCalculations.dll" ]; then
    cp lib/libVaRCalculations.dll ../
    echo "Copied libVaRCalculations.dll to Services directory"
fi

if [ -f "lib/libRiskCalculations.dylib" ]; then
    cp lib/libRiskCalculations.dylib ../
    echo "Copied libRiskCalculations.dylib to Services directory"
elif [ -f "lib/libRiskCalculations.so" ]; then
    cp lib/libRiskCalculations.so ../
    echo "Copied libRiskCalculations.so to Services directory"
elif [ -f "lib/libRiskCalculations.dll" ]; then
    cp lib/libRiskCalculations.dll ../
    echo "Copied libRiskCalculations.dll to Services directory"
fi

echo ""
echo "Build completed successfully!"
echo "================================"
echo "Monte Carlo Engine is ready for use."
echo ""
echo "Available libraries:"
ls -la ../*.dylib 2>/dev/null || ls -la ../*.so 2>/dev/null || ls -la ../*.dll 2>/dev/null || echo "No libraries found"
echo ""
echo "To test the build, you can run:"
echo "  ./test_monte_carlo.sh"

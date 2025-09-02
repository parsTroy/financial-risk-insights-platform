#!/bin/bash

# Build script for RiskCalculations C++ library
# This script builds the C++ library for different platforms

set -e

echo "🔨 Building RiskCalculations C++ library..."

# Create build directory
mkdir -p build
cd build

# Detect platform
if [[ "$OSTYPE" == "darwin"* ]]; then
    PLATFORM="macOS"
    echo "📱 Detected platform: macOS"
elif [[ "$OSTYPE" == "linux-gnu"* ]]; then
    PLATFORM="Linux"
    echo "🐧 Detected platform: Linux"
elif [[ "$OSTYPE" == "msys" ]] || [[ "$OSTYPE" == "cygwin" ]]; then
    PLATFORM="Windows"
    echo "🪟 Detected platform: Windows"
else
    echo "❌ Unknown platform: $OSTYPE"
    exit 1
fi

# Configure with CMake
echo "⚙️  Configuring with CMake..."
cmake .. -DCMAKE_BUILD_TYPE=Release

# Build the library
echo "🔨 Building library..."
cmake --build . --config Release

# Check if build was successful
if [ $? -eq 0 ]; then
    echo "✅ Build successful!"
    
    # List generated files
    echo "📁 Generated files:"
    find . -name "*.dll" -o -name "*.so" -o -name "*.dylib" | while read file; do
        echo "   $file"
    done
    
    # Copy library to appropriate location
    if [[ "$PLATFORM" == "macOS" ]]; then
        cp lib/libRiskCalculations.dylib ../RiskCalculations.dylib
        echo "📋 Copied library to: RiskCalculations.dylib"
    elif [[ "$PLATFORM" == "Linux" ]]; then
        cp lib/libRiskCalculations.so ../RiskCalculations.so
        echo "📋 Copied library to: RiskCalculations.so"
    elif [[ "$PLATFORM" == "Windows" ]]; then
        cp bin/RiskCalculations.dll ../RiskCalculations.dll
        echo "📋 Copied library to: RiskCalculations.dll"
    fi
    
    echo ""
    echo "🎯 C++ library build completed successfully!"
    echo "   Platform: $PLATFORM"
    echo "   Build type: Release"
    echo "   Library ready for use with C# backend"
    
else
    echo "❌ Build failed!"
    exit 1
fi

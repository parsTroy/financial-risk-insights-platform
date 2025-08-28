#!/bin/bash

echo "🚀 Running Financial Risk Platform Tests"
echo "========================================"
echo ""

# Navigate to the test project directory
cd FinancialRisk.Tests

echo "📦 Restoring NuGet packages..."
dotnet restore

echo ""
echo "🧪 Running Unit Tests..."
echo "------------------------"
dotnet test --logger "console;verbosity=detailed" --filter "Category!=Integration"

echo ""
echo "🔗 Running Integration Tests..."
echo "-------------------------------"
dotnet test --logger "console;verbosity=detailed" --filter "Category=Integration"

echo ""
echo "📊 Running All Tests with Coverage..."
echo "------------------------------------"
dotnet test --collect:"XPlat Code Coverage" --results-directory coverage

echo ""
echo "✅ All tests completed!"
echo ""
echo "📁 Test results and coverage reports saved in:"
echo "   - Test results: FinancialRisk.Tests/TestResults/"
echo "   - Coverage: FinancialRisk.Tests/coverage/"
echo ""
echo "💡 To run specific test categories:"
echo "   - Unit tests only: dotnet test --filter Category!=Integration"
echo "   - Integration tests only: dotnet test --filter Category=Integration"
echo "   - Specific test: dotnet test --filter \"FullyQualifiedName~TestName\""

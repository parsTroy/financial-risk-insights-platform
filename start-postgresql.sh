#!/bin/bash

echo "🚀 Starting Financial Risk Platform with PostgreSQL..."

# Check if Docker is running
if ! docker info > /dev/null 2>&1; then
    echo "❌ Docker is not running. Please start Docker Desktop first."
    exit 1
fi

# Start PostgreSQL container
echo "🐳 Starting PostgreSQL container..."
docker compose up -d postgres

# Wait for PostgreSQL to be ready
echo "⏳ Waiting for PostgreSQL to be ready..."
until docker exec financial-risk-postgres pg_isready -U postgres > /dev/null 2>&1; do
    echo "   Waiting for PostgreSQL..."
    sleep 2
done

echo "✅ PostgreSQL is ready!"

# Start pgAdmin (optional)
echo "🌐 Starting pgAdmin..."
docker compose up -d pgadmin

echo "📊 Database Management:"
echo "   - PostgreSQL: localhost:5432"
echo "   - pgAdmin: http://localhost:8080 (admin@financialrisk.com / admin)"
echo "   - Database: FinancialRiskDb"
echo "   - Username: postgres"
echo "   - Password: postgres"

echo ""
echo "🔧 Next steps:"
echo "   1. cd backend/FinancialRisk.Api"
echo "   2. dotnet run"
echo "   3. Open http://localhost:5001/swagger"
echo ""
echo "🧪 To run tests:"
echo "   1. cd backend/FinancialRisk.Tests"
echo "   2. dotnet test"
echo ""
echo "🎯 Your financial data platform is ready!"

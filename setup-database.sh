#!/bin/bash

echo "🗄️ Setting up Financial Risk Platform Database..."

# Check if PostgreSQL container is running
if ! docker ps | grep -q "financial-risk-postgres"; then
    echo "❌ PostgreSQL container is not running. Starting it first..."
    docker compose up -d postgres
    
    # Wait for PostgreSQL to be ready
    echo "⏳ Waiting for PostgreSQL to be ready..."
    until docker exec financial-risk-postgres pg_isready -U postgres > /dev/null 2>&1; do
        echo "   Waiting for PostgreSQL..."
        sleep 2
    done
fi

echo "✅ PostgreSQL is running"

# Check if database exists
if docker exec financial-risk-postgres psql -U postgres -lqt | cut -d \| -f 1 | grep -qw FinancialRiskDb; then
    echo "🗑️ Dropping existing database..."
    docker exec financial-risk-postgres psql -U postgres -c "DROP DATABASE \"FinancialRiskDb\";"
fi

echo "📊 Creating new database..."
docker exec financial-risk-postgres psql -U postgres -c "CREATE DATABASE \"FinancialRiskDb\";"

echo "🏗️ Creating tables from schema.sql..."
docker exec -i financial-risk-postgres psql -U postgres -d FinancialRiskDb < data/schema.sql

echo "✅ Database setup complete!"
echo ""
echo "🔧 Next steps:"
echo "   1. cd backend/FinancialRisk.Api"
echo "   2. dotnet run"
echo ""
echo "📊 Database Management:"
echo "   - PostgreSQL: localhost:5432"
echo "   - pgAdmin: http://localhost:8080 (admin@financialrisk.com / admin)"
echo "   - Database: FinancialRiskDb"
echo "   - Username: postgres"
echo "   - Password: postgres"
echo ""
echo "🎯 Your database is ready for the application!"

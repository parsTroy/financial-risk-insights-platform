# PostgreSQL Setup Guide for Financial Risk Platform

## 🚀 **Why PostgreSQL?**

PostgreSQL is the **optimal choice** for financial applications because:

- **Precise Decimal Handling**: NUMERIC type for exact financial calculations
- **JSON Support**: Store complex financial data structures
- **Analytical Performance**: Excellent for complex financial queries
- **ACID Compliance**: Critical for financial transaction integrity
- **Scalability**: Handles large datasets efficiently

## 🐳 **Option 1: Docker (Recommended for Development)**

### 1. Start PostgreSQL Container
```bash
# From the project root directory
docker compose up -d postgres
```

### 2. Verify Connection
```bash
# Check if PostgreSQL is running
docker ps

# Connect to database
docker exec -it financial-risk-postgres psql -U postgres -d FinancialRiskDb
```

### 3. Access pgAdmin (Optional)
- Open http://localhost:8080
- Login: admin@financialrisk.com / admin
- Add server: localhost:5432, postgres/postgres

## 💻 **Option 2: Local PostgreSQL Installation**

### macOS (using Homebrew)
```bash
# Install PostgreSQL
brew install postgresql@15

# Start service
brew services start postgresql@15

# Create database and user
createdb FinancialRiskDb
createuser -s postgres
```

### Windows
1. Download PostgreSQL from https://www.postgresql.org/download/windows/
2. Install with default settings
3. Create database `FinancialRiskDb`
4. Set password for `postgres` user to `postgres`

### Linux (Ubuntu/Debian)
```bash
# Install PostgreSQL
sudo apt update
sudo apt install postgresql postgresql-contrib

# Start service
sudo systemctl start postgresql
sudo systemctl enable postgresql

# Switch to postgres user and create database
sudo -u postgres psql
CREATE DATABASE "FinancialRiskDb";
CREATE USER postgres WITH PASSWORD 'postgres';
GRANT ALL PRIVILEGES ON DATABASE "FinancialRiskDb" TO postgres;
\q
```

## 🔧 **Configuration**

### Connection String
The application is configured to use:
```
Host=localhost;Database=FinancialRiskDb;Username=postgres;Password=postgres;Port=5432
```

### Environment Variables (Optional)
You can override the connection string using environment variables:
```bash
export ConnectionStrings__DefaultConnection="Host=localhost;Database=FinancialRiskDb;Username=postgres;Password=postgres;Port=5432"
```

## 🧪 **Testing the Setup**

### 1. Build the Application
```bash
cd backend/FinancialRisk.Api
dotnet build
```

### 2. Run the Application
```bash
dotnet run
```

### 3. Check Database Creation
The application will automatically:
- Create the database if it doesn't exist
- Create all tables with proper PostgreSQL types
- Seed with sample financial data

### 4. Run Tests
```bash
cd ../FinancialRisk.Tests
dotnet test
```

## 📊 **Database Schema**

### Tables Created
- **Assets**: Financial instruments (stocks, ETFs, bonds)
- **Prices**: Historical OHLCV data with NUMERIC precision
- **Portfolios**: Investment portfolios with risk strategies
- **PortfolioHoldings**: Asset allocations and weights

### PostgreSQL-Specific Features
- **SERIAL**: Auto-incrementing primary keys
- **NUMERIC(18,6)**: Precise decimal for financial calculations
- **BIGINT**: Large integers for volume data
- **Composite Keys**: Efficient portfolio holdings management

## 🔍 **Verification Commands**

### Connect to Database
```bash
# Using Docker
docker exec -it financial-risk-postgres psql -U postgres -d FinancialRiskDb

# Using local installation
psql -h localhost -U postgres -d FinancialRiskDb
```

### Check Tables
```sql
-- List all tables
\dt

-- Check table structure
\d Assets
\d Prices
\d Portfolios
\d PortfolioHoldings

-- Check sample data
SELECT COUNT(*) FROM Assets;
SELECT COUNT(*) FROM Prices;
SELECT COUNT(*) FROM Portfolios;
```

### Sample Queries
```sql
-- Get all technology stocks
SELECT Symbol, Name, Sector FROM Assets WHERE Sector = 'Technology';

-- Get recent prices for AAPL
SELECT Date, Close, Volume FROM Prices p
JOIN Assets a ON p.AssetId = a.Id
WHERE a.Symbol = 'AAPL'
ORDER BY Date DESC LIMIT 10;

-- Get portfolio performance
SELECT p.Name, COUNT(ph.AssetId) as AssetCount, 
       SUM(ph.Weight) as TotalWeight
FROM Portfolios p
LEFT JOIN PortfolioHoldings ph ON p.Id = ph.PortfolioId
GROUP BY p.Id, p.Name;
```

## 🚨 **Troubleshooting**

### Common Issues

#### 1. Connection Refused
```bash
# Check if PostgreSQL is running
docker ps  # for Docker
brew services list  # for macOS
sudo systemctl status postgresql  # for Linux
```

#### 2. Authentication Failed
```bash
# Reset postgres user password
docker exec -it financial-risk-postgres psql -U postgres
ALTER USER postgres PASSWORD 'postgres';
\q
```

#### 3. Database Already Exists
```bash
# Drop and recreate
docker exec -it financial-risk-postgres psql -U postgres
DROP DATABASE "FinancialRiskDb";
CREATE DATABASE "FinancialRiskDb";
\q
```

#### 4. Port Already in Use
```bash
# Check what's using port 5432
lsof -i :5432

# Kill process or change port in docker-compose.yml
```

### Performance Tuning
```sql
-- Enable query logging
ALTER SYSTEM SET log_statement = 'all';
ALTER SYSTEM SET log_min_duration_statement = 1000;

-- Reload configuration
SELECT pg_reload_conf();

-- Check slow queries
SELECT query, mean_time, calls 
FROM pg_stat_statements 
ORDER BY mean_time DESC 
LIMIT 10;
```

## 🔄 **Migration from SQL Server**

If you were previously using SQL Server:
1. **Data Types**: NUMERIC instead of decimal, BIGINT instead of bigint
2. **Identity**: SERIAL instead of IDENTITY
3. **Functions**: Use PostgreSQL equivalents (e.g., GETDATE() → NOW())
4. **Connection**: Npgsql instead of SqlClient

## 📈 **Next Steps**

1. **Run the Application**: `dotnet run` from the API project
2. **Test API Endpoints**: Use Swagger at `/swagger`
3. **Explore Data**: Use pgAdmin or psql to examine the database
4. **Build Frontend**: Integrate with Blazor components
5. **Add Financial Models**: Implement risk calculations and portfolio optimization

## 🎯 **Success Indicators**

✅ **Database Connection**: Application starts without connection errors
✅ **Table Creation**: All 4 tables exist with proper structure
✅ **Data Seeding**: Sample data is populated (14 assets, 3 portfolios)
✅ **API Endpoints**: All CRUD operations work correctly
✅ **Tests Pass**: All 6 database tests pass successfully

PostgreSQL is now your financial data powerhouse! 🚀

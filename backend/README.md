# Financial Risk Insights Platform - Backend API

This is the backend API for the Financial Risk Insights Platform, built with ASP.NET Core 9.0, Entity Framework Core, and **PostgreSQL** for optimal financial data handling.

## 🚀 Features

- **Asset Management**: CRUD operations for financial assets (stocks, ETFs, bonds)
- **Price Data**: Historical price data storage with precise decimal precision
- **Portfolio Management**: Create and manage investment portfolios
- **Portfolio Performance**: Calculate portfolio returns and performance metrics
- **Data Seeding**: Automatic population with sample financial data
- **PostgreSQL Optimized**: Built for financial applications with NUMERIC precision

## 🛠️ Prerequisites

- .NET 9.0 SDK
- **PostgreSQL 15** (or Docker for containerized setup)
- Visual Studio 2022 or VS Code

## 📦 Installation & Setup

### 🐳 **Option 1: Docker (Recommended)**

1. **Start PostgreSQL Container**
   ```bash
   # From project root
   docker-compose up -d postgres
   ```

2. **Verify Connection**
   ```bash
   docker ps
   # Should show financial-risk-postgres running on port 5432
   ```

### 💻 **Option 2: Local PostgreSQL**

#### macOS
```bash
brew install postgresql@15
brew services start postgresql@15
createdb FinancialRiskDb
```

#### Windows
- Download from https://www.postgresql.org/download/windows/
- Install with default settings
- Create database `FinancialRiskDb`
- Set postgres user password to `postgres`

#### Linux
```bash
sudo apt install postgresql postgresql-contrib
sudo systemctl start postgresql
sudo -u postgres createdb FinancialRiskDb
```

### 3. **Install Dependencies**
```bash
cd backend/FinancialRisk.Api
dotnet restore
```

### 4. **Run the Application**
```bash
dotnet run
```

The API will be available at `https://localhost:5001` or `http://localhost:5000`.

## 🗄️ Database Schema

### **Why PostgreSQL for Finance?**

- **NUMERIC(18,6)**: Exact decimal precision for financial calculations
- **JSON Support**: Store complex financial data structures
- **Analytical Performance**: Excellent for complex financial queries
- **ACID Compliance**: Critical for financial transaction integrity

### Tables Structure
```
Assets
├── Id (SERIAL PRIMARY KEY)
├── Symbol (VARCHAR(10) UNIQUE)
├── Name (VARCHAR(100))
├── Sector (VARCHAR(50))
├── Industry (VARCHAR(50))
├── AssetType (VARCHAR(50))
├── CreatedAt (TIMESTAMP)
└── UpdatedAt (TIMESTAMP)

Prices
├── Id (SERIAL PRIMARY KEY)
├── AssetId (INT REFERENCES Assets)
├── Date (DATE)
├── Open (NUMERIC(18,6))
├── High (NUMERIC(18,6))
├── Low (NUMERIC(18,6))
├── Close (NUMERIC(18,6))
├── AdjustedClose (NUMERIC(18,6))
├── Volume (BIGINT)
└── CreatedAt (TIMESTAMP)

Portfolios
├── Id (SERIAL PRIMARY KEY)
├── Name (VARCHAR(100))
├── Description (VARCHAR(500))
├── Strategy (VARCHAR(50))
├── TargetReturn (NUMERIC(18,6))
├── MaxRisk (NUMERIC(18,6))
├── CreatedAt (TIMESTAMP)
├── UpdatedAt (TIMESTAMP)
└── IsActive (BOOLEAN)

PortfolioHoldings
├── PortfolioId (INT REFERENCES Portfolios)
├── AssetId (INT REFERENCES Assets)
├── Weight (NUMERIC(18,6))
├── Quantity (NUMERIC(18,6))
├── AverageCost (NUMERIC(18,6))
├── CreatedAt (TIMESTAMP)
└── UpdatedAt (TIMESTAMP)
```

## 🔌 API Endpoints

### Assets
- `GET /api/Assets` - List all assets
- `GET /api/Assets/{id}` - Get asset by ID
- `GET /api/Assets/symbol/{symbol}` - Get asset by symbol
- `GET /api/Assets/{id}/prices` - Get price history
- `POST /api/Assets` - Create new asset
- `PUT /api/Assets/{id}` - Update asset
- `DELETE /api/Assets/{id}` - Delete asset

### Portfolios
- `GET /api/Portfolios` - List all portfolios
- `GET /api/Portfolios/{id}` - Get portfolio details
- `GET /api/Portfolios/{id}/performance` - Calculate performance
- `POST /api/Portfolios` - Create portfolio
- `PUT /api/Portfolios/{id}` - Update portfolio
- `DELETE /api/Portfolios/{id}` - Delete portfolio

### Portfolio Holdings
- `POST /api/Portfolios/{id}/holdings` - Add asset to portfolio
- `PUT /api/Portfolios/{id}/holdings/{assetId}` - Update holding
- `DELETE /api/Portfolios/{id}/holdings/{assetId}` - Remove holding

## 🌱 Sample Data

The application automatically seeds the database with:

- **14 Assets**: Major stocks (AAPL, MSFT, GOOGL, NVDA) and ETFs (SPY, QQQ, IEF, GLD)
- **2 Years of Price Data**: Daily OHLCV data with realistic volatility
- **3 Sample Portfolios**: Conservative, Balanced, and Aggressive strategies
- **Portfolio Allocations**: Realistic weightings based on risk strategy

## 🧪 Testing

### Run Tests
```bash
cd backend/FinancialRisk.Tests
dotnet test
```

### Test Database
Tests use separate PostgreSQL databases to avoid conflicts:
- `FinancialRiskTestDb1` through `FinancialRiskTestDb6`
- Each test gets its own isolated database

## 🔧 Development

### Adding New Entities
1. Create entity class in `Models/`
2. Add DbSet to `FinancialRiskDbContext`
3. Configure in `OnModelCreating`
4. Add to `DataSeederService` if needed
5. Create controller for API endpoints

### Database Migrations
Currently using `EnsureCreated()` for simplicity. For production:
```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

## 📊 Performance Features

- **Indexes**: On symbol and date combinations
- **Eager Loading**: Include() for related data
- **NUMERIC Precision**: Exact financial calculations
- **Composite Keys**: Efficient portfolio holdings
- **Cascade Delete**: Automatic cleanup

## 🚨 Error Handling

- Comprehensive try-catch blocks
- Structured logging
- Proper HTTP status codes
- Input validation with Data Annotations

## 🔐 Security Considerations

- Input validation on all endpoints
- SQL injection protection via EF Core
- Proper HTTP status codes
- Audit logging

## 🚀 Next Steps

1. **Authentication**: Add user management and authorization
2. **Real-time Data**: Integrate live financial data feeds
3. **Risk Metrics**: Implement VaR, Sharpe ratio, correlation analysis
4. **Portfolio Optimization**: Add Markowitz optimization algorithms
5. **Frontend Integration**: Connect with Blazor components

## 📞 Support

For issues or questions:
- Check the `SETUP_POSTGRESQL.md` for detailed setup instructions
- Review the Docker Compose configuration
- Check application logs for error details

## 🎯 Success Indicators

✅ **PostgreSQL Running**: Database accessible on port 5432
✅ **Application Starts**: No connection errors
✅ **Tables Created**: All 4 tables with proper structure
✅ **Data Seeded**: Sample financial data populated
✅ **API Working**: All endpoints respond correctly
✅ **Tests Pass**: All database tests successful

The platform is now optimized for financial applications with PostgreSQL! 🚀📈

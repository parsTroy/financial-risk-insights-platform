# Development Setup Guide

This guide explains how to set up the Financial Risk Insights Platform for local development and GitHub Pages deployment.

## Overview

The platform consists of:
- **Backend**: C# ASP.NET Core API with Python/C++ integration
- **Frontend**: Blazor WebAssembly application
- **Database**: PostgreSQL
- **Deployment**: GitHub Pages (frontend only)

## Prerequisites

### Required Software
- **.NET 9.0 SDK**
- **Python 3.11+**
- **PostgreSQL 15+**
- **Git**
- **Node.js 18+** (for development tools)

### Required Python Packages
```bash
pip install numpy scipy pandas psutil
```

## Local Development Setup

### 1. Clone the Repository
```bash
git clone https://github.com/your-username/financial-risk-insights-platform.git
cd financial-risk-insights-platform
```

### 2. Database Setup
```bash
# Start PostgreSQL (macOS with Homebrew)
brew services start postgresql

# Or start with Docker
docker run --name postgres-dev -e POSTGRES_PASSWORD=postgres -p 5432:5432 -d postgres:15

# Create database
createdb financialrisk_dev
```

### 3. Backend Setup
```bash
cd backend

# Restore dependencies
dotnet restore

# Update database
dotnet ef database update --project FinancialRisk.Api

# Run the API
dotnet run --project FinancialRisk.Api
```

The API will be available at `https://localhost:7001`

### 4. Frontend Setup
```bash
cd frontend/FinancialRisk.Frontend

# Restore dependencies
dotnet restore

# Run the frontend
dotnet run
```

The frontend will be available at `https://localhost:5001`

### 5. Full Stack Development
1. Start the backend API first
2. Start the frontend application
3. Access the full application at `https://localhost:5001`

## GitHub Pages Deployment

### 1. Enable GitHub Pages
1. Go to your repository settings
2. Navigate to "Pages" section
3. Select "GitHub Actions" as the source
4. The workflow will automatically deploy on push to `main` branch

### 2. Frontend-Only Deployment
The GitHub Pages deployment only includes the frontend. The backend API must be run locally for full functionality.

### 3. Configuration for GitHub Pages
The frontend is configured to work with GitHub Pages:
- Base URL: `https://your-username.github.io/financial-risk-insights-platform`
- API URL: `https://localhost:7001/api` (local development)

## Development Workflow

### 1. Backend Development
```bash
cd backend

# Run tests
dotnet test

# Run specific test category
dotnet test --filter "Category=Risk"

# Build for production
dotnet build --configuration Release
```

### 2. Frontend Development
```bash
cd frontend/FinancialRisk.Frontend

# Run tests
dotnet test

# Build for production
dotnet build --configuration Release

# Publish for GitHub Pages
dotnet publish --configuration Release --output ./publish
```

### 3. Python Models Development
```bash
cd backend/FinancialRisk.Api/Services

# Test Monte Carlo engine
python3 -c "from monte_carlo_engine import MonteCarloVaRCalculator; print('OK')"

# Test Portfolio optimizer
python3 -c "from portfolio_optimizer import PortfolioOptimizer; print('OK')"

# Test Quant models
python3 -c "from python_models.quant_models import QuantModelRegistry; print('OK')"
```

### 4. C++ Models Development
```bash
cd backend/FinancialRisk.Api/Services

# Build C++ engines
chmod +x build-monte-carlo.sh
./build-monte-carlo.sh

# Test C++ engines
chmod +x test_monte_carlo.sh
./test_monte_carlo.sh
```

## Configuration

### Backend Configuration
The backend uses `appsettings.json` for configuration:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=financialrisk_dev;Username=postgres;Password=postgres"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  }
}
```

### Frontend Configuration
The frontend uses `appsettings.json` for API configuration:
```json
{
  "ApiSettings": {
    "BaseUrl": "https://localhost:7001/api",
    "Timeout": 30000,
    "RetryCount": 3
  },
  "GitHubPages": {
    "BaseUrl": "https://your-username.github.io/financial-risk-insights-platform",
    "ApiUrl": "https://localhost:7001/api",
    "IsDevelopment": true
  }
}
```

## Testing

### 1. Backend Tests
```bash
cd backend

# Run all tests
dotnet test

# Run specific test file
dotnet test --filter "FullyQualifiedName~VaRCalculationServiceTests"

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

### 2. Frontend Tests
```bash
cd frontend/FinancialRisk.Frontend

# Run tests
dotnet test

# Build test
dotnet build --configuration Release
```

### 3. Integration Tests
```bash
cd backend

# Run integration tests
dotnet test --filter "Category=Integration"
```

## Troubleshooting

### Common Issues

#### 1. Database Connection Issues
```bash
# Check PostgreSQL status
brew services list | grep postgresql

# Start PostgreSQL
brew services start postgresql

# Check database exists
psql -l | grep financialrisk
```

#### 2. Python Import Errors
```bash
# Install missing packages
pip install numpy scipy pandas psutil

# Check Python version
python3 --version

# Test imports
python3 -c "import numpy, scipy, pandas, psutil; print('All packages available')"
```

#### 3. C++ Build Issues
```bash
# Install build tools (macOS)
xcode-select --install

# Install CMake
brew install cmake

# Check build tools
gcc --version
cmake --version
```

#### 4. Frontend Build Issues
```bash
# Clean and restore
dotnet clean
dotnet restore
dotnet build

# Check .NET version
dotnet --version
```

#### 5. API Connection Issues
- Ensure backend is running on `https://localhost:7001`
- Check CORS configuration in backend
- Verify API endpoints are accessible
- Check firewall settings

### Debugging

#### 1. Backend Debugging
```bash
# Run with detailed logging
dotnet run --project FinancialRisk.Api --verbosity detailed

# Check logs
tail -f logs/app.log
```

#### 2. Frontend Debugging
```bash
# Run with development tools
dotnet run --project FinancialRisk.Frontend --environment Development

# Check browser console for errors
# Open Developer Tools (F12)
```

#### 3. Database Debugging
```bash
# Connect to database
psql -d financialrisk_dev

# Check tables
\dt

# Check data
SELECT * FROM "StockQuotes" LIMIT 10;
```

## Production Considerations

### 1. Backend Deployment
The backend is not deployed to GitHub Pages. For production deployment, consider:
- **Azure App Service**
- **AWS Elastic Beanstalk**
- **Google Cloud Run**
- **Docker containers**

### 2. Database Deployment
For production database, consider:
- **Azure Database for PostgreSQL**
- **AWS RDS**
- **Google Cloud SQL**
- **Managed PostgreSQL services**

### 3. Frontend Deployment
The frontend is deployed to GitHub Pages:
- Automatic deployment on push to `main`
- Static hosting only
- No server-side processing

### 4. API Configuration
For production, update the frontend API configuration:
```json
{
  "ApiSettings": {
    "BaseUrl": "https://your-production-api.com/api",
    "Timeout": 30000,
    "RetryCount": 3
  }
}
```

## Security Considerations

### 1. API Security
- Use HTTPS in production
- Implement authentication/authorization
- Validate all inputs
- Use environment variables for secrets

### 2. Database Security
- Use strong passwords
- Enable SSL connections
- Regular security updates
- Backup strategies

### 3. Frontend Security
- Validate all user inputs
- Sanitize data before display
- Use HTTPS
- Implement proper error handling

## Performance Optimization

### 1. Backend Optimization
- Use connection pooling
- Implement caching
- Optimize database queries
- Use async/await patterns

### 2. Frontend Optimization
- Enable compression
- Use CDN for static assets
- Implement lazy loading
- Optimize bundle size

### 3. Database Optimization
- Create proper indexes
- Optimize queries
- Use connection pooling
- Regular maintenance

## Monitoring and Logging

### 1. Application Logging
- Structured logging with Serilog
- Log levels configuration
- Error tracking and alerting
- Performance monitoring

### 2. Database Monitoring
- Query performance monitoring
- Connection pool monitoring
- Disk usage monitoring
- Backup monitoring

### 3. Frontend Monitoring
- Error tracking (Sentry, etc.)
- Performance monitoring
- User analytics
- Real user monitoring

## Contributing

### 1. Development Process
1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Run tests
5. Submit a pull request

### 2. Code Standards
- Follow C# coding conventions
- Use meaningful variable names
- Add XML documentation
- Write unit tests

### 3. Testing Requirements
- All new code must have tests
- Maintain test coverage >80%
- Integration tests for new features
- Performance tests for critical paths

## Support

For issues and questions:
1. Check this documentation
2. Search existing issues
3. Create a new issue
4. Contact the development team

## License

This project is licensed under the MIT License - see the LICENSE file for details.

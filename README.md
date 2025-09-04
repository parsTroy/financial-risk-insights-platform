# Financial Risk Insights Platform

A comprehensive quantitative risk management and portfolio optimization platform built with modern web technologies.

## 🌟 Features

### 🧮 Value at Risk (VaR) Calculator
- **Historical VaR**: Calculate VaR using historical simulation
- **Parametric VaR**: Normal distribution-based VaR calculations
- **Monte Carlo VaR**: Advanced simulation-based VaR with multiple distributions
- **CVaR Support**: Conditional Value at Risk calculations
- **Multiple Confidence Levels**: 90%, 95%, 99% and custom levels

### 📊 Portfolio Optimization
- **Markowitz Mean-Variance**: Classic portfolio optimization
- **Minimum Variance**: Risk-minimizing portfolios
- **Maximum Sharpe Ratio**: Return-to-risk optimized portfolios
- **Risk Parity**: Equal risk contribution portfolios
- **Black-Litterman**: Market view integration
- **Equal Weight**: Simple equal allocation

### 🎲 Monte Carlo Simulation
- **Multiple Distributions**: Normal, T-Student, GARCH, Copula, Skewed-T, Mixture
- **Configurable Parameters**: Mean, volatility, degrees of freedom, correlation
- **Large Scale Simulations**: Support for up to 100,000 simulations
- **Reproducible Results**: Fixed random seed support
- **Performance Optimized**: Efficient simulation execution

### 📈 Advanced Analytics
- **Efficient Frontier**: Risk-return optimization curves
- **Risk Attribution**: Component-level risk analysis
- **Stress Testing**: Scenario-based risk assessment
- **Interactive Charts**: Real-time visualization
- **Export Capabilities**: Data export for further analysis

## 🚀 Quick Start

### Prerequisites
- **.NET 9.0 SDK**
- **Python 3.11+**
- **PostgreSQL 15+**
- **Git**

### Local Development

1. **Clone the repository**
   ```bash
   git clone https://github.com/your-username/financial-risk-insights-platform.git
   cd financial-risk-insights-platform
   ```

2. **Setup database**
   ```bash
   # Start PostgreSQL
   brew services start postgresql
   
   # Create database
   createdb financialrisk_dev
   ```

3. **Install Python dependencies**
   ```bash
   pip install numpy scipy pandas psutil
   ```

4. **Start backend API**
   ```bash
   cd backend
   dotnet restore
   dotnet ef database update --project FinancialRisk.Api
   dotnet run --project FinancialRisk.Api
   ```

5. **Start frontend**
   ```bash
   cd frontend/FinancialRisk.Frontend
   dotnet restore
   dotnet run
   ```

6. **Access the application**
   - Frontend: `https://localhost:5001`
   - API: `https://localhost:7001`

### GitHub Pages Deployment

The frontend is automatically deployed to GitHub Pages:

1. **Enable GitHub Pages**
   - Go to repository Settings > Pages
   - Select "GitHub Actions" as source

2. **Deploy**
   ```bash
   git add .
   git commit -m "Deploy to GitHub Pages"
   git push origin main
   ```

3. **Access deployed frontend**
   - URL: `https://your-username.github.io/financial-risk-insights-platform`
   - Note: Backend API must be run locally for full functionality

## 🏗️ Architecture

### Backend (.NET 9.0)
- **ASP.NET Core API**: RESTful API with comprehensive endpoints
- **Entity Framework Core**: Database ORM with PostgreSQL
- **Python Integration**: Monte Carlo and portfolio optimization engines
- **C++ Integration**: Performance-critical quantitative models
- **Comprehensive Testing**: 300+ unit and integration tests

### Frontend (Blazor WebAssembly)
- **Modern UI**: Responsive design with Bootstrap 5
- **Component Architecture**: Reusable Blazor components
- **API Integration**: HTTP client with error handling
- **Real-time Updates**: Live data visualization
- **Mobile Responsive**: Works on all device sizes

### Database (PostgreSQL)
- **Financial Data**: Stock quotes, market data, portfolio data
- **User Data**: Portfolios, calculations, historical data
- **Performance**: Optimized queries and indexing

## 📁 Project Structure

```
financial-risk-insights-platform/
├── backend/                    # C# ASP.NET Core API
│   ├── FinancialRisk.Api/     # Main API project
│   ├── FinancialRisk.Tests/   # Test project
│   └── FinancialRisk.sln      # Solution file
├── frontend/                   # Blazor WebAssembly
│   └── FinancialRisk.Frontend/ # Frontend project
├── .github/                    # GitHub Actions CI/CD
│   └── workflows/             # CI/CD pipelines
├── docs/                      # Documentation
└── README.md                  # This file
```

## 🧮 Quantitative Models

### Risk Management
- **VaR Calculations**: Historical, Parametric, Monte Carlo
- **CVaR Calculations**: Conditional Value at Risk
- **Stress Testing**: Scenario-based risk assessment
- **Risk Attribution**: Component-level risk analysis

### Portfolio Optimization
- **Markowitz Optimization**: Mean-variance optimization
- **Black-Litterman**: Market view integration
- **Risk Parity**: Equal risk contribution
- **Minimum Variance**: Risk-minimizing portfolios
- **Maximum Sharpe**: Return-to-risk optimization

### Statistical Models
- **GARCH Models**: Volatility modeling
- **Copula Models**: Dependency modeling
- **Regime Switching**: Market regime detection
- **Monte Carlo**: Advanced simulation techniques

### Pricing Models
- **Black-Scholes**: Option pricing
- **Binomial Tree**: Discrete option pricing
- **Monte Carlo Pricing**: Simulation-based pricing

## 🔧 Technology Stack

### Backend
- **.NET 9.0**: Latest .NET framework
- **ASP.NET Core**: Web API framework
- **Entity Framework Core**: Database ORM
- **PostgreSQL**: Relational database
- **Python 3.11**: Quantitative models
- **C++**: Performance-critical engines

### Frontend
- **Blazor WebAssembly**: Client-side web framework
- **Bootstrap 5**: CSS framework
- **System.Net.Http.Json**: HTTP client
- **Microsoft.Extensions.Http**: HTTP client factory

### Development
- **Git**: Version control
- **GitHub Actions**: CI/CD pipeline
- **GitHub Pages**: Frontend hosting
- **Docker**: Containerization (optional)

## 📊 API Endpoints

### VaR Calculations
- `POST /api/var/calculate` - Calculate VaR
- `POST /api/monte-carlo/var` - Monte Carlo VaR
- `POST /api/monte-carlo/portfolio-var` - Portfolio VaR

### Portfolio Optimization
- `POST /api/portfolio/optimize` - Optimize portfolio
- `POST /api/portfolio/efficient-frontier` - Generate efficient frontier
- `POST /api/portfolio/risk-budgeting` - Risk budgeting

### Quantitative Models
- `POST /api/interop/execute-model` - Execute quant model
- `GET /api/interop/list-models` - List available models
- `POST /api/interop/execute-python` - Execute Python model

## 🧪 Testing

### Test Coverage
- **300+ Unit Tests**: Comprehensive test coverage
- **Integration Tests**: API and database integration
- **Python Model Tests**: Quantitative model validation
- **C++ Engine Tests**: Performance model validation
- **Frontend Tests**: UI component testing

### Running Tests
```bash
# Backend tests
cd backend
dotnet test

# Frontend tests
cd frontend/FinancialRisk.Frontend
dotnet test

# Python model tests
cd backend/FinancialRisk.Api/Services
python3 -c "from monte_carlo_engine import MonteCarloVaRCalculator; print('OK')"
```

## 🚀 Deployment

### GitHub Pages (Frontend)
- **Automatic Deployment**: On push to main branch
- **Static Hosting**: Blazor WebAssembly frontend
- **Free Hosting**: No cost for public repositories
- **Custom Domain**: Optional custom domain support

### Local Development (Full Stack)
- **Backend API**: Run locally with `dotnet run`
- **Database**: Local PostgreSQL instance
- **Frontend**: Connect to local API
- **Full Functionality**: All features available

### Production Deployment (Future)
- **Backend**: Azure App Service, AWS Elastic Beanstalk, etc.
- **Database**: Azure Database, AWS RDS, etc.
- **Frontend**: GitHub Pages or CDN
- **Full Stack**: Complete production deployment

## 📚 Documentation

- **[Development Setup](DEVELOPMENT_SETUP.md)**: Complete setup guide
- **[GitHub Pages Deployment](GITHUB_PAGES_DEPLOYMENT.md)**: Deployment guide
- **[CI/CD Documentation](.github/CI_CD_README.md)**: Pipeline documentation
- **[API Documentation](backend/README.md)**: Backend API guide
- **[Frontend Documentation](frontend/FinancialRisk.Frontend/README.md)**: Frontend guide

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Run tests
5. Submit a pull request

### Development Guidelines
- Follow C# coding conventions
- Write unit tests for new features
- Update documentation
- Ensure CI/CD pipeline passes

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🆘 Support

For issues and questions:
1. Check the documentation
2. Search existing issues
3. Create a new issue
4. Contact the development team

## 🎯 Roadmap

### Completed Features
- ✅ Monte Carlo VaR Engine
- ✅ Portfolio Optimization Suite
- ✅ Blazor WebAssembly Frontend
- ✅ Python/C++ Integration
- ✅ Comprehensive Test Suite
- ✅ CI/CD Pipeline
- ✅ GitHub Pages Deployment

### Planned Features
- [ ] Real-time data integration
- [ ] Advanced charting and visualization
- [ ] User authentication and authorization
- [ ] Portfolio performance tracking
- [ ] Risk scenario modeling
- [ ] Export to Excel/PDF
- [ ] Mobile app (Xamarin/MAUI)
- [ ] Cloud deployment options

## 🙏 Acknowledgments

- **QuantLib**: Quantitative finance library inspiration
- **Blazor**: Microsoft's web framework
- **Bootstrap**: UI framework
- **Open Source Community**: For various libraries and tools

---

**Built with ❤️ for quantitative finance professionals**

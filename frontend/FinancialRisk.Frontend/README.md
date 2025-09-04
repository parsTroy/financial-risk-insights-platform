# Financial Risk Insights Frontend

A modern Blazor WebAssembly frontend for the Financial Risk Insights Platform, providing advanced quantitative risk management and portfolio optimization tools.

## Features

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

## Technology Stack

- **Blazor WebAssembly**: Client-side web framework
- **.NET 9.0**: Latest .NET framework
- **Bootstrap 5**: Responsive UI framework
- **Open Iconic**: Icon library
- **System.Net.Http.Json**: HTTP client with JSON support
- **Microsoft.Extensions.Http**: HTTP client factory
- **Microsoft.Extensions.Logging**: Logging framework

## Project Structure

```
FinancialRisk.Frontend/
├── Components/           # Reusable UI components
│   └── NavMenu.razor    # Navigation menu
├── Layout/              # Layout components
│   └── MainLayout.razor # Main application layout
├── Models/              # Data models
│   ├── ApiResponse.cs   # API response wrapper
│   ├── VaRModels.cs     # VaR calculation models
│   └── PortfolioModels.cs # Portfolio optimization models
├── Pages/               # Application pages
│   ├── Home.razor       # Landing page
│   ├── VaRCalculator.razor # VaR calculation page
│   ├── MonteCarloSimulation.razor # Monte Carlo page
│   └── PortfolioOptimization.razor # Portfolio optimization page
├── Services/            # API services
│   ├── ApiConfiguration.cs # API configuration
│   ├── ApiService.cs    # Base API service
│   ├── VaRApiService.cs # VaR API service
│   └── PortfolioApiService.cs # Portfolio API service
├── wwwroot/             # Static web assets
├── appsettings.json     # Configuration
├── Program.cs           # Application startup
└── README.md           # This file
```

## Getting Started

### Prerequisites
- .NET 9.0 SDK
- Node.js (for development)
- Backend API running on https://localhost:7001

### Installation

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd financial-risk-insights-platform/frontend/FinancialRisk.Frontend
   ```

2. **Restore dependencies**
   ```bash
   dotnet restore
   ```

3. **Update API configuration**
   Edit `appsettings.json` to point to your backend API:
   ```json
   {
     "ApiSettings": {
       "BaseUrl": "https://localhost:7001/api",
       "Timeout": 30000,
       "RetryCount": 3
     }
   }
   ```

4. **Run the application**
   ```bash
   dotnet run
   ```

5. **Open in browser**
   Navigate to `https://localhost:5001` (or the URL shown in the console)

### Development

#### Running in Development Mode
```bash
dotnet run --environment Development
```

#### Building for Production
```bash
dotnet build --configuration Release
```

#### Publishing
```bash
dotnet publish --configuration Release --output ./publish
```

## Configuration

### API Settings
The application connects to the backend API through configurable settings in `appsettings.json`:

```json
{
  "ApiSettings": {
    "BaseUrl": "https://localhost:7001/api",
    "Timeout": 30000,
    "RetryCount": 3
  }
}
```

### Environment Variables
You can override configuration using environment variables:
- `API_BASE_URL`: Override the API base URL
- `API_TIMEOUT`: Override the API timeout in milliseconds

## Usage

### VaR Calculator
1. Navigate to the VaR Calculator page
2. Enter historical returns (comma-separated values)
3. Select confidence level and time horizon
4. Choose calculation method (Historical, Parametric, Monte Carlo)
5. Click "Calculate VaR" to see results

### Monte Carlo Simulation
1. Go to the Monte Carlo Simulation page
2. Configure simulation parameters:
   - Distribution type
   - Number of simulations
   - Mean return and volatility
   - Random seed (optional)
3. Click "Run Monte Carlo Simulation"
4. View VaR results and simulation statistics

### Portfolio Optimization
1. Visit the Portfolio Optimization page
2. Add assets with expected returns and volatilities
3. Select optimization method
4. Configure risk aversion and constraints
5. Click "Optimize Portfolio" to see optimal weights

## API Integration

The frontend integrates with the backend API through dedicated services:

- **VaRApiService**: Handles VaR calculations and Monte Carlo simulations
- **PortfolioApiService**: Manages portfolio optimization requests
- **ApiService**: Base HTTP client with error handling and retry logic

### Error Handling
- Automatic retry on transient failures
- User-friendly error messages
- Logging for debugging
- Graceful degradation

### Performance
- HTTP client connection pooling
- Request timeout configuration
- Efficient JSON serialization
- Lazy loading of components

## Styling and UI

### Bootstrap 5 Integration
- Responsive design for all screen sizes
- Modern card-based layouts
- Consistent color scheme
- Accessible form controls

### Custom Styling
- Financial-themed color palette
- Professional typography
- Interactive hover effects
- Loading states and animations

## Browser Support

- Chrome 90+
- Firefox 88+
- Safari 14+
- Edge 90+

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests if applicable
5. Submit a pull request

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Support

For support and questions:
- Create an issue in the repository
- Contact the development team
- Check the documentation

## Roadmap

### Upcoming Features
- [ ] Interactive charts and visualizations
- [ ] Real-time data integration
- [ ] Advanced reporting capabilities
- [ ] User authentication and authorization
- [ ] Portfolio performance tracking
- [ ] Risk scenario modeling
- [ ] Export to Excel/PDF
- [ ] Mobile-responsive improvements

### Technical Improvements
- [ ] Unit test coverage
- [ ] Integration tests
- [ ] Performance optimization
- [ ] Accessibility improvements
- [ ] PWA capabilities
- [ ] Offline support

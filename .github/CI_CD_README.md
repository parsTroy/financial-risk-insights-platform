# CI Pipeline Documentation

This document describes the comprehensive CI pipeline for the Financial Risk Insights Platform, covering all components including the C# backend, Blazor frontend, Python quantitative models, and C++ engines.

## Overview

The CI pipeline consists of two main workflows:

1. **`ci.yml`** - Main CI pipeline for testing and validation
2. **`test-quant-models.yml`** - Specialized testing for quantitative models

**Note**: Deployment is handled by Railway, not GitHub Actions.

## Workflow Details

### 1. Main CI Pipeline (`ci.yml`)

**Triggers:**
- Push to `dev` or `main` branches
- Pull requests to `dev` or `main` branches
- Manual workflow dispatch

**Jobs:**

#### Backend Tests
- **Environment:** Ubuntu Latest
- **Services:** PostgreSQL 15
- **Dependencies:** .NET 9.0, Python 3.11
- **Tests:**
  - C# unit tests (excluding integration)
  - C++ Monte Carlo engine build test
  - Python Monte Carlo engine functionality test
  - Python Portfolio Optimizer test
  - Python Quant Models Registry test

#### Frontend Tests
- **Environment:** Ubuntu Latest
- **Dependencies:** .NET 9.0, Node.js 18
- **Tests:**
  - Blazor WebAssembly build test
  - Frontend file structure validation
  - Page and service component validation

#### Integration Tests
- **Environment:** Ubuntu Latest
- **Services:** PostgreSQL 15
- **Dependencies:** .NET 9.0, Python 3.11
- **Tests:**
  - Full integration test suite
  - Database integration tests
  - API endpoint integration tests

#### Security Checks
- **Environment:** Ubuntu Latest
- **Dependencies:** .NET 9.0
- **Tests:**
  - Security vulnerability scanning
  - Code quality checks
  - Dependency analysis

#### Build and Package
- **Environment:** Ubuntu Latest
- **Dependencies:** .NET 9.0, Python 3.11
- **Outputs:**
  - Production-ready backend package
  - Production-ready frontend package
  - Deployment artifacts

### 2. Railway Deployment

**Deployment Platform:** Railway
- **Frontend:** Static site hosting
- **Backend:** .NET service hosting
- **Database:** PostgreSQL managed service
- **Python Models:** Python service hosting
- **C++ Engines:** Docker container hosting

**Deployment Process:**
1. Connect GitHub repository to Railway
2. Configure services (frontend, backend, database, Python, C++)
3. Set environment variables
4. Deploy automatically on push to main branch

### 3. Quantitative Models Testing (`test-quant-models.yml`)

**Triggers:**
- Push to `dev` or `main` branches (quantitative model changes only)
- Pull requests affecting quantitative models
- Manual workflow dispatch

**Jobs:**

#### Python Monte Carlo Engine
- **Tests:**
  - Module import validation
  - VaR calculation functionality
  - Script execution testing

#### Python Portfolio Optimizer
- **Tests:**
  - Module import validation
  - Portfolio optimization functionality
  - Multiple optimization method testing

#### Python Quant Models Registry
- **Tests:**
  - Registry instantiation
  - Model listing functionality
  - Model execution testing

#### C++ Monte Carlo Engine
- **Tests:**
  - Build script validation
  - Source file verification
  - CMake configuration testing

#### C++ Quant Engine
- **Tests:**
  - Source file verification
  - CMake configuration testing
  - Build environment validation

#### .NET Integration
- **Tests:**
  - Monte Carlo controller tests
  - Portfolio optimization controller tests
  - Interop service tests

## Test Coverage

### Backend Tests
- **Unit Tests:** 65+ tests per category
  - Risk Models (VaR, CVaR, Stress Testing)
  - Portfolio Optimization (Markowitz, Black-Litterman, Risk Parity)
  - Monte Carlo Simulations (Multiple distributions)
  - Statistical Models (GARCH, Copula, Regime Switching)
  - Pricing Models (Black-Scholes, Binomial Tree)

- **Integration Tests:**
  - API Controller integration
  - Database context integration
  - Service layer integration
  - Interop service integration

### Frontend Tests
- **Build Tests:**
  - Blazor WebAssembly compilation
  - Asset validation
  - Configuration validation

- **Component Tests:**
  - Page component validation
  - Service component validation
  - Navigation component validation

### Quantitative Models Tests
- **Python Models:**
  - Monte Carlo VaR calculations
  - Portfolio optimization algorithms
  - Risk management models
  - Statistical models
  - Pricing models

- **C++ Models:**
  - Monte Carlo engine compilation
  - Quant engine compilation
  - Performance validation

## Environment Configuration

### Local Development Environment
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=financialrisk_dev;Username=postgres;Password=postgres"
  },
  "ApiSettings": {
    "BaseUrl": "https://localhost:7001/api",
    "Timeout": 30000,
    "RetryCount": 3
  }
}
```

### GitHub Pages Environment
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

## Dependencies

### Backend Dependencies
- **.NET 9.0 SDK**
- **PostgreSQL 15**
- **Python 3.11**
- **Python Packages:**
  - numpy
  - scipy
  - pandas
  - psutil

### Frontend Dependencies
- **.NET 9.0 SDK**
- **Node.js 18**
- **Blazor WebAssembly**

### Build Dependencies
- **CMake** (for C++ builds)
- **Build Essential** (for C++ compilation)
- **Git** (for version control)

## Artifacts

### Build Artifacts
- **Backend Package:** `financial-risk-platform-{run_number}/api/`
- **Frontend Package:** `financial-risk-platform-{run_number}/frontend/`
- **Configuration Files:** `docker-compose.yml`, `setup-database.sh`, `start-postgresql.sh`
- **Build Info:** `BUILD_INFO.txt`

### Deployment Artifacts
- **Staging:** `staging-deployment-{run_number}/`
- **Production:** `production-deployment-{run_number}/`

## Monitoring and Notifications

### Health Checks
- **Staging:** `https://staging-api.financialrisk.com/health`
- **Production:** `https://api.financialrisk.com/health`

### Notifications
- **Deployment Success:** Automated notifications for successful deployments
- **Test Failures:** Automated notifications for test failures
- **Security Issues:** Automated notifications for security vulnerabilities

## Troubleshooting

### Common Issues

#### Python Dependencies
```bash
# Install missing Python packages
pip install numpy scipy pandas psutil
```

#### C++ Build Issues
```bash
# Install build dependencies
sudo apt-get update
sudo apt-get install -y build-essential cmake
```

#### Database Connection Issues
```bash
# Check PostgreSQL service
sudo systemctl status postgresql
sudo systemctl start postgresql
```

#### .NET Build Issues
```bash
# Clean and restore
dotnet clean
dotnet restore
dotnet build
```

### Debugging

#### Enable Verbose Logging
```yaml
- name: Run tests with verbose logging
  run: dotnet test --verbosity detailed --logger "console;verbosity=detailed"
```

#### Check Build Artifacts
```bash
# List build artifacts
ls -la deployment-package/
```

#### Validate Configuration
```bash
# Check configuration files
cat appsettings.Production.json
```

## Best Practices

### Code Quality
- All code must pass linting checks
- Unit tests must have >80% coverage
- Integration tests must pass
- Security scans must pass

### Deployment
- Always deploy to staging first
- Run health checks after deployment
- Monitor application logs
- Keep deployment artifacts for rollback

### Testing
- Run all tests before merging
- Test quantitative models thoroughly
- Validate API endpoints
- Check frontend functionality

## Security Considerations

### Secrets Management
- Use GitHub Secrets for sensitive data
- Never commit passwords or API keys
- Use environment variables for configuration
- Rotate secrets regularly

### Access Control
- Limit deployment permissions
- Use environment-specific access controls
- Monitor deployment activities
- Audit access logs

### Vulnerability Scanning
- Regular dependency scanning
- Security patch management
- Code vulnerability analysis
- Container security scanning

## Performance Monitoring

### Metrics
- Build time tracking
- Test execution time
- Deployment duration
- Application performance

### Optimization
- Parallel job execution
- Caching strategies
- Resource optimization
- Build optimization

## Future Enhancements

### Planned Improvements
- [ ] Container-based deployments
- [ ] Kubernetes orchestration
- [ ] Advanced monitoring and alerting
- [ ] Automated rollback capabilities
- [ ] Performance benchmarking
- [ ] Load testing integration
- [ ] Multi-environment support
- [ ] Blue-green deployments

### Technical Debt
- [ ] Migrate to newer GitHub Actions versions
- [ ] Implement advanced caching strategies
- [ ] Add more comprehensive security scanning
- [ ] Enhance monitoring and observability
- [ ] Improve error handling and recovery

## Support

For issues with the CI/CD pipeline:
1. Check the GitHub Actions logs
2. Review this documentation
3. Contact the development team
4. Create an issue in the repository

## Contributing

To contribute to the CI/CD pipeline:
1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Test thoroughly
5. Submit a pull request

## License

This CI/CD configuration is part of the Financial Risk Insights Platform and is licensed under the same terms as the main project.

using FinancialRisk.Api.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace FinancialRisk.Api.Services
{
    public class PortfolioBuilderService : IPortfolioBuilderService
    {
        private readonly FinancialRiskContext _context;
        private readonly ILogger<PortfolioBuilderService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public PortfolioBuilderService(
            FinancialRiskContext context, 
            ILogger<PortfolioBuilderService> logger,
            IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<ApiResponse<AssetSearchResponse>> SearchAssetsAsync(AssetSearchRequest request)
        {
            try
            {
                _logger.LogInformation("Searching assets with query: {Query}", request.Query);

                // Mock asset search - in production, this would call a financial data API
                var mockAssets = GetMockAssets();
                
                var filteredAssets = mockAssets
                    .Where(a => a.Symbol.Contains(request.Query.ToUpper()) || 
                               a.Name.ToLower().Contains(request.Query.ToLower()))
                    .Skip((request.Page - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .ToList();

                var totalCount = mockAssets.Count(a => a.Symbol.Contains(request.Query.ToUpper()) || 
                                                     a.Name.ToLower().Contains(request.Query.ToLower()));

                var response = new AssetSearchResponse
                {
                    Assets = filteredAssets,
                    TotalCount = totalCount,
                    Page = request.Page,
                    PageSize = request.PageSize,
                    TotalPages = (int)Math.Ceiling((double)totalCount / request.PageSize)
                };

                return new ApiResponse<AssetSearchResponse>
                {
                    IsSuccess = true,
                    Data = response
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching assets");
                return new ApiResponse<AssetSearchResponse>
                {
                    IsSuccess = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<ApiResponse<Portfolio>> SavePortfolioAsync(PortfolioSaveRequest request)
        {
            try
            {
                _logger.LogInformation("Saving portfolio: {PortfolioName}", request.Name);

                // Validate portfolio
                var validation = await ValidatePortfolioAsync(new Portfolio
                {
                    Name = request.Name,
                    Description = request.Description,
                    Assets = request.Assets,
                    IsPublic = request.IsPublic,
                    Metadata = request.Metadata
                });

                if (!validation.Data?.IsValid == true)
                {
                    return new ApiResponse<Portfolio>
                    {
                        IsSuccess = false,
                        ErrorMessage = "Portfolio validation failed",
                        Errors = validation.Data?.Errors
                    };
                }

                var portfolio = new Portfolio
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = request.Name,
                    Description = request.Description,
                    Assets = request.Assets,
                    CreatedDate = DateTime.UtcNow,
                    ModifiedDate = DateTime.UtcNow,
                    CreatedBy = "user", // TODO: Get from authentication context
                    IsPublic = request.IsPublic,
                    Metadata = request.Metadata
                };

                // In a real implementation, you would save to database
                // For now, we'll just return the portfolio
                _logger.LogInformation("Portfolio saved successfully: {PortfolioId}", portfolio.Id);

                return new ApiResponse<Portfolio>
                {
                    IsSuccess = true,
                    Data = portfolio
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving portfolio");
                return new ApiResponse<Portfolio>
                {
                    IsSuccess = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<ApiResponse<Portfolio>> LoadPortfolioAsync(PortfolioLoadRequest request)
        {
            try
            {
                _logger.LogInformation("Loading portfolio: {PortfolioId}", request.PortfolioId);

                // Mock portfolio loading - in production, this would load from database
                var mockPortfolio = GetMockPortfolio(request.PortfolioId);

                if (mockPortfolio == null)
                {
                    return new ApiResponse<Portfolio>
                    {
                        IsSuccess = false,
                        ErrorMessage = "Portfolio not found"
                    };
                }

                return new ApiResponse<Portfolio>
                {
                    IsSuccess = true,
                    Data = mockPortfolio
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading portfolio");
                return new ApiResponse<Portfolio>
                {
                    IsSuccess = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<ApiResponse<PortfolioListResponse>> ListPortfoliosAsync(PortfolioListRequest request)
        {
            try
            {
                _logger.LogInformation("Listing portfolios");

                // Mock portfolio listing - in production, this would query database
                var mockPortfolios = GetMockPortfolios();
                
                var filteredPortfolios = mockPortfolios
                    .Where(p => string.IsNullOrEmpty(request.SearchQuery) || 
                               p.Name.Contains(request.SearchQuery, StringComparison.OrdinalIgnoreCase) ||
                               p.Description.Contains(request.SearchQuery, StringComparison.OrdinalIgnoreCase))
                    .OrderByDescending(p => p.ModifiedDate)
                    .Skip((request.Page - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .ToList();

                var totalCount = mockPortfolios.Count(p => string.IsNullOrEmpty(request.SearchQuery) || 
                                                         p.Name.Contains(request.SearchQuery, StringComparison.OrdinalIgnoreCase) ||
                                                         p.Description.Contains(request.SearchQuery, StringComparison.OrdinalIgnoreCase));

                var response = new PortfolioListResponse
                {
                    Portfolios = filteredPortfolios,
                    TotalCount = totalCount,
                    Page = request.Page,
                    PageSize = request.PageSize,
                    TotalPages = (int)Math.Ceiling((double)totalCount / request.PageSize)
                };

                return new ApiResponse<PortfolioListResponse>
                {
                    IsSuccess = true,
                    Data = response
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error listing portfolios");
                return new ApiResponse<PortfolioListResponse>
                {
                    IsSuccess = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<ApiResponse<bool>> DeletePortfolioAsync(string portfolioId)
        {
            try
            {
                _logger.LogInformation("Deleting portfolio: {PortfolioId}", portfolioId);

                // Mock portfolio deletion - in production, this would delete from database
                _logger.LogInformation("Portfolio deleted successfully: {PortfolioId}", portfolioId);

                return new ApiResponse<bool>
                {
                    IsSuccess = true,
                    Data = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting portfolio");
                return new ApiResponse<bool>
                {
                    IsSuccess = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<ApiResponse<PortfolioSummary>> GetPortfolioSummaryAsync(string portfolioId)
        {
            try
            {
                _logger.LogInformation("Getting portfolio summary: {PortfolioId}", portfolioId);

                var portfolio = GetMockPortfolio(portfolioId);
                if (portfolio == null)
                {
                    return new ApiResponse<PortfolioSummary>
                    {
                        IsSuccess = false,
                        ErrorMessage = "Portfolio not found"
                    };
                }

                var summary = CalculatePortfolioSummary(portfolio);

                return new ApiResponse<PortfolioSummary>
                {
                    IsSuccess = true,
                    Data = summary
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting portfolio summary");
                return new ApiResponse<PortfolioSummary>
                {
                    IsSuccess = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<ApiResponse<PortfolioValidationResult>> ValidatePortfolioAsync(Portfolio portfolio)
        {
            try
            {
                _logger.LogInformation("Validating portfolio: {PortfolioName}", portfolio.Name);

                var result = new PortfolioValidationResult { IsValid = true };

                // Validate portfolio name
                if (string.IsNullOrWhiteSpace(portfolio.Name))
                {
                    result.Errors.Add("Portfolio name is required");
                    result.IsValid = false;
                }

                // Validate assets
                if (!portfolio.Assets.Any())
                {
                    result.Errors.Add("Portfolio must contain at least one asset");
                    result.IsValid = false;
                }

                // Validate weights
                var totalWeight = portfolio.Assets.Sum(a => a.Weight);
                if (Math.Abs(totalWeight - 100.0) > 0.01)
                {
                    result.Errors.Add($"Portfolio weights must total 100%. Current total: {totalWeight:F1}%");
                    result.IsValid = false;
                }

                // Check for negative weights
                var negativeWeights = portfolio.Assets.Where(a => a.Weight < 0).ToList();
                if (negativeWeights.Any())
                {
                    result.Errors.Add("Asset weights cannot be negative");
                    result.IsValid = false;
                }

                // Check for duplicate assets
                var duplicateAssets = portfolio.Assets
                    .GroupBy(a => a.Symbol)
                    .Where(g => g.Count() > 1)
                    .Select(g => g.Key)
                    .ToList();
                
                if (duplicateAssets.Any())
                {
                    result.Errors.Add($"Duplicate assets found: {string.Join(", ", duplicateAssets)}");
                    result.IsValid = false;
                }

                // Add warnings
                if (portfolio.Assets.Count > 20)
                {
                    result.Warnings.Add("Portfolio contains more than 20 assets, which may impact diversification");
                }

                if (portfolio.Assets.Any(a => a.Weight > 50))
                {
                    result.Warnings.Add("Some assets have weights greater than 50%, which may indicate concentration risk");
                }

                return new ApiResponse<PortfolioValidationResult>
                {
                    IsSuccess = true,
                    Data = result
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating portfolio");
                return new ApiResponse<PortfolioValidationResult>
                {
                    IsSuccess = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<ApiResponse<PortfolioRebalanceRequest>> RebalancePortfolioAsync(PortfolioRebalanceRequest request)
        {
            try
            {
                _logger.LogInformation("Rebalancing portfolio: {PortfolioId}", request.PortfolioId);

                // Mock rebalancing logic - in production, this would calculate actual rebalancing
                foreach (var rebalance in request.Rebalances)
                {
                    rebalance.WeightDifference = rebalance.TargetWeight - rebalance.CurrentWeight;
                    rebalance.ValueDifference = rebalance.WeightDifference * 10000; // Mock portfolio value
                }

                return new ApiResponse<PortfolioRebalanceRequest>
                {
                    IsSuccess = true,
                    Data = request
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rebalancing portfolio");
                return new ApiResponse<PortfolioRebalanceRequest>
                {
                    IsSuccess = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<ApiResponse<PortfolioPerformanceMetrics>> GetPortfolioPerformanceAsync(string portfolioId, DateTime startDate, DateTime endDate)
        {
            try
            {
                _logger.LogInformation("Getting portfolio performance: {PortfolioId}", portfolioId);

                // Mock performance calculation - in production, this would calculate actual performance
                var metrics = new PortfolioPerformanceMetrics
                {
                    TotalReturn = 0.15,
                    AnnualizedReturn = 0.12,
                    Volatility = 0.18,
                    SharpeRatio = 0.67,
                    MaxDrawdown = -0.08,
                    VaR95 = -0.03,
                    CVaR95 = -0.05,
                    Beta = 1.2,
                    Alpha = 0.02,
                    TrackingError = 0.05,
                    InformationRatio = 0.4,
                    CalculationDate = DateTime.UtcNow
                };

                return new ApiResponse<PortfolioPerformanceMetrics>
                {
                    IsSuccess = true,
                    Data = metrics
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting portfolio performance");
                return new ApiResponse<PortfolioPerformanceMetrics>
                {
                    IsSuccess = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<ApiResponse<PortfolioComparisonResult>> ComparePortfoliosAsync(PortfolioComparisonRequest request)
        {
            try
            {
                _logger.LogInformation("Comparing portfolios");

                // Mock portfolio comparison - in production, this would compare actual portfolios
                var result = new PortfolioComparisonResult
                {
                    Portfolios = new List<PortfolioPerformance>(),
                    Benchmark = new PortfolioPerformance
                    {
                        PortfolioId = "BENCHMARK",
                        Name = "S&P 500",
                        TotalReturn = 0.10,
                        AnnualizedReturn = 0.08,
                        Volatility = 0.15,
                        SharpeRatio = 0.53,
                        MaxDrawdown = -0.06,
                        VaR95 = -0.025,
                        CVaR95 = -0.04
                    }
                };

                return new ApiResponse<PortfolioComparisonResult>
                {
                    IsSuccess = true,
                    Data = result
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error comparing portfolios");
                return new ApiResponse<PortfolioComparisonResult>
                {
                    IsSuccess = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<ApiResponse<List<string>>> GetAvailableSectorsAsync()
        {
            try
            {
                _logger.LogInformation("Getting available sectors");

                var sectors = new List<string>
                {
                    "Technology", "Healthcare", "Financial Services", "Consumer Discretionary",
                    "Consumer Staples", "Energy", "Industrials", "Materials", "Real Estate",
                    "Utilities", "Communication Services"
                };

                return new ApiResponse<List<string>>
                {
                    IsSuccess = true,
                    Data = sectors
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available sectors");
                return new ApiResponse<List<string>>
                {
                    IsSuccess = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<ApiResponse<List<string>>> GetAvailableExchangesAsync()
        {
            try
            {
                _logger.LogInformation("Getting available exchanges");

                var exchanges = new List<string>
                {
                    "NASDAQ", "NYSE", "AMEX", "OTC", "TSX", "LSE", "TSE", "HKEX"
                };

                return new ApiResponse<List<string>>
                {
                    IsSuccess = true,
                    Data = exchanges
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available exchanges");
                return new ApiResponse<List<string>>
                {
                    IsSuccess = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<ApiResponse<Dictionary<string, object>>> GetMarketDataAsync(List<string> symbols)
        {
            try
            {
                _logger.LogInformation("Getting market data for {SymbolCount} symbols", symbols.Count);

                var marketData = new Dictionary<string, object>();
                var random = new Random();

                foreach (var symbol in symbols)
                {
                    marketData[symbol] = new
                    {
                        Price = Math.Round(100 + random.NextDouble() * 200, 2),
                        Change = Math.Round((random.NextDouble() - 0.5) * 10, 2),
                        ChangePercent = Math.Round((random.NextDouble() - 0.5) * 5, 2),
                        Volume = random.Next(1000000, 10000000),
                        MarketCap = random.Next(1000000000, 1000000000000)
                    };
                }

                return new ApiResponse<Dictionary<string, object>>
                {
                    IsSuccess = true,
                    Data = marketData
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting market data");
                return new ApiResponse<Dictionary<string, object>>
                {
                    IsSuccess = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        private List<AssetSearchResult> GetMockAssets()
        {
            return new List<AssetSearchResult>
            {
                new() { Symbol = "AAPL", Name = "Apple Inc.", ExpectedReturn = 0.08, Volatility = 0.20, Sector = "Technology", Exchange = "NASDAQ", MarketCap = 3000000000000, Price = 150.00, Change = 2.50, ChangePercent = 1.69 },
                new() { Symbol = "MSFT", Name = "Microsoft Corporation", ExpectedReturn = 0.10, Volatility = 0.18, Sector = "Technology", Exchange = "NASDAQ", MarketCap = 2800000000000, Price = 350.00, Change = -1.20, ChangePercent = -0.34 },
                new() { Symbol = "GOOGL", Name = "Alphabet Inc.", ExpectedReturn = 0.12, Volatility = 0.25, Sector = "Technology", Exchange = "NASDAQ", MarketCap = 1800000000000, Price = 140.00, Change = 3.20, ChangePercent = 2.34 },
                new() { Symbol = "AMZN", Name = "Amazon.com Inc.", ExpectedReturn = 0.15, Volatility = 0.30, Sector = "Consumer Discretionary", Exchange = "NASDAQ", MarketCap = 1500000000000, Price = 130.00, Change = -2.10, ChangePercent = -1.59 },
                new() { Symbol = "TSLA", Name = "Tesla Inc.", ExpectedReturn = 0.20, Volatility = 0.40, Sector = "Consumer Discretionary", Exchange = "NASDAQ", MarketCap = 800000000000, Price = 250.00, Change = 5.50, ChangePercent = 2.25 },
                new() { Symbol = "NVDA", Name = "NVIDIA Corporation", ExpectedReturn = 0.18, Volatility = 0.35, Sector = "Technology", Exchange = "NASDAQ", MarketCap = 1200000000000, Price = 450.00, Change = 8.20, ChangePercent = 1.86 },
                new() { Symbol = "META", Name = "Meta Platforms Inc.", ExpectedReturn = 0.14, Volatility = 0.28, Sector = "Communication Services", Exchange = "NASDAQ", MarketCap = 700000000000, Price = 280.00, Change = -1.50, ChangePercent = -0.53 },
                new() { Symbol = "NFLX", Name = "Netflix Inc.", ExpectedReturn = 0.11, Volatility = 0.22, Sector = "Communication Services", Exchange = "NASDAQ", MarketCap = 200000000000, Price = 450.00, Change = 2.30, ChangePercent = 0.51 }
            };
        }

        private Portfolio? GetMockPortfolio(string portfolioId)
        {
            if (portfolioId == "mock-portfolio-1")
            {
                return new Portfolio
                {
                    Id = portfolioId,
                    Name = "Tech Growth Portfolio",
                    Description = "A diversified technology-focused portfolio",
                    Assets = new List<PortfolioAsset>
                    {
                        new() { Symbol = "AAPL", Name = "Apple Inc.", Weight = 30, ExpectedReturn = 0.08, Volatility = 0.20, Price = 150.00, Quantity = 100, Value = 15000, Sector = "Technology" },
                        new() { Symbol = "MSFT", Name = "Microsoft Corporation", Weight = 25, ExpectedReturn = 0.10, Volatility = 0.18, Price = 350.00, Quantity = 50, Value = 17500, Sector = "Technology" },
                        new() { Symbol = "GOOGL", Name = "Alphabet Inc.", Weight = 20, ExpectedReturn = 0.12, Volatility = 0.25, Price = 140.00, Quantity = 80, Value = 11200, Sector = "Technology" },
                        new() { Symbol = "NVDA", Name = "NVIDIA Corporation", Weight = 15, ExpectedReturn = 0.18, Volatility = 0.35, Price = 450.00, Quantity = 20, Value = 9000, Sector = "Technology" },
                        new() { Symbol = "TSLA", Name = "Tesla Inc.", Weight = 10, ExpectedReturn = 0.20, Volatility = 0.40, Price = 250.00, Quantity = 20, Value = 5000, Sector = "Consumer Discretionary" }
                    },
                    CreatedDate = DateTime.UtcNow.AddDays(-30),
                    ModifiedDate = DateTime.UtcNow.AddDays(-5),
                    CreatedBy = "user",
                    IsPublic = true
                };
            }
            return null;
        }

        private List<Portfolio> GetMockPortfolios()
        {
            return new List<Portfolio>
            {
                GetMockPortfolio("mock-portfolio-1")!,
                new Portfolio
                {
                    Id = "mock-portfolio-2",
                    Name = "Conservative Portfolio",
                    Description = "A conservative portfolio focused on stability",
                    Assets = new List<PortfolioAsset>
                    {
                        new() { Symbol = "JNJ", Name = "Johnson & Johnson", Weight = 40, ExpectedReturn = 0.06, Volatility = 0.12, Price = 160.00, Quantity = 100, Value = 16000, Sector = "Healthcare" },
                        new() { Symbol = "PG", Name = "Procter & Gamble", Weight = 30, ExpectedReturn = 0.05, Volatility = 0.10, Price = 150.00, Quantity = 80, Value = 12000, Sector = "Consumer Staples" },
                        new() { Symbol = "KO", Name = "Coca-Cola Company", Weight = 30, ExpectedReturn = 0.04, Volatility = 0.08, Price = 60.00, Quantity = 100, Value = 6000, Sector = "Consumer Staples" }
                    },
                    CreatedDate = DateTime.UtcNow.AddDays(-60),
                    ModifiedDate = DateTime.UtcNow.AddDays(-10),
                    CreatedBy = "user",
                    IsPublic = false
                }
            };
        }

        private PortfolioSummary CalculatePortfolioSummary(Portfolio portfolio)
        {
            var totalWeight = portfolio.Assets.Sum(a => a.Weight);
            var expectedReturn = portfolio.Assets.Sum(a => (a.Weight / 100.0) * a.ExpectedReturn);
            var expectedVolatility = Math.Sqrt(portfolio.Assets.Sum(a => Math.Pow(a.Weight / 100.0, 2) * Math.Pow(a.Volatility, 2)));
            var sharpeRatio = expectedReturn / expectedVolatility;
            
            var riskLevel = expectedVolatility switch
            {
                < 0.15 => "Low",
                < 0.25 => "Medium",
                _ => "High"
            };

            var sectorAllocation = portfolio.Assets
                .GroupBy(a => a.Sector)
                .ToDictionary(g => g.Key, g => g.Sum(a => a.Weight));

            return new PortfolioSummary
            {
                AssetCount = portfolio.Assets.Count,
                TotalWeight = totalWeight,
                ExpectedReturn = expectedReturn,
                ExpectedVolatility = expectedVolatility,
                SharpeRatio = sharpeRatio,
                RiskLevel = riskLevel,
                TotalValue = portfolio.Assets.Sum(a => a.Value),
                SectorAllocation = sectorAllocation
            };
        }
    }
}

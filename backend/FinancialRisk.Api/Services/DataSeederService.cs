using FinancialRisk.Api.Data;
using FinancialRisk.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace FinancialRisk.Api.Services;

public class DataSeederService
{
    private readonly FinancialRiskDbContext _context;
    private readonly ILogger<DataSeederService> _logger;

    public DataSeederService(FinancialRiskDbContext context, ILogger<DataSeederService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SeedDataAsync()
    {
        try
        {
            _logger.LogInformation("Starting database seeding...");

            // Check if data already exists
            try
            {
                if (await _context.Assets.AnyAsync())
                {
                    _logger.LogInformation("Database already contains data. Skipping seeding.");
                    return;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Could not check for existing data: {Message}. This might be expected on first run.", ex.Message);
            }

            // Try to seed data - if tables don't exist, this will fail gracefully
            try
            {
                await SeedAssetsAsync();
                await SeedPricesAsync();
                await SeedPortfoliosAsync();
                await SeedPortfolioHoldingsAsync();

                await _context.SaveChangesAsync();
                _logger.LogInformation("Database seeding completed successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Seeding failed - tables may not exist yet: {Message}", ex.Message);
                _logger.LogInformation("Please ensure the database schema is created first using the schema.sql file.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during database seeding");
            // Don't throw - just log the error and continue
        }
    }

    private async Task SeedAssetsAsync()
    {
        var assets = new List<Asset>
        {
            new Asset { Symbol = "AAPL", Name = "Apple Inc.", Sector = "Technology", Industry = "Consumer Electronics", AssetType = "Stock" },
            new Asset { Symbol = "MSFT", Name = "Microsoft Corporation", Sector = "Technology", Industry = "Software", AssetType = "Stock" },
            new Asset { Symbol = "GOOGL", Name = "Alphabet Inc.", Sector = "Technology", Industry = "Internet Services", AssetType = "Stock" },
            new Asset { Symbol = "AMZN", Name = "Amazon.com Inc.", Sector = "Consumer Cyclical", Industry = "Internet Retail", AssetType = "Stock" },
            new Asset { Symbol = "TSLA", Name = "Tesla Inc.", Sector = "Consumer Cyclical", Industry = "Auto Manufacturers", AssetType = "Stock" },
            new Asset { Symbol = "NVDA", Name = "NVIDIA Corporation", Sector = "Technology", Industry = "Semiconductors", AssetType = "Stock" },
            new Asset { Symbol = "JPM", Name = "JPMorgan Chase & Co.", Sector = "Financial Services", Industry = "Banks", AssetType = "Stock" },
            new Asset { Symbol = "JNJ", Name = "Johnson & Johnson", Sector = "Healthcare", Industry = "Drug Manufacturers", AssetType = "Stock" },
            new Asset { Symbol = "V", Name = "Visa Inc.", Sector = "Financial Services", Industry = "Credit Services", AssetType = "Stock" },
            new Asset { Symbol = "PG", Name = "Procter & Gamble Co.", Sector = "Consumer Defensive", Industry = "Household & Personal Products", AssetType = "Stock" },
            new Asset { Symbol = "SPY", Name = "SPDR S&P 500 ETF Trust", Sector = "ETF", Industry = "ETF", AssetType = "ETF" },
            new Asset { Symbol = "QQQ", Name = "Invesco QQQ Trust", Sector = "ETF", Industry = "ETF", AssetType = "ETF" },
            new Asset { Symbol = "IEF", Name = "iShares 7-10 Year Treasury Bond ETF", Sector = "ETF", Industry = "ETF", AssetType = "ETF" },
            new Asset { Symbol = "GLD", Name = "SPDR Gold Shares", Sector = "ETF", Industry = "ETF", AssetType = "ETF" }
        };

        await _context.Assets.AddRangeAsync(assets);
        _logger.LogInformation("Seeded {Count} assets", assets.Count);
    }

    private async Task SeedPricesAsync()
    {
        var assets = await _context.Assets.ToListAsync();
        var prices = new List<Price>();
        var random = new Random(42); // Fixed seed for reproducible data

        foreach (var asset in assets)
        {
            // Generate 2 years of daily price data
            var startDate = DateTime.Today.AddYears(-2);
            var currentPrice = GetBasePrice(asset.Symbol);
            
            for (var date = startDate; date <= DateTime.Today; date = date.AddDays(1))
            {
                if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                    continue;

                var volatility = GetVolatility(asset.Symbol);
                var dailyReturn = (random.NextDouble() - 0.5) * volatility;
                currentPrice *= (1 + dailyReturn);

                var high = currentPrice * (1 + random.NextDouble() * 0.02);
                var low = currentPrice * (1 - random.NextDouble() * 0.02);
                var open = currentPrice * (1 + (random.NextDouble() - 0.5) * 0.01);
                var volume = random.Next(1000000, 10000000);

                prices.Add(new Price
                {
                    AssetId = asset.Id,
                    Date = date,
                    Open = (decimal)open,
                    High = (decimal)high,
                    Low = (decimal)low,
                    Close = (decimal)currentPrice,
                    AdjustedClose = (decimal)currentPrice,
                    Volume = volume
                });
            }
        }

        await _context.Prices.AddRangeAsync(prices);
        _logger.LogInformation("Seeded {Count} price records", prices.Count);
    }

    private async Task SeedPortfoliosAsync()
    {
        var portfolios = new List<Portfolio>
        {
            new Portfolio 
            { 
                Name = "Conservative Growth", 
                Description = "Low-risk portfolio focused on stable growth",
                Strategy = "Conservative",
                TargetReturn = 0.06m,
                MaxRisk = 0.12m
            },
            new Portfolio 
            { 
                Name = "Balanced", 
                Description = "Moderate risk portfolio with balanced allocation",
                Strategy = "Moderate",
                TargetReturn = 0.08m,
                MaxRisk = 0.18m
            },
            new Portfolio 
            { 
                Name = "Aggressive Growth", 
                Description = "High-risk portfolio targeting maximum growth",
                Strategy = "Aggressive",
                TargetReturn = 0.12m,
                MaxRisk = 0.25m
            }
        };

        await _context.Portfolios.AddRangeAsync(portfolios);
        _logger.LogInformation("Seeded {Count} portfolios", portfolios.Count);
    }

    private async Task SeedPortfolioHoldingsAsync()
    {
        var portfolios = await _context.Portfolios.ToListAsync();
        var assets = await _context.Assets.Where(a => a.AssetType == "Stock").Take(10).ToListAsync();
        var holdings = new List<PortfolioHolding>();

        foreach (var portfolio in portfolios)
        {
            var weights = GetPortfolioWeights(portfolio.Strategy, assets.Count);
            
            for (int i = 0; i < assets.Count && i < weights.Length; i++)
            {
                holdings.Add(new PortfolioHolding
                {
                    PortfolioId = portfolio.Id,
                    AssetId = assets[i].Id,
                    Weight = weights[i],
                    Quantity = 1000, // Sample quantity
                    AverageCost = 100m // Sample average cost
                });
            }
        }

        await _context.PortfolioHoldings.AddRangeAsync(holdings);
        _logger.LogInformation("Seeded {Count} portfolio holdings", holdings.Count);
    }

    private double GetBasePrice(string symbol)
    {
        return symbol switch
        {
            "AAPL" => 150.0,
            "MSFT" => 300.0,
            "GOOGL" => 2500.0,
            "AMZN" => 3000.0,
            "TSLA" => 800.0,
            "NVDA" => 400.0,
            "JPM" => 150.0,
            "JNJ" => 160.0,
            "V" => 250.0,
            "PG" => 140.0,
            _ => 100.0
        };
    }

    private double GetVolatility(string symbol)
    {
        return symbol switch
        {
            "TSLA" => 0.04, // High volatility
            "NVDA" => 0.035,
            "AAPL" => 0.025,
            "MSFT" => 0.025,
            "GOOGL" => 0.03,
            "AMZN" => 0.035,
            "JPM" => 0.025,
            "JNJ" => 0.02, // Low volatility
            "V" => 0.025,
            "PG" => 0.02,
            _ => 0.025
        };
    }

    private decimal[] GetPortfolioWeights(string strategy, int assetCount)
    {
        var weights = new decimal[assetCount];
        var random = new Random(42);

        switch (strategy)
        {
            case "Conservative":
                // More concentrated in fewer assets
                for (int i = 0; i < assetCount; i++)
                {
                    weights[i] = i < 5 ? 0.15m : 0.05m;
                }
                break;
            case "Moderate":
                // Balanced allocation
                for (int i = 0; i < assetCount; i++)
                {
                    weights[i] = 1.0m / assetCount;
                }
                break;
            case "Aggressive":
                // Concentrated in top performers
                for (int i = 0; i < assetCount; i++)
                {
                    weights[i] = i < 3 ? 0.25m : 0.025m;
                }
                break;
            default:
                // Equal weight
                for (int i = 0; i < assetCount; i++)
                {
                    weights[i] = 1.0m / assetCount;
                }
                break;
        }

        // Normalize weights to sum to 1.0
        var total = weights.Sum();
        for (int i = 0; i < weights.Length; i++)
        {
            weights[i] = weights[i] / total;
        }

        return weights;
    }
}

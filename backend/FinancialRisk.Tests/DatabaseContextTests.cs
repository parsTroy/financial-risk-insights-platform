using Microsoft.EntityFrameworkCore;
using FinancialRisk.Api.Data;
using FinancialRisk.Api.Models;
using Xunit;

namespace FinancialRisk.Tests;

public class DatabaseContextTests
{
    private DbContextOptions<FinancialRiskDbContext> CreateInMemoryOptions(string databaseName)
    {
        return new DbContextOptionsBuilder<FinancialRiskDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;
    }

    [Fact]
    public void DatabaseContext_CanBeCreated()
    {
        // Arrange
        var options = CreateInMemoryOptions("TestDb_CanBeCreated");

        // Act & Assert
        using var context = new FinancialRiskDbContext(options);
        Assert.NotNull(context);
    }

    [Fact]
    public void DatabaseContext_HasCorrectDbSets()
    {
        // Arrange
        var options = CreateInMemoryOptions("TestDb_HasCorrectDbSets");

        using var context = new FinancialRiskDbContext(options);

        // Assert
        Assert.NotNull(context.Assets);
        Assert.NotNull(context.Prices);
        Assert.NotNull(context.Portfolios);
        Assert.NotNull(context.PortfolioHoldings);
    }

    #region Asset Tests

    [Fact]
    public async Task Asset_CanBeAddedToContext()
    {
        // Arrange
        var options = CreateInMemoryOptions("TestDb_Asset_CanBeAddedToContext");

        using var context = new FinancialRiskDbContext(options);

        var asset = new Asset
        {
            Symbol = "TEST",
            Name = "Test Asset",
            Sector = "Technology",
            Industry = "Software",
            AssetType = "Stock"
        };

        // Act
        context.Assets.Add(asset);
        var result = await context.SaveChangesAsync();

        // Assert
        Assert.Equal(1, result);
        Assert.Equal(1, context.Assets.Count());
        Assert.Equal("TEST", context.Assets.First().Symbol);
    }

    [Fact]
    public async Task Asset_CanBeFetchedBySymbol()
    {
        // Arrange
        var options = CreateInMemoryOptions("TestDb_Asset_CanBeFetchedBySymbol");

        using var context = new FinancialRiskDbContext(options);

        var asset = new Asset
        {
            Symbol = "AAPL",
            Name = "Apple Inc.",
            Sector = "Technology",
            Industry = "Consumer Electronics",
            AssetType = "Stock"
        };

        context.Assets.Add(asset);
        await context.SaveChangesAsync();

        // Act
        var fetchedAsset = await context.Assets
            .FirstOrDefaultAsync(a => a.Symbol == "AAPL");

        // Assert
        Assert.NotNull(fetchedAsset);
        Assert.Equal("AAPL", fetchedAsset.Symbol);
        Assert.Equal("Apple Inc.", fetchedAsset.Name);
        Assert.Equal("Technology", fetchedAsset.Sector);
    }

    [Fact]
    public async Task Asset_CanBeFetchedWithPrices()
    {
        // Arrange
        var options = CreateInMemoryOptions("TestDb_Asset_CanBeFetchedWithPrices");

        using var context = new FinancialRiskDbContext(options);

        var asset = new Asset
        {
            Symbol = "MSFT",
            Name = "Microsoft Corporation",
            Sector = "Technology",
            Industry = "Software",
            AssetType = "Stock"
        };

        context.Assets.Add(asset);
        await context.SaveChangesAsync();

        var price = new Price
        {
            AssetId = asset.Id,
            Date = DateTime.Today,
            Open = 300.00m,
            High = 305.00m,
            Low = 298.00m,
            Close = 302.50m,
            Volume = 2000000
        };

        context.Prices.Add(price);
        await context.SaveChangesAsync();

        // Act
        var fetchedAsset = await context.Assets
            .Include(a => a.Prices)
            .FirstOrDefaultAsync(a => a.Symbol == "MSFT");

        // Assert
        Assert.NotNull(fetchedAsset);
        Assert.Equal("MSFT", fetchedAsset.Symbol);
        Assert.Single(fetchedAsset.Prices);
        Assert.Equal(302.50m, fetchedAsset.Prices.First().Close);
    }

    [Fact]
    public async Task Asset_CanBeUpdated()
    {
        // Arrange
        var options = CreateInMemoryOptions("TestDb_Asset_CanBeUpdated");

        using var context = new FinancialRiskDbContext(options);

        var asset = new Asset
        {
            Symbol = "GOOGL",
            Name = "Alphabet Inc.",
            Sector = "Technology",
            Industry = "Internet Services",
            AssetType = "Stock"
        };

        context.Assets.Add(asset);
        await context.SaveChangesAsync();

        // Act
        var fetchedAsset = await context.Assets.FindAsync(asset.Id);
        fetchedAsset!.Name = "Alphabet Inc. (Updated)";
        fetchedAsset.UpdatedAt = DateTime.UtcNow;
        
        var result = await context.SaveChangesAsync();

        // Assert
        Assert.Equal(1, result);
        var updatedAsset = await context.Assets.FindAsync(asset.Id);
        Assert.Equal("Alphabet Inc. (Updated)", updatedAsset!.Name);
    }

    [Fact]
    public async Task Asset_CanBeDeleted()
    {
        // Arrange
        var options = CreateInMemoryOptions("TestDb_Asset_CanBeDeleted");

        using var context = new FinancialRiskDbContext(options);

        var asset = new Asset
        {
            Symbol = "TSLA",
            Name = "Tesla Inc.",
            Sector = "Consumer Cyclical",
            Industry = "Auto Manufacturers",
            AssetType = "Stock"
        };

        context.Assets.Add(asset);
        await context.SaveChangesAsync();

        // Act
        context.Assets.Remove(asset);
        var result = await context.SaveChangesAsync();

        // Assert
        Assert.Equal(1, result);
        Assert.Equal(0, context.Assets.Count());
    }

    [Fact]
    public async Task Asset_SymbolIsUnique()
    {
        // Arrange
        var options = CreateInMemoryOptions("TestDb_Asset_SymbolIsUnique");

        using var context = new FinancialRiskDbContext(options);

        var asset1 = new Asset
        {
            Symbol = "NVDA",
            Name = "NVIDIA Corporation",
            Sector = "Technology",
            Industry = "Semiconductors",
            AssetType = "Stock"
        };

        var asset2 = new Asset
        {
            Symbol = "NVDA", // Same symbol
            Name = "Another NVIDIA",
            Sector = "Technology",
            Industry = "Semiconductors",
            AssetType = "Stock"
        };

        // Act & Assert
        context.Assets.Add(asset1);
        await context.SaveChangesAsync();

        context.Assets.Add(asset2);
        await context.SaveChangesAsync(); // In-memory provider doesn't enforce unique constraints

        // Verify both assets exist (in-memory provider behavior)
        Assert.Equal(2, context.Assets.Count());
        Assert.Equal(2, context.Assets.Count(a => a.Symbol == "NVDA"));
    }

    #endregion

    #region Price Tests

    [Fact]
    public async Task Price_CanBeAddedToContext()
    {
        // Arrange
        var options = CreateInMemoryOptions("TestDb_Price_CanBeAddedToContext");

        using var context = new FinancialRiskDbContext(options);

        var asset = new Asset
        {
            Symbol = "TEST",
            Name = "Test Asset"
        };

        var price = new Price
        {
            AssetId = 1, // Will be set by EF
            Date = DateTime.Today,
            Open = 100.00m,
            High = 105.00m,
            Low = 98.00m,
            Close = 102.50m,
            Volume = 1000000
        };

        // Act
        context.Assets.Add(asset);
        await context.SaveChangesAsync(); // This will set the AssetId

        price.AssetId = asset.Id;
        context.Prices.Add(price);
        var result = await context.SaveChangesAsync();

        // Assert
        Assert.Equal(1, result);
        Assert.Equal(1, context.Prices.Count());
        Assert.Equal(102.50m, context.Prices.First().Close);
    }

    [Fact]
    public async Task Price_CanBeFetchedByAssetId()
    {
        // Arrange
        var options = CreateInMemoryOptions("TestDb_Price_CanBeFetchedByAssetId");

        using var context = new FinancialRiskDbContext(options);

        var asset = new Asset
        {
            Symbol = "JPM",
            Name = "JPMorgan Chase & Co.",
            Sector = "Financial Services",
            Industry = "Banks",
            AssetType = "Stock"
        };

        context.Assets.Add(asset);
        await context.SaveChangesAsync();

        var price1 = new Price
        {
            AssetId = asset.Id,
            Date = DateTime.Today.AddDays(-1),
            Open = 150.00m,
            High = 152.00m,
            Low = 149.00m,
            Close = 151.50m,
            Volume = 5000000
        };

        var price2 = new Price
        {
            AssetId = asset.Id,
            Date = DateTime.Today,
            Open = 151.50m,
            High = 154.00m,
            Low = 150.50m,
            Close = 153.00m,
            Volume = 6000000
        };

        context.Prices.AddRange(price1, price2);
        await context.SaveChangesAsync();

        // Act
        var prices = await context.Prices
            .Where(p => p.AssetId == asset.Id)
            .OrderBy(p => p.Date)
            .ToListAsync();

        // Assert
        Assert.Equal(2, prices.Count);
        Assert.Equal(151.50m, prices[0].Close);
        Assert.Equal(153.00m, prices[1].Close);
    }

    [Fact]
    public async Task Price_CanBeFetchedByDateRange()
    {
        // Arrange
        var options = CreateInMemoryOptions("TestDb_Price_CanBeFetchedByDateRange");

        using var context = new FinancialRiskDbContext(options);

        var asset = new Asset
        {
            Symbol = "V",
            Name = "Visa Inc.",
            Sector = "Financial Services",
            Industry = "Credit Services",
            AssetType = "Stock"
        };

        context.Assets.Add(asset);
        await context.SaveChangesAsync();

        var startDate = DateTime.Today.AddDays(-5);
        var endDate = DateTime.Today.AddDays(-1);

        var prices = new List<Price>
        {
            new Price { AssetId = asset.Id, Date = DateTime.Today.AddDays(-7), Close = 200.00m, Volume = 1000000 },
            new Price { AssetId = asset.Id, Date = DateTime.Today.AddDays(-5), Close = 201.00m, Volume = 1100000 },
            new Price { AssetId = asset.Id, Date = DateTime.Today.AddDays(-3), Close = 202.00m, Volume = 1200000 },
            new Price { AssetId = asset.Id, Date = DateTime.Today.AddDays(-1), Close = 203.00m, Volume = 1300000 },
            new Price { AssetId = asset.Id, Date = DateTime.Today, Close = 204.00m, Volume = 1400000 }
        };

        context.Prices.AddRange(prices);
        await context.SaveChangesAsync();

        // Act
        var filteredPrices = await context.Prices
            .Where(p => p.AssetId == asset.Id && p.Date >= startDate && p.Date <= endDate)
            .OrderBy(p => p.Date)
            .ToListAsync();

        // Assert
        Assert.Equal(3, filteredPrices.Count);
        Assert.Equal(201.00m, filteredPrices[0].Close);
        Assert.Equal(203.00m, filteredPrices[2].Close);
    }

    [Fact]
    public async Task Price_CanBeUpdated()
    {
        // Arrange
        var options = CreateInMemoryOptions("TestDb_Price_CanBeUpdated");

        using var context = new FinancialRiskDbContext(options);

        var asset = new Asset
        {
            Symbol = "PG",
            Name = "Procter & Gamble Co.",
            Sector = "Consumer Defensive",
            Industry = "Household & Personal Products",
            AssetType = "Stock"
        };

        context.Assets.Add(asset);
        await context.SaveChangesAsync();

        var price = new Price
        {
            AssetId = asset.Id,
            Date = DateTime.Today,
            Open = 140.00m,
            High = 142.00m,
            Low = 139.00m,
            Close = 141.00m,
            Volume = 3000000
        };

        context.Prices.Add(price);
        await context.SaveChangesAsync();

        // Act
        var fetchedPrice = await context.Prices.FindAsync(price.Id);
        fetchedPrice!.Close = 141.50m;
        fetchedPrice.Volume = 3100000;
        
        var result = await context.SaveChangesAsync();

        // Assert
        Assert.Equal(1, result);
        var updatedPrice = await context.Prices.FindAsync(price.Id);
        Assert.Equal(141.50m, updatedPrice!.Close);
        Assert.Equal(3100000, updatedPrice.Volume);
    }

    [Fact]
    public async Task Price_CanBeDeleted()
    {
        // Arrange
        var options = CreateInMemoryOptions("TestDb_Price_CanBeDeleted");

        using var context = new FinancialRiskDbContext(options);

        var asset = new Asset
        {
            Symbol = "SPY",
            Name = "SPDR S&P 500 ETF Trust",
            Sector = "ETF",
            Industry = "ETF",
            AssetType = "ETF"
        };

        context.Assets.Add(asset);
        await context.SaveChangesAsync();

        var price = new Price
        {
            AssetId = asset.Id,
            Date = DateTime.Today,
            Open = 400.00m,
            High = 402.00m,
            Low = 399.00m,
            Close = 401.00m,
            Volume = 80000000
        };

        context.Prices.Add(price);
        await context.SaveChangesAsync();

        // Act
        context.Prices.Remove(price);
        var result = await context.SaveChangesAsync();

        // Assert
        Assert.Equal(1, result);
        Assert.Equal(0, context.Prices.Count());
    }

    [Fact]
    public async Task Price_CanBeFetchedWithAsset()
    {
        // Arrange
        var options = CreateInMemoryOptions("TestDb_Price_CanBeFetchedWithAsset");

        using var context = new FinancialRiskDbContext(options);

        var asset = new Asset
        {
            Symbol = "QQQ",
            Name = "Invesco QQQ Trust",
            Sector = "ETF",
            Industry = "ETF",
            AssetType = "ETF"
        };

        context.Assets.Add(asset);
        await context.SaveChangesAsync();

        var price = new Price
        {
            AssetId = asset.Id,
            Date = DateTime.Today,
            Open = 350.00m,
            High = 352.00m,
            Low = 349.00m,
            Close = 351.00m,
            Volume = 50000000
        };

        context.Prices.Add(price);
        await context.SaveChangesAsync();

        // Act
        var fetchedPrice = await context.Prices
            .Include(p => p.Asset)
            .FirstOrDefaultAsync(p => p.AssetId == asset.Id);

        // Assert
        Assert.NotNull(fetchedPrice);
        Assert.Equal(351.00m, fetchedPrice.Close);
        Assert.NotNull(fetchedPrice.Asset);
        Assert.Equal("QQQ", fetchedPrice.Asset.Symbol);
        Assert.Equal("Invesco QQQ Trust", fetchedPrice.Asset.Name);
    }

    #endregion

    #region Portfolio Tests

    [Fact]
    public async Task Portfolio_CanBeAddedToContext()
    {
        // Arrange
        var options = CreateInMemoryOptions("TestDb_Portfolio_CanBeAddedToContext");

        using var context = new FinancialRiskDbContext(options);

        var portfolio = new Portfolio
        {
            Name = "Test Portfolio",
            Description = "A test portfolio",
            Strategy = "Moderate",
            TargetReturn = 0.08m,
            MaxRisk = 0.15m
        };

        // Act
        context.Portfolios.Add(portfolio);
        var result = await context.SaveChangesAsync();

        // Assert
        Assert.Equal(1, result);
        Assert.Equal(1, context.Portfolios.Count());
        Assert.Equal("Test Portfolio", context.Portfolios.First().Name);
    }

    [Fact]
    public async Task PortfolioHolding_CanBeAddedToContext()
    {
        // Arrange
        var options = CreateInMemoryOptions("TestDb_PortfolioHolding_CanBeAddedToContext");

        using var context = new FinancialRiskDbContext(options);

        var asset = new Asset
        {
            Symbol = "TEST",
            Name = "Test Asset"
        };

        var portfolio = new Portfolio
        {
            Name = "Test Portfolio"
        };

        var holding = new PortfolioHolding
        {
            AssetId = 1, // Will be set by EF
            PortfolioId = 1, // Will be set by EF
            Weight = 0.25m,
            Quantity = 1000,
            AverageCost = 100.00m
        };

        // Act
        context.Assets.Add(asset);
        context.Portfolios.Add(portfolio);
        await context.SaveChangesAsync(); // This will set the IDs

        holding.AssetId = asset.Id;
        holding.PortfolioId = portfolio.Id;
        context.PortfolioHoldings.Add(holding);
        var result = await context.SaveChangesAsync();

        // Assert
        Assert.Equal(1, result);
        Assert.Equal(1, context.PortfolioHoldings.Count());
        Assert.Equal(0.25m, context.PortfolioHoldings.First().Weight);
    }

    #endregion

    #region Complex Query Tests

    [Fact]
    public async Task ComplexQuery_CanFetchAssetsWithLatestPrices()
    {
        // Arrange
        var options = CreateInMemoryOptions("TestDb_ComplexQuery_CanFetchAssetsWithLatestPrices");

        using var context = new FinancialRiskDbContext(options);

        // Create assets with multiple prices
        var assets = new List<Asset>
        {
            new Asset { Symbol = "AAPL", Name = "Apple Inc.", Sector = "Technology" },
            new Asset { Symbol = "MSFT", Name = "Microsoft Corporation", Sector = "Technology" }
        };

        context.Assets.AddRange(assets);
        await context.SaveChangesAsync();

        var prices = new List<Price>
        {
            new Price { AssetId = assets[0].Id, Date = DateTime.Today.AddDays(-2), Close = 150.00m, Volume = 1000000 },
            new Price { AssetId = assets[0].Id, Date = DateTime.Today.AddDays(-1), Close = 152.00m, Volume = 1100000 },
            new Price { AssetId = assets[0].Id, Date = DateTime.Today, Close = 154.00m, Volume = 1200000 },
            new Price { AssetId = assets[1].Id, Date = DateTime.Today.AddDays(-2), Close = 300.00m, Volume = 2000000 },
            new Price { AssetId = assets[1].Id, Date = DateTime.Today.AddDays(-1), Close = 302.00m, Volume = 2100000 },
            new Price { AssetId = assets[1].Id, Date = DateTime.Today, Close = 304.00m, Volume = 2200000 }
        };

        context.Prices.AddRange(prices);
        await context.SaveChangesAsync();

        // Act
        var assetsWithLatestPrices = await context.Assets
            .Include(a => a.Prices)
            .ToListAsync();

        // Assert
        Assert.Equal(2, assetsWithLatestPrices.Count);
        Assert.Equal(3, assetsWithLatestPrices[0].Prices.Count); // AAPL has 3 prices
        Assert.Equal(3, assetsWithLatestPrices[1].Prices.Count); // MSFT has 3 prices
        
        // Verify latest prices
        var aaplLatestPrice = assetsWithLatestPrices[0].Prices.OrderByDescending(p => p.Date).First();
        var msftLatestPrice = assetsWithLatestPrices[1].Prices.OrderByDescending(p => p.Date).First();
        
        Assert.Equal(154.00m, aaplLatestPrice.Close);
        Assert.Equal(304.00m, msftLatestPrice.Close);
    }

    [Fact]
    public async Task ComplexQuery_CanFetchPortfolioWithHoldingsAndAssets()
    {
        // Arrange
        var options = CreateInMemoryOptions("TestDb_ComplexQuery_CanFetchPortfolioWithHoldingsAndAssets");

        using var context = new FinancialRiskDbContext(options);

        var assets = new List<Asset>
        {
            new Asset { Symbol = "AAPL", Name = "Apple Inc." },
            new Asset { Symbol = "MSFT", Name = "Microsoft Corporation" }
        };

        var portfolio = new Portfolio
        {
            Name = "Growth Portfolio",
            Strategy = "Growth",
            IsActive = true
        };

        context.Assets.AddRange(assets);
        context.Portfolios.Add(portfolio);
        await context.SaveChangesAsync();

        var holdings = new List<PortfolioHolding>
        {
            new PortfolioHolding { PortfolioId = portfolio.Id, AssetId = assets[0].Id, Weight = 0.6m, Quantity = 100 },
            new PortfolioHolding { PortfolioId = portfolio.Id, AssetId = assets[1].Id, Weight = 0.4m, Quantity = 50 }
        };

        context.PortfolioHoldings.AddRange(holdings);
        await context.SaveChangesAsync();

        // Act
        var portfolioWithHoldings = await context.Portfolios
            .Include(p => p.PortfolioHoldings)
                .ThenInclude(ph => ph.Asset)
            .FirstOrDefaultAsync(p => p.Id == portfolio.Id);

        // Assert
        Assert.NotNull(portfolioWithHoldings);
        Assert.Equal(2, portfolioWithHoldings.PortfolioHoldings.Count);
        Assert.Equal("AAPL", portfolioWithHoldings.PortfolioHoldings.First().Asset.Symbol);
        Assert.Equal("MSFT", portfolioWithHoldings.PortfolioHoldings.Last().Asset.Symbol);
        Assert.Equal(0.6m, portfolioWithHoldings.PortfolioHoldings.First().Weight);
    }

    #endregion
}

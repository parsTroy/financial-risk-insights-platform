using Microsoft.EntityFrameworkCore;
using FinancialRisk.Api.Data;
using FinancialRisk.Api.Models;
using FinancialRisk.Api.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace FinancialRisk.Tests;

public class DataPersistenceServiceTests
{
    private DbContextOptions<FinancialRiskDbContext> CreateInMemoryOptions(string databaseName)
    {
        return new DbContextOptionsBuilder<FinancialRiskDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;
    }

    private ILogger<DataPersistenceService> CreateMockLogger()
    {
        return Mock.Of<ILogger<DataPersistenceService>>();
    }

    #region Asset Management Tests

    [Fact]
    public async Task GetOrCreateAssetAsync_WithNewSymbol_CreatesAsset()
    {
        // Arrange
        var options = CreateInMemoryOptions("TestDb_GetOrCreateAsset_NewSymbol");
        var logger = CreateMockLogger();

        using var context = new FinancialRiskDbContext(options);
        var service = new DataPersistenceService(context, logger);

        // Act
        var asset = await service.GetOrCreateAssetAsync("AAPL", "Apple Inc.", "Technology", "Consumer Electronics", "Stock");

        // Assert
        Assert.NotNull(asset);
        Assert.Equal("AAPL", asset.Symbol);
        Assert.Equal("Apple Inc.", asset.Name);
        Assert.Equal("Technology", asset.Sector);
        Assert.Equal("Consumer Electronics", asset.Industry);
        Assert.Equal("Stock", asset.AssetType);
        Assert.NotEqual(0, asset.Id);
        Assert.True(asset.CreatedAt > DateTime.MinValue);
        Assert.True(asset.UpdatedAt > DateTime.MinValue);

        // Verify it was saved to database
        var savedAsset = await context.Assets.FindAsync(asset.Id);
        Assert.NotNull(savedAsset);
        Assert.Equal("AAPL", savedAsset.Symbol);
    }

    [Fact]
    public async Task GetOrCreateAssetAsync_WithExistingSymbol_ReturnsExistingAsset()
    {
        // Arrange
        var options = CreateInMemoryOptions("TestDb_GetOrCreateAsset_ExistingSymbol");
        var logger = CreateMockLogger();

        using var context = new FinancialRiskDbContext(options);
        var service = new DataPersistenceService(context, logger);

        // Create initial asset
        var initialAsset = new Asset
        {
            Symbol = "MSFT",
            Name = "Microsoft Corporation",
            Sector = "Technology",
            Industry = "Software",
            AssetType = "Stock"
        };

        context.Assets.Add(initialAsset);
        await context.SaveChangesAsync();

        // Act
        var fetchedAsset = await service.GetOrCreateAssetAsync("MSFT", "Microsoft Corp", "Tech", "Software", "Stock");

        // Assert
        Assert.NotNull(fetchedAsset);
        Assert.Equal(initialAsset.Id, fetchedAsset.Id);
        Assert.Equal("MSFT", fetchedAsset.Symbol);
        Assert.Equal("Microsoft Corporation", fetchedAsset.Name); // Should return original name, not updated
        Assert.Equal("Technology", fetchedAsset.Sector);
    }

    [Fact]
    public async Task GetOrCreateAssetAsync_WithNullParameters_UsesDefaults()
    {
        // Arrange
        var options = CreateInMemoryOptions("TestDb_GetOrCreateAsset_NullParams");
        var logger = CreateMockLogger();

        using var context = new FinancialRiskDbContext(options);
        var service = new DataPersistenceService(context, logger);

        // Act
        var asset = await service.GetOrCreateAssetAsync("TEST");

        // Assert
        Assert.NotNull(asset);
        Assert.Equal("TEST", asset.Symbol);
        Assert.Equal("TEST", asset.Name); // Should use symbol as default name
        Assert.Equal("Stock", asset.AssetType); // Should use default asset type
        Assert.Null(asset.Sector);
        Assert.Null(asset.Industry);
    }

    #endregion

    #region Stock Quote Persistence Tests

    [Fact]
    public async Task SaveStockQuoteAsync_WithNewQuote_SavesToDatabase()
    {
        // Arrange
        var options = CreateInMemoryOptions("TestDb_SaveStockQuote_NewQuote");
        var logger = CreateMockLogger();

        using var context = new FinancialRiskDbContext(options);
        var service = new DataPersistenceService(context, logger);

        var stockQuote = new StockQuote
        {
            Symbol = "AAPL",
            Open = 150.00m,
            High = 155.00m,
            Low = 149.00m,
            Close = 152.50m,
            Volume = 1000000,
            Change = 2.50m,
            ChangePercent = 1.67m,
            Timestamp = DateTime.Today
        };

        // Act
        var result = await service.SaveStockQuoteAsync(stockQuote);

        // Assert
        Assert.True(result);

        // Verify asset was created
        var asset = await context.Assets.FirstOrDefaultAsync(a => a.Symbol == "AAPL");
        Assert.NotNull(asset);
        Assert.Equal("AAPL", asset.Symbol);

        // Verify price was saved
        var price = await context.Prices.FirstOrDefaultAsync(p => p.AssetId == asset.Id);
        Assert.NotNull(price);
        Assert.Equal(150.00m, price.Open);
        Assert.Equal(155.00m, price.High);
        Assert.Equal(149.00m, price.Low);
        Assert.Equal(152.50m, price.Close);
        Assert.Equal(1000000, price.Volume);
        Assert.Equal(DateTime.Today.Date, price.Date);
    }

    [Fact]
    public async Task SaveStockQuoteAsync_WithDuplicateDate_SkipsDuplicate()
    {
        // Arrange
        var options = CreateInMemoryOptions("TestDb_SaveStockQuote_DuplicateDate");
        var logger = CreateMockLogger();

        using var context = new FinancialRiskDbContext(options);
        var service = new DataPersistenceService(context, logger);

        // Create asset and initial price
        var asset = new Asset { Symbol = "MSFT", Name = "Microsoft Corporation" };
        context.Assets.Add(asset);
        await context.SaveChangesAsync();

        var existingPrice = new Price
        {
            AssetId = asset.Id,
            Date = DateTime.Today,
            Close = 300.00m,
            Volume = 2000000
        };

        context.Prices.Add(existingPrice);
        await context.SaveChangesAsync();

        var stockQuote = new StockQuote
        {
            Symbol = "MSFT",
            Open = 301.00m,
            High = 305.00m,
            Low = 299.00m,
            Close = 302.50m,
            Volume = 2100000,
            Change = 2.50m,
            ChangePercent = 0.83m,
            Timestamp = DateTime.Today
        };

        // Act
        var result = await service.SaveStockQuoteAsync(stockQuote);

        // Assert
        Assert.True(result);

        // Verify no new price was added
        var prices = await context.Prices.Where(p => p.AssetId == asset.Id).ToListAsync();
        Assert.Single(prices);
        Assert.Equal(300.00m, prices[0].Close); // Should keep original price
    }

    [Fact]
    public async Task SaveStockQuoteAsync_WithDifferentDate_AddsNewPrice()
    {
        // Arrange
        var options = CreateInMemoryOptions("TestDb_SaveStockQuote_DifferentDate");
        var logger = CreateMockLogger();

        using var context = new FinancialRiskDbContext(options);
        var service = new DataPersistenceService(context, logger);

        // Create asset and initial price
        var asset = new Asset { Symbol = "GOOGL", Name = "Alphabet Inc." };
        context.Assets.Add(asset);
        await context.SaveChangesAsync();

        var existingPrice = new Price
        {
            AssetId = asset.Id,
            Date = DateTime.Today.AddDays(-1),
            Close = 2500.00m,
            Volume = 1500000
        };

        context.Prices.Add(existingPrice);
        await context.SaveChangesAsync();

        var stockQuote = new StockQuote
        {
            Symbol = "GOOGL",
            Open = 2510.00m,
            High = 2520.00m,
            Low = 2505.00m,
            Close = 2515.00m,
            Volume = 1600000,
            Change = 15.00m,
            ChangePercent = 0.60m,
            Timestamp = DateTime.Today
        };

        // Act
        var result = await service.SaveStockQuoteAsync(stockQuote);

        // Assert
        Assert.True(result);

        // Verify both prices exist
        var prices = await context.Prices.Where(p => p.AssetId == asset.Id).ToListAsync();
        Assert.Equal(2, prices.Count);

        var todayPrice = prices.FirstOrDefault(p => p.Date == DateTime.Today);
        Assert.NotNull(todayPrice);
        Assert.Equal(2515.00m, todayPrice.Close);
    }

    #endregion

    #region Stock History Persistence Tests

    [Fact]
    public async Task SaveStockHistoryAsync_WithMultipleQuotes_SavesAllToDatabase()
    {
        // Arrange
        var options = CreateInMemoryOptions("TestDb_SaveStockHistory_MultipleQuotes");
        var logger = CreateMockLogger();

        using var context = new FinancialRiskDbContext(options);
        var service = new DataPersistenceService(context, logger);

        var stockQuotes = new List<StockQuote>
        {
            new StockQuote
            {
                Symbol = "TSLA",
                Open = 200.00m,
                High = 205.00m,
                Low = 198.00m,
                Close = 202.50m,
                Volume = 5000000,
                Change = 2.50m,
                ChangePercent = 1.25m,
                Timestamp = DateTime.Today.AddDays(-2)
            },
            new StockQuote
            {
                Symbol = "TSLA",
                Open = 202.50m,
                High = 208.00m,
                Low = 201.00m,
                Close = 206.00m,
                Volume = 5200000,
                Change = 3.50m,
                ChangePercent = 1.73m,
                Timestamp = DateTime.Today.AddDays(-1)
            },
            new StockQuote
            {
                Symbol = "TSLA",
                Open = 206.00m,
                High = 210.00m,
                Low = 204.00m,
                Close = 208.50m,
                Volume = 5400000,
                Change = 2.50m,
                ChangePercent = 1.21m,
                Timestamp = DateTime.Today
            }
        };

        // Act
        var result = await service.SaveStockHistoryAsync("TSLA", stockQuotes);

        // Assert
        Assert.True(result);

        // Verify asset was created
        var asset = await context.Assets.FirstOrDefaultAsync(a => a.Symbol == "TSLA");
        Assert.NotNull(asset);

        // Verify all prices were saved
        var prices = await context.Prices.Where(p => p.AssetId == asset.Id).ToListAsync();
        Assert.Equal(3, prices.Count);

        // Verify specific prices
        var day1Price = prices.FirstOrDefault(p => p.Date == DateTime.Today.AddDays(-2));
        Assert.NotNull(day1Price);
        Assert.Equal(202.50m, day1Price.Close);

        var day2Price = prices.FirstOrDefault(p => p.Date == DateTime.Today.AddDays(-1));
        Assert.NotNull(day2Price);
        Assert.Equal(206.00m, day2Price.Close);

        var day3Price = prices.FirstOrDefault(p => p.Date == DateTime.Today);
        Assert.NotNull(day3Price);
        Assert.Equal(208.50m, day3Price.Close);
    }

    [Fact]
    public async Task SaveStockHistoryAsync_WithMixedNewAndExistingDates_HandlesCorrectly()
    {
        // Arrange
        var options = CreateInMemoryOptions("TestDb_SaveStockHistory_MixedDates");
        var logger = CreateMockLogger();

        using var context = new FinancialRiskDbContext(options);
        var service = new DataPersistenceService(context, logger);

        // Create asset and existing price
        var asset = new Asset { Symbol = "NVDA", Name = "NVIDIA Corporation" };
        context.Assets.Add(asset);
        await context.SaveChangesAsync();

        var existingPrice = new Price
        {
            AssetId = asset.Id,
            Date = DateTime.Today.AddDays(-1),
            Close = 400.00m,
            Volume = 8000000
        };

        context.Prices.Add(existingPrice);
        await context.SaveChangesAsync();

        var stockQuotes = new List<StockQuote>
        {
            new StockQuote
            {
                Symbol = "NVDA",
                Close = 400.00m, // Same date as existing
                Volume = 8000000,
                Timestamp = DateTime.Today.AddDays(-1)
            },
            new StockQuote
            {
                Symbol = "NVDA",
                Close = 410.00m, // New date
                Volume = 8200000,
                Timestamp = DateTime.Today
            }
        };

        // Act
        var result = await service.SaveStockHistoryAsync("NVDA", stockQuotes);

        // Assert
        Assert.True(result);

        // Verify only new price was added
        var prices = await context.Prices.Where(p => p.AssetId == asset.Id).ToListAsync();
        Assert.Equal(2, prices.Count);

        var todayPrice = prices.FirstOrDefault(p => p.Date == DateTime.Today);
        Assert.NotNull(todayPrice);
        Assert.Equal(410.00m, todayPrice.Close);
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task SaveStockQuoteAsync_WithInvalidData_HandlesGracefully()
    {
        // Arrange
        var options = CreateInMemoryOptions("TestDb_SaveStockQuote_InvalidData");
        var logger = CreateMockLogger();

        using var context = new FinancialRiskDbContext(options);
        var service = new DataPersistenceService(context, logger);

        var stockQuote = new StockQuote
        {
            Symbol = "", // Invalid empty symbol
            Open = -100.00m, // Invalid negative price
            High = 0.00m, // Invalid zero price
            Low = 0.00m,
            Close = 0.00m,
            Volume = -1000, // Invalid negative volume
            Change = 0.00m,
            ChangePercent = 0.00m,
            Timestamp = DateTime.Today
        };

        // Act
        var result = await service.SaveStockQuoteAsync(stockQuote);

        // Assert
        // The service should handle this gracefully and still create the asset/price
        // In a real scenario, you might want to add validation
        Assert.True(result);

        // Verify asset was created (even with empty symbol)
        var asset = await context.Assets.FirstOrDefaultAsync(a => a.Symbol == "");
        Assert.NotNull(asset);
    }

    [Fact]
    public async Task GetOrCreateAssetAsync_WithDatabaseError_ThrowsException()
    {
        // Arrange
        var options = CreateInMemoryOptions("TestDb_GetOrCreateAsset_DatabaseError");
        var logger = CreateMockLogger();

        using var context = new FinancialRiskDbContext(options);
        var service = new DataPersistenceService(context, logger);

        // Simulate database error by disposing context
        context.Dispose();

        // Act & Assert
        await Assert.ThrowsAsync<ObjectDisposedException>(async () =>
            await service.GetOrCreateAssetAsync("TEST"));
    }

    #endregion

    #region Performance Tests

    [Fact]
    public async Task SaveStockHistoryAsync_WithLargeDataset_PerformsEfficiently()
    {
        // Arrange
        var options = CreateInMemoryOptions("TestDb_SaveStockHistory_LargeDataset");
        var logger = CreateMockLogger();

        using var context = new FinancialRiskDbContext(options);
        var service = new DataPersistenceService(context, logger);

        // Create 100 stock quotes
        var stockQuotes = Enumerable.Range(0, 100).Select(i => new StockQuote
        {
            Symbol = "BULK",
            Open = 100.00m + i,
            High = 105.00m + i,
            Low = 98.00m + i,
            Close = 102.50m + i,
            Volume = 1000000 + (i * 10000),
            Change = 2.50m,
            ChangePercent = 1.25m,
            Timestamp = DateTime.Today.AddDays(-i)
        }).ToList();

        // Act
        var startTime = DateTime.UtcNow;
        var result = await service.SaveStockHistoryAsync("BULK", stockQuotes);
        var endTime = DateTime.UtcNow;

        // Assert
        Assert.True(result);
        Assert.True(endTime - startTime < TimeSpan.FromSeconds(5)); // Should complete within 5 seconds

        // Verify all prices were saved
        var asset = await context.Assets.FirstOrDefaultAsync(a => a.Symbol == "BULK");
        Assert.NotNull(asset);

        var prices = await context.Prices.Where(p => p.AssetId == asset.Id).ToListAsync();
        Assert.Equal(100, prices.Count);
    }

    #endregion
}

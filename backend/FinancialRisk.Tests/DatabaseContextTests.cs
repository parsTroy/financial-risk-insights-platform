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
}

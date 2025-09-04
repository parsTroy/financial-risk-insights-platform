using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using FinancialRisk.Api;
using FinancialRisk.Api.Data;
using FinancialRisk.Api.Models;
using System.Net;
using System.Text;
using System.Text.Json;
using Xunit;

namespace FinancialRisk.Tests;

public class AssetsApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public AssetsApiIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Use in-memory database for testing
                var dbContextDescriptors = services.Where(
                    d => d.ServiceType == typeof(DbContextOptions<FinancialRiskDbContext>)).ToList();
                foreach (var descriptor in dbContextDescriptors)
                {
                    services.Remove(descriptor);
                }

                services.AddDbContext<FinancialRiskDbContext>(options =>
                {
                    options.UseInMemoryDatabase("TestDb_AssetsApi_" + Guid.NewGuid().ToString());
                });
            });
        });
    }

    #region GET /api/assets Tests

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetAssets_WithEmptyDatabase_ReturnsEmptyList()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/assets");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var assets = JsonSerializer.Deserialize<List<Asset>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        
        Assert.NotNull(assets);
        Assert.Empty(assets);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetAssets_WithPopulatedDatabase_ReturnsAllAssets()
    {
        // Arrange
        var client = _factory.CreateClient();
        
        // Seed the database with test data
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<FinancialRiskDbContext>();
        
        var assets = new List<Asset>
        {
            new Asset { Symbol = "AAPL", Name = "Apple Inc.", Sector = "Technology", Industry = "Consumer Electronics", AssetType = "Stock" },
            new Asset { Symbol = "MSFT", Name = "Microsoft Corporation", Sector = "Technology", Industry = "Software", AssetType = "Stock" },
            new Asset { Symbol = "GOOGL", Name = "Alphabet Inc.", Sector = "Technology", Industry = "Internet Services", AssetType = "Stock" }
        };
        
        context.Assets.AddRange(assets);
        await context.SaveChangesAsync();

        // Act
        var response = await client.GetAsync("/api/assets");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var returnedAssets = JsonSerializer.Deserialize<List<Asset>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        
        Assert.NotNull(returnedAssets);
        Assert.Equal(3, returnedAssets.Count);
        Assert.Contains(returnedAssets, a => a.Symbol == "AAPL");
        Assert.Contains(returnedAssets, a => a.Symbol == "MSFT");
        Assert.Contains(returnedAssets, a => a.Symbol == "GOOGL");
    }

    #endregion

    #region GET /api/assets/symbol/{symbol} Tests

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetAssetBySymbol_WithValidSymbol_ReturnsAsset()
    {
        // Arrange
        var client = _factory.CreateClient();
        
        // Seed the database
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<FinancialRiskDbContext>();
        
        var asset = new Asset { Symbol = "V", Name = "Visa Inc.", Sector = "Financial Services", Industry = "Credit Services", AssetType = "Stock" };
        context.Assets.Add(asset);
        await context.SaveChangesAsync();

        // Act
        var response = await client.GetAsync("/api/assets/symbol/V");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var returnedAsset = JsonSerializer.Deserialize<Asset>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        
        Assert.NotNull(returnedAsset);
        Assert.Equal("V", returnedAsset.Symbol);
        Assert.Equal("Visa Inc.", returnedAsset.Name);
        Assert.Equal("Financial Services", returnedAsset.Sector);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetAssetBySymbol_WithInvalidSymbol_ReturnsNotFound()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/assets/symbol/INVALID");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    #endregion

    #region POST /api/assets Tests

    [Fact]
    [Trait("Category", "Integration")]
    public async Task CreateAsset_WithValidData_CreatesAsset()
    {
        // Arrange
        var client = _factory.CreateClient();
        
        var newAsset = new
        {
            Symbol = "SPY",
            Name = "SPDR S&P 500 ETF Trust",
            Sector = "ETF",
            Industry = "ETF",
            AssetType = "ETF"
        };
        
        var json = JsonSerializer.Serialize(newAsset);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await client.PostAsync("/api/assets", content);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        var createdAsset = JsonSerializer.Deserialize<Asset>(responseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        
        Assert.NotNull(createdAsset);
        Assert.Equal("SPY", createdAsset.Symbol);
        Assert.Equal("SPDR S&P 500 ETF Trust", createdAsset.Name);
        Assert.NotEqual(0, createdAsset.Id);
        
        // Verify it was saved to database
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<FinancialRiskDbContext>();
        var savedAsset = await context.Assets.FirstOrDefaultAsync(a => a.Symbol == "SPY");
        Assert.NotNull(savedAsset);
        Assert.Equal("SPDR S&P 500 ETF Trust", savedAsset.Name);
    }

    #endregion
}

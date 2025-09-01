using FinancialRisk.Api.Data;
using FinancialRisk.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FinancialRisk.Api.Services
{
    public class DataPersistenceService : IDataPersistenceService
    {
        private readonly FinancialRiskDbContext _context;
        private readonly ILogger<DataPersistenceService> _logger;

        public DataPersistenceService(
            FinancialRiskDbContext context,
            ILogger<DataPersistenceService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<bool> SaveStockQuoteAsync(StockQuote stockQuote)
        {
            try
            {
                _logger.LogInformation("Saving stock quote for {Symbol} at {Timestamp}", stockQuote.Symbol, stockQuote.Timestamp);

                // Get or create the asset
                var asset = await GetOrCreateAssetAsync(stockQuote.Symbol);
                _logger.LogDebug("Asset ID {AssetId} for symbol {Symbol}", asset.Id, stockQuote.Symbol);

                // Check if price already exists for this asset and date
                var existingPrice = await _context.Prices
                    .FirstOrDefaultAsync(p => p.AssetId == asset.Id && p.Date.Date == stockQuote.Timestamp.Date);

                if (existingPrice != null)
                {
                    _logger.LogInformation("Price already exists for {Symbol} on {Date} (ID: {PriceId}), skipping duplicate", 
                        stockQuote.Symbol, stockQuote.Timestamp.Date, existingPrice.Id);
                    return true; // Consider this a success since we're avoiding duplicates
                }

                // Create new price record
                var price = new Price
                {
                    AssetId = asset.Id,
                    Date = stockQuote.Timestamp.Date,
                    Open = stockQuote.Open,
                    High = stockQuote.High,
                    Low = stockQuote.Low,
                    Close = stockQuote.Close,
                    Volume = stockQuote.Volume,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Prices.Add(price);
                var savedCount = await _context.SaveChangesAsync();

                _logger.LogInformation("Successfully saved stock quote for {Symbol} on {Date} (Records affected: {SavedCount})", 
                    stockQuote.Symbol, stockQuote.Timestamp.Date, savedCount);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save stock quote for {Symbol} at {Timestamp}", stockQuote.Symbol, stockQuote.Timestamp);
                return false;
            }
        }

        public async Task<bool> SaveStockHistoryAsync(string symbol, List<StockQuote> stockQuotes)
        {
            try
            {
                _logger.LogInformation("Saving {Count} stock history records for {Symbol}", stockQuotes.Count, symbol);

                // Get or create the asset
                var asset = await GetOrCreateAssetAsync(symbol);
                _logger.LogDebug("Asset ID {AssetId} for symbol {Symbol}", asset.Id, symbol);

                var savedCount = 0;
                var skippedCount = 0;

                foreach (var stockQuote in stockQuotes)
                {
                    // Check if price already exists for this asset and date
                    var existingPrice = await _context.Prices
                        .FirstOrDefaultAsync(p => p.AssetId == asset.Id && p.Date.Date == stockQuote.Timestamp.Date);

                    if (existingPrice != null)
                    {
                        skippedCount++;
                        _logger.LogDebug("Skipping duplicate price for {Symbol} on {Date}", symbol, stockQuote.Timestamp.Date);
                        continue; // Skip duplicates
                    }

                    // Create new price record
                    var price = new Price
                    {
                        AssetId = asset.Id,
                        Date = stockQuote.Timestamp.Date,
                        Open = stockQuote.Open,
                        High = stockQuote.High,
                        Low = stockQuote.Low,
                        Close = stockQuote.Close,
                        Volume = stockQuote.Volume,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.Prices.Add(price);
                    savedCount++;
                }

                if (savedCount > 0)
                {
                    var totalSaved = await _context.SaveChangesAsync();
                    _logger.LogDebug("Database save operation affected {TotalSaved} records", totalSaved);
                }

                _logger.LogInformation("Successfully saved {SavedCount} new price records for {Symbol}, skipped {SkippedCount} duplicates", 
                    savedCount, symbol, skippedCount);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save stock history for {Symbol}", symbol);
                return false;
            }
        }

        public async Task<Asset> GetOrCreateAssetAsync(string symbol, string? name = null, string? sector = null, string? industry = null, string? assetType = null)
        {
            try
            {
                // Try to find existing asset
                var asset = await _context.Assets
                    .FirstOrDefaultAsync(a => a.Symbol == symbol);

                if (asset != null)
                {
                    _logger.LogDebug("Found existing asset for symbol {Symbol} (ID: {AssetId})", symbol, asset.Id);
                    return asset;
                }

                // Create new asset if it doesn't exist
                asset = new Asset
                {
                    Symbol = symbol,
                    Name = name ?? symbol,
                    Sector = sector,
                    Industry = industry,
                    AssetType = assetType ?? "Stock",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Assets.Add(asset);
                var savedCount = await _context.SaveChangesAsync();

                _logger.LogInformation("Created new asset for symbol {Symbol} with name {Name} (ID: {AssetId}, Records affected: {SavedCount})", 
                    symbol, asset.Name, asset.Id, savedCount);
                return asset;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get or create asset for symbol {Symbol}", symbol);
                throw;
            }
        }
    }
}

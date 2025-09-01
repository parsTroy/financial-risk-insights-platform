using FinancialRisk.Api.Models;

namespace FinancialRisk.Api.Services
{
    public interface IDataPersistenceService
    {
        Task<bool> SaveStockQuoteAsync(StockQuote stockQuote);
        Task<bool> SaveStockHistoryAsync(string symbol, List<StockQuote> stockQuotes);
        Task<Asset> GetOrCreateAssetAsync(string symbol, string? name = null, string? sector = null, string? industry = null, string? assetType = null);
    }
}

using FinancialRisk.Api.Models;

namespace FinancialRisk.Api.Services
{
    public interface IFinancialDataService
    {
        Task<ApiResponse<StockQuote>> GetStockQuoteAsync(string symbol);
        Task<ApiResponse<List<StockQuote>>> GetStockHistoryAsync(string symbol, int days = 30);
        Task<ApiResponse<decimal>> GetCurrentPriceAsync(string symbol);
        
        // New methods for saving data to database
        Task<ApiResponse<bool>> SaveStockQuoteToDatabaseAsync(string symbol);
        Task<ApiResponse<bool>> SaveStockHistoryToDatabaseAsync(string symbol, int days = 30);
    }
}

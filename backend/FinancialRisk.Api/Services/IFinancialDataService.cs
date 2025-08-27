using FinancialRisk.Api.Models;

namespace FinancialRisk.Api.Services
{
    public interface IFinancialDataService
    {
        Task<ApiResponse<StockQuote>> GetStockQuoteAsync(string symbol);
        Task<ApiResponse<ForexQuote>> GetForexQuoteAsync(string fromCurrency, string toCurrency);
        Task<ApiResponse<List<StockQuote>>> GetStockHistoryAsync(string symbol, int days = 30);
        Task<ApiResponse<decimal>> GetCurrentPriceAsync(string symbol);
    }
}

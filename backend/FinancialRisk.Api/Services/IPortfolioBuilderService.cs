using FinancialRisk.Api.Models;

namespace FinancialRisk.Api.Services
{
    public interface IPortfolioBuilderService
    {
        Task<ApiResponse<AssetSearchResponse>> SearchAssetsAsync(AssetSearchRequest request);
        Task<ApiResponse<Portfolio>> SavePortfolioAsync(PortfolioSaveRequest request);
        Task<ApiResponse<Portfolio>> LoadPortfolioAsync(PortfolioLoadRequest request);
        Task<ApiResponse<PortfolioListResponse>> ListPortfoliosAsync(PortfolioListRequest request);
        Task<ApiResponse<bool>> DeletePortfolioAsync(string portfolioId);
        Task<ApiResponse<PortfolioSummary>> GetPortfolioSummaryAsync(string portfolioId);
        Task<ApiResponse<PortfolioValidationResult>> ValidatePortfolioAsync(Portfolio portfolio);
        Task<ApiResponse<PortfolioRebalanceRequest>> RebalancePortfolioAsync(PortfolioRebalanceRequest request);
        Task<ApiResponse<PortfolioPerformanceMetrics>> GetPortfolioPerformanceAsync(string portfolioId, DateTime startDate, DateTime endDate);
        Task<ApiResponse<PortfolioComparisonResult>> ComparePortfoliosAsync(PortfolioComparisonRequest request);
        Task<ApiResponse<List<string>>> GetAvailableSectorsAsync();
        Task<ApiResponse<List<string>>> GetAvailableExchangesAsync();
        Task<ApiResponse<Dictionary<string, object>>> GetMarketDataAsync(List<string> symbols);
    }
}

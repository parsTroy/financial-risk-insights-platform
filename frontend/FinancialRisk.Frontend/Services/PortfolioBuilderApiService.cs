using FinancialRisk.Frontend.Models;

namespace FinancialRisk.Frontend.Services
{
    public class PortfolioBuilderApiService
    {
        private readonly ApiService _apiService;
        private readonly ILogger<PortfolioBuilderApiService> _logger;

        public PortfolioBuilderApiService(ApiService apiService, ILogger<PortfolioBuilderApiService> logger)
        {
            _apiService = apiService;
            _logger = logger;
        }

        public async Task<ApiResponse<AssetSearchResponse>?> SearchAssetsAsync(AssetSearchRequest request)
        {
            try
            {
                _logger.LogInformation("Searching assets with query: {Query}", request.Query);
                return await _apiService.PostAsync<ApiResponse<AssetSearchResponse>>("portfoliobuilder/search-assets", request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching assets");
                return new ApiResponse<AssetSearchResponse>
                {
                    IsSuccess = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<ApiResponse<Portfolio>?> SavePortfolioAsync(PortfolioSaveRequest request)
        {
            try
            {
                _logger.LogInformation("Saving portfolio: {PortfolioName}", request.Name);
                return await _apiService.PostAsync<ApiResponse<Portfolio>>("portfoliobuilder/save", request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving portfolio");
                return new ApiResponse<Portfolio>
                {
                    IsSuccess = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<ApiResponse<Portfolio>?> LoadPortfolioAsync(PortfolioLoadRequest request)
        {
            try
            {
                _logger.LogInformation("Loading portfolio: {PortfolioId}", request.PortfolioId);
                return await _apiService.PostAsync<ApiResponse<Portfolio>>("portfoliobuilder/load", request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading portfolio");
                return new ApiResponse<Portfolio>
                {
                    IsSuccess = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<ApiResponse<PortfolioListResponse>?> ListPortfoliosAsync(PortfolioListRequest request)
        {
            try
            {
                _logger.LogInformation("Listing portfolios for user");
                return await _apiService.PostAsync<ApiResponse<PortfolioListResponse>>("portfoliobuilder/list", request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error listing portfolios");
                return new ApiResponse<PortfolioListResponse>
                {
                    IsSuccess = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<ApiResponse<bool>?> DeletePortfolioAsync(string portfolioId)
        {
            try
            {
                _logger.LogInformation("Deleting portfolio: {PortfolioId}", portfolioId);
                var success = await _apiService.DeleteAsync($"portfoliobuilder/{portfolioId}");
                return new ApiResponse<bool>
                {
                    IsSuccess = success,
                    Data = success,
                    ErrorMessage = success ? null : "Failed to delete portfolio"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting portfolio");
                return new ApiResponse<bool>
                {
                    IsSuccess = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<ApiResponse<PortfolioSummary>?> GetPortfolioSummaryAsync(string portfolioId)
        {
            try
            {
                _logger.LogInformation("Getting portfolio summary: {PortfolioId}", portfolioId);
                return await _apiService.GetAsync<ApiResponse<PortfolioSummary>>($"portfoliobuilder/{portfolioId}/summary");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting portfolio summary");
                return new ApiResponse<PortfolioSummary>
                {
                    IsSuccess = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<ApiResponse<PortfolioValidationResult>?> ValidatePortfolioAsync(Portfolio portfolio)
        {
            try
            {
                _logger.LogInformation("Validating portfolio: {PortfolioId}", portfolio.Id);
                return await _apiService.PostAsync<ApiResponse<PortfolioValidationResult>>("portfoliobuilder/validate", portfolio);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating portfolio");
                return new ApiResponse<PortfolioValidationResult>
                {
                    IsSuccess = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<ApiResponse<PortfolioRebalanceRequest>?> RebalancePortfolioAsync(PortfolioRebalanceRequest request)
        {
            try
            {
                _logger.LogInformation("Rebalancing portfolio: {PortfolioId}", request.PortfolioId);
                return await _apiService.PostAsync<ApiResponse<PortfolioRebalanceRequest>>("portfoliobuilder/rebalance", request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rebalancing portfolio");
                return new ApiResponse<PortfolioRebalanceRequest>
                {
                    IsSuccess = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<ApiResponse<PortfolioPerformanceMetrics>?> GetPortfolioPerformanceAsync(string portfolioId, DateTime startDate, DateTime endDate)
        {
            try
            {
                _logger.LogInformation("Getting portfolio performance: {PortfolioId}", portfolioId);
                var request = new { PortfolioId = portfolioId, StartDate = startDate, EndDate = endDate };
                return await _apiService.PostAsync<ApiResponse<PortfolioPerformanceMetrics>>("portfoliobuilder/performance", request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting portfolio performance");
                return new ApiResponse<PortfolioPerformanceMetrics>
                {
                    IsSuccess = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<ApiResponse<PortfolioComparisonResult>?> ComparePortfoliosAsync(PortfolioComparisonRequest request)
        {
            try
            {
                _logger.LogInformation("Comparing portfolios");
                return await _apiService.PostAsync<ApiResponse<PortfolioComparisonResult>>("portfoliobuilder/compare", request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error comparing portfolios");
                return new ApiResponse<PortfolioComparisonResult>
                {
                    IsSuccess = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<ApiResponse<List<string>>?> GetAvailableSectorsAsync()
        {
            try
            {
                _logger.LogInformation("Getting available sectors");
                return await _apiService.GetAsync<ApiResponse<List<string>>>("portfoliobuilder/sectors");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available sectors");
                return new ApiResponse<List<string>>
                {
                    IsSuccess = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<ApiResponse<List<string>>?> GetAvailableExchangesAsync()
        {
            try
            {
                _logger.LogInformation("Getting available exchanges");
                return await _apiService.GetAsync<ApiResponse<List<string>>>("portfoliobuilder/exchanges");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available exchanges");
                return new ApiResponse<List<string>>
                {
                    IsSuccess = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<ApiResponse<Dictionary<string, object>>?> GetMarketDataAsync(List<string> symbols)
        {
            try
            {
                _logger.LogInformation("Getting market data for {SymbolCount} symbols", symbols.Count);
                var request = new { Symbols = symbols };
                return await _apiService.PostAsync<ApiResponse<Dictionary<string, object>>>("portfoliobuilder/market-data", request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting market data");
                return new ApiResponse<Dictionary<string, object>>
                {
                    IsSuccess = false,
                    ErrorMessage = ex.Message
                };
            }
        }
    }
}

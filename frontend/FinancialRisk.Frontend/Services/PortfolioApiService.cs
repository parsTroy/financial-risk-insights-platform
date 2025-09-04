using FinancialRisk.Frontend.Models;

namespace FinancialRisk.Frontend.Services
{
    public class PortfolioApiService
    {
        private readonly ApiService _apiService;
        private readonly ILogger<PortfolioApiService> _logger;

        public PortfolioApiService(ApiService apiService, ILogger<PortfolioApiService> logger)
        {
            _apiService = apiService;
            _logger = logger;
        }

        public async Task<ApiResponse<PortfolioOptimizationResult>?> OptimizePortfolioAsync(PortfolioOptimizationRequest request)
        {
            try
            {
                _logger.LogInformation("Optimizing portfolio with method: {Method}", request.Method);
                return await _apiService.PostAsync<ApiResponse<PortfolioOptimizationResult>>("api/PortfolioOptimization/optimize", request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error optimizing portfolio");
                return new ApiResponse<PortfolioOptimizationResult>
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<ApiResponse<EfficientFrontierResult>?> CalculateEfficientFrontierAsync(EfficientFrontierRequest request)
        {
            try
            {
                _logger.LogInformation("Calculating efficient frontier with {NumPoints} points", request.NumPoints);
                return await _apiService.PostAsync<ApiResponse<EfficientFrontierResult>>("api/PortfolioOptimization/efficient-frontier", request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating efficient frontier");
                return new ApiResponse<EfficientFrontierResult>
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<ApiResponse<RiskBudgetingResult>?> PerformRiskBudgetingAsync(RiskBudgetingRequest request)
        {
            try
            {
                _logger.LogInformation("Performing risk budgeting");
                return await _apiService.PostAsync<ApiResponse<RiskBudgetingResult>>("api/PortfolioOptimization/risk-budgeting", request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing risk budgeting");
                return new ApiResponse<RiskBudgetingResult>
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<ApiResponse<BlackLittermanResult>?> PerformBlackLittermanOptimizationAsync(BlackLittermanRequest request)
        {
            try
            {
                _logger.LogInformation("Performing Black-Litterman optimization");
                return await _apiService.PostAsync<ApiResponse<BlackLittermanResult>>("api/PortfolioOptimization/black-litterman", request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing Black-Litterman optimization");
                return new ApiResponse<BlackLittermanResult>
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<ApiResponse<List<string>>?> GetAvailableOptimizationMethodsAsync()
        {
            try
            {
                _logger.LogInformation("Getting available optimization methods");
                return await _apiService.GetAsync<ApiResponse<List<string>>>("api/PortfolioOptimization/methods");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available optimization methods");
                return new ApiResponse<List<string>>
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<ApiResponse<List<string>>?> GetAvailableConstraintsAsync()
        {
            try
            {
                _logger.LogInformation("Getting available constraints");
                return await _apiService.GetAsync<ApiResponse<List<string>>>("api/PortfolioOptimization/constraints");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available constraints");
                return new ApiResponse<List<string>>
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<ApiResponse<object>?> GetOptimizationHistoryAsync(string portfolioName)
        {
            try
            {
                _logger.LogInformation("Getting optimization history for portfolio: {PortfolioName}", portfolioName);
                return await _apiService.GetAsync<ApiResponse<object>>($"api/PortfolioOptimization/history/{portfolioName}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting optimization history");
                return new ApiResponse<object>
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<ApiResponse<object>?> GetOptimizationStatisticsAsync()
        {
            try
            {
                _logger.LogInformation("Getting optimization statistics");
                return await _apiService.GetAsync<ApiResponse<object>>("api/PortfolioOptimization/stats");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting optimization statistics");
                return new ApiResponse<object>
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }
    }
}

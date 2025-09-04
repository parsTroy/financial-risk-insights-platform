using FinancialRisk.Frontend.Models;

namespace FinancialRisk.Frontend.Services
{
    public class VaRApiService
    {
        private readonly ApiService _apiService;
        private readonly ILogger<VaRApiService> _logger;

        public VaRApiService(ApiService apiService, ILogger<VaRApiService> logger)
        {
            _apiService = apiService;
            _logger = logger;
        }

        public async Task<ApiResponse<VaRCalculationResult>?> CalculateVaRAsync(VaRCalculationRequest request)
        {
            try
            {
                _logger.LogInformation("Calculating VaR with method: {Method}", request.Method);
                return await _apiService.PostAsync<ApiResponse<VaRCalculationResult>>("var/calculate", request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating VaR");
                return new ApiResponse<VaRCalculationResult>
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<ApiResponse<MonteCarloSimulationResult>?> CalculateMonteCarloVaRAsync(MonteCarloSimulationRequest request)
        {
            try
            {
                _logger.LogInformation("Calculating Monte Carlo VaR with {NumSimulations} simulations", request.NumSimulations);
                return await _apiService.PostAsync<ApiResponse<MonteCarloSimulationResult>>("monte-carlo/var", request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating Monte Carlo VaR");
                return new ApiResponse<MonteCarloSimulationResult>
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<ApiResponse<PortfolioVaRResult>?> CalculatePortfolioVaRAsync(PortfolioVaRRequest request)
        {
            try
            {
                _logger.LogInformation("Calculating portfolio VaR for {AssetCount} assets", request.Assets.Count);
                return await _apiService.PostAsync<ApiResponse<PortfolioVaRResult>>("monte-carlo/portfolio-var", request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating portfolio VaR");
                return new ApiResponse<PortfolioVaRResult>
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<ApiResponse<List<string>>?> GetAvailableVaRMethodsAsync()
        {
            try
            {
                _logger.LogInformation("Getting available VaR methods");
                return await _apiService.GetAsync<ApiResponse<List<string>>>("var/methods");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available VaR methods");
                return new ApiResponse<List<string>>
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<ApiResponse<object>?> PerformStressTestAsync(MonteCarloSimulationRequest request)
        {
            try
            {
                _logger.LogInformation("Performing stress test");
                return await _apiService.PostAsync<ApiResponse<object>>("monte-carlo/stress-test", request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing stress test");
                return new ApiResponse<object>
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }
    }
}

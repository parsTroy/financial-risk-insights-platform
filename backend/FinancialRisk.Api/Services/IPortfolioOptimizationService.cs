using FinancialRisk.Api.Models;

namespace FinancialRisk.Api.Services
{
    public interface IPortfolioOptimizationService
    {
        Task<PortfolioOptimizationResponse> OptimizePortfolioAsync(PortfolioOptimizationRequest request);
        Task<EfficientFrontier> CalculateEfficientFrontierAsync(PortfolioOptimizationRequest request);
        Task<RiskBudgetingResult> OptimizeRiskBudgetingAsync(RiskBudgetingRequest request);
        Task<BlackLittermanResult> OptimizeBlackLittermanAsync(BlackLittermanRequest request);
        Task<TransactionCostOptimizationResult> OptimizeTransactionCostsAsync(TransactionCostOptimizationRequest request);
        Task<List<PortfolioOptimizationResult>> GetOptimizationHistoryAsync(string portfolioName, int limit = 100);
        Task<PortfolioOptimizationResult> GetOptimizationByIdAsync(int id);
        Task<bool> SaveOptimizationResultAsync(PortfolioOptimizationResult result);
        Task<List<string>> GetAvailableOptimizationMethodsAsync();
        Task<Dictionary<string, object>> GetOptimizationConstraintsAsync();
    }
}

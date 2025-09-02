using FinancialRisk.Api.Models;
using Microsoft.Extensions.Logging;
using System.Runtime.InteropServices;

namespace FinancialRisk.Api.Services
{
    public interface IRiskMetricsService
    {
        Task<RiskMetrics> CalculateRiskMetricsAsync(string symbol, int days = 252);
        Task<PortfolioRiskMetrics> CalculatePortfolioRiskMetricsAsync(List<string> symbols, List<decimal> weights, int days = 252);
        Task<List<RiskMetrics>> CalculateMultipleAssetRiskMetricsAsync(List<string> symbols, int days = 252);
    }

    public class RiskMetricsService : IRiskMetricsService
    {
        private readonly ILogger<RiskMetricsService> _logger;
        private readonly IFinancialDataService _financialDataService;
        private readonly IDataPersistenceService _dataPersistenceService;

        // C++ library imports
        [DllImport("RiskCalculations.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern double CalculateVolatility(double[] returns, int length);

        [DllImport("RiskCalculations.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern double CalculateBeta(double[] assetReturns, double[] benchmarkReturns, int length);

        [DllImport("RiskCalculations.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern double CalculateSharpeRatio(double[] returns, double riskFreeRate, int length);

        [DllImport("RiskCalculations.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern double CalculateSortinoRatio(double[] returns, double riskFreeRate, int length);

        [DllImport("RiskCalculations.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern double CalculateValueAtRisk(double[] returns, double confidenceLevel, int length);

        [DllImport("RiskCalculations.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern double CalculateExpectedShortfall(double[] returns, double confidenceLevel, int length);

        public RiskMetricsService(
            ILogger<RiskMetricsService> logger,
            IFinancialDataService financialDataService,
            IDataPersistenceService dataPersistenceService)
        {
            _logger = logger;
            _financialDataService = financialDataService;
            _dataPersistenceService = dataPersistenceService;
        }

        public async Task<RiskMetrics> CalculateRiskMetricsAsync(string symbol, int days = 252)
        {
            try
            {
                _logger.LogInformation("Calculating risk metrics for {Symbol} over {Days} days", symbol, days);

                // Fetch historical data
                var historyResult = await _financialDataService.GetStockHistoryAsync(symbol, days);
                if (!historyResult.Success || historyResult.Data == null || !historyResult.Data.Any())
                {
                    _logger.LogWarning("No historical data available for {Symbol}", symbol);
                    return new RiskMetrics { Symbol = symbol, Error = "No historical data available" };
                }

                // Calculate returns
                var returns = CalculateReturns(historyResult.Data);
                if (returns.Length < 2)
                {
                    _logger.LogWarning("Insufficient data for risk calculations for {Symbol}", symbol);
                    return new RiskMetrics { Symbol = symbol, Error = "Insufficient data for calculations" };
                }

                // Calculate risk metrics using C++ library
                var volatility = CalculateVolatility(returns, returns.Length);
                var sharpeRatio = CalculateSharpeRatio(returns, 0.02, returns.Length); // 2% risk-free rate
                var sortinoRatio = CalculateSortinoRatio(returns, 0.02, returns.Length);
                var var95 = CalculateValueAtRisk(returns, 0.95, returns.Length);
                var var99 = CalculateValueAtRisk(returns, 0.99, returns.Length);
                var expectedShortfall95 = CalculateExpectedShortfall(returns, 0.95, returns.Length);
                var expectedShortfall99 = CalculateExpectedShortfall(returns, 0.99, returns.Length);

                var riskMetrics = new RiskMetrics
                {
                    Symbol = symbol,
                    Volatility = volatility,
                    SharpeRatio = sharpeRatio,
                    SortinoRatio = sortinoRatio,
                    ValueAtRisk95 = var95,
                    ValueAtRisk99 = var99,
                    ExpectedShortfall95 = expectedShortfall95,
                    ExpectedShortfall99 = expectedShortfall99,
                    CalculationDate = DateTime.UtcNow,
                    DataPoints = returns.Length
                };

                _logger.LogInformation("Successfully calculated risk metrics for {Symbol}: Volatility={Volatility:F4}, Sharpe={Sharpe:F4}", 
                    symbol, volatility, sharpeRatio);

                return riskMetrics;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating risk metrics for {Symbol}", symbol);
                return new RiskMetrics { Symbol = symbol, Error = ex.Message };
            }
        }

        public async Task<PortfolioRiskMetrics> CalculatePortfolioRiskMetricsAsync(List<string> symbols, List<decimal> weights, int days = 252)
        {
            try
            {
                _logger.LogInformation("Calculating portfolio risk metrics for {SymbolCount} assets", symbols.Count);

                if (symbols.Count != weights.Count)
                {
                    throw new ArgumentException("Number of symbols must match number of weights");
                }

                // Fetch data for all assets
                var assetData = new Dictionary<string, double[]>();
                foreach (var symbol in symbols)
                {
                    var historyResult = await _financialDataService.GetStockHistoryAsync(symbol, days);
                    if (historyResult.Success && historyResult.Data != null && historyResult.Data.Any())
                    {
                        var returns = CalculateReturns(historyResult.Data);
                        if (returns.Length >= 2)
                        {
                            assetData[symbol] = returns;
                        }
                    }
                }

                if (assetData.Count < 2)
                {
                    _logger.LogWarning("Insufficient asset data for portfolio calculations");
                    return new PortfolioRiskMetrics { Error = "Insufficient asset data" };
                }

                // Calculate portfolio returns (weighted average)
                var portfolioReturns = CalculatePortfolioReturns(assetData, weights);

                // Calculate portfolio risk metrics
                var portfolioVolatility = CalculateVolatility(portfolioReturns, portfolioReturns.Length);
                var portfolioSharpe = CalculateSharpeRatio(portfolioReturns, 0.02, portfolioReturns.Length);
                var portfolioSortino = CalculateSortinoRatio(portfolioReturns, 0.02, portfolioReturns.Length);
                var portfolioVar95 = CalculateValueAtRisk(portfolioReturns, 0.95, portfolioReturns.Length);
                var portfolioVar99 = CalculateValueAtRisk(portfolioReturns, 0.99, portfolioReturns.Length);

                var portfolioMetrics = new PortfolioRiskMetrics
                {
                    Symbols = symbols,
                    Weights = weights,
                    Volatility = portfolioVolatility,
                    SharpeRatio = portfolioSharpe,
                    SortinoRatio = portfolioSortino,
                    ValueAtRisk95 = portfolioVar95,
                    ValueAtRisk99 = portfolioVar99,
                    CalculationDate = DateTime.UtcNow,
                    DataPoints = portfolioReturns.Length
                };

                _logger.LogInformation("Successfully calculated portfolio risk metrics: Volatility={Volatility:F4}, Sharpe={Sharpe:F4}", 
                    portfolioVolatility, portfolioSharpe);

                return portfolioMetrics;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating portfolio risk metrics");
                return new PortfolioRiskMetrics { Error = ex.Message };
            }
        }

        public async Task<List<RiskMetrics>> CalculateMultipleAssetRiskMetricsAsync(List<string> symbols, int days = 252)
        {
            var results = new List<RiskMetrics>();
            
            foreach (var symbol in symbols)
            {
                var metrics = await CalculateRiskMetricsAsync(symbol, days);
                results.Add(metrics);
            }

            return results;
        }

        private double[] CalculateReturns(List<StockQuote> prices)
        {
            if (prices.Count < 2) return new double[0];

            var returns = new double[prices.Count - 1];
            for (int i = 1; i < prices.Count; i++)
            {
                returns[i - 1] = (double)((prices[i].Close - prices[i - 1].Close) / prices[i - 1].Close);
            }
            return returns;
        }

        private double[] CalculatePortfolioReturns(Dictionary<string, double[]> assetData, List<decimal> weights)
        {
            var minLength = assetData.Values.Min(arr => arr.Length);
            var portfolioReturns = new double[minLength];

            for (int i = 0; i < minLength; i++)
            {
                portfolioReturns[i] = 0;
                int weightIndex = 0;
                foreach (var kvp in assetData)
                {
                    portfolioReturns[i] += (double)weights[weightIndex] * kvp.Value[i];
                    weightIndex++;
                }
            }

            return portfolioReturns;
        }
    }
}

using FinancialRisk.Api.Models;
using Microsoft.Extensions.Logging;

namespace FinancialRisk.Api.Services
{
    public class RiskMetricsServiceFallback : IRiskMetricsService
    {
        private readonly ILogger<RiskMetricsServiceFallback> _logger;
        private readonly IFinancialDataService _financialDataService;
        private readonly IDataPersistenceService _dataPersistenceService;

        public RiskMetricsServiceFallback(
            ILogger<RiskMetricsServiceFallback> logger,
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
                _logger.LogInformation("Calculating risk metrics for {Symbol} using fallback implementation", symbol);

                // Get historical data
                var historicalDataResponse = await _financialDataService.GetStockHistoryAsync(symbol, days);
                if (!historicalDataResponse.Success || historicalDataResponse.Data == null || !historicalDataResponse.Data.Any())
                {
                    return new RiskMetrics
                    {
                        Symbol = symbol,
                        Error = "No historical data available"
                    };
                }

                // Calculate returns
                var returns = CalculateReturns(historicalDataResponse.Data);
                if (returns.Count < 2)
                {
                    return new RiskMetrics
                    {
                        Symbol = symbol,
                        Error = "Insufficient data for risk calculations"
                    };
                }

                // Calculate risk metrics using C# implementations
                var volatility = CalculateVolatility(returns);
                var meanReturn = returns.Average();
                var riskFreeRate = 0.02; // 2% risk-free rate
                var sharpeRatio = CalculateSharpeRatio(returns, riskFreeRate);
                var sortinoRatio = CalculateSortinoRatio(returns, riskFreeRate);
                var var95 = CalculateValueAtRisk(returns, 0.95);
                var var99 = CalculateValueAtRisk(returns, 0.99);
                var es95 = CalculateExpectedShortfall(returns, 0.95);
                var es99 = CalculateExpectedShortfall(returns, 0.99);
                var maxDrawdown = CalculateMaximumDrawdown(returns);

                return new RiskMetrics
                {
                    Symbol = symbol,
                    Volatility = volatility,
                    SharpeRatio = sharpeRatio,
                    SortinoRatio = sortinoRatio,
                    ValueAtRisk95 = var95,
                    ValueAtRisk99 = var99,
                    ExpectedShortfall95 = es95,
                    ExpectedShortfall99 = es99,
                    MaximumDrawdown = maxDrawdown,
                    InformationRatio = 0, // Not calculated in fallback
                    CalculationDate = DateTime.UtcNow,
                    DataPoints = returns.Count
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating risk metrics for {Symbol}", symbol);
                return new RiskMetrics
                {
                    Symbol = symbol,
                    Error = ex.Message
                };
            }
        }

        public async Task<PortfolioRiskMetrics> CalculatePortfolioRiskMetricsAsync(List<string> symbols, List<decimal> weights, int days = 252)
        {
            try
            {
                _logger.LogInformation("Calculating portfolio risk metrics for {Count} assets using fallback implementation", symbols.Count);

                if (symbols.Count != weights.Count)
                {
                    return new PortfolioRiskMetrics
                    {
                        Error = "Number of symbols must match number of weights"
                    };
                }

                // Get individual asset metrics
                var assetMetrics = new List<RiskMetrics>();
                for (int i = 0; i < symbols.Count; i++)
                {
                    var metrics = await CalculateRiskMetricsAsync(symbols[i], days);
                    if (!string.IsNullOrEmpty(metrics.Error))
                    {
                        return new PortfolioRiskMetrics
                        {
                            Error = $"Error calculating metrics for {symbols[i]}: {metrics.Error}"
                        };
                    }
                    assetMetrics.Add(metrics);
                }

                // Calculate portfolio metrics (simplified)
                var portfolioVolatility = CalculatePortfolioVolatility(assetMetrics, weights);
                var portfolioReturn = CalculatePortfolioReturn(assetMetrics, weights);
                var riskFreeRate = 0.02;
                var portfolioSharpe = (portfolioReturn - riskFreeRate) / portfolioVolatility;

                return new PortfolioRiskMetrics
                {
                    Symbols = symbols,
                    Weights = weights,
                    Volatility = portfolioVolatility,
                    SharpeRatio = portfolioSharpe,
                    SortinoRatio = portfolioSharpe, // Simplified
                    ValueAtRisk95 = portfolioVolatility * 1.645, // Simplified VaR
                    ValueAtRisk99 = portfolioVolatility * 2.326, // Simplified VaR
                    ExpectedShortfall95 = portfolioVolatility * 2.0, // Simplified ES
                    ExpectedShortfall99 = portfolioVolatility * 2.5, // Simplified ES
                    MaximumDrawdown = portfolioVolatility * 2.0, // Simplified
                    CalculationDate = DateTime.UtcNow,
                    DataPoints = assetMetrics.FirstOrDefault()?.DataPoints ?? 0
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating portfolio risk metrics");
                return new PortfolioRiskMetrics
                {
                    Error = ex.Message
                };
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

        private List<double> CalculateReturns(List<StockQuote> historicalData)
        {
            var returns = new List<double>();
            var prices = historicalData.OrderBy(d => d.Timestamp).Select(d => d.Close).ToList();

            for (int i = 1; i < prices.Count; i++)
            {
                var dailyReturn = (double)((prices[i] - prices[i - 1]) / prices[i - 1]);
                returns.Add(dailyReturn);
            }

            return returns;
        }

        private double CalculateVolatility(List<double> returns)
        {
            if (returns.Count < 2) return 0;

            var mean = returns.Average();
            var variance = returns.Sum(r => Math.Pow(r - mean, 2)) / (returns.Count - 1);
            return Math.Sqrt(variance);
        }

        private double CalculateSharpeRatio(List<double> returns, double riskFreeRate)
        {
            if (returns.Count < 2) return 0;

            var meanReturn = returns.Average();
            var volatility = CalculateVolatility(returns);
            
            if (volatility == 0) return 0;
            
            return (meanReturn - riskFreeRate / 252) / volatility; // Daily risk-free rate
        }

        private double CalculateSortinoRatio(List<double> returns, double riskFreeRate)
        {
            if (returns.Count < 2) return 0;

            var meanReturn = returns.Average();
            var downsideReturns = returns.Where(r => r < riskFreeRate / 252).ToList();
            
            if (downsideReturns.Count == 0) return double.PositiveInfinity;
            
            var downsideVariance = downsideReturns.Sum(r => Math.Pow(r - riskFreeRate / 252, 2)) / downsideReturns.Count;
            var downsideDeviation = Math.Sqrt(downsideVariance);
            
            if (downsideDeviation == 0) return double.PositiveInfinity;
            
            return (meanReturn - riskFreeRate / 252) / downsideDeviation;
        }

        private double CalculateValueAtRisk(List<double> returns, double confidenceLevel)
        {
            if (returns.Count == 0) return 0;

            var sortedReturns = returns.OrderBy(r => r).ToList();
            var index = (int)Math.Ceiling((1 - confidenceLevel) * sortedReturns.Count) - 1;
            index = Math.Max(0, Math.Min(index, sortedReturns.Count - 1));
            
            return Math.Abs(sortedReturns[index]);
        }

        private double CalculateExpectedShortfall(List<double> returns, double confidenceLevel)
        {
            if (returns.Count == 0) return 0;

            var var = CalculateValueAtRisk(returns, confidenceLevel);
            var tailReturns = returns.Where(r => r <= -var).ToList();
            
            if (tailReturns.Count == 0) return var;
            
            return Math.Abs(tailReturns.Average());
        }

        private double CalculateMaximumDrawdown(List<double> returns)
        {
            if (returns.Count == 0) return 0;

            var cumulativeReturns = new List<double> { 0 };
            for (int i = 0; i < returns.Count; i++)
            {
                cumulativeReturns.Add(cumulativeReturns[i] + returns[i]);
            }

            var peak = cumulativeReturns[0];
            var maxDrawdown = 0.0;

            foreach (var value in cumulativeReturns)
            {
                if (value > peak)
                {
                    peak = value;
                }
                else
                {
                    var drawdown = peak - value;
                    if (drawdown > maxDrawdown)
                    {
                        maxDrawdown = drawdown;
                    }
                }
            }

            return maxDrawdown;
        }

        private double CalculatePortfolioVolatility(List<RiskMetrics> assetMetrics, List<decimal> weights)
        {
            // Simplified portfolio volatility calculation
            var weightedVolatility = 0.0;
            for (int i = 0; i < assetMetrics.Count; i++)
            {
                weightedVolatility += (double)weights[i] * assetMetrics[i].Volatility;
            }
            return weightedVolatility;
        }

        private double CalculatePortfolioReturn(List<RiskMetrics> assetMetrics, List<decimal> weights)
        {
            // Simplified portfolio return calculation
            var weightedReturn = 0.0;
            for (int i = 0; i < assetMetrics.Count; i++)
            {
                weightedReturn += (double)weights[i] * (assetMetrics[i].Volatility * 0.1); // Simplified expected return
            }
            return weightedReturn;
        }
    }
}

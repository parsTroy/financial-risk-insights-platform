using System.Net.Http.Json;
using FinancialRisk.Frontend.Models;

namespace FinancialRisk.Frontend.Services
{
    public class RiskMetricsApiService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<RiskMetricsApiService> _logger;

        public RiskMetricsApiService(HttpClient httpClient, ILogger<RiskMetricsApiService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<RiskMetrics?> GetAssetRiskMetricsAsync(string symbol, int days = 252)
        {
            try
            {
                _logger.LogInformation("Fetching risk metrics for symbol: {Symbol}", symbol);
                var response = await _httpClient.GetFromJsonAsync<RiskMetrics>($"api/riskmetrics/asset/{symbol}?days={days}");
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching risk metrics for symbol: {Symbol}", symbol);
                return null;
            }
        }

        public async Task<List<RiskMetrics>?> GetMultipleAssetRiskMetricsAsync(List<string> symbols, int days = 252)
        {
            try
            {
                _logger.LogInformation("Fetching risk metrics for {Count} symbols", symbols.Count);
                var response = await _httpClient.PostAsJsonAsync($"api/riskmetrics/assets/batch?days={days}", symbols);
                
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<List<RiskMetrics>>();
                }
                
                _logger.LogWarning("Failed to fetch multiple asset risk metrics. Status: {StatusCode}", response.StatusCode);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching multiple asset risk metrics");
                return null;
            }
        }

        public async Task<PortfolioRiskMetrics?> GetPortfolioRiskMetricsAsync(PortfolioRiskMetricsRequest request)
        {
            try
            {
                _logger.LogInformation("Fetching portfolio risk metrics for {Count} assets", request.Symbols.Count);
                var response = await _httpClient.PostAsJsonAsync("api/riskmetrics/portfolio", request);
                
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<PortfolioRiskMetrics>();
                }
                
                _logger.LogWarning("Failed to fetch portfolio risk metrics. Status: {StatusCode}", response.StatusCode);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching portfolio risk metrics");
                return null;
            }
        }

        public List<RiskAlert> GenerateRiskAlerts(List<RiskMetrics> metrics)
        {
            var alerts = new List<RiskAlert>();

            foreach (var metric in metrics)
            {
                // High volatility alert
                if (metric.Volatility > 0.3) // 30% volatility threshold
                {
                    alerts.Add(new RiskAlert
                    {
                        Symbol = metric.Symbol,
                        AlertType = "High Volatility",
                        Message = $"Volatility is {metric.Volatility:P2}, exceeding 30% threshold",
                        Severity = metric.Volatility > 0.5 ? "critical" : "high",
                        CurrentValue = metric.Volatility,
                        Threshold = 0.3,
                        Timestamp = DateTime.UtcNow
                    });
                }

                // Low Sharpe ratio alert
                if (metric.SharpeRatio < 0.5) // Low Sharpe ratio threshold
                {
                    alerts.Add(new RiskAlert
                    {
                        Symbol = metric.Symbol,
                        AlertType = "Low Sharpe Ratio",
                        Message = $"Sharpe ratio is {metric.SharpeRatio:F2}, below 0.5 threshold",
                        Severity = metric.SharpeRatio < 0 ? "critical" : "medium",
                        CurrentValue = metric.SharpeRatio,
                        Threshold = 0.5,
                        Timestamp = DateTime.UtcNow
                    });
                }

                // High VaR alert
                if (metric.ValueAtRisk95 > 0.05) // 5% VaR threshold
                {
                    alerts.Add(new RiskAlert
                    {
                        Symbol = metric.Symbol,
                        AlertType = "High VaR",
                        Message = $"95% VaR is {metric.ValueAtRisk95:P2}, exceeding 5% threshold",
                        Severity = metric.ValueAtRisk95 > 0.1 ? "critical" : "high",
                        CurrentValue = metric.ValueAtRisk95,
                        Threshold = 0.05,
                        Timestamp = DateTime.UtcNow
                    });
                }

                // High maximum drawdown alert
                if (metric.MaximumDrawdown > 0.2) // 20% drawdown threshold
                {
                    alerts.Add(new RiskAlert
                    {
                        Symbol = metric.Symbol,
                        AlertType = "High Drawdown",
                        Message = $"Maximum drawdown is {metric.MaximumDrawdown:P2}, exceeding 20% threshold",
                        Severity = metric.MaximumDrawdown > 0.4 ? "critical" : "high",
                        CurrentValue = metric.MaximumDrawdown,
                        Threshold = 0.2,
                        Timestamp = DateTime.UtcNow
                    });
                }
            }

            return alerts.OrderByDescending(a => a.Severity == "critical" ? 4 : a.Severity == "high" ? 3 : a.Severity == "medium" ? 2 : 1).ToList();
        }
    }
}

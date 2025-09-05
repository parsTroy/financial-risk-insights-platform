using System.Net.Http.Json;
using FinancialRisk.Frontend.Models;

namespace FinancialRisk.Frontend.Services
{
    public class EfficientFrontierApiService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<EfficientFrontierApiService> _logger;

        public EfficientFrontierApiService(HttpClient httpClient, ILogger<EfficientFrontierApiService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<EfficientFrontier?> CalculateEfficientFrontierAsync(EfficientFrontierRequest request)
        {
            try
            {
                _logger.LogInformation("Calculating efficient frontier for {PortfolioName} with {Count} assets", 
                    request.PortfolioName, request.Assets.Count);
                
                var response = await _httpClient.PostAsJsonAsync("api/portfolio/efficient-frontier", request);
                
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<EfficientFrontier>();
                }
                
                _logger.LogWarning("Failed to calculate efficient frontier. Status: {StatusCode}", response.StatusCode);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating efficient frontier");
                return null;
            }
        }

        public async Task<List<string>?> GetOptimizationMethodsAsync()
        {
            try
            {
                _logger.LogInformation("Fetching available optimization methods");
                var response = await _httpClient.GetFromJsonAsync<List<string>>("api/portfolio/methods");
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching optimization methods");
                return null;
            }
        }

        public async Task<Dictionary<string, object>?> GetOptimizationConstraintsAsync()
        {
            try
            {
                _logger.LogInformation("Fetching optimization constraints");
                var response = await _httpClient.GetFromJsonAsync<Dictionary<string, object>>("api/portfolio/constraints");
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching optimization constraints");
                return null;
            }
        }

        public EfficientFrontierChartData PrepareChartData(EfficientFrontier frontier, List<AssetOptimizationData> assets, EfficientFrontierConstraints constraints)
        {
            var chartData = new EfficientFrontierChartData();

            if (!frontier.Success || !frontier.Points.Any())
            {
                return chartData;
            }

            // Prepare frontier points
            chartData.FrontierPoints = frontier.Points.Select(p => new ChartPoint
            {
                X = p.ExpectedVolatility,
                Y = p.ExpectedReturn,
                Label = $"Vol: {p.ExpectedVolatility:P2}, Ret: {p.ExpectedReturn:P2}",
                Color = "#007bff",
                Weights = p.AssetWeights
            }).ToList();

            // Prepare individual asset points
            if (constraints.ShowIndividualAssets)
            {
                chartData.IndividualAssets = assets.Select(a => new ChartPoint
                {
                    X = a.Volatility,
                    Y = a.ExpectedReturn,
                    Label = a.Symbol,
                    Color = "#dc3545",
                    Symbol = a.Symbol
                }).ToList();
            }

            // Prepare special points
            if (constraints.ShowMinVolatility && frontier.MinVolatilityPoint != null)
            {
                chartData.MinVolatilityPoint = new ChartPoint
                {
                    X = frontier.MinVolatilityPoint.ExpectedVolatility,
                    Y = frontier.MinVolatilityPoint.ExpectedReturn,
                    Label = "Min Volatility",
                    Color = "#28a745",
                    Weights = frontier.MinVolatilityPoint.AssetWeights
                };
            }

            if (constraints.ShowMaxSharpe && frontier.MaxSharpePoint != null)
            {
                chartData.MaxSharpePoint = new ChartPoint
                {
                    X = frontier.MaxSharpePoint.ExpectedVolatility,
                    Y = frontier.MaxSharpePoint.ExpectedReturn,
                    Label = "Max Sharpe",
                    Color = "#ffc107",
                    Weights = frontier.MaxSharpePoint.AssetWeights
                };
            }

            if (constraints.ShowMaxReturn && frontier.MaxReturnPoint != null)
            {
                chartData.MaxReturnPoint = new ChartPoint
                {
                    X = frontier.MaxReturnPoint.ExpectedVolatility,
                    Y = frontier.MaxReturnPoint.ExpectedReturn,
                    Label = "Max Return",
                    Color = "#17a2b8",
                    Weights = frontier.MaxReturnPoint.AssetWeights
                };
            }

            // Calculate bounds
            var allPoints = chartData.FrontierPoints.Concat(chartData.IndividualAssets).ToList();
            if (allPoints.Any())
            {
                chartData.MinVolatility = allPoints.Min(p => p.X);
                chartData.MaxVolatility = allPoints.Max(p => p.X);
                chartData.MinReturn = allPoints.Min(p => p.Y);
                chartData.MaxReturn = allPoints.Max(p => p.Y);
            }

            return chartData;
        }

        public List<List<double>> GenerateCovarianceMatrix(List<AssetOptimizationData> assets)
        {
            var n = assets.Count;
            var matrix = new List<List<double>>();
            var random = new Random(42); // Fixed seed for consistency

            for (int i = 0; i < n; i++)
            {
                var row = new List<double>();
                for (int j = 0; j < n; j++)
                {
                    if (i == j)
                    {
                        // Variance = volatility squared
                        row.Add(Math.Pow(assets[i].Volatility, 2));
                    }
                    else
                    {
                        // Covariance = correlation * sqrt(var1 * var2)
                        var correlation = random.NextDouble() * 0.6 + 0.2; // Random correlation between 0.2 and 0.8
                        var covariance = correlation * assets[i].Volatility * assets[j].Volatility;
                        row.Add(covariance);
                    }
                }
                matrix.Add(row);
            }

            return matrix;
        }
    }
}

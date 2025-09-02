using FinancialRisk.Api.Models;
using FinancialRisk.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace FinancialRisk.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RiskMetricsController : ControllerBase
    {
        private readonly ILogger<RiskMetricsController> _logger;
        private readonly IRiskMetricsService _riskMetricsService;

        public RiskMetricsController(
            ILogger<RiskMetricsController> logger,
            IRiskMetricsService riskMetricsService)
        {
            _logger = logger;
            _riskMetricsService = riskMetricsService;
        }

        /// <summary>
        /// Calculate risk metrics for a single asset
        /// </summary>
        /// <param name="symbol">Asset symbol (e.g., AAPL, MSFT)</param>
        /// <param name="days">Number of days for historical data (default: 252)</param>
        /// <returns>Risk metrics including volatility, Sharpe ratio, VaR, etc.</returns>
        [HttpGet("asset/{symbol}")]
        public async Task<IActionResult> GetAssetRiskMetrics(string symbol, [FromQuery] int days = 252)
        {
            if (string.IsNullOrWhiteSpace(symbol))
            {
                return BadRequest("Symbol is required");
            }

            if (days < 30 || days > 1000)
            {
                return BadRequest("Days must be between 30 and 1000");
            }

            try
            {
                _logger.LogInformation("Calculating risk metrics for {Symbol} over {Days} days", symbol, days);
                
                var riskMetrics = await _riskMetricsService.CalculateRiskMetricsAsync(symbol, days);
                
                if (!string.IsNullOrEmpty(riskMetrics.Error))
                {
                    return StatusCode(500, new { error = riskMetrics.Error });
                }

                return Ok(riskMetrics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating risk metrics for {Symbol}", symbol);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Calculate risk metrics for multiple assets
        /// </summary>
        /// <param name="request">Request containing symbols and parameters</param>
        /// <returns>List of risk metrics for each asset</returns>
        [HttpPost("assets/batch")]
        public async Task<IActionResult> GetMultipleAssetRiskMetrics([FromBody] List<string> symbols, [FromQuery] int days = 252)
        {
            if (symbols == null || !symbols.Any())
            {
                return BadRequest("Symbols list is required");
            }

            if (symbols.Count > 50)
            {
                return BadRequest("Maximum 50 symbols allowed per request");
            }

            if (days < 30 || days > 1000)
            {
                return BadRequest("Days must be between 30 and 1000");
            }

            try
            {
                _logger.LogInformation("Calculating risk metrics for {Count} assets over {Days} days", symbols.Count, days);
                
                var riskMetricsList = await _riskMetricsService.CalculateMultipleAssetRiskMetricsAsync(symbols, days);
                
                return Ok(riskMetricsList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating risk metrics for multiple assets");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Calculate portfolio risk metrics
        /// </summary>
        /// <param name="request">Portfolio configuration with symbols and weights</param>
        /// <returns>Portfolio risk metrics</returns>
        [HttpPost("portfolio")]
        public async Task<IActionResult> GetPortfolioRiskMetrics([FromBody] PortfolioRiskMetricsRequest request)
        {
            if (request.Symbols == null || !request.Symbols.Any())
            {
                return BadRequest("Symbols list is required");
            }

            if (request.Weights == null || !request.Weights.Any())
            {
                return BadRequest("Weights list is required");
            }

            if (request.Symbols.Count != request.Weights.Count)
            {
                return BadRequest("Number of symbols must match number of weights");
            }

            if (request.Symbols.Count > 50)
            {
                return BadRequest("Maximum 50 assets allowed per portfolio");
            }

            if (request.Days < 30 || request.Days > 1000)
            {
                return BadRequest("Days must be between 30 and 1000");
            }

            // Validate weights sum to approximately 1.0
            var weightSum = request.Weights.Sum();
            if (Math.Abs(weightSum - 1.0m) > 0.01m)
            {
                return BadRequest("Weights must sum to 1.0");
            }

            try
            {
                _logger.LogInformation("Calculating portfolio risk metrics for {Count} assets", request.Symbols.Count);
                
                var portfolioMetrics = await _riskMetricsService.CalculatePortfolioRiskMetricsAsync(
                    request.Symbols, request.Weights, request.Days);
                
                if (!string.IsNullOrEmpty(portfolioMetrics.Error))
                {
                    return StatusCode(500, new { error = portfolioMetrics.Error });
                }

                return Ok(portfolioMetrics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating portfolio risk metrics");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Get risk metrics comparison for multiple assets
        /// </summary>
        /// <param name="symbols">List of asset symbols to compare</param>
        /// <param name="days">Number of days for historical data</param>
        /// <returns>Comparison of risk metrics across assets</returns>
        [HttpGet("compare")]
        public async Task<IActionResult> CompareRiskMetrics([FromQuery] List<string> symbols, [FromQuery] int days = 252)
        {
            if (symbols == null || !symbols.Any())
            {
                return BadRequest("Symbols list is required");
            }

            if (symbols.Count > 20)
            {
                return BadRequest("Maximum 20 symbols allowed for comparison");
            }

            if (days < 30 || days > 1000)
            {
                return BadRequest("Days must be between 30 and 1000");
            }

            try
            {
                _logger.LogInformation("Comparing risk metrics for {Count} assets", symbols.Count);
                
                var riskMetricsList = await _riskMetricsService.CalculateMultipleAssetRiskMetricsAsync(symbols, days);
                
                // Create comparison summary
                var comparison = new
                {
                    Assets = riskMetricsList,
                    Summary = new
                    {
                        HighestVolatility = riskMetricsList.OrderByDescending(r => r.Volatility).FirstOrDefault()?.Symbol,
                        HighestSharpe = riskMetricsList.OrderByDescending(r => r.SharpeRatio).FirstOrDefault()?.Symbol,
                        LowestVaR = riskMetricsList.OrderBy(r => r.ValueAtRisk95).FirstOrDefault()?.Symbol,
                        AverageVolatility = riskMetricsList.Average(r => r.Volatility),
                        AverageSharpe = riskMetricsList.Average(r => r.SharpeRatio),
                        AverageVaR = riskMetricsList.Average(r => r.ValueAtRisk95)
                    }
                };

                return Ok(comparison);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error comparing risk metrics");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }
    }
}

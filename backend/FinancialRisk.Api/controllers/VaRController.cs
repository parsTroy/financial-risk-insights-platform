using FinancialRisk.Api.Models;
using FinancialRisk.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace FinancialRisk.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VaRController : ControllerBase
    {
        private readonly ILogger<VaRController> _logger;
        private readonly IVaRCalculationService _varCalculationService;

        public VaRController(
            ILogger<VaRController> logger,
            IVaRCalculationService varCalculationService)
        {
            _logger = logger;
            _varCalculationService = varCalculationService;
        }

        /// <summary>
        /// Calculate VaR and CVaR for a single asset
        /// </summary>
        /// <param name="request">VaR calculation request</param>
        /// <returns>VaR calculation result</returns>
        [HttpPost("calculate")]
        public async Task<IActionResult> CalculateVaR([FromBody] VaRCalculationRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Symbol))
            {
                return BadRequest("Symbol is required");
            }

            if (request.Days < 30 || request.Days > 1000)
            {
                return BadRequest("Days must be between 30 and 1000");
            }

            if (request.SimulationCount < 1000 || request.SimulationCount > 100000)
            {
                return BadRequest("Simulation count must be between 1000 and 100000");
            }

            try
            {
                _logger.LogInformation("Calculating VaR for {Symbol} using {Method}", request.Symbol, request.CalculationType);
                
                var result = await _varCalculationService.CalculateVaRAsync(request);
                
                if (!result.Success)
                {
                    return StatusCode(500, new { error = result.Error });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating VaR for {Symbol}", request.Symbol);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Calculate VaR and CVaR for a portfolio
        /// </summary>
        /// <param name="request">Portfolio VaR calculation request</param>
        /// <returns>Portfolio VaR calculation result</returns>
        [HttpPost("portfolio/calculate")]
        public async Task<IActionResult> CalculatePortfolioVaR([FromBody] PortfolioVaRCalculationRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.PortfolioName))
            {
                return BadRequest("Portfolio name is required");
            }

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

            // Validate weights sum to approximately 1.0
            var weightSum = request.Weights.Sum();
            if (Math.Abs(weightSum - 1.0m) > 0.01m)
            {
                return BadRequest("Weights must sum to 1.0");
            }

            try
            {
                _logger.LogInformation("Calculating portfolio VaR for {PortfolioName}", request.PortfolioName);
                
                var result = await _varCalculationService.CalculatePortfolioVaRAsync(request);
                
                if (!result.Success)
                {
                    return StatusCode(500, new { error = result.Error });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating portfolio VaR for {PortfolioName}", request.PortfolioName);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Perform VaR stress test
        /// </summary>
        /// <param name="request">Stress test request</param>
        /// <returns>Stress test result</returns>
        [HttpPost("stress-test")]
        public async Task<IActionResult> PerformStressTest([FromBody] VaRStressTestRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Symbol))
            {
                return BadRequest("Symbol is required");
            }

            if (string.IsNullOrWhiteSpace(request.ScenarioName))
            {
                return BadRequest("Scenario name is required");
            }

            if (request.StressFactor <= 0)
            {
                return BadRequest("Stress factor must be positive");
            }

            try
            {
                _logger.LogInformation("Performing stress test for {Symbol} with scenario {ScenarioName}", 
                    request.Symbol, request.ScenarioName);
                
                var result = await _varCalculationService.PerformStressTestAsync(request);
                
                if (!result.Success)
                {
                    return StatusCode(500, new { error = result.Error });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing stress test for {Symbol}", request.Symbol);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Compare different VaR calculation methods
        /// </summary>
        /// <param name="symbol">Asset symbol</param>
        /// <param name="days">Number of days for historical data</param>
        /// <returns>VaR method comparison results</returns>
        [HttpGet("compare/{symbol}")]
        public async Task<IActionResult> CompareVaRMethods(string symbol, [FromQuery] int days = 252)
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
                _logger.LogInformation("Comparing VaR methods for {Symbol}", symbol);
                
                var results = await _varCalculationService.CompareVaRMethodsAsync(symbol, days);
                
                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error comparing VaR methods for {Symbol}", symbol);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Perform VaR backtest
        /// </summary>
        /// <param name="symbol">Asset symbol</param>
        /// <param name="method">VaR calculation method</param>
        /// <param name="confidenceLevel">Confidence level (e.g., 0.95, 0.99)</param>
        /// <param name="backtestDays">Number of days for backtest</param>
        /// <returns>Backtest result</returns>
        [HttpPost("backtest/{symbol}")]
        public async Task<IActionResult> PerformBacktest(
            string symbol, 
            [FromQuery] string method = "Historical",
            [FromQuery] double confidenceLevel = 0.95,
            [FromQuery] int backtestDays = 252)
        {
            if (string.IsNullOrWhiteSpace(symbol))
            {
                return BadRequest("Symbol is required");
            }

            if (confidenceLevel <= 0 || confidenceLevel >= 1)
            {
                return BadRequest("Confidence level must be between 0 and 1");
            }

            if (backtestDays < 30 || backtestDays > 1000)
            {
                return BadRequest("Backtest days must be between 30 and 1000");
            }

            try
            {
                _logger.LogInformation("Performing VaR backtest for {Symbol} using {Method}", symbol, method);
                
                var result = await _varCalculationService.PerformBacktestAsync(symbol, method, confidenceLevel, backtestDays);
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing VaR backtest for {Symbol}", symbol);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Get VaR calculation history for an asset
        /// </summary>
        /// <param name="symbol">Asset symbol</param>
        /// <param name="limit">Maximum number of results</param>
        /// <returns>VaR calculation history</returns>
        [HttpGet("history/{symbol}")]
        public async Task<IActionResult> GetVaRHistory(string symbol, [FromQuery] int limit = 100)
        {
            if (string.IsNullOrWhiteSpace(symbol))
            {
                return BadRequest("Symbol is required");
            }

            if (limit < 1 || limit > 1000)
            {
                return BadRequest("Limit must be between 1 and 1000");
            }

            try
            {
                _logger.LogInformation("Getting VaR history for {Symbol}", symbol);
                
                var results = await _varCalculationService.GetVaRHistoryAsync(symbol, limit);
                
                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting VaR history for {Symbol}", symbol);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Get portfolio VaR calculation history
        /// </summary>
        /// <param name="portfolioName">Portfolio name</param>
        /// <param name="limit">Maximum number of results</param>
        /// <returns>Portfolio VaR calculation history</returns>
        [HttpGet("portfolio/history/{portfolioName}")]
        public async Task<IActionResult> GetPortfolioVaRHistory(string portfolioName, [FromQuery] int limit = 100)
        {
            if (string.IsNullOrWhiteSpace(portfolioName))
            {
                return BadRequest("Portfolio name is required");
            }

            if (limit < 1 || limit > 1000)
            {
                return BadRequest("Limit must be between 1 and 1000");
            }

            try
            {
                _logger.LogInformation("Getting portfolio VaR history for {PortfolioName}", portfolioName);
                
                var results = await _varCalculationService.GetPortfolioVaRHistoryAsync(portfolioName, limit);
                
                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting portfolio VaR history for {PortfolioName}", portfolioName);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Get available VaR calculation methods
        /// </summary>
        /// <returns>List of available methods</returns>
        [HttpGet("methods")]
        public IActionResult GetVaRMethods()
        {
            var methods = new
            {
                CalculationTypes = new[] { "Historical", "MonteCarlo", "Parametric", "Bootstrap" },
                DistributionTypes = new[] { "Normal", "TStudent", "SkewedT", "GARCH", "Copula" },
                ConfidenceLevels = new[] { 0.90, 0.95, 0.99, 0.999 },
                ScenarioTypes = new[] { "Historical", "Hypothetical", "MonteCarlo" }
            };

            return Ok(methods);
        }

        /// <summary>
        /// Get VaR calculation statistics
        /// </summary>
        /// <param name="symbol">Asset symbol</param>
        /// <returns>VaR calculation statistics</returns>
        [HttpGet("stats/{symbol}")]
        public async Task<IActionResult> GetVaRStats(string symbol)
        {
            if (string.IsNullOrWhiteSpace(symbol))
            {
                return BadRequest("Symbol is required");
            }

            try
            {
                _logger.LogInformation("Getting VaR stats for {Symbol}", symbol);
                
                // This would calculate and return VaR statistics
                var stats = new
                {
                    Symbol = symbol,
                    TotalCalculations = 0,
                    AverageVaR95 = 0.0,
                    AverageVaR99 = 0.0,
                    AverageCVaR95 = 0.0,
                    AverageCVaR99 = 0.0,
                    BestMethod = "Historical",
                    LastCalculation = DateTime.UtcNow
                };

                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting VaR stats for {Symbol}", symbol);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }
    }
}

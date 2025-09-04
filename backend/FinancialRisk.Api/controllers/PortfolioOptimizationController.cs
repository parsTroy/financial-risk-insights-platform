using FinancialRisk.Api.Models;
using FinancialRisk.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace FinancialRisk.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PortfolioOptimizationController : ControllerBase
    {
        private readonly ILogger<PortfolioOptimizationController> _logger;
        private readonly IPortfolioOptimizationService _optimizationService;

        public PortfolioOptimizationController(
            ILogger<PortfolioOptimizationController> logger,
            IPortfolioOptimizationService optimizationService)
        {
            _logger = logger;
            _optimizationService = optimizationService;
        }

        /// <summary>
        /// Optimize portfolio using Markowitz mean-variance optimization
        /// </summary>
        /// <param name="request">Portfolio optimization parameters</param>
        /// <returns>Optimized portfolio weights and metrics</returns>
        [HttpPost("optimize")]
        public async Task<ActionResult<PortfolioOptimizationResponse>> OptimizePortfolio([FromBody] PortfolioOptimizationRequest request)
        {
            try
            {
                _logger.LogInformation("Portfolio optimization requested for {PortfolioName}", request.PortfolioName);

                if (string.IsNullOrEmpty(request.PortfolioName))
                {
                    return BadRequest(new PortfolioOptimizationResponse
                    {
                        Success = false,
                        Error = "Portfolio name is required"
                    });
                }

                if (request.Symbols == null || request.Symbols.Count < 2)
                {
                    return BadRequest(new PortfolioOptimizationResponse
                    {
                        Success = false,
                        Error = "At least 2 assets are required for portfolio optimization"
                    });
                }

                var result = await _optimizationService.OptimizePortfolioAsync(request);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in portfolio optimization for {PortfolioName}", request.PortfolioName);
                return StatusCode(500, new PortfolioOptimizationResponse
                {
                    Success = false,
                    Error = ex.Message
                });
            }
        }

        /// <summary>
        /// Calculate efficient frontier for a portfolio
        /// </summary>
        /// <param name="request">Portfolio optimization parameters</param>
        /// <returns>Efficient frontier points and key metrics</returns>
        [HttpPost("efficient-frontier")]
        public async Task<ActionResult<EfficientFrontier>> CalculateEfficientFrontier([FromBody] PortfolioOptimizationRequest request)
        {
            try
            {
                _logger.LogInformation("Efficient frontier calculation requested for {PortfolioName}", request.PortfolioName);

                if (string.IsNullOrEmpty(request.PortfolioName))
                {
                    return BadRequest(new EfficientFrontier
                    {
                        Success = false,
                        Error = "Portfolio name is required"
                    });
                }

                if (request.Symbols == null || request.Symbols.Count < 2)
                {
                    return BadRequest(new EfficientFrontier
                    {
                        Success = false,
                        Error = "At least 2 assets are required for efficient frontier calculation"
                    });
                }

                var result = await _optimizationService.CalculateEfficientFrontierAsync(request);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating efficient frontier for {PortfolioName}", request.PortfolioName);
                return StatusCode(500, new EfficientFrontier
                {
                    Success = false,
                    Error = ex.Message
                });
            }
        }

        /// <summary>
        /// Optimize portfolio using risk budgeting approach
        /// </summary>
        /// <param name="request">Risk budgeting parameters</param>
        /// <returns>Risk-budgeted portfolio weights</returns>
        [HttpPost("risk-budgeting")]
        public async Task<ActionResult<RiskBudgetingResult>> OptimizeRiskBudgeting([FromBody] RiskBudgetingRequest request)
        {
            try
            {
                _logger.LogInformation("Risk budgeting optimization requested for {PortfolioName}", request.PortfolioName);

                if (string.IsNullOrEmpty(request.PortfolioName))
                {
                    return BadRequest(new RiskBudgetingResult
                    {
                        Success = false,
                        Error = "Portfolio name is required"
                    });
                }

                if (request.Symbols == null || request.Symbols.Count < 2)
                {
                    return BadRequest(new RiskBudgetingResult
                    {
                        Success = false,
                        Error = "At least 2 assets are required for risk budgeting"
                    });
                }

                if (request.RiskBudgets == null || request.RiskBudgets.Count != request.Symbols.Count)
                {
                    return BadRequest(new RiskBudgetingResult
                    {
                        Success = false,
                        Error = "Risk budgets must be provided for all assets"
                    });
                }

                var result = await _optimizationService.OptimizeRiskBudgetingAsync(request);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in risk budgeting optimization for {PortfolioName}", request.PortfolioName);
                return StatusCode(500, new RiskBudgetingResult
                {
                    Success = false,
                    Error = ex.Message,
                    PortfolioName = request.PortfolioName,
                    OptimalWeights = new List<double>(),
                    RiskBudgets = new List<double>(),
                    ActualRiskContributions = new List<double>(),
                    PortfolioVolatility = 0.0,
                    AssetWeights = new List<AssetWeight>(),
                    OptimizationMetadata = new Dictionary<string, object>(),
                    CalculationDate = DateTime.UtcNow
                });
            }
        }

        /// <summary>
        /// Optimize portfolio using Black-Litterman model
        /// </summary>
        /// <param name="request">Black-Litterman parameters</param>
        /// <returns>Black-Litterman optimized portfolio weights</returns>
        [HttpPost("black-litterman")]
        public async Task<ActionResult<BlackLittermanResult>> OptimizeBlackLitterman([FromBody] BlackLittermanRequest request)
        {
            try
            {
                _logger.LogInformation("Black-Litterman optimization requested for {PortfolioName}", request.PortfolioName);

                if (string.IsNullOrEmpty(request.PortfolioName))
                {
                    return BadRequest(new BlackLittermanResult
                    {
                        Success = false,
                        Error = "Portfolio name is required"
                    });
                }

                if (request.Symbols == null || request.Symbols.Count < 2)
                {
                    return BadRequest(new BlackLittermanResult
                    {
                        Success = false,
                        Error = "At least 2 assets are required for Black-Litterman optimization"
                    });
                }

                var result = await _optimizationService.OptimizeBlackLittermanAsync(request);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Black-Litterman optimization for {PortfolioName}", request.PortfolioName);
                return StatusCode(500, new BlackLittermanResult
                {
                    Success = false,
                    Error = ex.Message,
                    PortfolioName = request.PortfolioName,
                    OptimalWeights = new List<double>(),
                    ImpliedReturns = new List<double>(),
                    AdjustedReturns = new List<double>(),
                    ExpectedReturn = 0.0,
                    ExpectedVolatility = 0.0,
                    SharpeRatio = 0.0,
                    AssetWeights = new List<AssetWeight>(),
                    OptimizationMetadata = new Dictionary<string, object>(),
                    CalculationDate = DateTime.UtcNow
                });
            }
        }

        /// <summary>
        /// Optimize portfolio considering transaction costs
        /// </summary>
        /// <param name="request">Transaction cost optimization parameters</param>
        /// <returns>Transaction cost optimized portfolio weights</returns>
        [HttpPost("transaction-costs")]
        public async Task<ActionResult<TransactionCostOptimizationResult>> OptimizeTransactionCosts([FromBody] TransactionCostOptimizationRequest request)
        {
            try
            {
                _logger.LogInformation("Transaction cost optimization requested for {PortfolioName}", request.PortfolioName);

                if (string.IsNullOrEmpty(request.PortfolioName))
                {
                    return BadRequest(new TransactionCostOptimizationResult
                    {
                        Success = false,
                        Error = "Portfolio name is required"
                    });
                }

                if (request.Symbols == null || request.Symbols.Count < 2)
                {
                    return BadRequest(new TransactionCostOptimizationResult
                    {
                        Success = false,
                        Error = "At least 2 assets are required for transaction cost optimization"
                    });
                }

                var result = await _optimizationService.OptimizeTransactionCostsAsync(request);
                
                if (result.Success)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in transaction cost optimization for {PortfolioName}", request.PortfolioName);
                return StatusCode(500, new TransactionCostOptimizationResult
                {
                    Success = false,
                    Error = ex.Message,
                    PortfolioName = request.PortfolioName,
                    OptimalWeights = new List<double>(),
                    RebalancingWeights = new List<double>(),
                    TotalTransactionCosts = 0.0,
                    Turnover = 0.0,
                    AssetWeights = new List<AssetWeight>(),
                    OptimizationMetadata = new Dictionary<string, object>(),
                    CalculationDate = DateTime.UtcNow
                });
            }
        }

        /// <summary>
        /// Get optimization history for a portfolio
        /// </summary>
        /// <param name="portfolioName">Portfolio name</param>
        /// <param name="limit">Maximum number of results to return</param>
        /// <returns>List of optimization results</returns>
        [HttpGet("history/{portfolioName}")]
        public async Task<ActionResult<List<PortfolioOptimizationResult>>> GetOptimizationHistory(string portfolioName, int limit = 100)
        {
            try
            {
                _logger.LogInformation("Optimization history requested for {PortfolioName}", portfolioName);

                if (string.IsNullOrEmpty(portfolioName))
                {
                    return BadRequest(new List<PortfolioOptimizationResult>());
                }

                var result = await _optimizationService.GetOptimizationHistoryAsync(portfolioName, limit);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving optimization history for {PortfolioName}", portfolioName);
                return StatusCode(500, new List<PortfolioOptimizationResult>());
            }
        }

        /// <summary>
        /// Get specific optimization result by ID
        /// </summary>
        /// <param name="id">Optimization result ID</param>
        /// <returns>Optimization result</returns>
        [HttpGet("result/{id}")]
        public async Task<ActionResult<PortfolioOptimizationResult>> GetOptimizationResult(int id)
        {
            try
            {
                _logger.LogInformation("Optimization result requested for ID {Id}", id);

                var result = await _optimizationService.GetOptimizationByIdAsync(id);
                
                if (result == null)
                {
                    return NotFound();
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving optimization result with ID {Id}", id);
                return StatusCode(500);
            }
        }

        /// <summary>
        /// Get available optimization methods
        /// </summary>
        /// <returns>List of available optimization methods</returns>
        [HttpGet("methods")]
        public async Task<ActionResult<List<string>>> GetOptimizationMethods()
        {
            try
            {
                var methods = await _optimizationService.GetAvailableOptimizationMethodsAsync();
                return Ok(methods);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving optimization methods");
                return StatusCode(500, new List<string>());
            }
        }

        /// <summary>
        /// Get available optimization constraints
        /// </summary>
        /// <returns>Dictionary of available constraints</returns>
        [HttpGet("constraints")]
        public async Task<ActionResult<Dictionary<string, object>>> GetOptimizationConstraints()
        {
            try
            {
                var constraints = await _optimizationService.GetOptimizationConstraintsAsync();
                return Ok(constraints);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving optimization constraints");
                return StatusCode(500, new Dictionary<string, object>());
            }
        }

        /// <summary>
        /// Get optimization statistics and metadata
        /// </summary>
        /// <returns>Optimization statistics</returns>
        [HttpGet("stats")]
        public async Task<ActionResult<Dictionary<string, object>>> GetOptimizationStats()
        {
            try
            {
                var stats = new Dictionary<string, object>
                {
                    ["total_optimizations"] = 0, // Would be calculated from database
                    ["success_rate"] = 0.95,
                    ["average_execution_time"] = 1.5,
                    ["most_used_method"] = "MeanVariance",
                    ["supported_methods"] = await _optimizationService.GetAvailableOptimizationMethodsAsync(),
                    ["constraints"] = await _optimizationService.GetOptimizationConstraintsAsync(),
                    ["last_updated"] = DateTime.UtcNow
                };

                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving optimization statistics");
                return StatusCode(500, new Dictionary<string, object>());
            }
        }
    }
}

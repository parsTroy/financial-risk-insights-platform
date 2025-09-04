using FinancialRisk.Api.Models;
using FinancialRisk.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace FinancialRisk.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PortfolioBuilderController : ControllerBase
    {
        private readonly IPortfolioBuilderService _portfolioBuilderService;
        private readonly ILogger<PortfolioBuilderController> _logger;

        public PortfolioBuilderController(
            IPortfolioBuilderService portfolioBuilderService,
            ILogger<PortfolioBuilderController> logger)
        {
            _portfolioBuilderService = portfolioBuilderService;
            _logger = logger;
        }

        /// <summary>
        /// Search for assets by symbol or name
        /// </summary>
        [HttpPost("search-assets")]
        public async Task<ActionResult<ApiResponse<AssetSearchResponse>>> SearchAssets([FromBody] AssetSearchRequest request)
        {
            try
            {
                _logger.LogInformation("Searching assets with query: {Query}", request.Query);
                
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse<AssetSearchResponse>
                    {
                        IsSuccess = false,
                        ErrorMessage = "Invalid request data",
                        Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()
                    });
                }

                var result = await _portfolioBuilderService.SearchAssetsAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching assets");
                return StatusCode(500, new ApiResponse<AssetSearchResponse>
                {
                    IsSuccess = false,
                    ErrorMessage = "Internal server error"
                });
            }
        }

        /// <summary>
        /// Save a portfolio
        /// </summary>
        [HttpPost("save")]
        public async Task<ActionResult<ApiResponse<Portfolio>>> SavePortfolio([FromBody] PortfolioSaveRequest request)
        {
            try
            {
                _logger.LogInformation("Saving portfolio: {PortfolioName}", request.Name);
                
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse<Portfolio>
                    {
                        IsSuccess = false,
                        ErrorMessage = "Invalid request data",
                        Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()
                    });
                }

                var result = await _portfolioBuilderService.SavePortfolioAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving portfolio");
                return StatusCode(500, new ApiResponse<Portfolio>
                {
                    IsSuccess = false,
                    ErrorMessage = "Internal server error"
                });
            }
        }

        /// <summary>
        /// Load a portfolio by ID
        /// </summary>
        [HttpPost("load")]
        public async Task<ActionResult<ApiResponse<Portfolio>>> LoadPortfolio([FromBody] PortfolioLoadRequest request)
        {
            try
            {
                _logger.LogInformation("Loading portfolio: {PortfolioId}", request.PortfolioId);
                
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse<Portfolio>
                    {
                        IsSuccess = false,
                        ErrorMessage = "Invalid request data",
                        Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()
                    });
                }

                var result = await _portfolioBuilderService.LoadPortfolioAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading portfolio");
                return StatusCode(500, new ApiResponse<Portfolio>
                {
                    IsSuccess = false,
                    ErrorMessage = "Internal server error"
                });
            }
        }

        /// <summary>
        /// List portfolios with pagination and search
        /// </summary>
        [HttpPost("list")]
        public async Task<ActionResult<ApiResponse<PortfolioListResponse>>> ListPortfolios([FromBody] PortfolioListRequest request)
        {
            try
            {
                _logger.LogInformation("Listing portfolios");
                
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse<PortfolioListResponse>
                    {
                        IsSuccess = false,
                        ErrorMessage = "Invalid request data",
                        Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()
                    });
                }

                var result = await _portfolioBuilderService.ListPortfoliosAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error listing portfolios");
                return StatusCode(500, new ApiResponse<PortfolioListResponse>
                {
                    IsSuccess = false,
                    ErrorMessage = "Internal server error"
                });
            }
        }

        /// <summary>
        /// Delete a portfolio by ID
        /// </summary>
        [HttpDelete("{portfolioId}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeletePortfolio(string portfolioId)
        {
            try
            {
                _logger.LogInformation("Deleting portfolio: {PortfolioId}", portfolioId);
                
                if (string.IsNullOrEmpty(portfolioId))
                {
                    return BadRequest(new ApiResponse<bool>
                    {
                        IsSuccess = false,
                        ErrorMessage = "Portfolio ID is required"
                    });
                }

                var result = await _portfolioBuilderService.DeletePortfolioAsync(portfolioId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting portfolio");
                return StatusCode(500, new ApiResponse<bool>
                {
                    IsSuccess = false,
                    ErrorMessage = "Internal server error"
                });
            }
        }

        /// <summary>
        /// Get portfolio summary by ID
        /// </summary>
        [HttpGet("{portfolioId}/summary")]
        public async Task<ActionResult<ApiResponse<PortfolioSummary>>> GetPortfolioSummary(string portfolioId)
        {
            try
            {
                _logger.LogInformation("Getting portfolio summary: {PortfolioId}", portfolioId);
                
                if (string.IsNullOrEmpty(portfolioId))
                {
                    return BadRequest(new ApiResponse<PortfolioSummary>
                    {
                        IsSuccess = false,
                        ErrorMessage = "Portfolio ID is required"
                    });
                }

                var result = await _portfolioBuilderService.GetPortfolioSummaryAsync(portfolioId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting portfolio summary");
                return StatusCode(500, new ApiResponse<PortfolioSummary>
                {
                    IsSuccess = false,
                    ErrorMessage = "Internal server error"
                });
            }
        }

        /// <summary>
        /// Validate a portfolio
        /// </summary>
        [HttpPost("validate")]
        public async Task<ActionResult<ApiResponse<PortfolioValidationResult>>> ValidatePortfolio([FromBody] Portfolio portfolio)
        {
            try
            {
                _logger.LogInformation("Validating portfolio: {PortfolioName}", portfolio.Name);
                
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse<PortfolioValidationResult>
                    {
                        IsSuccess = false,
                        ErrorMessage = "Invalid request data",
                        Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()
                    });
                }

                var result = await _portfolioBuilderService.ValidatePortfolioAsync(portfolio);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating portfolio");
                return StatusCode(500, new ApiResponse<PortfolioValidationResult>
                {
                    IsSuccess = false,
                    ErrorMessage = "Internal server error"
                });
            }
        }

        /// <summary>
        /// Rebalance a portfolio
        /// </summary>
        [HttpPost("rebalance")]
        public async Task<ActionResult<ApiResponse<PortfolioRebalanceRequest>>> RebalancePortfolio([FromBody] PortfolioRebalanceRequest request)
        {
            try
            {
                _logger.LogInformation("Rebalancing portfolio: {PortfolioId}", request.PortfolioId);
                
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse<PortfolioRebalanceRequest>
                    {
                        IsSuccess = false,
                        ErrorMessage = "Invalid request data",
                        Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()
                    });
                }

                var result = await _portfolioBuilderService.RebalancePortfolioAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rebalancing portfolio");
                return StatusCode(500, new ApiResponse<PortfolioRebalanceRequest>
                {
                    IsSuccess = false,
                    ErrorMessage = "Internal server error"
                });
            }
        }

        /// <summary>
        /// Get portfolio performance metrics
        /// </summary>
        [HttpPost("performance")]
        public async Task<ActionResult<ApiResponse<PortfolioPerformanceMetrics>>> GetPortfolioPerformance([FromBody] PortfolioPerformanceRequest request)
        {
            try
            {
                _logger.LogInformation("Getting portfolio performance: {PortfolioId}", request.PortfolioId);
                
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse<PortfolioPerformanceMetrics>
                    {
                        IsSuccess = false,
                        ErrorMessage = "Invalid request data",
                        Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()
                    });
                }

                var result = await _portfolioBuilderService.GetPortfolioPerformanceAsync(request.PortfolioId, request.StartDate, request.EndDate);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting portfolio performance");
                return StatusCode(500, new ApiResponse<PortfolioPerformanceMetrics>
                {
                    IsSuccess = false,
                    ErrorMessage = "Internal server error"
                });
            }
        }

        /// <summary>
        /// Compare multiple portfolios
        /// </summary>
        [HttpPost("compare")]
        public async Task<ActionResult<ApiResponse<PortfolioComparisonResult>>> ComparePortfolios([FromBody] PortfolioComparisonRequest request)
        {
            try
            {
                _logger.LogInformation("Comparing portfolios");
                
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse<PortfolioComparisonResult>
                    {
                        IsSuccess = false,
                        ErrorMessage = "Invalid request data",
                        Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()
                    });
                }

                var result = await _portfolioBuilderService.ComparePortfoliosAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error comparing portfolios");
                return StatusCode(500, new ApiResponse<PortfolioComparisonResult>
                {
                    IsSuccess = false,
                    ErrorMessage = "Internal server error"
                });
            }
        }

        /// <summary>
        /// Get available sectors
        /// </summary>
        [HttpGet("sectors")]
        public async Task<ActionResult<ApiResponse<List<string>>>> GetAvailableSectors()
        {
            try
            {
                _logger.LogInformation("Getting available sectors");
                
                var result = await _portfolioBuilderService.GetAvailableSectorsAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available sectors");
                return StatusCode(500, new ApiResponse<List<string>>
                {
                    IsSuccess = false,
                    ErrorMessage = "Internal server error"
                });
            }
        }

        /// <summary>
        /// Get available exchanges
        /// </summary>
        [HttpGet("exchanges")]
        public async Task<ActionResult<ApiResponse<List<string>>>> GetAvailableExchanges()
        {
            try
            {
                _logger.LogInformation("Getting available exchanges");
                
                var result = await _portfolioBuilderService.GetAvailableExchangesAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available exchanges");
                return StatusCode(500, new ApiResponse<List<string>>
                {
                    IsSuccess = false,
                    ErrorMessage = "Internal server error"
                });
            }
        }

        /// <summary>
        /// Get market data for multiple symbols
        /// </summary>
        [HttpPost("market-data")]
        public async Task<ActionResult<ApiResponse<Dictionary<string, object>>>> GetMarketData([FromBody] MarketDataRequest request)
        {
            try
            {
                _logger.LogInformation("Getting market data for {SymbolCount} symbols", request.Symbols.Count);
                
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse<Dictionary<string, object>>
                    {
                        IsSuccess = false,
                        ErrorMessage = "Invalid request data",
                        Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()
                    });
                }

                var result = await _portfolioBuilderService.GetMarketDataAsync(request.Symbols);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting market data");
                return StatusCode(500, new ApiResponse<Dictionary<string, object>>
                {
                    IsSuccess = false,
                    ErrorMessage = "Internal server error"
                });
            }
        }
    }

    public class PortfolioPerformanceRequest
    {
        public string PortfolioId { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class MarketDataRequest
    {
        public List<string> Symbols { get; set; } = new();
    }
}

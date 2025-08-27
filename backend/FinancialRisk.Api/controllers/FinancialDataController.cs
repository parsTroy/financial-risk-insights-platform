using FinancialRisk.Api.Models;
using FinancialRisk.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace FinancialRisk.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FinancialDataController : ControllerBase
    {
        private readonly IFinancialDataService _financialDataService;
        private readonly ILogger<FinancialDataController> _logger;

        public FinancialDataController(
            IFinancialDataService financialDataService,
            ILogger<FinancialDataController> logger)
        {
            _financialDataService = financialDataService;
            _logger = logger;
        }

        [HttpGet("stock/{symbol}")]
        public async Task<IActionResult> GetStockQuote(string symbol)
        {
            if (string.IsNullOrWhiteSpace(symbol))
            {
                return BadRequest("Symbol is required");
            }

            _logger.LogInformation("Requesting stock quote for symbol: {Symbol}", symbol);
            
            var result = await _financialDataService.GetStockQuoteAsync(symbol.ToUpper());
            
            if (result.Success)
            {
                return Ok(result);
            }
            
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("forex/{fromCurrency}/{toCurrency}")]
        public async Task<IActionResult> GetForexQuote(string fromCurrency, string toCurrency)
        {
            if (string.IsNullOrWhiteSpace(fromCurrency) || string.IsNullOrWhiteSpace(toCurrency))
            {
                return BadRequest("Both fromCurrency and toCurrency are required");
            }

            _logger.LogInformation("Requesting forex quote for {FromCurrency}/{ToCurrency}", fromCurrency, toCurrency);
            
            var result = await _financialDataService.GetForexQuoteAsync(fromCurrency.ToUpper(), toCurrency.ToUpper());
            
            if (result.Success)
            {
                return Ok(result);
            }
            
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("stock/{symbol}/history")]
        public async Task<IActionResult> GetStockHistory(string symbol, [FromQuery] int days = 30)
        {
            if (string.IsNullOrWhiteSpace(symbol))
            {
                return BadRequest("Symbol is required");
            }

            if (days <= 0 || days > 365)
            {
                return BadRequest("Days must be between 1 and 365");
            }

            _logger.LogInformation("Requesting stock history for {Symbol} for {Days} days", symbol, days);
            
            var result = await _financialDataService.GetStockHistoryAsync(symbol.ToUpper(), days);
            
            if (result.Success)
            {
                return Ok(result);
            }
            
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("stock/{symbol}/price")]
        public async Task<IActionResult> GetCurrentPrice(string symbol)
        {
            if (string.IsNullOrWhiteSpace(symbol))
            {
                return BadRequest("Symbol is required");
            }

            _logger.LogInformation("Requesting current price for symbol: {Symbol}", symbol);
            
            var result = await _financialDataService.GetCurrentPriceAsync(symbol.ToUpper());
            
            if (result.Success)
            {
                return Ok(result);
            }
            
            return StatusCode(result.StatusCode, result);
        }
    }
}

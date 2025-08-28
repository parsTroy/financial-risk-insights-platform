using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FinancialRisk.Api.Data;
using FinancialRisk.Api.Models;

namespace FinancialRisk.Api.controllers;

[ApiController]
[Route("api/[controller]")]
public class PortfoliosController : ControllerBase
{
    private readonly FinancialRiskDbContext _context;
    private readonly ILogger<PortfoliosController> _logger;

    public PortfoliosController(FinancialRiskDbContext context, ILogger<PortfoliosController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // GET: api/Portfolios
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Portfolio>>> GetPortfolios()
    {
        try
        {
            var portfolios = await _context.Portfolios
                .Include(p => p.PortfolioHoldings)
                    .ThenInclude(ph => ph.Asset)
                .ToListAsync();
            
            return Ok(portfolios);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving portfolios");
            return StatusCode(500, "Internal server error");
        }
    }

    // GET: api/Portfolios/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Portfolio>> GetPortfolio(int id)
    {
        try
        {
            var portfolio = await _context.Portfolios
                .Include(p => p.PortfolioHoldings)
                    .ThenInclude(ph => ph.Asset)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (portfolio == null)
            {
                return NotFound();
            }

            return Ok(portfolio);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving portfolio with ID {PortfolioId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    // GET: api/Portfolios/5/performance
    [HttpGet("{id}/performance")]
    public async Task<ActionResult<object>> GetPortfolioPerformance(int id, [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        try
        {
            var portfolio = await _context.Portfolios
                .Include(p => p.PortfolioHoldings)
                    .ThenInclude(ph => ph.Asset)
                        .ThenInclude(a => a.Prices)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (portfolio == null)
            {
                return NotFound();
            }

            if (!portfolio.PortfolioHoldings.Any())
            {
                return Ok(new { message = "Portfolio has no holdings" });
            }

            var start = startDate ?? DateTime.Today.AddYears(-1);
            var end = endDate ?? DateTime.Today;

            var performance = new List<object>();

            foreach (var holding in portfolio.PortfolioHoldings)
            {
                var prices = holding.Asset.Prices
                    .Where(p => p.Date >= start && p.Date <= end)
                    .OrderBy(p => p.Date)
                    .ToList();

                if (prices.Any())
                {
                    var firstPrice = prices.First().Close ?? 0;
                    var lastPrice = prices.Last().Close ?? 0;
                    var return_pct = firstPrice > 0 ? (lastPrice - firstPrice) / firstPrice : 0;

                    performance.Add(new
                    {
                        symbol = holding.Asset.Symbol,
                        name = holding.Asset.Name,
                        weight = holding.Weight,
                        firstPrice = firstPrice,
                        lastPrice = lastPrice,
                        return_pct = return_pct,
                        weightedReturn = return_pct * holding.Weight
                    });
                }
            }

            var totalWeightedReturn = performance.Sum(p => (decimal)p.GetType().GetProperty("weightedReturn").GetValue(p));
            var totalWeight = portfolio.PortfolioHoldings.Sum(h => h.Weight);

            return Ok(new
            {
                portfolioId = portfolio.Id,
                portfolioName = portfolio.Name,
                startDate = start,
                endDate = end,
                totalWeight = totalWeight,
                totalReturn = totalWeightedReturn,
                holdings = performance
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating portfolio performance for ID {PortfolioId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    // POST: api/Portfolios
    [HttpPost]
    public async Task<ActionResult<Portfolio>> CreatePortfolio(Portfolio portfolio)
    {
        try
        {
            portfolio.CreatedAt = DateTime.UtcNow;
            portfolio.UpdatedAt = DateTime.UtcNow;

            _context.Portfolios.Add(portfolio);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created new portfolio: {Name}", portfolio.Name);

            return CreatedAtAction(nameof(GetPortfolio), new { id = portfolio.Id }, portfolio);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating portfolio");
            return StatusCode(500, "Internal server error");
        }
    }

    // PUT: api/Portfolios/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePortfolio(int id, Portfolio portfolio)
    {
        try
        {
            if (id != portfolio.Id)
            {
                return BadRequest();
            }

            var existingPortfolio = await _context.Portfolios.FindAsync(id);
            if (existingPortfolio == null)
            {
                return NotFound();
            }

            existingPortfolio.Name = portfolio.Name;
            existingPortfolio.Description = portfolio.Description;
            existingPortfolio.Strategy = portfolio.Strategy;
            existingPortfolio.TargetReturn = portfolio.TargetReturn;
            existingPortfolio.MaxRisk = portfolio.MaxRisk;
            existingPortfolio.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Updated portfolio: {Name}", portfolio.Name);

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating portfolio with ID {PortfolioId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    // DELETE: api/Portfolios/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePortfolio(int id)
    {
        try
        {
            var portfolio = await _context.Portfolios.FindAsync(id);
            if (portfolio == null)
            {
                return NotFound();
            }

            _context.Portfolios.Remove(portfolio);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Deleted portfolio: {Name}", portfolio.Name);

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting portfolio with ID {PortfolioId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    // POST: api/Portfolios/5/holdings
    [HttpPost("{id}/holdings")]
    public async Task<ActionResult<PortfolioHolding>> AddHolding(int id, PortfolioHolding holding)
    {
        try
        {
            var portfolio = await _context.Portfolios.FindAsync(id);
            if (portfolio == null)
            {
                return NotFound("Portfolio not found");
            }

            var asset = await _context.Assets.FindAsync(holding.AssetId);
            if (asset == null)
            {
                return NotFound("Asset not found");
            }

            // Check if holding already exists
            var existingHolding = await _context.PortfolioHoldings
                .FirstOrDefaultAsync(ph => ph.PortfolioId == id && ph.AssetId == holding.AssetId);

            if (existingHolding != null)
            {
                return BadRequest("Asset already exists in portfolio");
            }

            holding.PortfolioId = id;
            holding.CreatedAt = DateTime.UtcNow;
            holding.UpdatedAt = DateTime.UtcNow;

            _context.PortfolioHoldings.Add(holding);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Added holding {AssetSymbol} to portfolio {PortfolioName}", 
                asset.Symbol, portfolio.Name);

            return CreatedAtAction(nameof(GetPortfolio), new { id = id }, holding);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding holding to portfolio {PortfolioId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    // PUT: api/Portfolios/5/holdings/{assetId}
    [HttpPut("{id}/holdings/{assetId}")]
    public async Task<IActionResult> UpdateHolding(int id, int assetId, PortfolioHolding holding)
    {
        try
        {
            if (id != holding.PortfolioId || assetId != holding.AssetId)
            {
                return BadRequest();
            }

            var existingHolding = await _context.PortfolioHoldings
                .FirstOrDefaultAsync(ph => ph.PortfolioId == id && ph.AssetId == assetId);

            if (existingHolding == null)
            {
                return NotFound();
            }

            existingHolding.Weight = holding.Weight;
            existingHolding.Quantity = holding.Quantity;
            existingHolding.AverageCost = holding.AverageCost;
            existingHolding.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Updated holding in portfolio {PortfolioId} for asset {AssetId}", id, assetId);

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating holding in portfolio {PortfolioId} for asset {AssetId}", id, assetId);
            return StatusCode(500, "Internal server error");
        }
    }

    // DELETE: api/Portfolios/5/holdings/{assetId}
    [HttpDelete("{id}/holdings/{assetId}")]
    public async Task<IActionResult> RemoveHolding(int id, int assetId)
    {
        try
        {
            var holding = await _context.PortfolioHoldings
                .FirstOrDefaultAsync(ph => ph.PortfolioId == id && ph.AssetId == assetId);

            if (holding == null)
            {
                return NotFound();
            }

            _context.PortfolioHoldings.Remove(holding);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Removed holding from portfolio {PortfolioId} for asset {AssetId}", id, assetId);

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing holding from portfolio {PortfolioId} for asset {AssetId}", id, assetId);
            return StatusCode(500, "Internal server error");
        }
    }
}

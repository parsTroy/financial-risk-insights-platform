using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FinancialRisk.Api.Data;
using FinancialRisk.Api.Models;

namespace FinancialRisk.Api.controllers;

[ApiController]
[Route("api/[controller]")]
public class AssetsController : ControllerBase
{
    private readonly FinancialRiskDbContext _context;
    private readonly ILogger<AssetsController> _logger;

    public AssetsController(FinancialRiskDbContext context, ILogger<AssetsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // GET: api/Assets
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Asset>>> GetAssets()
    {
        try
        {
            var assets = await _context.Assets
                .Include(a => a.Prices.OrderByDescending(p => p.Date).Take(1))
                .ToListAsync();
            
            return Ok(assets);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving assets");
            return StatusCode(500, "Internal server error");
        }
    }

    // GET: api/Assets/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Asset>> GetAsset(int id)
    {
        try
        {
            var asset = await _context.Assets
                .Include(a => a.Prices.OrderByDescending(p => p.Date).Take(30))
                .FirstOrDefaultAsync(a => a.Id == id);

            if (asset == null)
            {
                return NotFound();
            }

            return Ok(asset);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving asset with ID {AssetId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    // GET: api/Assets/symbol/AAPL
    [HttpGet("symbol/{symbol}")]
    public async Task<ActionResult<Asset>> GetAssetBySymbol(string symbol)
    {
        try
        {
            var asset = await _context.Assets
                .Include(a => a.Prices.OrderByDescending(p => p.Date).Take(30))
                .FirstOrDefaultAsync(a => a.Symbol == symbol.ToUpper());

            if (asset == null)
            {
                return NotFound($"Asset with symbol {symbol} not found");
            }

            return Ok(asset);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving asset with symbol {Symbol}", symbol);
            return StatusCode(500, "Internal server error");
        }
    }

    // GET: api/Assets/5/prices
    [HttpGet("{id}/prices")]
    public async Task<ActionResult<IEnumerable<Price>>> GetAssetPrices(int id, [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        try
        {
            var query = _context.Prices.Where(p => p.AssetId == id);

            if (startDate.HasValue)
                query = query.Where(p => p.Date >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(p => p.Date <= endDate.Value);

            var prices = await query
                .OrderBy(p => p.Date)
                .ToListAsync();

            if (!prices.Any())
            {
                return NotFound($"No prices found for asset ID {id}");
            }

            return Ok(prices);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving prices for asset ID {AssetId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    // POST: api/Assets
    [HttpPost]
    public async Task<ActionResult<Asset>> CreateAsset(Asset asset)
    {
        try
        {
            if (await _context.Assets.AnyAsync(a => a.Symbol == asset.Symbol))
            {
                return BadRequest($"Asset with symbol {asset.Symbol} already exists");
            }

            asset.CreatedAt = DateTime.UtcNow;
            asset.UpdatedAt = DateTime.UtcNow;

            _context.Assets.Add(asset);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created new asset: {Symbol} - {Name}", asset.Symbol, asset.Name);

            return CreatedAtAction(nameof(GetAsset), new { id = asset.Id }, asset);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating asset");
            return StatusCode(500, "Internal server error");
        }
    }

    // PUT: api/Assets/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAsset(int id, Asset asset)
    {
        try
        {
            if (id != asset.Id)
            {
                return BadRequest();
            }

            var existingAsset = await _context.Assets.FindAsync(id);
            if (existingAsset == null)
            {
                return NotFound();
            }

            // Check if symbol is being changed and if it conflicts with existing assets
            if (existingAsset.Symbol != asset.Symbol && 
                await _context.Assets.AnyAsync(a => a.Symbol == asset.Symbol && a.Id != id))
            {
                return BadRequest($"Asset with symbol {asset.Symbol} already exists");
            }

            existingAsset.Symbol = asset.Symbol;
            existingAsset.Name = asset.Name;
            existingAsset.Sector = asset.Sector;
            existingAsset.Industry = asset.Industry;
            existingAsset.AssetType = asset.AssetType;
            existingAsset.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Updated asset: {Symbol} - {Name}", asset.Symbol, asset.Name);

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating asset with ID {AssetId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    // DELETE: api/Assets/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAsset(int id)
    {
        try
        {
            var asset = await _context.Assets.FindAsync(id);
            if (asset == null)
            {
                return NotFound();
            }

            _context.Assets.Remove(asset);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Deleted asset: {Symbol} - {Name}", asset.Symbol, asset.Name);

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting asset with ID {AssetId}", id);
            return StatusCode(500, "Internal server error");
        }
    }
}

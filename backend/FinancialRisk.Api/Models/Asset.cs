using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinancialRisk.Api.Models;

public class Asset
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [StringLength(10)]
    public string Symbol { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(50)]
    public string? Sector { get; set; }
    
    [StringLength(50)]
    public string? Industry { get; set; }
    
    [StringLength(50)]
    public string? AssetType { get; set; } // Stock, Bond, ETF, etc.
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual ICollection<Price> Prices { get; set; } = new List<Price>();
    public virtual ICollection<PortfolioHolding> PortfolioHoldings { get; set; } = new List<PortfolioHolding>();
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinancialRisk.Api.Models;

public class PortfolioHolding
{
    [Required]
    public int PortfolioId { get; set; }
    
    [Required]
    public int AssetId { get; set; }
    
    [Required]
    [Column(TypeName = "decimal(18,6)")]
    [Range(0, 1)]
    public decimal Weight { get; set; }
    
    [Column(TypeName = "decimal(18,6)")]
    public decimal? Quantity { get; set; }
    
    [Column(TypeName = "decimal(18,6)")]
    public decimal? AverageCost { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    [ForeignKey("PortfolioId")]
    public virtual Portfolio Portfolio { get; set; } = null!;
    
    [ForeignKey("AssetId")]
    public virtual Asset Asset { get; set; } = null!;
    
    // Composite primary key
    [Key]
    public PortfolioHoldingKey Key => new(PortfolioId, AssetId);
}

// Composite key for PortfolioHolding
public record PortfolioHoldingKey(int PortfolioId, int AssetId);

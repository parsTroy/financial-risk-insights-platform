using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinancialRisk.Api.Models;

public class Portfolio
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    [StringLength(50)]
    public string? Strategy { get; set; } // Conservative, Moderate, Aggressive
    
    [Column(TypeName = "decimal(18,6)")]
    public decimal? TargetReturn { get; set; }
    
    [Column(TypeName = "decimal(18,6)")]
    public decimal? MaxRisk { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    public bool IsActive { get; set; } = true;
    
    // Navigation properties
    public virtual ICollection<PortfolioHolding> PortfolioHoldings { get; set; } = new List<PortfolioHolding>();
}

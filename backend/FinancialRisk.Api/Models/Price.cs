using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinancialRisk.Api.Models;

public class Price
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    public int AssetId { get; set; }
    
    [Required]
    public DateTime Date { get; set; }
    
    [Column(TypeName = "decimal(18,6)")]
    public decimal? Open { get; set; }
    
    [Column(TypeName = "decimal(18,6)")]
    public decimal? High { get; set; }
    
    [Column(TypeName = "decimal(18,6)")]
    public decimal? Low { get; set; }
    
    [Column(TypeName = "decimal(18,6)")]
    public decimal? Close { get; set; }
    
    [Column(TypeName = "decimal(18,6)")]
    public decimal? AdjustedClose { get; set; }
    
    public long? Volume { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation property
    [ForeignKey("AssetId")]
    public virtual Asset Asset { get; set; } = null!;
}

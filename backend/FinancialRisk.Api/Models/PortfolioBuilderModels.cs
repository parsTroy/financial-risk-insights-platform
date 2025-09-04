using System.ComponentModel.DataAnnotations;

namespace FinancialRisk.Api.Models
{
    public class AssetSearchResult
    {
        public string Symbol { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public double ExpectedReturn { get; set; }
        public double Volatility { get; set; }
        public string Sector { get; set; } = string.Empty;
        public string Exchange { get; set; } = string.Empty;
        public double MarketCap { get; set; }
        public double Price { get; set; }
        public double Change { get; set; }
        public double ChangePercent { get; set; }
    }

    public class PortfolioAsset
    {
        public string Symbol { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public double Weight { get; set; }
        public double ExpectedReturn { get; set; }
        public double Volatility { get; set; }
        public double Price { get; set; }
        public int Quantity { get; set; }
        public double Value { get; set; }
        public string Sector { get; set; } = string.Empty;
        public DateTime AddedDate { get; set; } = DateTime.UtcNow;
    }

    public class PortfolioBuilder
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<PortfolioAsset> Assets { get; set; } = new();
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime ModifiedDate { get; set; } = DateTime.UtcNow;
        public string CreatedBy { get; set; } = string.Empty;
        public bool IsPublic { get; set; } = false;
        public Dictionary<string, object> Metadata { get; set; } = new();
    }

    public class PortfolioSummary
    {
        public int AssetCount { get; set; }
        public double TotalWeight { get; set; }
        public double ExpectedReturn { get; set; }
        public double ExpectedVolatility { get; set; }
        public double SharpeRatio { get; set; }
        public string RiskLevel { get; set; } = string.Empty;
        public double TotalValue { get; set; }
        public Dictionary<string, double> SectorAllocation { get; set; } = new();
        public Dictionary<string, double> RiskContribution { get; set; } = new();
    }

    public class PortfolioSaveRequest
    {
        [Required]
        [StringLength(100, MinimumLength = 1)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;
        
        [Required]
        public List<PortfolioAsset> Assets { get; set; } = new();
        
        public bool IsPublic { get; set; } = false;
        public Dictionary<string, object> Metadata { get; set; } = new();
    }

    public class PortfolioLoadRequest
    {
        [Required]
        public string PortfolioId { get; set; } = string.Empty;
    }

    public class PortfolioListRequest
    {
        public string? UserId { get; set; }
        public bool IncludePublic { get; set; } = true;
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? SearchQuery { get; set; }
        public string? SortBy { get; set; } = "ModifiedDate";
        public bool SortDescending { get; set; } = true;
    }

    public class PortfolioListResponse
    {
        public List<PortfolioBuilder> Portfolios { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    public class AssetSearchRequest
    {
        [Required]
        [StringLength(50, MinimumLength = 1)]
        public string Query { get; set; } = string.Empty;
        
        public string? Sector { get; set; }
        public string? Exchange { get; set; }
        public double? MinMarketCap { get; set; }
        public double? MaxMarketCap { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class AssetSearchResponse
    {
        public List<AssetSearchResult> Assets { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    public class PortfolioValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
        public Dictionary<string, object> Suggestions { get; set; } = new();
    }

    public class PortfolioRebalanceRequest
    {
        [Required]
        public string PortfolioId { get; set; } = string.Empty;
        
        [Required]
        public List<AssetRebalance> Rebalances { get; set; } = new();
        
        public bool ExecuteRebalance { get; set; } = false;
    }

    public class AssetRebalance
    {
        public string Symbol { get; set; } = string.Empty;
        public double CurrentWeight { get; set; }
        public double TargetWeight { get; set; }
        public double WeightDifference { get; set; }
        public double ValueDifference { get; set; }
    }

    public class PortfolioPerformanceMetrics
    {
        public double TotalReturn { get; set; }
        public double AnnualizedReturn { get; set; }
        public double Volatility { get; set; }
        public double SharpeRatio { get; set; }
        public double MaxDrawdown { get; set; }
        public double VaR95 { get; set; }
        public double CVaR95 { get; set; }
        public double Beta { get; set; }
        public double Alpha { get; set; }
        public double TrackingError { get; set; }
        public double InformationRatio { get; set; }
        public DateTime CalculationDate { get; set; } = DateTime.UtcNow;
    }

    public class PortfolioComparisonRequest
    {
        [Required]
        public List<string> PortfolioIds { get; set; } = new();
        
        public string Benchmark { get; set; } = "SPY";
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class PortfolioComparisonResult
    {
        public List<PortfolioPerformance> Portfolios { get; set; } = new();
        public PortfolioPerformance Benchmark { get; set; } = new();
        public Dictionary<string, double> CorrelationMatrix { get; set; } = new();
        public List<RiskReturnPoint> EfficientFrontier { get; set; } = new();
    }

    public class PortfolioPerformance
    {
        public string PortfolioId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public double TotalReturn { get; set; }
        public double AnnualizedReturn { get; set; }
        public double Volatility { get; set; }
        public double SharpeRatio { get; set; }
        public double MaxDrawdown { get; set; }
        public double VaR95 { get; set; }
        public double CVaR95 { get; set; }
    }

    public class RiskReturnPoint
    {
        public double Risk { get; set; }
        public double Return { get; set; }
        public List<AssetWeight> Weights { get; set; } = new();
    }
}

namespace FinancialRisk.Frontend.Models
{
    public class RiskMetrics
    {
        public string Symbol { get; set; } = string.Empty;
        public double Volatility { get; set; }
        public double SharpeRatio { get; set; }
        public double SortinoRatio { get; set; }
        public double ValueAtRisk95 { get; set; }
        public double ValueAtRisk99 { get; set; }
        public double ExpectedShortfall95 { get; set; }
        public double ExpectedShortfall99 { get; set; }
        public double MaximumDrawdown { get; set; }
        public double InformationRatio { get; set; }
        public DateTime CalculationDate { get; set; }
        public int DataPoints { get; set; }
        public string? Error { get; set; }
    }

    public class PortfolioRiskMetrics
    {
        public List<string> Symbols { get; set; } = new();
        public List<decimal> Weights { get; set; } = new();
        public double Volatility { get; set; }
        public double SharpeRatio { get; set; }
        public double SortinoRatio { get; set; }
        public double ValueAtRisk95 { get; set; }
        public double ValueAtRisk99 { get; set; }
        public double ExpectedShortfall95 { get; set; }
        public double ExpectedShortfall99 { get; set; }
        public double MaximumDrawdown { get; set; }
        public DateTime CalculationDate { get; set; }
        public int DataPoints { get; set; }
        public string? Error { get; set; }
    }

    public class RiskMetricsRequest
    {
        public string Symbol { get; set; } = string.Empty;
        public int Days { get; set; } = 252;
        public double RiskFreeRate { get; set; } = 0.02;
    }

    public class PortfolioRiskMetricsRequest
    {
        public List<string> Symbols { get; set; } = new();
        public List<decimal> Weights { get; set; } = new();
        public int Days { get; set; } = 252;
        public double RiskFreeRate { get; set; } = 0.02;
    }

    public class RiskAlert
    {
        public string Symbol { get; set; } = string.Empty;
        public string AlertType { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty; // "low", "medium", "high", "critical"
        public double CurrentValue { get; set; }
        public double Threshold { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class RiskDashboardData
    {
        public List<RiskMetrics> AssetMetrics { get; set; } = new();
        public PortfolioRiskMetrics? PortfolioMetrics { get; set; }
        public List<RiskAlert> Alerts { get; set; } = new();
        public DateTime LastUpdated { get; set; }
    }
}

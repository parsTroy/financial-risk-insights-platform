namespace FinancialRisk.Frontend.Models
{
    public class EfficientFrontierPoint
    {
        public double ExpectedReturn { get; set; }
        public double ExpectedVolatility { get; set; }
        public List<double> Weights { get; set; } = new();
        public List<AssetWeight> AssetWeights { get; set; } = new();
        public double SharpeRatio { get; set; }
    }

    public class AssetWeight
    {
        public string Symbol { get; set; } = string.Empty;
        public double Weight { get; set; }
        public double ExpectedReturn { get; set; }
        public double Volatility { get; set; }
        public double RiskContribution { get; set; }
        public double ReturnContribution { get; set; }
    }

    public class EfficientFrontier
    {
        public List<EfficientFrontierPoint> Points { get; set; } = new();
        public EfficientFrontierPoint? MinVolatilityPoint { get; set; }
        public EfficientFrontierPoint? MaxSharpePoint { get; set; }
        public EfficientFrontierPoint? MaxReturnPoint { get; set; }
        public bool Success { get; set; }
        public string? Error { get; set; }
        public string PortfolioName { get; set; } = string.Empty;
        public DateTime CalculationDate { get; set; }
        public int NumPoints { get; set; }
    }

    public class EfficientFrontierRequest
    {
        public string PortfolioName { get; set; } = "Efficient Frontier Portfolio";
        public List<string> Symbols { get; set; } = new();
        public List<AssetOptimizationData> Assets { get; set; } = new();
        public List<List<double>> CovarianceMatrix { get; set; } = new();
        public int EfficientFrontierPoints { get; set; } = 50;
        public int NumPoints { get; set; } = 50; // Alias for EfficientFrontierPoints
        public double RiskFreeRate { get; set; } = 0.02;
        public int LookbackPeriod { get; set; } = 252;
        public string OptimizationMethod { get; set; } = "mean_variance";
        public double RiskAversion { get; set; } = 1.0;
        public double MaxWeight { get; set; } = 1.0;
        public double MinWeight { get; set; } = 0.0;
    }

    public class EfficientFrontierConstraints
    {
        public double MinWeight { get; set; } = 0.0;
        public double MaxWeight { get; set; } = 1.0;
        public double RiskAversion { get; set; } = 1.0;
        public double RiskFreeRate { get; set; } = 0.02;
        public int NumPoints { get; set; } = 50;
        public bool ShowMinVolatility { get; set; } = true;
        public bool ShowMaxSharpe { get; set; } = true;
        public bool ShowMaxReturn { get; set; } = true;
        public bool ShowIndividualAssets { get; set; } = true;
    }

    public class ChartPoint
    {
        public double X { get; set; }
        public double Y { get; set; }
        public string Label { get; set; } = string.Empty;
        public string Color { get; set; } = "#007bff";
        public string Symbol { get; set; } = string.Empty;
        public List<AssetWeight>? Weights { get; set; }
    }

    public class EfficientFrontierChartData
    {
        public List<ChartPoint> FrontierPoints { get; set; } = new();
        public List<ChartPoint> IndividualAssets { get; set; } = new();
        public ChartPoint? MinVolatilityPoint { get; set; }
        public ChartPoint? MaxSharpePoint { get; set; }
        public ChartPoint? MaxReturnPoint { get; set; }
        public double MinVolatility { get; set; }
        public double MaxVolatility { get; set; }
        public double MinReturn { get; set; }
        public double MaxReturn { get; set; }
    }
}

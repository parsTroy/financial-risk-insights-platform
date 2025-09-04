namespace FinancialRisk.Frontend.Models
{
    public class PortfolioOptimizationRequest
    {
        public string Method { get; set; } = "mean_variance";
        public List<AssetOptimizationData> Assets { get; set; } = new();
        public List<List<double>> CovarianceMatrix { get; set; } = new();
        public double RiskAversion { get; set; } = 1.0;
        public double RiskFreeRate { get; set; } = 0.02;
        public Dictionary<string, object> Constraints { get; set; } = new();
    }

    public class AssetOptimizationData
    {
        public string Symbol { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public double ExpectedReturn { get; set; }
        public double Volatility { get; set; }
        public double Weight { get; set; }
    }

    public class PortfolioOptimizationResult
    {
        public List<AssetWeight> OptimalWeights { get; set; } = new();
        public double ExpectedReturn { get; set; }
        public double ExpectedVolatility { get; set; }
        public double SharpeRatio { get; set; }
        public double RiskAversion { get; set; }
        public double ExecutionTime { get; set; }
    }

    public class AssetWeight
    {
        public string Symbol { get; set; } = string.Empty;
        public double Weight { get; set; }
        public double ExpectedReturn { get; set; }
        public double RiskContribution { get; set; }
    }

    public class EfficientFrontierRequest
    {
        public List<AssetOptimizationData> Assets { get; set; } = new();
        public List<List<double>> CovarianceMatrix { get; set; } = new();
        public int NumPoints { get; set; } = 50;
    }

    public class EfficientFrontierResult
    {
        public List<EfficientFrontierPoint> FrontierPoints { get; set; } = new();
        public int NumPoints { get; set; }
        public double ExecutionTime { get; set; }
    }

    public class EfficientFrontierPoint
    {
        public double ExpectedReturn { get; set; }
        public double ExpectedVolatility { get; set; }
        public double SharpeRatio { get; set; }
        public List<AssetWeight> Weights { get; set; } = new();
    }

    public class RiskBudgetingRequest
    {
        public List<AssetOptimizationData> Assets { get; set; } = new();
        public List<List<double>> CovarianceMatrix { get; set; } = new();
        public List<double> RiskBudgets { get; set; } = new();
    }

    public class RiskBudgetingResult
    {
        public List<AssetWeight> OptimalWeights { get; set; } = new();
        public List<double> RiskBudgets { get; set; } = new();
        public List<double> ActualRiskContributions { get; set; } = new();
        public double ExecutionTime { get; set; }
    }

    public class BlackLittermanRequest
    {
        public List<AssetOptimizationData> Assets { get; set; } = new();
        public List<List<double>> CovarianceMatrix { get; set; } = new();
        public List<double> MarketCapWeights { get; set; } = new();
        public List<double> Views { get; set; } = new();
        public double RiskAversion { get; set; } = 1.0;
        public double Tau { get; set; } = 0.025;
    }

    public class BlackLittermanResult
    {
        public List<AssetWeight> OptimalWeights { get; set; } = new();
        public List<double> ImpliedReturns { get; set; } = new();
        public double RiskAversion { get; set; }
        public double ExecutionTime { get; set; }
    }
}

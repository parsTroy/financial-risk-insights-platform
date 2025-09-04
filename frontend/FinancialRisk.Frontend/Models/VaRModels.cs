namespace FinancialRisk.Frontend.Models
{
    public class VaRCalculationRequest
    {
        public List<double> Returns { get; set; } = new();
        public double ConfidenceLevel { get; set; } = 0.95;
        public int TimeHorizon { get; set; } = 1;
        public string Method { get; set; } = "historical";
    }

    public class VaRCalculationResult
    {
        public double VaR { get; set; }
        public double CVaR { get; set; }
        public double ConfidenceLevel { get; set; }
        public string Method { get; set; } = string.Empty;
        public int SampleSize { get; set; }
        public double MeanReturn { get; set; }
        public double StandardDeviation { get; set; }
    }

    public class MonteCarloSimulationRequest
    {
        public List<double> Returns { get; set; } = new();
        public double ConfidenceLevel { get; set; } = 0.95;
        public int TimeHorizon { get; set; } = 1;
        public string DistributionType { get; set; } = "normal";
        public int NumSimulations { get; set; } = 10000;
        public double? MeanReturn { get; set; }
        public double? Volatility { get; set; }
        public int? RandomSeed { get; set; }
    }

    public class MonteCarloSimulationResult
    {
        public double VaR { get; set; }
        public double CVaR { get; set; }
        public double ConfidenceLevel { get; set; }
        public string Method { get; set; } = string.Empty;
        public int NumSimulations { get; set; }
        public double ExecutionTime { get; set; }
        public List<double>? SimulatedReturns { get; set; }
    }

    public class PortfolioVaRRequest
    {
        public List<PortfolioAsset> Assets { get; set; } = new();
        public List<double> Weights { get; set; } = new();
        public double ConfidenceLevel { get; set; } = 0.95;
        public int TimeHorizon { get; set; } = 1;
        public string Method { get; set; } = "historical";
    }

    public class PortfolioAsset
    {
        public string Symbol { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public List<double> Returns { get; set; } = new();
        public double Weight { get; set; }
    }

    public class PortfolioVaRResult
    {
        public double PortfolioVaR { get; set; }
        public double PortfolioCVaR { get; set; }
        public double ConfidenceLevel { get; set; }
        public List<AssetContribution> AssetContributions { get; set; } = new();
        public double ExecutionTime { get; set; }
    }

    public class AssetContribution
    {
        public string Symbol { get; set; } = string.Empty;
        public double Weight { get; set; }
        public double VaRContribution { get; set; }
        public double CVaRContribution { get; set; }
        public double PercentageContribution { get; set; }
    }
}

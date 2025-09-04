namespace FinancialRisk.Api.Models
{
    public class VaRCalculation
    {
        public int Id { get; set; }
        public string Symbol { get; set; } = string.Empty;
        public string CalculationType { get; set; } = string.Empty; // "Historical", "MonteCarlo", "Parametric"
        public string DistributionType { get; set; } = string.Empty; // "Normal", "TStudent", "GARCH", "Copula"
        public double ConfidenceLevel { get; set; }
        public double VaR { get; set; }
        public double CVaR { get; set; }
        public double VaRLowerBound { get; set; }
        public double VaRUpperBound { get; set; }
        public double CVaRLowerBound { get; set; }
        public double CVaRUpperBound { get; set; }
        public int SampleSize { get; set; }
        public int SimulationCount { get; set; }
        public double TimeHorizon { get; set; } // in days
        public DateTime CalculationDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? Parameters { get; set; } // JSON string for additional parameters
        public string? Error { get; set; }
    }

    public class PortfolioVaRCalculation
    {
        public int Id { get; set; }
        public string PortfolioName { get; set; } = string.Empty;
        public string CalculationType { get; set; } = string.Empty;
        public string DistributionType { get; set; } = string.Empty;
        public double ConfidenceLevel { get; set; }
        public double PortfolioVaR { get; set; }
        public double PortfolioCVaR { get; set; }
        public double VaRLowerBound { get; set; }
        public double VaRUpperBound { get; set; }
        public double CVaRLowerBound { get; set; }
        public double CVaRUpperBound { get; set; }
        public int SampleSize { get; set; }
        public int SimulationCount { get; set; }
        public double TimeHorizon { get; set; }
        public DateTime CalculationDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? Parameters { get; set; }
        public string? Error { get; set; }
    }

    public class VaRAssetContribution
    {
        public int Id { get; set; }
        public int PortfolioVaRCalculationId { get; set; }
        public string Symbol { get; set; } = string.Empty;
        public double Weight { get; set; }
        public double VaRContribution { get; set; }
        public double CVaRContribution { get; set; }
        public double MarginalVaR { get; set; }
        public double ComponentVaR { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class VaRStressTest
    {
        public int Id { get; set; }
        public string Symbol { get; set; } = string.Empty;
        public string ScenarioName { get; set; } = string.Empty;
        public string ScenarioType { get; set; } = string.Empty; // "Historical", "Hypothetical", "MonteCarlo"
        public double StressFactor { get; set; }
        public double VaR { get; set; }
        public double CVaR { get; set; }
        public double ExpectedLoss { get; set; }
        public double UnexpectedLoss { get; set; }
        public DateTime ScenarioDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? Parameters { get; set; }
    }

    public class VaRCalculationRequest
    {
        public string Symbol { get; set; } = string.Empty;
        public string CalculationType { get; set; } = "Historical";
        public string DistributionType { get; set; } = "Normal";
        public List<double> ConfidenceLevels { get; set; } = new() { 0.95, 0.99 };
        public int Days { get; set; } = 252;
        public int SimulationCount { get; set; } = 10000;
        public double TimeHorizon { get; set; } = 1.0; // 1 day
        public Dictionary<string, object>? Parameters { get; set; }
    }

    public class PortfolioVaRCalculationRequest
    {
        public string PortfolioName { get; set; } = string.Empty;
        public List<string> Symbols { get; set; } = new();
        public List<decimal> Weights { get; set; } = new();
        public string CalculationType { get; set; } = "Historical";
        public string DistributionType { get; set; } = "Normal";
        public List<double> ConfidenceLevels { get; set; } = new() { 0.95, 0.99 };
        public int Days { get; set; } = 252;
        public int SimulationCount { get; set; } = 10000;
        public double TimeHorizon { get; set; } = 1.0;
        public Dictionary<string, object>? Parameters { get; set; }
    }

    public class VaRStressTestRequest
    {
        public string Symbol { get; set; } = string.Empty;
        public string ScenarioName { get; set; } = string.Empty;
        public string ScenarioType { get; set; } = "Historical";
        public double StressFactor { get; set; } = 1.0;
        public List<double> ConfidenceLevels { get; set; } = new() { 0.95, 0.99 };
        public int Days { get; set; } = 252;
        public Dictionary<string, object>? Parameters { get; set; }
    }

    public class VaRCalculationResponse
    {
        public bool Success { get; set; }
        public string? Error { get; set; }
        public VaRCalculation? Data { get; set; }
    }

    public class PortfolioVaRCalculationResponse
    {
        public bool Success { get; set; }
        public string? Error { get; set; }
        public PortfolioVaRCalculation? Data { get; set; }
        public List<VaRAssetContribution>? AssetContributions { get; set; }
    }

    public class VaRStressTestResponse
    {
        public bool Success { get; set; }
        public string? Error { get; set; }
        public VaRStressTest? Data { get; set; }
    }

    public class VaRComparisonResult
    {
        public string Symbol { get; set; } = string.Empty;
        public Dictionary<string, double> VaRResults { get; set; } = new();
        public Dictionary<string, double> CVaRResults { get; set; } = new();
        public Dictionary<string, double> ConfidenceIntervals { get; set; } = new();
        public string BestMethod { get; set; } = string.Empty;
        public double BestVaR { get; set; }
        public double BestCVaR { get; set; }
    }

    public class VaRBacktestResult
    {
        public int Id { get; set; }
        public string Symbol { get; set; } = string.Empty;
        public string Method { get; set; } = string.Empty;
        public double ConfidenceLevel { get; set; }
        public int BacktestPeriod { get; set; }
        public int Violations { get; set; }
        public double ViolationRate { get; set; }
        public double KupiecTestStatistic { get; set; }
        public double KupiecPValue { get; set; }
        public bool KupiecTestPassed { get; set; }
        public double ChristoffersenTestStatistic { get; set; }
        public double ChristoffersenPValue { get; set; }
        public bool ChristoffersenTestPassed { get; set; }
        public DateTime BacktestDate { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}

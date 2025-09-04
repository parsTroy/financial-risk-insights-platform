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

    // Monte Carlo specific models
    public class MonteCarloSimulationRequest
    {
        public string Symbol { get; set; } = string.Empty;
        public string DistributionType { get; set; } = "Normal";
        public int NumSimulations { get; set; } = 10000;
        public int TimeHorizon { get; set; } = 1;
        public List<double> ConfidenceLevels { get; set; } = new() { 0.95, 0.99 };
        public bool UseAntitheticVariates { get; set; } = false;
        public bool UseControlVariates { get; set; } = false;
        public bool UseQuasiMonteCarlo { get; set; } = false;
        public int? Seed { get; set; }
        public Dictionary<string, object>? CustomParameters { get; set; }
    }

    public class MonteCarloPortfolioSimulationRequest
    {
        public string PortfolioName { get; set; } = string.Empty;
        public List<string> Symbols { get; set; } = new();
        public List<decimal> Weights { get; set; } = new();
        public string DistributionType { get; set; } = "Normal";
        public int NumSimulations { get; set; } = 10000;
        public int TimeHorizon { get; set; } = 1;
        public List<double> ConfidenceLevels { get; set; } = new() { 0.95, 0.99 };
        public bool UseCorrelation { get; set; } = true;
        public int? Seed { get; set; }
        public Dictionary<string, object>? CustomParameters { get; set; }
    }

    public class MonteCarloStressTestRequest
    {
        public string Symbol { get; set; } = string.Empty;
        public string ScenarioName { get; set; } = string.Empty;
        public string ScenarioType { get; set; } = "VolatilityShock";
        public double StressFactor { get; set; } = 1.5;
        public string DistributionType { get; set; } = "Normal";
        public int NumSimulations { get; set; } = 10000;
        public List<double> ConfidenceLevels { get; set; } = new() { 0.95, 0.99 };
        public Dictionary<string, object>? CustomParameters { get; set; }
    }

    public class MonteCarloSimulationResult
    {
        public bool Success { get; set; }
        public string? Error { get; set; }
        public string Symbol { get; set; } = string.Empty;
        public string DistributionType { get; set; } = string.Empty;
        public int NumSimulations { get; set; }
        public int TimeHorizon { get; set; }
        public Dictionary<double, double> VaRValues { get; set; } = new();
        public Dictionary<double, double> CVaRValues { get; set; } = new();
        public double ExpectedValue { get; set; }
        public double StandardDeviation { get; set; }
        public double Skewness { get; set; }
        public double Kurtosis { get; set; }
        public Dictionary<double, double> Percentiles { get; set; } = new();
        public List<double> SimulatedReturns { get; set; } = new();
        public List<double> SimulatedPrices { get; set; } = new();
        public Dictionary<string, object> SimulationMetadata { get; set; } = new();
        public DateTime CalculationDate { get; set; }
    }

    public class MonteCarloPortfolioSimulationResult
    {
        public bool Success { get; set; }
        public string? Error { get; set; }
        public string PortfolioName { get; set; } = string.Empty;
        public string DistributionType { get; set; } = string.Empty;
        public int NumSimulations { get; set; }
        public int TimeHorizon { get; set; }
        public Dictionary<double, double> PortfolioVaRValues { get; set; } = new();
        public Dictionary<double, double> PortfolioCVaRValues { get; set; } = new();
        public double ExpectedReturn { get; set; }
        public double PortfolioVolatility { get; set; }
        public List<MonteCarloSimulationResult> AssetResults { get; set; } = new();
        public List<double> VaRContributions { get; set; } = new();
        public List<double> MarginalVaR { get; set; } = new();
        public List<double> ComponentVaR { get; set; } = new();
        public double DiversificationRatio { get; set; }
        public List<double> PortfolioReturns { get; set; } = new();
        public List<double> PortfolioValues { get; set; } = new();
        public Dictionary<string, object> SimulationMetadata { get; set; } = new();
        public DateTime CalculationDate { get; set; }
    }

    public class MonteCarloStressTestResult
    {
        public bool Success { get; set; }
        public string? Error { get; set; }
        public string Symbol { get; set; } = string.Empty;
        public string ScenarioName { get; set; } = string.Empty;
        public string ScenarioType { get; set; } = string.Empty;
        public double StressFactor { get; set; }
        public Dictionary<string, MonteCarloSimulationResult> ScenarioResults { get; set; } = new();
        public Dictionary<string, double> VaRComparison { get; set; } = new();
        public Dictionary<string, double> CVaRComparison { get; set; } = new();
        public DateTime CalculationDate { get; set; }
    }

    public class MonteCarloComparisonResult
    {
        public string Symbol { get; set; } = string.Empty;
        public Dictionary<string, double> VaRResults { get; set; } = new();
        public Dictionary<string, double> CVaRResults { get; set; } = new();
        public Dictionary<string, double> ExecutionTimes { get; set; } = new();
        public Dictionary<string, double> AccuracyMetrics { get; set; } = new();
        public string BestMethod { get; set; } = string.Empty;
        public string BestDistribution { get; set; } = string.Empty;
        public DateTime ComparisonDate { get; set; }
    }

    // Portfolio Optimization Models
    public class PortfolioOptimizationRequest
    {
        public string PortfolioName { get; set; } = string.Empty;
        public List<string> Symbols { get; set; } = new();
        public string OptimizationMethod { get; set; } = "MeanVariance";
        public double RiskAversion { get; set; } = 1.0;
        public double? TargetReturn { get; set; }
        public double? TargetVolatility { get; set; }
        public double MaxWeight { get; set; } = 1.0;
        public double MinWeight { get; set; } = 0.0;
        public double MaxLeverage { get; set; } = 1.0;
        public double TransactionCosts { get; set; } = 0.0;
        public int LookbackPeriod { get; set; } = 252;
        public double ConfidenceLevel { get; set; } = 0.95;
        public Dictionary<string, object>? CustomConstraints { get; set; }
        public bool CalculateEfficientFrontier { get; set; } = false;
        public int EfficientFrontierPoints { get; set; } = 50;
    }

    public class AssetOptimizationData
    {
        public string Symbol { get; set; } = string.Empty;
        public double ExpectedReturn { get; set; }
        public double Volatility { get; set; }
        public List<double> HistoricalReturns { get; set; } = new();
        public string? Sector { get; set; }
        public double? MarketCap { get; set; }
        public double? Beta { get; set; }
    }

    public class PortfolioOptimizationResult
    {
        public bool Success { get; set; }
        public string? Error { get; set; }
        public string PortfolioName { get; set; } = string.Empty;
        public string OptimizationMethod { get; set; } = string.Empty;
        public List<double> OptimalWeights { get; set; } = new();
        public double ExpectedReturn { get; set; }
        public double ExpectedVolatility { get; set; }
        public double SharpeRatio { get; set; }
        public double VaR { get; set; }
        public double CVaR { get; set; }
        public double DiversificationRatio { get; set; }
        public double ConcentrationRatio { get; set; }
        public List<AssetWeight> AssetWeights { get; set; } = new();
        public Dictionary<string, object> OptimizationMetadata { get; set; } = new();
        public DateTime CalculationDate { get; set; }
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

    public class EfficientFrontierPoint
    {
        public double ExpectedReturn { get; set; }
        public double ExpectedVolatility { get; set; }
        public double SharpeRatio { get; set; }
        public List<double> Weights { get; set; } = new();
    }

    public class EfficientFrontier
    {
        public bool Success { get; set; }
        public string? Error { get; set; }
        public List<EfficientFrontierPoint> Points { get; set; } = new();
        public EfficientFrontierPoint? MinVolatilityPoint { get; set; }
        public EfficientFrontierPoint? MaxSharpePoint { get; set; }
        public EfficientFrontierPoint? MaxReturnPoint { get; set; }
        public Dictionary<string, object> FrontierMetadata { get; set; } = new();
        public DateTime CalculationDate { get; set; }
    }

    public class PortfolioOptimizationResponse
    {
        public bool Success { get; set; }
        public string? Error { get; set; }
        public PortfolioOptimizationResult? OptimizationResult { get; set; }
        public EfficientFrontier? EfficientFrontier { get; set; }
    }

    public class RiskBudgetingRequest
    {
        public string PortfolioName { get; set; } = string.Empty;
        public List<string> Symbols { get; set; } = new();
        public List<double> RiskBudgets { get; set; } = new();
        public double MaxWeight { get; set; } = 1.0;
        public double MinWeight { get; set; } = 0.0;
        public int LookbackPeriod { get; set; } = 252;
        public Dictionary<string, object>? CustomConstraints { get; set; }
    }

    public class RiskBudgetingResult
    {
        public bool Success { get; set; }
        public string? Error { get; set; }
        public string PortfolioName { get; set; } = string.Empty;
        public List<double> OptimalWeights { get; set; } = new();
        public List<double> RiskBudgets { get; set; } = new();
        public List<double> ActualRiskContributions { get; set; } = new();
        public double PortfolioVolatility { get; set; }
        public List<AssetWeight> AssetWeights { get; set; } = new();
        public Dictionary<string, object> OptimizationMetadata { get; set; } = new();
        public DateTime CalculationDate { get; set; }
    }

    public class BlackLittermanRequest
    {
        public string PortfolioName { get; set; } = string.Empty;
        public List<string> Symbols { get; set; } = new();
        public List<double> MarketCapWeights { get; set; } = new();
        public List<double> Views { get; set; } = new();
        public List<List<double>> PickMatrix { get; set; } = new();
        public List<double> ViewUncertainties { get; set; } = new();
        public double RiskAversion { get; set; } = 1.0;
        public double Tau { get; set; } = 0.025;
        public double MaxWeight { get; set; } = 1.0;
        public double MinWeight { get; set; } = 0.0;
        public int LookbackPeriod { get; set; } = 252;
    }

    public class BlackLittermanResult
    {
        public bool Success { get; set; }
        public string? Error { get; set; }
        public string PortfolioName { get; set; } = string.Empty;
        public List<double> OptimalWeights { get; set; } = new();
        public List<double> ImpliedReturns { get; set; } = new();
        public List<double> AdjustedReturns { get; set; } = new();
        public double ExpectedReturn { get; set; }
        public double ExpectedVolatility { get; set; }
        public double SharpeRatio { get; set; }
        public List<AssetWeight> AssetWeights { get; set; } = new();
        public Dictionary<string, object> OptimizationMetadata { get; set; } = new();
        public DateTime CalculationDate { get; set; }
    }

    public class TransactionCostOptimizationRequest
    {
        public string PortfolioName { get; set; } = string.Empty;
        public List<string> Symbols { get; set; } = new();
        public List<double> CurrentWeights { get; set; } = new();
        public List<double> TargetWeights { get; set; } = new();
        public List<double> TransactionCosts { get; set; } = new();
        public double MaxTurnover { get; set; } = 1.0;
        public double MaxWeight { get; set; } = 1.0;
        public double MinWeight { get; set; } = 0.0;
        public Dictionary<string, object>? CustomConstraints { get; set; }
    }

    public class TransactionCostOptimizationResult
    {
        public bool Success { get; set; }
        public string? Error { get; set; }
        public string PortfolioName { get; set; } = string.Empty;
        public List<double> OptimalWeights { get; set; } = new();
        public List<double> RebalancingWeights { get; set; } = new();
        public double TotalTransactionCosts { get; set; }
        public double Turnover { get; set; }
        public List<AssetWeight> AssetWeights { get; set; } = new();
        public Dictionary<string, object> OptimizationMetadata { get; set; } = new();
        public DateTime CalculationDate { get; set; }
    }
}

namespace FinancialRisk.Frontend.Services
{
    public class ApiConfiguration
    {
        public string BaseUrl { get; set; } = string.Empty;
        public int Timeout { get; set; } = 30000;
        public int RetryCount { get; set; } = 3;
    }
}

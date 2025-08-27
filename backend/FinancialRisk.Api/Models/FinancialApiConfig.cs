namespace FinancialRisk.Api.Models
{
    public class FinancialApiConfig
    {
        public string ApiKey { get; set; } = string.Empty;
        public string BaseUrl { get; set; } = string.Empty;
        public int RequestTimeoutSeconds { get; set; } = 30;
        public int MaxRequestsPerMinute { get; set; } = 5;
        public string Provider { get; set; } = "AlphaVantage";
    }
}

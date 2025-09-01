using FinancialRisk.Api.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace FinancialRisk.Api.Services
{
    public class AlphaVantageService : IFinancialDataService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<AlphaVantageService> _logger;
        private readonly FinancialApiConfig _config;
        private readonly IDataPersistenceService _dataPersistenceService;
        private readonly SemaphoreSlim _rateLimiter;
        private readonly Queue<DateTime> _requestTimes;

        public AlphaVantageService(
            HttpClient httpClient,
            ILogger<AlphaVantageService> logger,
            IOptions<FinancialApiConfig> config,
            IDataPersistenceService dataPersistenceService)
        {
            _httpClient = httpClient;
            _logger = logger;
            _config = config.Value;
            _dataPersistenceService = dataPersistenceService;
            _rateLimiter = new SemaphoreSlim(1, 1);
            _requestTimes = new Queue<DateTime>();
            
            _httpClient.BaseAddress = new Uri(_config.BaseUrl);
            _httpClient.Timeout = TimeSpan.FromSeconds(_config.RequestTimeoutSeconds);
        }

        public async Task<ApiResponse<StockQuote>> GetStockQuoteAsync(string symbol)
        {
            try
            {
                _logger.LogInformation("Fetching stock quote for symbol: {Symbol}", symbol);
                
                await EnforceRateLimitAsync();
                
                var url = $"query?function=GLOBAL_QUOTE&symbol={symbol}&apikey={_config.ApiKey}";
                var response = await _httpClient.GetAsync(url);
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("API request failed for symbol {Symbol}. Status: {StatusCode}", symbol, response.StatusCode);
                    return new ApiResponse<StockQuote>
                    {
                        Success = false,
                        StatusCode = (int)response.StatusCode,
                        ErrorMessage = $"API request failed with status {response.StatusCode}"
                    };
                }

                var content = await response.Content.ReadAsStringAsync();
                var stockQuote = ParseStockQuoteResponse(content, symbol);
                
                if (stockQuote != null)
                {
                    _logger.LogInformation("Successfully fetched stock quote for {Symbol}: ${Close}", symbol, stockQuote.Close);
                    return new ApiResponse<StockQuote>
                    {
                        Success = true,
                        Data = stockQuote,
                        StatusCode = 200
                    };
                }
                
                return new ApiResponse<StockQuote>
                {
                    Success = false,
                    StatusCode = 500,
                    ErrorMessage = "Failed to parse API response"
                };
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request error while fetching stock quote for {Symbol}", symbol);
                return new ApiResponse<StockQuote>
                {
                    Success = false,
                    StatusCode = 500,
                    ErrorMessage = "Network error occurred while fetching data"
                };
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "Request timeout while fetching stock quote for {Symbol}", symbol);
                return new ApiResponse<StockQuote>
                {
                    Success = false,
                    StatusCode = 408,
                    ErrorMessage = "Request timed out"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while fetching stock quote for {Symbol}", symbol);
                return new ApiResponse<StockQuote>
                {
                    Success = false,
                    StatusCode = 500,
                    ErrorMessage = "An unexpected error occurred"
                };
            }
        }

        public async Task<ApiResponse<List<StockQuote>>> GetStockHistoryAsync(string symbol, int days = 30)
        {
            try
            {
                _logger.LogInformation("Fetching stock history for {Symbol} for {Days} days", symbol, days);
                
                await EnforceRateLimitAsync();
                
                var url = $"query?function=TIME_SERIES_DAILY&symbol={symbol}&apikey={_config.ApiKey}";
                var response = await _httpClient.GetAsync(url);
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("API request failed for {Symbol} history. Status: {StatusCode}", symbol, response.StatusCode);
                    return new ApiResponse<List<StockQuote>>
                    {
                        Success = false,
                        StatusCode = (int)response.StatusCode,
                        ErrorMessage = $"API request failed with status {response.StatusCode}"
                    };
                }

                var content = await response.Content.ReadAsStringAsync();
                var stockHistory = ParseStockHistoryResponse(content, symbol, days);
                
                if (stockHistory != null)
                {
                    _logger.LogInformation("Successfully fetched {Count} days of stock history for {Symbol}", stockHistory.Count, symbol);
                    return new ApiResponse<List<StockQuote>>
                    {
                        Success = true,
                        Data = stockHistory,
                        StatusCode = 200
                    };
                }
                
                return new ApiResponse<List<StockQuote>>
                {
                    Success = false,
                    StatusCode = 500,
                    ErrorMessage = "Failed to parse API response"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while fetching stock history for {Symbol}", symbol);
                return new ApiResponse<List<StockQuote>>
                {
                    Success = false,
                    StatusCode = 500,
                    ErrorMessage = "An error occurred while fetching stock history"
                };
            }
        }

        public async Task<ApiResponse<decimal>> GetCurrentPriceAsync(string symbol)
        {
            var quoteResponse = await GetStockQuoteAsync(symbol);
            if (quoteResponse.Success && quoteResponse.Data != null)
            {
                return new ApiResponse<decimal>
                {
                    Success = true,
                    Data = quoteResponse.Data.Close,
                    StatusCode = 200
                };
            }
            
            return new ApiResponse<decimal>
            {
                Success = false,
                StatusCode = quoteResponse.StatusCode,
                ErrorMessage = quoteResponse.ErrorMessage
            };
        }

        public async Task<ApiResponse<bool>> SaveStockQuoteToDatabaseAsync(string symbol)
        {
            try
            {
                _logger.LogInformation("Fetching and saving stock quote for {Symbol} to database", symbol);
                
                var quoteResponse = await GetStockQuoteAsync(symbol);
                if (!quoteResponse.Success || quoteResponse.Data == null)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        StatusCode = quoteResponse.StatusCode,
                        ErrorMessage = quoteResponse.ErrorMessage
                    };
                }

                var success = await _dataPersistenceService.SaveStockQuoteAsync(quoteResponse.Data);
                
                return new ApiResponse<bool>
                {
                    Success = true,
                    Data = success,
                    StatusCode = 200
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save stock quote for {Symbol} to database", symbol);
                return new ApiResponse<bool>
                {
                    Success = false,
                    StatusCode = 500,
                    ErrorMessage = "An error occurred while saving data to database"
                };
            }
        }

        public async Task<ApiResponse<bool>> SaveStockHistoryToDatabaseAsync(string symbol, int days = 30)
        {
            try
            {
                _logger.LogInformation("Fetching and saving {Days} days of stock history for {Symbol} to database", days, symbol);
                
                var historyResponse = await GetStockHistoryAsync(symbol, days);
                if (!historyResponse.Success || historyResponse.Data == null)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        StatusCode = historyResponse.StatusCode,
                        ErrorMessage = historyResponse.ErrorMessage
                    };
                }

                var success = await _dataPersistenceService.SaveStockHistoryAsync(symbol, historyResponse.Data);
                
                return new ApiResponse<bool>
                {
                    Success = true,
                    Data = success,
                    StatusCode = 200
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save stock history for {Symbol} to database", symbol);
                return new ApiResponse<bool>
                {
                    Success = false,
                    StatusCode = 500,
                    ErrorMessage = "An error occurred while saving data to database"
                };
            }
        }

        private async Task EnforceRateLimitAsync()
        {
            await _rateLimiter.WaitAsync();
            try
            {
                var now = DateTime.UtcNow;
                
                // Remove requests older than 1 minute
                while (_requestTimes.Count > 0 && _requestTimes.Peek() < now.AddMinutes(-1))
                {
                    _requestTimes.Dequeue();
                }
                
                // If we've made too many requests, wait
                if (_requestTimes.Count >= _config.MaxRequestsPerMinute)
                {
                    var oldestRequest = _requestTimes.Peek();
                    var waitTime = oldestRequest.AddMinutes(1) - now;
                    if (waitTime > TimeSpan.Zero)
                    {
                        _logger.LogInformation("Rate limit reached. Waiting {WaitTime}ms", waitTime.TotalMilliseconds);
                        await Task.Delay(waitTime);
                    }
                }
                
                _requestTimes.Enqueue(now);
            }
            finally
            {
                _rateLimiter.Release();
            }
        }

        private StockQuote? ParseStockQuoteResponse(string content, string symbol)
        {
            try
            {
                using var document = JsonDocument.Parse(content);
                var root = document.RootElement;
                
                if (root.TryGetProperty("Global Quote", out var globalQuote))
                {
                    var quote = globalQuote.GetProperty("05. price");
                    var change = globalQuote.GetProperty("09. change");
                    var changePercent = globalQuote.GetProperty("10. change percent");
                    var volume = globalQuote.GetProperty("06. volume");
                    var open = globalQuote.GetProperty("02. open");
                    var high = globalQuote.GetProperty("03. high");
                    var low = globalQuote.GetProperty("04. low");
                    
                    return new StockQuote
                    {
                        Symbol = symbol,
                        Close = decimal.Parse(quote.GetString() ?? "0"),
                        Change = decimal.Parse(change.GetString() ?? "0"),
                        ChangePercent = decimal.Parse(changePercent.GetString()?.TrimEnd('%') ?? "0"),
                        Volume = long.Parse(volume.GetString() ?? "0"),
                        Open = decimal.Parse(open.GetString() ?? "0"),
                        High = decimal.Parse(high.GetString() ?? "0"),
                        Low = decimal.Parse(low.GetString() ?? "0"),
                        Timestamp = DateTime.UtcNow
                    };
                }
                
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to parse stock quote response for {Symbol}", symbol);
                return null;
            }
        }

        private List<StockQuote>? ParseStockHistoryResponse(string content, string symbol, int days)
        {
            try
            {
                var stockQuotes = new List<StockQuote>();
                using var document = JsonDocument.Parse(content);
                var root = document.RootElement;
                
                if (root.TryGetProperty("Time Series (Daily)", out var timeSeries))
                {
                    var count = 0;
                    foreach (var day in timeSeries.EnumerateObject())
                    {
                        if (count >= days) break;
                        
                        var data = day.Value;
                        var stockQuote = new StockQuote
                        {
                            Symbol = symbol,
                            Open = decimal.Parse(data.GetProperty("1. open").GetString() ?? "0"),
                            High = decimal.Parse(data.GetProperty("2. high").GetString() ?? "0"),
                            Low = decimal.Parse(data.GetProperty("3. low").GetString() ?? "0"),
                            Close = decimal.Parse(data.GetProperty("4. close").GetString() ?? "0"),
                            Volume = long.Parse(data.GetProperty("5. volume").GetString() ?? "0"),
                            Timestamp = DateTime.Parse(day.Name)
                        };
                        
                        stockQuotes.Add(stockQuote);
                        count++;
                    }
                }
                
                return stockQuotes.OrderByDescending(q => q.Timestamp).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to parse stock history response for {Symbol}", symbol);
                return null;
            }
        }

        public void Dispose()
        {
            _rateLimiter?.Dispose();
        }
    }
}

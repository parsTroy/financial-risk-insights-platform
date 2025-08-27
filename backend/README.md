# Financial Risk Insights Platform - Backend

## Financial Data API Integration

This backend integrates with Alpha Vantage API to provide real-time financial data including stock quotes, forex rates, and historical data.

## Setup Instructions

### 1. Get Alpha Vantage API Key

1. Visit [Alpha Vantage](https://www.alphavantage.co/support/#api-key)
2. Sign up for a free account
3. Get your API key (free tier allows 5 API calls per minute)

### 2. Configure Environment Variables

#### Option A: Using .env file (Recommended)

1. Copy the example environment file:
   ```bash
   cp env.example .env
   ```

2. Edit the `.env` file and add your API key:
   ```bash
   # Financial API Configuration
   ALPHA_VANTAGE_API_KEY=your_actual_api_key_here
   FINANCIAL_API_BASE_URL=https://www.alphavantage.co/
   FINANCIAL_API_TIMEOUT_SECONDS=30
   FINANCIAL_API_MAX_REQUESTS_PER_MINUTE=5
   FINANCIAL_API_PROVIDER=AlphaVantage
   ```

#### Option B: Environment Variables

You can also set the API key via environment variables:

```bash
export ALPHA_VANTAGE_API_KEY="your_api_key"
export FINANCIAL_API_BASE_URL="https://www.alphavantage.co/"
export FINANCIAL_API_TIMEOUT_SECONDS="30"
export FINANCIAL_API_MAX_REQUESTS_PER_MINUTE="5"
export FINANCIAL_API_PROVIDER="AlphaVantage"
```

### 3. Security Notes

- **Never commit your `.env` file** to version control
- The `.env` file is already in `.gitignore`
- Use different API keys for development, staging, and production
- Rotate your API keys regularly

## Available Endpoints

### Stock Data
- `GET /api/financialdata/stock/{symbol}` - Get current stock quote
- `GET /api/financialdata/stock/{symbol}/history?days=30` - Get stock history
- `GET /api/financialdata/stock/{symbol}/price` - Get current price only

### Forex Data
- `GET /api/financialdata/forex/{fromCurrency}/{toCurrency}` - Get exchange rate

## Example Usage

```bash
# Get Apple stock quote
curl "http://localhost:5290/api/financialdata/stock/AAPL"

# Get USD to EUR exchange rate
curl "http://localhost:5290/api/financialdata/forex/USD/EUR"

# Get 30 days of Microsoft stock history
curl "http://localhost:5290/api/financialdata/stock/MSFT/history?days=30"
```

## Features

- **Environment Variables**: Secure .env file support
- **Rate Limiting**: Respects Alpha Vantage's 5 requests/minute limit
- **Error Handling**: Comprehensive error handling with detailed logging
- **Async Operations**: Non-blocking API calls
- **Configuration**: Environment-based configuration
- **Logging**: Structured logging for monitoring and debugging
- **Type Safety**: Strongly typed models and responses

## Rate Limiting

The service automatically enforces rate limiting to comply with Alpha Vantage's free tier limits:
- Maximum 5 requests per minute
- Automatic queuing and waiting when limit is reached
- Configurable limits via environment variables

## Error Handling

The API returns structured error responses:

```json
{
  "success": false,
  "data": null,
  "errorMessage": "API request failed with status 429",
  "statusCode": 429
}
```

## Logging

All API calls are logged with structured logging:
- Request details (symbol, currency pairs)
- Response status and timing
- Error details with stack traces
- Rate limiting information

## Testing

Run the tests to verify the implementation:

```bash
dotnet test ./backend/FinancialRisk.sln
```

## Dependencies

- .NET 9.0
- dotenv.net (for .env file support)
- Microsoft.AspNetCore.OpenApi
- System.Text.Json (built-in)
- Microsoft.Extensions.Logging (built-in)
- Microsoft.Extensions.Options (built-in)
- Microsoft.Extensions.Http (built-in)

## Troubleshooting

### Common Issues

1. **API Key Not Found**: Ensure your `.env` file exists and contains `ALPHA_VANTAGE_API_KEY`
2. **Rate Limiting**: Free tier allows only 5 requests per minute
3. **Invalid Symbol**: Check that stock symbols are valid (e.g., AAPL, MSFT, GOOGL)

### Debug Environment Variables

To verify your environment variables are loaded correctly, check the logs when the application starts.

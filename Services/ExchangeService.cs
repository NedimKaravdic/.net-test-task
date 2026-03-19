using System.Text.Json;
using EHSExchangeDashboard.Data;
using EHSExchangeDashboard.Entities;
using EHSExchangeDashboard.Interfaces;
using EHSExchangeDashboard.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EHSExchangeDashboard.Services;

public class ExchangeService : IExchangeService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ExchangeService> _logger;
    private readonly IDistributedCache _cache;
    private const string CacheKey = "RatesList";

    public ExchangeService(
        IHttpClientFactory httpClientFactory, 
        IConfiguration configuration,
        ILogger<ExchangeService> logger,
        IDistributedCache cache)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
        _cache = cache;
    }

    public async Task<List<ExchangeRate>> GetCachedRatesAsync()
    {
        var cachedData = await _cache.GetStringAsync(CacheKey);

        if (!string.IsNullOrEmpty(cachedData))
        {
            _logger.LogInformation("Loaded rates from Redis cache.");
            return JsonSerializer.Deserialize<List<ExchangeRate>>(cachedData)!;
        }

        _logger.LogInformation("Redis miss. Fetching fresh rates from API.");
        await FetchLiveRatesAsync();
        
        // Re-read after sync
        cachedData = await _cache.GetStringAsync(CacheKey);
        return !string.IsNullOrEmpty(cachedData) 
            ? JsonSerializer.Deserialize<List<ExchangeRate>>(cachedData)! 
            : new List<ExchangeRate>();
    }

    public async Task SyncRatesAsync()
    {
        // For Timeframe/Historical, we directly sync to Redis now
        await FetchLiveRatesAsync(); 
    }

    public async Task FetchLiveRatesAsync()
    {
        var apiKey = _configuration["ApiKey"];
        if (string.IsNullOrEmpty(apiKey) || apiKey == "YOUR_API_KEY_HERE")
        {
            _logger.LogWarning("API key not configured. Skipping live rate poll.");
            return;
        }

        var client = _httpClientFactory.CreateClient();
        string url = $"https://api.exchangerate.host/live?access_key={apiKey}";

        try
        {
            _logger.LogInformation("🔄 FETCHING: Requesting fresh market data from API (/live)...");
            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<LiveApiResponse>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (result != null && result.Success && result.Quotes != null)
            {
                string todayDate = DateTime.UtcNow.ToString("yyyy-MM-dd");
                var now = DateTime.UtcNow;
                var filteredRates = new List<ExchangeRate>();

                foreach (var quote in result.Quotes)
                {
                    string currencyCode = quote.Key;
                    if (currencyCode.StartsWith(result.Source) && currencyCode.Length > result.Source.Length)
                    {
                        currencyCode = currencyCode.Substring(result.Source.Length);
                    }

                    // Filter noise - only mainstream international currencies
                    if (!AppConstants.SupportedCurrencies.Contains(currencyCode))
                    {
                        continue;
                    }

                    decimal rawRate = quote.Value;
                    decimal transformedRate = rawRate * (1m + AppConstants.ServiceFeeMargin);
                    string volatilityTag = transformedRate > 2.0m ? "High" : "Low";

                    filteredRates.Add(new ExchangeRate
                    {
                        CurrencyCode = currencyCode,
                        BaseCurrency = result.Source,
                        ProviderDate = todayDate,
                        RawRate = rawRate,
                        TransformedRate = transformedRate,
                        UpdatedAt = now,
                        VolatilityTag = volatilityTag
                    });
                }

                var options = new DistributedCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(AppConstants.CacheExpirationMinutes));
                
                await _cache.SetStringAsync(CacheKey, JsonSerializer.Serialize(filteredRates), options);
                _logger.LogInformation("✅ SYNC SUCCESS: {Count} filtered rates cached in Redis (API call successful).", filteredRates.Count);
            }
            else
            {
                _logger.LogWarning("Live API /live returned error. Response: {Content}", content);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching live rates from exchangerate.host.");
        }
    }
}

public class TimeframeApiResponse
{
    public bool Success { get; set; }
    public string Source { get; set; } = string.Empty;
    public string Start_Date { get; set; } = string.Empty;
    public string End_Date { get; set; } = string.Empty;
    public Dictionary<string, Dictionary<string, decimal>> Quotes { get; set; } = new();
}

public class LiveApiResponse
{
    public bool Success { get; set; }
    public string Source { get; set; } = string.Empty;
    public Dictionary<string, decimal> Quotes { get; set; } = new();
}

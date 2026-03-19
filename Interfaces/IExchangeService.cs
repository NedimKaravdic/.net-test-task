using EHSExchangeDashboard.Entities;

namespace EHSExchangeDashboard.Interfaces;

public interface IExchangeService
{
    Task SyncRatesAsync();
    Task FetchLiveRatesAsync();
    Task<List<ExchangeRate>> GetCachedRatesAsync();
}

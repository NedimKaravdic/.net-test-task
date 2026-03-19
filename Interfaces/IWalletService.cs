using EHSExchangeDashboard.Entities;

namespace EHSExchangeDashboard.Interfaces;

public interface IWalletService
{
    Task<List<WalletBalance>> GetAllBalancesAsync();
    Task<decimal> GetBalanceAsync(string currencyCode);
    Task TopUpAsync(string currencyCode, decimal amount);
    Task<bool> TradeAsync(string fromCurrency, string toCurrency, decimal amountToSell);
}

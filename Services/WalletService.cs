using EHSExchangeDashboard.Data;
using EHSExchangeDashboard.Entities;
using EHSExchangeDashboard.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace EHSExchangeDashboard.Services;

public class WalletService : IWalletService
{
    private readonly AppDbContext _dbContext;
    private readonly IExchangeService _exchangeService;
    private readonly AuthenticationStateProvider _authStateProvider;

    public WalletService(
        AppDbContext dbContext, 
        IExchangeService exchangeService,
        AuthenticationStateProvider authStateProvider)
    {
        _dbContext = dbContext;
        _exchangeService = exchangeService;
        _authStateProvider = authStateProvider;
    }

    private async Task<string?> GetCurrentUserIdAsync()
    {
        var authState = await _authStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;
        
        if (user.Identity?.IsAuthenticated != true)
            return null;

        // NameIdentifier is the auto-generated UUID from ASP.NET Identity
        return user.FindFirstValue(ClaimTypes.NameIdentifier);
    }

    public async Task<List<WalletBalance>> GetAllBalancesAsync()
    {
        var userId = await GetCurrentUserIdAsync();
        if (string.IsNullOrEmpty(userId)) return new List<WalletBalance>();
        
        return await _dbContext.WalletBalances.Where(b => b.UserId == userId).ToListAsync();
    }

    public async Task<decimal> GetBalanceAsync(string currencyCode)
    {
        var userId = await GetCurrentUserIdAsync();
        if (string.IsNullOrEmpty(userId)) return 0m;

        var balance = await _dbContext.WalletBalances.FindAsync(userId, currencyCode);
        return balance?.Amount ?? 0m;
    }

    public async Task TopUpAsync(string currencyCode, decimal amount)
    {
        var userId = await GetCurrentUserIdAsync();
        if (string.IsNullOrEmpty(userId)) throw new Exception("You must be logged in to deposit funds.");

        var balance = await _dbContext.WalletBalances.FindAsync(userId, currencyCode);
        if (balance == null)
        {
            balance = new WalletBalance { UserId = userId, CurrencyCode = currencyCode, Amount = 0 };
            _dbContext.WalletBalances.Add(balance);
        }

        balance.Amount += amount;
        await _dbContext.SaveChangesAsync();
    }

    public async Task<bool> TradeAsync(string fromCurrency, string toCurrency, decimal amountToSell)
    {
        var userId = await GetCurrentUserIdAsync();
        if (string.IsNullOrEmpty(userId)) return false;

        var balances = await _dbContext.WalletBalances
            .Where(b => b.UserId == userId)
            .ToDictionaryAsync(b => b.CurrencyCode, b => b);

        if (!balances.ContainsKey(fromCurrency) || balances[fromCurrency].Amount < amountToSell)
        {
            return false; // Insufficient balance
        }

        var rates = await _exchangeService.GetCachedRatesAsync();
        var fromRate = rates.FirstOrDefault(r => r.CurrencyCode == fromCurrency)?.TransformedRate ?? 1m;
        var toRate = rates.FirstOrDefault(r => r.CurrencyCode == toCurrency)?.TransformedRate ?? 1m;

        if (fromRate == 0) return false;

        decimal amountReceived = (amountToSell / fromRate) * toRate;

        balances[fromCurrency].Amount -= amountToSell;
        
        if (!balances.ContainsKey(toCurrency))
        {
            balances[toCurrency] = new WalletBalance { UserId = userId, CurrencyCode = toCurrency, Amount = 0 };
            _dbContext.WalletBalances.Add(balances[toCurrency]);
        }
        balances[toCurrency].Amount += amountReceived;

        await _dbContext.SaveChangesAsync();
        return true;
    }
}

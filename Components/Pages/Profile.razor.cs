using EHSExchangeDashboard.Common;
using EHSExchangeDashboard.Entities;
using EHSExchangeDashboard.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace EHSExchangeDashboard.Components.Pages;

public partial class Profile : ComponentBase
{
    [Inject] private IWalletService WalletService { get; set; } = default!;
    [Inject] private IExchangeService ExchangeService { get; set; } = default!;
    [Inject] private AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;

    private List<WalletBalance> balances = new();
    private List<ExchangeRate> rates = new();
    private string displayCurrency = AppConstants.DefaultCurrency;
    private decimal depositAmount = 0;
    private bool isProcessing = false;
    private bool isLoading = true;

    private decimal TotalEstimatedBalanceUsd
    {
        get
        {
            if (balances == null || !balances.Any()) return 0;
            decimal totalUsd = 0;

            foreach (var b in balances)
            {
                if (b.CurrencyCode == AppConstants.DefaultCurrency)
                {
                    totalUsd += b.Amount;
                }
                else
                {
                    // Rate is "1 USD = X Currency" (e.g. 1 USD = 0.95 EUR)
                    var rate = rates.FirstOrDefault(r => r.CurrencyCode == b.CurrencyCode);
                    if (rate != null && rate.TransformedRate != 0)
                    {
                        // To get USD: Amount / Rate (e.g. 95 EUR / 0.95 = 100 USD)
                        totalUsd += b.Amount / rate.TransformedRate;
                    }
                }
            }
            return totalUsd;
        }
    }

    private decimal DisplayBalance
    {
        get
        {
            var totalUsd = TotalEstimatedBalanceUsd;
            if (displayCurrency == AppConstants.DefaultCurrency) return totalUsd;

            // Rate is "1 USD = X Currency" (e.g. 1 USD = 10.50 SEK)
            var rate = rates.FirstOrDefault(r => r.CurrencyCode == displayCurrency);
            // To get Target Currency: totalUsd * Rate (e.g. 10,000 USD * 10.50 = 105,000 SEK)
            return (rate != null && rate.TransformedRate != 0) ? totalUsd * rate.TransformedRate : 0;
        }
    }

    protected override async Task OnInitializedAsync()
    {
        rates = await ExchangeService.GetCachedRatesAsync();
        await LoadWalletAsync();
        isLoading = false;
    }

    private async Task LoadWalletAsync()
    {
        balances = await WalletService.GetAllBalancesAsync();
    }

    private async Task HandleDeposit()
    {
        if (depositAmount <= 0) return;

        isProcessing = true;
        try 
        {
            await WalletService.TopUpAsync(AppConstants.DefaultCurrency, depositAmount);
            await LoadWalletAsync();
            depositAmount = 0;
        }
        finally
        {
            isProcessing = false;
        }
    }
}

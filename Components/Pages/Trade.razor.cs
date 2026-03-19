using EHSExchangeDashboard.Common;
using EHSExchangeDashboard.Entities;
using EHSExchangeDashboard.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Timers;

namespace EHSExchangeDashboard.Components.Pages;

public partial class Trade : ComponentBase, IDisposable
{
    [Inject] private IExchangeService ExchangeService { get; set; } = default!;
    [Inject] private IWalletService WalletService { get; set; } = default!;
    [Inject] private AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;

    private List<ExchangeRate> rates = new();
    
    private string _fromCurrency = AppConstants.DefaultCurrency;
    private string FromCurrency 
    { 
        get => _fromCurrency; 
        set { if (_fromCurrency != value) { _fromCurrency = value; OnTradeParametersChanged(); } } 
    }

    private string _toCurrency = "EUR";
    private string ToCurrency 
    { 
        get => _toCurrency; 
        set { if (_toCurrency != value) { _toCurrency = value; OnTradeParametersChanged(); } } 
    }

    private decimal _amountToSell = 0;
    private decimal AmountToSell 
    { 
        get => _amountToSell; 
        set { if (_amountToSell != value) { _amountToSell = value; OnTradeParametersChanged(); } } 
    }

    private decimal FromBalance = 0;
    private decimal ToBalance = 0;
    private List<string> OwnedCurrencies = new();
    private bool IsTrading = false;
    private bool IsCalculating = false;
    private string? ErrorMessage;
    private string? SuccessMessage;

    private System.Timers.Timer? _debounceTimer;

    private decimal EstimatedReceive { get; set; }
    private decimal CurrentRate { get; set; }

    protected override async Task OnInitializedAsync()
    {
        rates = await ExchangeService.GetCachedRatesAsync();
        await LoadBalancesAsync();
        CalculateConversion();
    }

    private void OnTradeParametersChanged()
    {
        IsCalculating = true;
        _debounceTimer?.Stop();
        _debounceTimer?.Dispose();

        _debounceTimer = new System.Timers.Timer(500);
        _debounceTimer.Elapsed += async (sender, e) => await HandleTimerElapsed();
        _debounceTimer.AutoReset = false;
        _debounceTimer.Start();
        
        StateHasChanged();
    }

    private async Task HandleTimerElapsed()
    {
        await InvokeAsync(async () =>
        {
            CalculateConversion();
            IsCalculating = false;
            StateHasChanged();
        });
    }

    private void CalculateConversion()
    {
        if (AmountToSell <= 0) 
        {
            EstimatedReceive = 0;
            // Still calculate the rate for preview
            UpdateRatePreview();
            return;
        }

        if (FromCurrency == ToCurrency)
        {
            EstimatedReceive = AmountToSell;
            CurrentRate = 1.0m;
            return;
        }

        decimal fromRateVal = FromCurrency == AppConstants.DefaultCurrency 
            ? 1.0m 
            : rates.FirstOrDefault(r => r.CurrencyCode == FromCurrency)?.TransformedRate ?? 0;

        decimal toRateVal = ToCurrency == AppConstants.DefaultCurrency 
            ? 1.0m 
            : rates.FirstOrDefault(r => r.CurrencyCode == ToCurrency)?.TransformedRate ?? 0;

        if (fromRateVal == 0 || toRateVal == 0)
        {
            EstimatedReceive = 0;
            CurrentRate = 0;
            return;
        }

        decimal amountInUsd = AmountToSell / fromRateVal;
        EstimatedReceive = amountInUsd * toRateVal;
        CurrentRate = toRateVal / fromRateVal;
    }

    private void UpdateRatePreview()
    {
        if (FromCurrency == ToCurrency)
        {
            CurrentRate = 1.0m;
            return;
        }

        decimal fromRateVal = FromCurrency == AppConstants.DefaultCurrency 
            ? 1.0m 
            : rates.FirstOrDefault(r => r.CurrencyCode == FromCurrency)?.TransformedRate ?? 0;

        decimal toRateVal = ToCurrency == AppConstants.DefaultCurrency 
            ? 1.0m 
            : rates.FirstOrDefault(r => r.CurrencyCode == ToCurrency)?.TransformedRate ?? 0;

        if (fromRateVal > 0)
        {
            CurrentRate = toRateVal / fromRateVal;
        }
        else
        {
            CurrentRate = 0;
        }
    }

    private async Task LoadBalancesAsync()
    {
        var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;

        if (user.Identity?.IsAuthenticated == true)
        {
            var balances = await WalletService.GetAllBalancesAsync();
            OwnedCurrencies = balances.Select(b => b.CurrencyCode).Distinct().ToList();
            
            if (!OwnedCurrencies.Contains(AppConstants.DefaultCurrency))
            {
                OwnedCurrencies.Add(AppConstants.DefaultCurrency);
            }

            FromBalance = balances.FirstOrDefault(b => b.CurrencyCode == FromCurrency)?.Amount ?? 0;
            ToBalance = balances.FirstOrDefault(b => b.CurrencyCode == ToCurrency)?.Amount ?? 0;
        }
    }

    private async Task ExecuteTrade()
    {
        ErrorMessage = null;
        SuccessMessage = null;

        if (FromCurrency == ToCurrency)
        {
            ErrorMessage = "Cannot trade same currency.";
            return;
        }

        if (AmountToSell <= 0)
        {
            ErrorMessage = AppConstants.MsgInvalidAmount;
            return;
        }

        if (AmountToSell > FromBalance)
        {
            ErrorMessage = $"{AppConstants.MsgInsufficientBalance} {FromBalance:N4} {FromCurrency}.";
            return;
        }

        IsTrading = true;
        try
        {
            var expectedReceive = EstimatedReceive;
            var success = await WalletService.TradeAsync(FromCurrency, ToCurrency, AmountToSell);

            if (success)
            {
                SuccessMessage = $"{AppConstants.MsgTradeSuccess} {expectedReceive:N4} {ToCurrency}.";
                _amountToSell = 0; // Bypass property to avoid double recalculation
                await LoadBalancesAsync();
                CalculateConversion();
            }
            else
            {
                ErrorMessage = AppConstants.MsgTradeFailed;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsTrading = false;
        }
    }

    public void Dispose()
    {
        _debounceTimer?.Dispose();
    }
}

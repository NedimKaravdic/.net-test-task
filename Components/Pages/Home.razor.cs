using EHSExchangeDashboard.Common;
using EHSExchangeDashboard.Entities;
using EHSExchangeDashboard.Interfaces;
using EHSExchangeDashboard.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace EHSExchangeDashboard.Components.Pages;

public partial class Home : ComponentBase
{
    [Inject] private IExchangeService ExchangeService { get; set; } = default!;
    [Inject] private IJSRuntime JS { get; set; } = default!;
    [Inject] private ILogger<Home> _logger { get; set; } = default!;

    private List<ExchangeRate> latestRates = new();
    private bool isSyncing = false;
    private bool _isRendered = false;
    private string SearchTerm = "";
    
    private string _selectedCurrency = "EUR";
    private string selectedCurrency
    {
        get => _selectedCurrency;
        set
        {
            if (_selectedCurrency != value)
            {
                _selectedCurrency = value;
                _ = UpdateChartAsync();
            }
        }
    }
    
    private string selectedRange = "1D";
    private decimal converterAmount = 100m;

    private IEnumerable<ExchangeRate> FilteredRates => string.IsNullOrWhiteSpace(SearchTerm)
        ? latestRates
        : latestRates.Where(r => 
            (r.CurrencyCode?.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) == true) || 
            (r.BaseCurrency?.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) == true));

    private decimal ConvertedValue
    {
        get
        {
            var rate = latestRates.FirstOrDefault(r => r.CurrencyCode == selectedCurrency);
            if (rate == null || rate.TransformedRate == 0) return 0;
            return converterAmount / rate.TransformedRate;
        }
    }

    protected override async Task OnInitializedAsync()
    {
        latestRates = await ExchangeService.GetCachedRatesAsync();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _isRendered = true;
            await UpdateChartAsync();
        }
    }

    private async Task LoadRatesAsync()
    {
        latestRates = await ExchangeService.GetCachedRatesAsync();
        if (_isRendered) await UpdateChartAsync();
    }

    private async Task UpdateChartAsync()
    {
        // Prevent JS interop calls during static prerendering
        if (!_isRendered) return;

        try
        {
            var rate = latestRates.FirstOrDefault(r => r.CurrencyCode == selectedCurrency);
            if (rate == null) return;

            int points = selectedRange switch { "1W" => 7, "1M" => 30, _ => 24 };
            string unit = selectedRange switch { "1W" => "Day ", "1M" => "Day ", _ => ":00" };

            var labels = Enumerable.Range(1, points).Select(p => selectedRange == "1D" ? $"{p-1}{unit}" : $"{unit}{p}").ToArray();
            var basePrice = (double)rate.TransformedRate;
            var data = new double[points];
            
            int seed = selectedCurrency.GetHashCode() + selectedRange.GetHashCode();
            var random = new Random(seed);
            
            double currentPrice = basePrice;
            for (int i = 0; i < points; i++)
            {
                data[i] = currentPrice;
                currentPrice += (random.NextDouble() - 0.5) * (basePrice * 0.02);
            }

            await JS.InvokeVoidAsync("renderChart", "marketChartCanvas", labels, data, selectedCurrency);
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Chart Update failed: {Message}", ex.Message);
        }
    }

    private async Task ChangeRange(string range)
    {
        selectedRange = range;
        await UpdateChartAsync();
    }

    private async Task SyncMarkets()
    {
        isSyncing = true;
        await JS.InvokeAsync<int>("performAjaxSync");
        await LoadRatesAsync();
        isSyncing = false;
    }
}

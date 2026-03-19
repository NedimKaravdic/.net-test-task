using System;

namespace EHSExchangeDashboard.Entities;

public class ExchangeRate
{
    public int Id { get; set; }
    public required string CurrencyCode { get; set; }
    public string? BaseCurrency { get; set; }
    public string? ProviderDate { get; set; }
    public decimal RawRate { get; set; }
    public decimal TransformedRate { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string? VolatilityTag { get; set; }
}

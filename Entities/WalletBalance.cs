using System.ComponentModel.DataAnnotations;

namespace EHSExchangeDashboard.Entities;

public class WalletBalance
{
    public string UserId { get; set; } = string.Empty;
    public string CurrencyCode { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

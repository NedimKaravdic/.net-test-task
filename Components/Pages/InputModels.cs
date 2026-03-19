using System.ComponentModel.DataAnnotations;
using EHSExchangeDashboard.Common;

namespace EHSExchangeDashboard.Components.Pages;

public class LoginInputModel
{
    [Required]
    [EmailAddress]
    [Display(Name = AppConstants.LabelEmail)]
    public string Email { get; set; } = "";

    [Required]
    [DataType(DataType.Password)]
    [Display(Name = AppConstants.LabelPassword)]
    public string Password { get; set; } = "";

    [Display(Name = AppConstants.LabelRememberMe)]
    public bool RememberMe { get; set; }
}

public class RegisterInputModel
{
    [Required]
    [EmailAddress]
    [Display(Name = AppConstants.LabelEmail)]
    public string Email { get; set; } = "";

    [Required]
    [StringLength(100, MinimumLength = 6)]
    [DataType(DataType.Password)]
    [Display(Name = AppConstants.LabelPassword)]
    public string Password { get; set; } = "";

    [DataType(DataType.Password)]
    [Display(Name = AppConstants.LabelConfirmPassword)]
    [Compare("Password")]
    public string ConfirmPassword { get; set; } = "";
}

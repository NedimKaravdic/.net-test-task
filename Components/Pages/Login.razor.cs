using EHSExchangeDashboard.Common;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;

using Microsoft.AspNetCore.Components.Routing;

namespace EHSExchangeDashboard.Components.Pages;

[ExcludeFromInteractiveRouting]
public partial class Login : ComponentBase
{
    [Inject] public SignInManager<IdentityUser> SignInManager { get; set; } = default!;
    [Inject] public ILogger<Login> Logger { get; set; } = default!;
    [Inject] public NavigationManager NavigationManager { get; set; } = default!;

    [SupplyParameterFromForm]
    public LoginInputModel Input { get; set; } = default!;

    public string? errorMessage;

    [CascadingParameter]
    public HttpContext? HttpContext { get; set; }

    protected override async Task OnInitializedAsync()
    {
        Input ??= new LoginInputModel();

        if (HttpContext?.Request != null && HttpMethods.IsGet(HttpContext.Request.Method))
        {
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
        }
    }

    public async Task LoginUser()
    {
        var result = await SignInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: false);
        if (result.Succeeded)
        {
            Logger.LogInformation("User logged in.");
            NavigationManager.NavigateTo("/", true);
        }
        else
        {
            errorMessage = "Error: Invalid login attempt.";
        }
    }
}

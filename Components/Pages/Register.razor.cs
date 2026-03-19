using EHSExchangeDashboard.Common;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;

using Microsoft.AspNetCore.Components.Routing;

namespace EHSExchangeDashboard.Components.Pages;

[ExcludeFromInteractiveRouting]
public partial class Register : ComponentBase
{
    [Inject] public UserManager<IdentityUser> UserManager { get; set; } = default!;
    [Inject] public SignInManager<IdentityUser> SignInManager { get; set; } = default!;
    [Inject] public ILogger<Register> Logger { get; set; } = default!;
    [Inject] public NavigationManager NavigationManager { get; set; } = default!;

    [SupplyParameterFromForm]
    public RegisterInputModel Input { get; set; } = default!;

    protected override void OnInitialized()
    {
        Input ??= new RegisterInputModel();
    }

    public IEnumerable<IdentityError>? identityErrors;
    public string? errorMessage;

    public async Task RegisterUser()
    {
        var user = new IdentityUser { UserName = Input.Email, Email = Input.Email };
        var result = await UserManager.CreateAsync(user, Input.Password);

        if (result.Succeeded)
        {
            Logger.LogInformation("User created a new account with password.");
            await SignInManager.SignInAsync(user, isPersistent: false);
            NavigationManager.NavigateTo("/");
        }
        else
        {
            identityErrors = result.Errors;
            errorMessage = "Error: Failed to create account.";
        }
    }
}

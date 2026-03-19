using EHSExchangeDashboard.Data;
using EHSExchangeDashboard.Services;
using EHSExchangeDashboard.Interfaces;
using EHSExchangeDashboard.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using EHSExchangeDashboard.Components;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddCascadingAuthenticationState();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 31))));

builder.Services.AddHttpClient();
builder.Services.AddScoped<IExchangeService, ExchangeService>();
builder.Services.AddScoped<IWalletService, WalletService>();

builder.Services.AddHostedService<RatePollingService>();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    })
    .AddIdentityCookies();

builder.Services.AddAuthorization();

builder.Services.AddIdentityCore<IdentityUser>(options => 
    {
        options.SignIn.RequireConfirmedAccount = false;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireLowercase = false;
        options.Password.RequireDigit = false;
        options.Password.RequiredLength = 6;
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379";
    options.InstanceName = "ExchangePro_";
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    var maxRetries = 10;
    
    for (int i = 0; i < maxRetries; i++)
    {
        try
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            logger.LogInformation("Applying database migrations (attempt {Attempt}/{Max})...", i + 1, maxRetries);
            await dbContext.Database.MigrateAsync();
            logger.LogInformation("Database migrations applied successfully.");
            break;
        }
        catch (Exception ex) when (i < maxRetries - 1)
        {
            logger.LogWarning(ex, "Database not ready (attempt {Attempt}/{Max}). Retrying in 5s...", i + 1, maxRetries);
            await Task.Delay(5000);
        }
    }
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

// Ensure Logout works reliably via POST
app.MapPost("/api/logout", async (SignInManager<IdentityUser> signInManager) =>
{
    await signInManager.SignOutAsync();
    return Results.Redirect("/login");
});

app.MapPost("/api/sync", async (IExchangeService exchangeService) =>
{
    try
    {
        await exchangeService.SyncRatesAsync();
        return Results.Ok(new { Message = "Rates synced successfully" });
    }
    catch (Exception ex)
    {
        return Results.Problem(detail: ex.Message, title: "Failed to sync rates");
    }
});

app.MapGet("/api/rates", async (IExchangeService exchangeService) => 
{
    var rates = await exchangeService.GetCachedRatesAsync();
    return Results.Ok(rates);
});

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

namespace EHSExchangeDashboard.Common;

public static class AppConstants
{
    // Branding
    public const string AppName = "Berg Exchange";
    public const string AppTagline = "Real-time currency exchange & portfolio simulation";

    // Financial Constants
    public const decimal ServiceFeeMargin = 0.015m;
    public const string ServiceFeeDisplay = "1.5%";
    public const string DefaultCurrency = "USD";

    // System Config
    public const int CacheExpirationMinutes = 15;
    public const int DefaultPollingInterval = 60;
    
    // Page Header Titles & Subtitles
    public const string DashboardTitle = "Market Overview";
    public const string PortfolioTitle = "Portfolio & Wallet";
    public const string PortfolioSubTitle = "Manage your simulated assets and account balance.";
    public const string TradeTitle = "Swap Assets";
    public const string TradeSubTitle = "Instantly trade your simulated assets at real market rates.";

    // Login & Register Pages
    public const string LoginTitle = "Welcome Back";
    public const string LoginSubTitle = "Login to manage your portfolio and trades.";
    public const string RegisterTitle = "Create Account";
    public const string RegisterSubTitle = "Join Berg Exchange and start trading with virtual assets.";

    // UI Labels & Buttons
    public const string LabelEmail = "Email Address";
    public const string LabelPassword = "Password";
    public const string LabelConfirmPassword = "Confirm Password";
    public const string LabelRememberMe = "Remember me";
    public const string LabelReturnToLogin = "Return to Login";
    public const string LinkRegister = "Don't have an account? Register";
    public const string LinkLogin = "Already have an account? Login";

    public const string LabelEstimatedBalance = "Estimated Total Balance";
    public const string LabelDisplayIn = "Display In";
    public const string LabelYourAssets = "Your Assets";
    public const string LabelCurrentPrice = "Current Price";
    public const string LabelSell = "Sell";
    public const string LabelBuy = "Buy";
    public const string LabelBalance = "Balance";
    public const string LabelRate = "Rate";
    public const string LabelMarketLeaders = "Market Leaders";
    public const string LabelAllMarkets = "All Markets";
    public const string LabelSystemMargin = "System Margin";
    public const string LabelApplied = "Applied";
    public const string LabelProviderDate = "Provider Date";
    public const string LabelMinutes = "minutes";
    public const string LabelInstantConverter = "Instant Converter";
    public const string LabelNextRefresh = "Next Refresh";
    public const string LabelActiveAssets = "Active Assets";
    public const string LabelTracked = "Tracked";
    public const string LabelHighest = "Highest";
    public const string LabelMarketAsset = "Market Asset";
    public const string LabelResult = "Result";

    public const string BtnDeposit = "Deposit";
    public const string BtnTrade = "Trade";
    public const string BtnConfirmTrade = "Confirm Trade";
    public const string BtnExecuting = "Executing...";
    public const string BtnSyncMarkets = "Sync Markets";
    public const string BtnSyncing = "Processing Sync...";
    public const string BtnLogout = "Logout";
    public const string BtnLogin = "Login";
    public const string BtnRegister = "Register";
    public const string BtnHome = "Home";
    public const string BtnDashboard = "Dashboard";
    public const string BtnCancel = "Cancel";

    // Authentication Messages
    public const string AuthLoggingIn = "Logging in...";
    public const string AuthCreating = "Creating account...";

    // Table Headers
    public const string HeaderAsset = "Asset";
    public const string HeaderBalance = "Balance";
    public const string HeaderPrice = "Price";
    public const string HeaderValue = "Value";
    public const string HeaderAction = "Action";
    public const string HeaderSymbol = "Symbol";
    public const string HeaderBase = "Base";
    public const string HeaderQuote = "Raw Quote";
    public const string HeaderVolatility = "Volatility";
    public const string HeaderUpdated = "Updated";

    // Messages & Placeholders
    public const string PlaceholderAmount = "Amount";
    public const string PlaceholderSearch = "Search assets...";
    public const string PlaceholderEmail = "name@company.com";
    public const string PlaceholderPassword = "Enter your password";
    public const string PlaceholderConfirmPassword = "Confirm your password";
    public const string MsgEmptyWallet = "Your wallet is empty. Deposit some USD to start trading!";
    public const string MsgNoAssetsFound = "No assets found.";
    public const string MsgTradeSuccess = "Trade executed! You received";
    public const string MsgTradeFailed = "Trade failed due to a processing error or insufficient balance.";
    public const string MsgInvalidAmount = "Enter an amount greater than zero.";
    public const string MsgInsufficientBalance = "Insufficient balance. You only have";

    // Supported International Currencies (ISO 4217)
    public static readonly string[] SupportedCurrencies = new[]
    {
        "USD", "EUR", "GBP", "JPY", "CHF", "CAD", "AUD", "NZD", 
        "CNY", "HKD", "SGD", "INR", "MXN", "BRL", "ZAR", "TRY", 
        "AED", "SEK", "NOK", "DKK", "PLN", "CZK", "HUF", "ILS"
    };
}

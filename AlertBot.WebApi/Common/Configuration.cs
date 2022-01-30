namespace AlertBot.WebApi.Common;

internal class Configuration
{
    public Configuration()
    {
        TelegramApiKey = Environment.GetEnvironmentVariable("TELEGRAM_API_KEY") ?? throw new ArgumentNullException(nameof(TelegramApiKey));
        BinanceApiKey = Environment.GetEnvironmentVariable("BINANCE_API_KEY") ?? throw new ArgumentNullException(nameof(BinanceApiKey));
        BinanceApiSecret = Environment.GetEnvironmentVariable("BINANCE_API_SECRET") ?? throw new ArgumentNullException(nameof(BinanceApiSecret));
        FirebaseProjectId = Environment.GetEnvironmentVariable("FIREBASE_PROJECT_ID") ?? throw new ArgumentNullException(nameof(FirebaseProjectId));
    }

    public string BinanceApiSecret { get; }

    public string BinanceApiKey { get; }

    public string TelegramApiKey { get; }
    public string FirebaseProjectId { get; }
}
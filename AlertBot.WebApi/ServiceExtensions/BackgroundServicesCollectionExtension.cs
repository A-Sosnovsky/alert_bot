using AlertBot.WebApi.BackgroundServices;

namespace AlertBot.WebApi.ServiceExtensions;

internal static class BackgroundServicesCollectionExtension
{
    public static void AddBackgroundServices(this IServiceCollection services)
    {
        services.AddHostedService<TelegramBotPollingService>();
        services.AddHostedService<BinanceListenService>();
    }
}
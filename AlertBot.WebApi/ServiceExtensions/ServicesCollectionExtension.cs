using AlertBot.WebApi.Common;
using Binance.Net;
using Binance.Net.Interfaces;
using Binance.Net.Objects;
using CryptoExchange.Net.Authentication;
using Telegram.Bot;

namespace AlertBot.WebApi.ServiceExtensions;

internal static class ServicesCollectionExtension
{
    public static void AddServices(this IServiceCollection services)
    {
        var configuration = new Configuration();

        services.AddSingleton(_ => configuration);
        services.AddSingleton<ITelegramBotClient>(_ => new TelegramBotClient(configuration.TelegramApiKey));
        services.AddSingleton<IBinanceClient>(_ => new BinanceClient(new BinanceClientOptions
        {
            ApiCredentials = new ApiCredentials(configuration.BinanceApiKey, configuration.BinanceApiSecret)
        }));
        services.AddSingleton<IBinanceSocketClient, BinanceSocketClient>();
    }
}
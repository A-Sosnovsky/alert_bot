using Telegram.Bot;
using Telegram.Bot.Types;

namespace AlertBot.WebApi.BackgroundServices;

internal class TelegramBotPollingService : BackgroundService
{
    private readonly ITelegramBotClient _telegramBotClient;

    public TelegramBotPollingService(ITelegramBotClient telegramBotClient)
    {
        _telegramBotClient = telegramBotClient;
    }

    protected override Task ExecuteAsync(CancellationToken cancellationToken)
    {
        _telegramBotClient.StartReceiving(HandleUpdateAsync, HandleErrorAsync, null, cancellationToken);
        return Task.CompletedTask;
    }

    private Task HandleErrorAsync(ITelegramBotClient client, Exception exception, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private async Task HandleUpdateAsync(ITelegramBotClient client, Update update, CancellationToken cancellationToken)
    {
        await client.SendTextMessageAsync(update.Message.Chat.Id, $"ChatId = {update.Message.Chat.Id}", cancellationToken: cancellationToken);
    }
}
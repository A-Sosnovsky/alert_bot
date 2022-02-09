using AlertBot.WebApi.Models;
using Binance.Net.Interfaces;
using Binance.Net.Objects;
using Binance.Net.Objects.Futures.UserStream;
using CryptoExchange.Net.Sockets;
using Telegram.Bot;

namespace AlertBot.WebApi.BackgroundServices;

internal class BinanceListenService : BackgroundService
{
    private readonly ILogger<BinanceListenService> _logger;
    private readonly IBinanceSocketClient _binanceSocketClient;
    private readonly IBinanceClient _binanceClient;
    private readonly TelegramBotClient _telegramBotClient;
    private bool _subscribeRequired = true;
    private readonly long? _chatId;
    
    public BinanceListenService(ILogger<BinanceListenService> logger,
        IBinanceSocketClient binanceSocketClient, 
        IBinanceClient binanceClient,
        TelegramBotClient telegramBotClient,
        UserConfiguration configuration)
    {
        _logger = logger;
        _binanceSocketClient = binanceSocketClient;
        _binanceClient = binanceClient;
        _telegramBotClient = telegramBotClient;
        _chatId = configuration.ChatId;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await UpdateSubscriptions(stoppingToken);
            }
            catch (Exception exception) when (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogError(exception, "UpdateSubscriptions error");
            }

            if (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                }
                catch
                {
                    // Ignore cancellation exceptions, since cancellation is handled by the outer loop.
                }
            }
        }
    }

    private async Task UpdateSubscriptions(CancellationToken cancellationToken)
    {
        if(!_subscribeRequired) return;
            
        var key = await _binanceClient.FuturesUsdt.UserStream.StartUserStreamAsync(cancellationToken);

        var callResult = await _binanceSocketClient.FuturesUsdt.SubscribeToUserDataUpdatesAsync(key.Data,
            OnLeverageUpdate, OnMarginUpdate, OnAccountUpdate, OnOrderUpdate, OnListenKeyExpired);

        if (callResult.Success)
        {
            KeepAliveUserStream(key.Data, cancellationToken);
            _subscribeRequired = false;
        }
        else
        {
            _logger.LogCritical("Can't subscribe to user data update");
        }
    }

    private void OnOrderUpdate(DataEvent<BinanceFuturesStreamOrderUpdate> dataEvent)
    {
        switch (dataEvent.Data.Event)
        {
                case "ORDER_TRADE_UPDATE":
                    Task.Run(() => SendOrderChangeInfo(dataEvent.Data));
                break;
            default:
                break;
        }
    }

    private async Task SendOrderChangeInfo(BinanceFuturesStreamOrderUpdate dataEventData)
    {
        if (_chatId == null)
        {
            _logger.LogWarning("Can't send order update. ChatId is null");
            return;
        }

        var message = $@"Symbol: {dataEventData.UpdateData.Symbol}. 
Side: {dataEventData.UpdateData.Side.ToString()}
OrderType: {dataEventData.UpdateData.OriginalType.ToString()}
PositionSide: {dataEventData.UpdateData.PositionSide.ToString()}
ExecutionType: {dataEventData.UpdateData.ExecutionType.ToString()}
Price: {dataEventData.UpdateData.Price}
RealizedProfit: {dataEventData.UpdateData.RealizedProfit}
";
        await _telegramBotClient.SendTextMessageAsync(_chatId.Value, message);
    }

    private void KeepAliveUserStream(string key, CancellationToken cancellationToken)
    {
        Task.Run(async () =>
        {
            while (true)
            {
                try
                {
                    await _binanceClient.FuturesUsdt.UserStream.KeepAliveUserStreamAsync(key, cancellationToken);
                    await Task.Delay(TimeSpan.FromMinutes(30), cancellationToken);
                }
                catch (TaskCanceledException)
                {
                    return;
                }
            }
        }, cancellationToken);

    }

    private void OnAccountUpdate(DataEvent<BinanceFuturesStreamAccountUpdate> dataEvent)
    {
        
    }

    private void OnMarginUpdate(DataEvent<BinanceFuturesStreamMarginUpdate> dataEvent)
    {
        
    }

    private void OnListenKeyExpired(DataEvent<BinanceStreamEvent> dataEvent)
    {
        _subscribeRequired = true;
    }

    private void OnLeverageUpdate(DataEvent<BinanceFuturesStreamConfigUpdate> dataEvent)
    {
        
    }

    public override void Dispose()
    {
        _binanceClient.Dispose();
        _binanceSocketClient.Dispose();
        base.Dispose();
    }
}
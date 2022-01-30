using Binance.Net.Interfaces;
using Binance.Net.Objects;
using Binance.Net.Objects.Futures.UserStream;
using CryptoExchange.Net.Sockets;

namespace AlertBot.WebApi.BackgroundServices;

internal class BinanceListenService : BackgroundService
{
    private readonly ILogger<BinanceListenService> _logger;
    private readonly IBinanceSocketClient _binanceSocketClient;
    private readonly IBinanceClient _binanceClient;
    private bool _subscribeRequired = true;
    
    public BinanceListenService(ILogger<BinanceListenService> logger,
        IBinanceSocketClient binanceSocketClient, 
        IBinanceClient binanceClient)
    {
        _logger = logger;
        _binanceSocketClient = binanceSocketClient;
        _binanceClient = binanceClient;
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
                break;
            default:
                break;
        }
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
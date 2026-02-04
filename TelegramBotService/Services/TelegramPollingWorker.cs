using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TelegramBotService.Contracts;
using TelegramBotService.Models;
using TelegramBotService.Services;

public sealed class TelegramPollingWorker : BackgroundService
{
    private readonly TelegramClient _tg;
    private readonly IUpdateHandler _handler;
    private readonly ITelegramEffectApplier _applier;
    private readonly ILogger<TelegramPollingWorker> _log;

    private long? _offset;

    public TelegramPollingWorker(
        TelegramClient tg,
        IUpdateHandler handler,
        ITelegramEffectApplier applier,
        ILogger<TelegramPollingWorker> log)
    {
        _tg = tg;
        _handler = handler;
        _applier = applier;
        _log = log;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _log.LogInformation("Telegram polling started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var updates = await _tg.GetUpdatesAsync(_offset, timeoutSeconds: 50, stoppingToken);

                if (!updates.Ok || updates.Result.Count == 0)
                    continue;

                foreach (var u in updates.Result)
                {
                    _offset = u.UpdateId + 1;

                    AppUpdate? appUpdate = null;

                    // Callback
                    if (u.CallbackQuery is not null)
                    {
                        var cb = u.CallbackQuery;
                        var msg = cb.Message;
                        if (msg is null) continue;

                        appUpdate = new AppCallback(
                            ChatId: msg.Chat.Id,
                            UserId: cb.From.Id,
                            MessageId: msg.MessageId,
                            CallbackId: cb.Id,
                            Data: cb.Data ?? ""
                        );
                    }
                    // Text message
                    else if (u.Message?.Text is not null)
                    {
                        appUpdate = new AppText(u.Message.Chat.Id, UserId: u.Message.From.Id, u.Message.Text);
                    }

                    if (appUpdate is null) continue;

                    var effects = await _handler.HandleAsync(appUpdate, stoppingToken);

                    foreach (var ef in effects)
                        await _applier.ApplyAsync(ef, stoppingToken);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                _log.LogInformation("Telegram polling stopping (cancellation requested).");
                break;
            }
            catch (TelegramTransportException ex)
            {
                _log.LogWarning(ex, "Telegram transport error. Will retry shortly.");
                await SafeDelay(2000, stoppingToken);
            }
            catch (TelegramApiException ex)
            {
                _log.LogError("Telegram API error. StatusCode={StatusCode} Body={Body}", ex.StatusCode, ex.TelegramResponse);
                var delayMs = ex.StatusCode == 401 ? 10_000 : 2_000;
                await SafeDelay(delayMs, stoppingToken);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Polling loop unexpected error.");
                await SafeDelay(2000, stoppingToken);
            }
        }
    }

    private static async Task SafeDelay(int milliseconds, CancellationToken ct)
    {
        try { await Task.Delay(milliseconds, ct); }
        catch (OperationCanceledException) { }
    }
}
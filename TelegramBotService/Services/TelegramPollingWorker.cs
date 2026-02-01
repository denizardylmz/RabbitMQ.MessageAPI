//using Microsoft.Extensions.Hosting;
//using Microsoft.Extensions.Logging;
//using TelegramBotService.Services;

//public sealed class TelegramPollingWorker : BackgroundService
//{
//    private readonly TelegramClient _tg;
//    private readonly CommandRouter _router;
//    private readonly ILogger<TelegramPollingWorker> _log;

//    private long? _offset;

//    public TelegramPollingWorker(TelegramClient tg, CommandRouter router, ILogger<TelegramPollingWorker> log)
//    {
//        _tg = tg;
//        _router = router;
//        _log = log;
//    }

//    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
//    {
//        Console.WriteLine("Telegram polling started.");

//        while (!stoppingToken.IsCancellationRequested)
//        {
//            try
//            {
//                var updates = await _tg.GetUpdatesAsync(_offset, timeoutSeconds: 50, stoppingToken);

//                if (!updates.Ok || updates.Result.Count == 0)
//                    continue;

//                foreach (var u in updates.Result)
//                {
//                    _offset = u.UpdateId + 1;


//                    #region CallbackQuery Handler
//                    if (u.CallbackQuery is not null)
//                    {
//                        var cb = u.CallbackQuery;

//                        await _tg.AnswerCallbackAsync(cb.Id, stoppingToken, text: "Talebiniz Alındı", showAlert: true);

//                        var data = cb.Data ?? "";
//                        var msg = cb.Message;
//                        if (msg is null)
//                        {
//                            continue;
//                        }

//                        await Task.Delay(1000, stoppingToken);

//                        var chatId = msg.Chat.Id;

//                        switch (data)
//                        {
//                            case "test:cancel":
//                                await _tg.EditMessageTextAsync(chatId, msg.MessageId, "Cancel seçildi.", stoppingToken);
//                                break;

//                            case "test:ok":
//                                await _tg.EditMessageTextAsync(chatId, msg.MessageId, "Ok Seçildi ✅", stoppingToken);
//                                break;

//                            default:
//                                await _tg.SendMessageAsync(chatId, $"Bilinmeyen buton: {data}", stoppingToken);
//                                break;
//                        }

//                        continue;
//                    }
//                    #endregion

//                    #region Message Handler
//                    var msg2 = u.Message;
//                    if (msg2?.Text is null) continue;

//                    var (cmd, args) = _router.Parse(msg2.Text);

//                    switch (cmd)
//                    {
//                        case "/start":
//                            await _tg.SendMessageAsync(msg2.Chat.Id, "Merhaba. /echo <text> deneyebilirsin.", stoppingToken);
//                            break;

//                        case "/echo":
//                            await _tg.SendMessageAsync(msg2.Chat.Id, string.IsNullOrWhiteSpace(args) ? "Ne echo’layayım?" : args, stoppingToken);
//                            break;

//                        case "_text":
//                            await _tg.SendMessageAsync(msg2.Chat.Id, "Komutlar: /start, /echo <text>", stoppingToken);
//                            break;

//                        default:
//                            await _tg.SendMessageAsync(msg2.Chat.Id, "Bilinmeyen komut. /start yaz.", stoppingToken);
//                            break;
//                    }
//                    #endregion


//                }
//            }
//            catch (TaskCanceledException) when (stoppingToken.IsCancellationRequested)
//            {

//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine($"Polling loop error. EX: {ex.Message}");
//                await Task.Delay(1000, stoppingToken);
//            }
//        }
//    }
//}

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TelegramBotService.Models;
using TelegramBotService.Services;

public sealed class TelegramPollingWorker : BackgroundService
{
    private readonly TelegramClient _tg;
    private readonly CommandRouter _router;
    private readonly ILogger<TelegramPollingWorker> _log;

    private long? _offset;

    public TelegramPollingWorker(TelegramClient tg, CommandRouter router, ILogger<TelegramPollingWorker> log)
    {
        _tg = tg;
        _router = router;
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

                    // --------------------
                    // CallbackQuery Handler
                    // --------------------
                    if (u.CallbackQuery is not null)
                    {
                        var cb = u.CallbackQuery;

                        // ACK (spinner kapansın) - bunu try içinde tutuyoruz
                        await _tg.AnswerCallbackAsync(cb.Id, stoppingToken, text: "Talebiniz Alındı", showAlert: true);

                        var data = cb.Data ?? "";
                        var msg = cb.Message;
                        if (msg is null) continue;

                        var chatId = msg.Chat.Id;

                        switch (data)
                        {
                            case "test:cancel":
                                await _tg.EditMessageTextAsync(chatId, msg.MessageId, "Cancel seçildi.", stoppingToken);
                                break;

                            case "test:ok":
                                await _tg.EditMessageTextAsync(chatId, msg.MessageId, "Ok Seçildi ✅", stoppingToken);
                                break;

                            default:
                                await _tg.SendMessageAsync(chatId, $"Bilinmeyen buton: {data}", stoppingToken);
                                break;
                        }

                        continue;
                    }

                    // --------------
                    // Message Handler
                    // --------------
                    var msg2 = u.Message;
                    if (msg2?.Text is null) continue;

                    var (cmd, args) = _router.Parse(msg2.Text);

                    switch (cmd)
                    {
                        case "/start":
                            await _tg.SendMessageAsync(msg2.Chat.Id, "Merhaba. /echo <text> deneyebilirsin.", stoppingToken);
                            break;

                        case "/echo":
                            await _tg.SendMessageAsync(msg2.Chat.Id, string.IsNullOrWhiteSpace(args) ? "Ne echo’layayım?" : args, stoppingToken);
                            break;

                        case "_text":
                            await _tg.SendMessageAsync(msg2.Chat.Id, "Komutlar: /start, /echo <text>", stoppingToken);
                            break;

                        default:
                            await _tg.SendMessageAsync(msg2.Chat.Id, "Bilinmeyen komut. /start yaz.", stoppingToken);
                            break;
                    }
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


using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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
        Console.WriteLine("Telegram polling started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var updates = await _tg.GetUpdatesAsync(_offset, timeoutSeconds: 50, stoppingToken);

                if (!updates.Ok || updates.Result.Count == 0)
                    continue;

                foreach (var u in updates.Result)
                {
                    _offset = u.UpdateId + 1; // kritik

                    var msg = u.Message;
                    if (msg?.Text is null) continue;

                    var (cmd, args) = _router.Parse(msg.Text);

                    switch (cmd)
                    {
                        case "/start":
                            await _tg.SendMessageAsync(msg.Chat.Id, "Merhaba. /echo <text> deneyebilirsin.", stoppingToken);
                            break;

                        case "/echo":
                            await _tg.SendMessageAsync(msg.Chat.Id, string.IsNullOrWhiteSpace(args) ? "Ne echo’layayım?" : args, stoppingToken);
                            break;

                        case "_text":
                            await _tg.SendMessageAsync(msg.Chat.Id, "Komutlar: /start, /echo <text>", stoppingToken);
                            break;

                        default:
                            await _tg.SendMessageAsync(msg.Chat.Id, "Bilinmeyen komut. /start yaz.", stoppingToken);
                            break;
                    }
                }
            }
            catch (TaskCanceledException) when (stoppingToken.IsCancellationRequested)
            {

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Polling loop error. EX: {ex.Message}");
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}

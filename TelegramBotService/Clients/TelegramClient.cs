using System.ComponentModel.DataAnnotations;
using System.Net.Http.Json;
using System.Text.Json;
using TelegramBotService.Models;

public sealed class TelegramClient
{
    private readonly HttpClient _http;

    public TelegramClient(HttpClient http) => _http = http;

    public async Task<GetUpdatesResponse> GetUpdatesAsync( long? offset, int timeoutSeconds, CancellationToken ct)
    {
        var url = $"getUpdates?timeout={timeoutSeconds}";
        if (offset.HasValue)
            url += $"&offset={offset.Value}";

        try
        {
            using var resp = await _http.GetAsync(url, ct);
            var body = await resp.Content.ReadAsStringAsync(ct);

            if (!resp.IsSuccessStatusCode)
                throw new TelegramApiException((int)resp.StatusCode, body);

            var result = JsonSerializer.Deserialize<GetUpdatesResponse>(body);
            return result ?? new GetUpdatesResponse { Ok = false, Result = new() };
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (HttpRequestException ex)
        {
            throw new TelegramTransportException("Telegram getUpdates HTTP error", ex);
        }
    }


    public async Task SendMessageAsync(long chatId, string text, CancellationToken ct)
    {
#if DEBUG
        text += $"  // Chat Id: {chatId}";
#endif

        var message = SendMessageRequest.Create(chatId).WithText(text);

        try
        {
            using var resp = await _http.PostAsJsonAsync("sendMessage", message, ct);
            var body = await resp.Content.ReadAsStringAsync(ct);

            if (!resp.IsSuccessStatusCode)
                throw new TelegramApiException((int)resp.StatusCode, body);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (HttpRequestException ex)
        {
            throw new TelegramTransportException("Telegram HTTP error", ex);
        }
    }
    public async Task SendMessageAsync(SendMessageRequest message, CancellationToken ct)
    {
        try
        {
            var resp = await _http.PostAsJsonAsync("sendMessage", message, ct);
            var body = await resp.Content.ReadAsStringAsync(ct);

            if (!resp.IsSuccessStatusCode)
                throw new TelegramApiException((int)resp.StatusCode, body);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (HttpRequestException ex)
        {
            throw new TelegramTransportException("Telegram HTTP error", ex);
        }
    }
    public sealed record TgError(bool ok, int error_code, string description);
    public async Task AnswerCallbackAsync( string callbackQueryId, CancellationToken ct, string? text = null, bool showAlert = false)
    {
        if (string.IsNullOrWhiteSpace(callbackQueryId))
            throw new ArgumentException("callbackQueryId cannot be empty", nameof(callbackQueryId));

        var payload = new
        {
            callback_query_id = callbackQueryId,
            text = string.IsNullOrWhiteSpace(text) ? null : text,
            show_alert = showAlert ? true : (bool?)null // false ise hiç yazma
        };

        try
        {
            using var resp = await _http.PostAsJsonAsync("answerCallbackQuery", payload, ct);
            var body = await resp.Content.ReadAsStringAsync(ct);

            if (!resp.IsSuccessStatusCode)
                throw new TelegramApiException((int)resp.StatusCode, body);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (HttpRequestException ex)
        {
            throw new TelegramTransportException("Telegram answerCallbackQuery HTTP error", ex);
        }
    }
    public async Task EditMessageTextAsync(long chatId, long messageId, string text, CancellationToken ct)
    {
        if (chatId == 0) throw new ArgumentException("chatId cannot be 0", nameof(chatId));
        if (messageId == 0) throw new ArgumentException("messageId cannot be 0", nameof(messageId));
        if (string.IsNullOrWhiteSpace(text)) throw new ArgumentException("text cannot be empty", nameof(text));

        var payload = new
        {
            chat_id = chatId,
            message_id = messageId,
            text
        };

        try
        {
            using var resp = await _http.PostAsJsonAsync("editMessageText", payload, ct);
            var body = await resp.Content.ReadAsStringAsync(ct);

            if (!resp.IsSuccessStatusCode)
                throw new TelegramApiException((int)resp.StatusCode, body);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (HttpRequestException ex)
        {
            throw new TelegramTransportException("Telegram editMessageText HTTP error", ex);
        }
    }

    public async Task EditMessageTextAsync(SendMessageRequest message , CancellationToken ct)
    {
        try
        {
            using var resp = await _http.PostAsJsonAsync("editMessageText", message, ct);
            var body = await resp.Content.ReadAsStringAsync(ct);

            if (!resp.IsSuccessStatusCode)
                throw new TelegramApiException((int)resp.StatusCode, body);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (HttpRequestException ex)
        {
            throw new TelegramTransportException("Telegram editMessageText HTTP error", ex);
        }
    }


}

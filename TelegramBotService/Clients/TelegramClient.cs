using MessageAPI.Domain.TelegramDto;
using System.ComponentModel.DataAnnotations;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using TelegramBotService.Models;

public sealed class TelegramClient
{
    private readonly HttpClient _http;
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

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

    public Task SendMessageAsync(long chatId, string text, CancellationToken ct) 
            =>  SendMessageAsync(SendMessageRequest.Create(chatId).WithText(text), ct);
    public Task SendMessageAsync(long chatId, string text, InlineKeyboardMarkup? replyMarkup, CancellationToken ct)
        => SendMessageAsync(SendMessageRequest.Create(chatId).WithText(text).WithInlineKeyboard(x => replyMarkup), ct);

    public async Task SendMessageAsync(SendMessageRequest message, CancellationToken ct)
    {
        if (message.ChatId == 0) throw new ArgumentException("chatId cannot be 0", nameof(message.ChatId));
        if (string.IsNullOrWhiteSpace(message.Text)) throw new ArgumentException("text cannot be empty", nameof(message.Text));

        try
        {
            var resp = await _http.PostAsJsonAsync("sendMessage", message, ct);
            var body = await resp.Content.ReadAsStringAsync(ct);

            if (!resp.IsSuccessStatusCode)
                throw new TelegramApiException((int)resp.StatusCode, body);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
        catch (HttpRequestException ex)
        {
            throw new TelegramTransportException("Telegram sendMessage HTTP error", ex);
        }
    }

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


    public Task EditMessageTextAsync(long chatId, long messageId, string text, CancellationToken ct)
        => EditMessageTextAsync(chatId, messageId, text, replyMarkup: null, ct);

    public Task EditMessageTextAsync(long chatId, long messageId, string text, InlineKeyboardMarkup? replyMarkup, CancellationToken ct)
        => EditMessageTextAsync(new EditMessageTextRequest
        {
            ChatId = chatId,
            MessageId = messageId,
            Text = text,
            ReplyMarkup = replyMarkup
        }, ct);

    public async Task EditMessageTextAsync(EditMessageTextRequest message, CancellationToken ct)
    {
        if (message.ChatId == 0) throw new ArgumentException("chatId cannot be 0", nameof(message.ChatId));
        if (message.MessageId == 0) throw new ArgumentException("messageId cannot be 0", nameof(message.MessageId));
        if (string.IsNullOrWhiteSpace(message.Text)) throw new ArgumentException("text cannot be empty", nameof(message.Text));

        try
        {
            using var resp = await _http.PostAsJsonAsync("editMessageText", message, JsonOptions, ct);
            var body = await resp.Content.ReadAsStringAsync(ct);

            if (!resp.IsSuccessStatusCode)
                throw new TelegramApiException((int)resp.StatusCode, body);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
        catch (HttpRequestException ex)
        {
            throw new TelegramTransportException("Telegram editMessageText HTTP error", ex);
        }
    }
}



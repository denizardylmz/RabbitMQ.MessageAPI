using System.Net.Http.Json;
using TelegramBotService.Models;

public sealed class TelegramClient
{
    private readonly HttpClient _http;

    public TelegramClient(HttpClient http) => _http = http;

    public async Task<GetUpdatesResponse> GetUpdatesAsync(long? offset, int timeoutSeconds, CancellationToken ct)
    {
        var url = $"getUpdates?timeout={timeoutSeconds}";
        if (offset.HasValue) url += $"&offset={offset.Value}";

        var res = await _http.GetFromJsonAsync<GetUpdatesResponse>(url, ct);
        return res ?? new GetUpdatesResponse { Ok = false, Result = [] };
    }

    public async Task SendMessageAsync(long chatId, string text, CancellationToken ct)
    {
        var payload = new { chat_id = chatId, text };
        var resp = await _http.PostAsJsonAsync("sendMessage", payload, ct);
        resp.EnsureSuccessStatusCode();
    }
}

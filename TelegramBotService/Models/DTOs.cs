using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;
namespace TelegramBotService.Models
{


    public sealed class GetUpdatesResponse
    {
        [JsonPropertyName("ok")] public bool Ok { get; set; }
        [JsonPropertyName("result")] public List<Update> Result { get; set; } = [];
    }

    public sealed class Update
    {
        [JsonPropertyName("update_id")] public long UpdateId { get; set; }
        [JsonPropertyName("message")] public Message? Message { get; set; }
    }

    public sealed class Message
    {
        [JsonPropertyName("message_id")] public long MessageId { get; set; }
        [JsonPropertyName("chat")] public Chat Chat { get; set; } = default!;
        [JsonPropertyName("text")] public string? Text { get; set; }
    }

    public sealed class Chat
    {
        [JsonPropertyName("id")] public long Id { get; set; }
    }


}

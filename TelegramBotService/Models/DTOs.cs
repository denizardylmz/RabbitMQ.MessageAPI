using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;
namespace TelegramBotService.Models
{


    using System.Text.Json.Serialization;

    #region GetUpdates (Incoming)

    public sealed class GetUpdatesResponse
    {
        [JsonPropertyName("ok")]
        public bool Ok { get; set; }

        [JsonPropertyName("result")]
        public List<Update> Result { get; set; } = new();
    }

    public sealed class Update
    {
        [JsonPropertyName("update_id")]
        public long UpdateId { get; set; }

        // Normal text message (user typed)
        [JsonPropertyName("message")]
        public Message? Message { get; set; }

        // Inline keyboard button click
        [JsonPropertyName("callback_query")]
        public CallbackQuery? CallbackQuery { get; set; }
    }

    #endregion

    #region Core Objects (Incoming)

    public sealed class Message
    {
        [JsonPropertyName("message_id")]
        public long MessageId { get; set; }

        [JsonPropertyName("chat")]
        public Chat Chat { get; set; } = default!;

        [JsonPropertyName("from")]
        public TgUser From { get; set; } = default!;

        [JsonPropertyName("entities")]
        public List<TgEntity> Entities { get; set; } = new();

        [JsonPropertyName("date")]
        public long? Date { get; set; }

        [JsonPropertyName("text")]
        public string? Text { get; set; }

        [JsonIgnore]
        public DateTime DateUtc =>
            DateTimeOffset.FromUnixTimeSeconds(Date.Value).UtcDateTime;

    }

    public sealed class Chat
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("first_name")]
        public string? Firstname { get; set; }
        
        [JsonPropertyName("username")]
        public string? Username { get; set; }
        
        [JsonPropertyName("type")]
        public string? Type { get; set; }
    }

    public sealed class TgUser
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }
        [JsonPropertyName("is_bot")]
        public bool? IsBot { get; set; }

        [JsonPropertyName("username")]
        public string? Username { get; set; }

        [JsonPropertyName("first_name")]
        public string? FirstName { get; set; }

        [JsonPropertyName("last_name")]
        public string? LastName { get; set; }
        
        [JsonPropertyName("language_code")]
        public string? LanguageCode { get; set; }
    }

    public class TgEntity
    {
        [JsonPropertyName("offset")]
        public int Offset { get; set; }
        
        [JsonPropertyName("length")]
        public int Length { get; set; }

        [JsonPropertyName("type")]
        public string? Type { get; set; }
    }
    #endregion

    #region CallbackQuery (Incoming)

    public sealed class CallbackQuery
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("data")]
        public string? Data { get; set; }

        [JsonPropertyName("from")]
        public TgUser From { get; set; } = default!;

        [JsonPropertyName("message")]
        public Message? Message { get; set; }
    }

    #endregion

    #region SendMessage (Outgoing)

    public sealed class SendMessageRequest
    {
        [JsonPropertyName("chat_id")]
        public long ChatId { get; private set; }

        [JsonPropertyName("text")]
        public string Text { get; private set; } = string.Empty;
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("reply_markup")]
        public InlineKeyboardMarkup? ReplyMarkup { get; private set; }

        private SendMessageRequest(long chatId)
        {
            ChatId = chatId;
        }
        public static SendMessageRequest Create(long chatId) => new(chatId);

        public SendMessageRequest WithText(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                throw new ArgumentException("Message text cannot be empty");

            Text = text;
            return this;
        }

        public SendMessageRequest WithInlineKeyboard(Func<InlineKeyboardMarkup, InlineKeyboardMarkup> builder)
        {
            ReplyMarkup = builder(new InlineKeyboardMarkup());
            return this;
        }
    }


    public sealed class InlineKeyboardMarkup
    {
        [JsonPropertyName("inline_keyboard")]
        public List<List<InlineKeyboardButton>> InlineKeyboard { get; private set; } = new();

        public InlineKeyboardMarkup Row(params InlineKeyboardButton[] buttons)
        {
            if (buttons.Length == 0)
                throw new ArgumentException("At least one button is required");

            InlineKeyboard.Add(buttons.ToList());
            return this;
        }
    }

    public sealed class InlineKeyboardButton
    {
        [JsonPropertyName("text")]
        public string Text { get; private set; } = string.Empty;

        [JsonPropertyName("callback_data")]
        public string CallbackData { get; private set; } = string.Empty;

        private InlineKeyboardButton(string text, string callbackData)
        {
            Text = text;
            CallbackData = callbackData;
        }

        public static InlineKeyboardButton Create(string text, string callbackData)
            => new(text, callbackData);

        public static InlineKeyboardButton Ok(string callbackData)
            => new("✅ OK", callbackData);

        public static InlineKeyboardButton Cancel(string callbackData)
            => new("❌ Cancel", callbackData);
    }


    #endregion

}

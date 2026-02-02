using System;
using System.Collections.Generic;
using System.Text;
using TelegramBotService.Contracts;
using TelegramBotService.Models;

namespace TelegramBotService.Services
{
    public interface ITelegramEffectApplier
    {
        Task ApplyAsync(IAppEffect effect, CancellationToken ct);
    }

    public sealed class TelegramEffectApplier : ITelegramEffectApplier
    {
        private readonly TelegramClient _tg;

        public TelegramEffectApplier(TelegramClient tg)
        {
            _tg = tg;
        }

        public async Task ApplyAsync(IAppEffect effect, CancellationToken ct)
        {
            switch (effect)
            {
                case AckCallback ack:
                    await _tg.AnswerCallbackAsync(ack.CallbackId, ct, text: ack.Text, showAlert: ack.ShowAlert);
                    break;

                case SendText s:
                    await _tg.SendMessageAsync(s.ChatId, s.Text, ct);
                    break;

                case EditText e:
                    await _tg.EditMessageTextAsync(e.ChatId, e.MessageId, e.Text, ct);
                    break;

                case ShowMainMenu m:
                    await SendMenu(m.ChatId, ct);
                    break;

                default:
                    // Bilinmeyen effect’leri loglamak istersen buraya logger ekleyebilirsin
                    break;
            }
        }

        private Task SendMenu(long chatId, CancellationToken ct)
        {
            var message = SendMessageRequest.Create(chatId)
                .WithText("Merhaba")
                .WithInlineKeyboard(kb => kb
                    .Row(
                        InlineKeyboardButton.Create("Mesai Başlat", "menu:shiftIn"),
                        InlineKeyboardButton.Create("Mesai Bitir", "menu:shiftOut")
                    )
                    .Row(
                        InlineKeyboardButton.Create("Mola Başlat", "menu:breakStart"),
                        InlineKeyboardButton.Create("Mola Bitir", "menu:breakEnd")
                    )
                    .Row(
                        InlineKeyboardButton.Create("ℹ️ Yardım", "help")
                    )
                );

            return _tg.SendMessageAsync(message, ct);
        }
    }
}

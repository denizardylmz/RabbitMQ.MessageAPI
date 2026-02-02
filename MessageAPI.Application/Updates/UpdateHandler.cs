
using System;
using System.Collections.Generic;
using System.Text;
using TelegramBotService.Contracts;

namespace MessageAPI.Application.Updates
{
    public sealed class UpdateHandler : IUpdateHandler
    {
        public Task<IReadOnlyList<IAppEffect>> HandleAsync(AppUpdate update, CancellationToken ct)
        {
            return update switch
            {
                AppCallback cb => HandleCallback(cb, ct),
                AppText txt => HandleText(txt, ct),
                _ => Task.FromResult<IReadOnlyList<IAppEffect>>(Array.Empty<IAppEffect>())
            };
        }

        private Task<IReadOnlyList<IAppEffect>> HandleCallback(AppCallback cb, CancellationToken ct)
        {
            var data = cb.Data ?? "";

            var effects = new List<IAppEffect>
            {
                new AckCallback(cb.CallbackId, "Talebiniz Alındı", false)
            };

            switch (data)
            {
                case "button:cancel":
                    effects.Add(new EditText(cb.ChatId, cb.MessageId, "Cancel seçildi."));
                    effects.Add(new ShowMainMenu(cb.ChatId));
                    break;

                case "button:ok":
                    effects.Add(new EditText(cb.ChatId, cb.MessageId, "Ok Seçildi ✅"));
                    effects.Add(new ShowMainMenu(cb.ChatId));
                    break;

                case "menu:shiftIn":
                    // TODO: burada ileride ShiftInUseCase + DB yazma
                    effects.Add(new EditText(cb.ChatId, cb.MessageId, "Mesai Başladı"));
                    effects.Add(new ShowMainMenu(cb.ChatId));
                    break;

                case "menu:shiftOut":
                    effects.Add(new EditText(cb.ChatId, cb.MessageId, "Mesai Bitti"));
                    effects.Add(new ShowMainMenu(cb.ChatId));
                    break;

                case "menu:breakStart":
                    effects.Add(new EditText(cb.ChatId, cb.MessageId, "Mola Başladı"));
                    effects.Add(new ShowMainMenu(cb.ChatId));
                    break;

                case "menu:breakEnd":
                    effects.Add(new EditText(cb.ChatId, cb.MessageId, "Mola Bitti"));
                    effects.Add(new ShowMainMenu(cb.ChatId));
                    break;

                case "help":
                    effects.Add(new EditText(cb.ChatId, cb.MessageId, "Komutlar: /start, /echo <text>"));
                    effects.Add(new ShowMainMenu(cb.ChatId));
                    break;

                default:
                    effects.Add(new SendText(cb.ChatId, $"Bilinmeyen buton: {data}"));
                    break;
            }

            return Task.FromResult<IReadOnlyList<IAppEffect>>(effects);
        }

        private Task<IReadOnlyList<IAppEffect>> HandleText(AppText txt, CancellationToken ct)
        {
            var text = (txt.Text ?? "").Trim();

            if (text.StartsWith("/start", StringComparison.OrdinalIgnoreCase))
            {
                return Task.FromResult<IReadOnlyList<IAppEffect>>(new IAppEffect[]
                {
                new SendText(txt.ChatId, "Mesai Botu;"),
                new ShowMainMenu(txt.ChatId)
                });
            }

            if (text.StartsWith("/echo", StringComparison.OrdinalIgnoreCase))
            {
                var args = text.Length > 5 ? text[5..].Trim() : "";
                return Task.FromResult<IReadOnlyList<IAppEffect>>(new IAppEffect[]
                {
                new SendText(txt.ChatId, string.IsNullOrWhiteSpace(args) ? "Ne echo’layayım?" : args)
                });
            }

            // default
            return Task.FromResult<IReadOnlyList<IAppEffect>>(new IAppEffect[]
            {
            new SendText(txt.ChatId, "Komutlar: /start, /echo <text>")
            });
        }
    }
}

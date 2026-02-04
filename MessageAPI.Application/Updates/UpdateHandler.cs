
using MessageAPI.Application.Handlers;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using TelegramBotService.Contracts;
using static System.Net.Mime.MediaTypeNames;

namespace MessageAPI.Application.Updates
{
    public sealed class UpdateHandler : IUpdateHandler
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public UpdateHandler(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        public async Task<IReadOnlyList<IAppEffect>> HandleAsync(AppUpdate update, CancellationToken ct)
        {
            if (IsAnonymousAllowed(update))
            {
                return update switch
                {
                    AppCallback cb => await HandleCallback(cb, ct),
                    AppText txt => await HandleText(txt, ct),
                    _ => Array.Empty<IAppEffect>()
                };
            }

            using var scope = _scopeFactory.CreateScope();
            var resolver = scope.ServiceProvider.GetRequiredService<ResolveUserHandler>();

            var user = await resolver.Handle(new ResolveUserCommand(update.UserId), ct);

            if (user == null)
            {
                return new List<IAppEffect>{
                            new SendText(
                                update.ChatId,
                                "🚫 Yetkisiz erişim\n\n" +
                                "Bu kullanıcı henüz yetkilendirilmemiş.\n" +
                                "Lütfen kurumunuzdan onay alarak erişim talebinde bulunun.\n\n" +
                                "Giriş için: /login <PIN>"
                            )
                        };
            }

            return update switch
            {
                AppCallback cb => await HandleCallback(cb, ct),
                AppText txt => await HandleText(txt, ct),
                _ => Array.Empty<IAppEffect>()
            };
        }


        private  bool IsAnonymousAllowed(AppUpdate update)
        {
            if (update is AppText txt)
            {
                var t = (txt.Text ?? "").Trim();
                return t.StartsWith("/login", StringComparison.OrdinalIgnoreCase);
            }

            return false;
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

        private async Task<IReadOnlyList<IAppEffect>> HandleText(AppText txt, CancellationToken ct)
        {
            var text = (txt.Text ?? "").Trim();

            if (text.StartsWith("/start", StringComparison.OrdinalIgnoreCase))
            {
                return new List<IAppEffect>(new IAppEffect[]
                {
                    new SendText(txt.ChatId, "Mesai Botu;"),
                    new ShowMainMenu(txt.ChatId)
                });
            }

            if (text.StartsWith("/login ", StringComparison.OrdinalIgnoreCase))
            {
                var args = text.Length > 7 ? text[7..].Trim() : "";

                using var scope = _scopeFactory.CreateScope();
                var checkUserPinHandler = scope.ServiceProvider.GetRequiredService<CheckUserPinHandler>();

                var user = await checkUserPinHandler.Handle(new CheckUserPinCommand(args, txt.UserId), ct);

                if (user > 0)
                {
                    return new List<IAppEffect>(new IAppEffect[]
                        {
                            new SendText(txt.ChatId, "Kullanıcı Aktive Edildi.")
                        });
                }
                else
                {

                    return new List<IAppEffect>(new IAppEffect[]
                    {
                        new SendText(txt.ChatId, "Geçersiz PIN Kodu.")
                    });
                }
            }

            return new List<IAppEffect>(new IAppEffect[]
            {
                new SendText(txt.ChatId, "Komutlar: /start, /login <text>")
            });
        }
    }
}

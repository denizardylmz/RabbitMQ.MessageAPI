
using MessageAPI.Abstractions.Contracts;
using MessageAPI.Application.Handlers;
using MessageAPI.Domain.TelegramDto;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using TelegramBotService.Contracts;
using TelegramBotService.Models;
using static System.Net.Mime.MediaTypeNames;

namespace MessageAPI.Application.Updates
{
    public sealed class UpdateHandler : IUpdateHandler
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<UpdateHandler> _logger;

        public UpdateHandler(IServiceScopeFactory scopeFactory, ILogger<UpdateHandler> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
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

        private async Task<IReadOnlyList<IAppEffect>> HandleCallback(AppCallback cb, CancellationToken ct)
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
                    effects.Add(new SendText(cb.ChatId, "Mesai Botu; \n", Keyboard: Keyboards.MainMenu()));
                    break;

                case "button:ok":
                    effects.Add(new EditText(cb.ChatId, cb.MessageId, "Ok Seçildi ✅"));
                    effects.Add(new SendText(cb.ChatId, "Mesai Botu; \n", Keyboard: Keyboards.MainMenu()));
                    break;

                case "menu:breakStart":
                    {
                        try
                        {
                            using var scope = _scopeFactory.CreateScope();
                            var handler = scope.ServiceProvider.GetRequiredService<SendBreakStartHandler>();
                            await handler.Handle(new ShiftCommand(cb.UserId, DateTime.UtcNow), ct);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Break start failed");
                            effects.Add(new EditText(cb.ChatId, cb.MessageId, "Mola başlatılamadı. Tekrar dener misiniz?"));
                        }
                    }
                    break;

                case "menu:breakEnd":
                    {
                        try
                        {
                            using var scope = _scopeFactory.CreateScope();
                            var handler = scope.ServiceProvider.GetRequiredService<SendBreakEndHandler>();
                            await handler.Handle(new ShiftCommand(cb.UserId, DateTime.UtcNow), ct);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Break end failed");
                            effects.Add(new EditText(cb.ChatId, cb.MessageId, "Mola bitirilemedi. Tekrar dener misiniz?"));
                        }
                    }
                    break;

                case "menu:shiftIn":
                    {
                        try
                        {
                            using var scope = _scopeFactory.CreateScope();
                            var handler = scope.ServiceProvider.GetRequiredService<SendShiftStartHandler>();
                            await handler.Handle(new ShiftCommand(cb.UserId, DateTime.UtcNow), ct);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "shiftIn failed");
                            effects.Add(new EditText(cb.ChatId, cb.MessageId, "Mesai başlatılamadı. Tekrar dener misiniz?"));
                        }
                    }
                    break;

                case "menu:shiftOut":
                    {
                        effects.Add(new EditText(
                            cb.ChatId,
                            cb.MessageId,
                            "Mesaiyi bitirmek istediğinize emin misiniz?\n\nBu işlem çıkış saatinizi kaydeder.",
                           Keyboard: Keyboards.ConfirmShiftOut()
                        ));
                        break;
                    }

                case "confirm:shiftOut:no":
                    {
                        effects.Add(new EditText(cb.ChatId, cb.MessageId, "İşlem iptal edildi.", Keyboard: null));

                        effects.Add(new SendText(cb.ChatId, "Mesai Botu; \n", Keyboard: Keyboards.MainMenu()));
                        break;
                    }

                case "confirm:shiftOut:yes":
                    {
                        try
                        {
                            using var scope = _scopeFactory.CreateScope();
                            var handler = scope.ServiceProvider.GetRequiredService<SendShiftEndHandler>();
                            await handler.Handle(new ShiftCommand(cb.UserId, DateTime.UtcNow), ct);


                            /// Mesai bitişi sonrası, workerdan gelen messagle ile başka bi handlerda handler edilecek. 
                            // Bu aşşağıdaki yapı değişecek.
                            effects.Add(new EditText(cb.ChatId, cb.MessageId, "✅ Mesai kapatıldı.", Keyboard : null));
                            effects.Add(new SendText(cb.ChatId, "Mesai Botu; \n", Keyboard: Keyboards.MainMenu()));
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "shiftEnd failed");
                            effects.Add(new EditText(cb.ChatId, cb.MessageId, "Mesai bitirilemedi. Tekrar dener misiniz?", Keyboard: Keyboards.BackToMenuOnly()));
                        }
                        break;
                    }



                case "help":
                    effects.Add(new EditText(cb.ChatId, cb.MessageId, "Komutlar: /start, /echo <text>"));
                    effects.Add(new SendText(cb.ChatId, "Mesai Botu; \n", Keyboard: Keyboards.MainMenu()));
                    break;

                default:
                    effects.Add(new SendText(cb.ChatId, $"Bilinmeyen buton: {data}"));
                    break;
            }

            return effects;
        }

        private async Task<IReadOnlyList<IAppEffect>> HandleText(AppText txt, CancellationToken ct)
        {
            var text = (txt.Text ?? "").Trim();

            if (text.StartsWith("/start", StringComparison.OrdinalIgnoreCase))
            {
                return new List<IAppEffect>(new IAppEffect[]
                {
                    new SendText(txt.ChatId, "Mesai Botu; \n", Keyboard: Keyboards.MainMenu())
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


    public static class Keyboards
    {
        public static InlineKeyboardMarkup ConfirmShiftOut()
            => new InlineKeyboardMarkup()
                .Row(
                    InlineKeyboardButton.Ok("confirm:shiftOut:yes"),
                    InlineKeyboardButton.Cancel("confirm:shiftOut:no")
                );

        public static InlineKeyboardMarkup MainMenu()
            => new InlineKeyboardMarkup()
                .Row(
                    InlineKeyboardButton.Create("🟢 Mesai Başlat", "menu:shiftIn"),
                    InlineKeyboardButton.Create("🔴 Mesai Bitir", "menu:shiftOut")
                )
                .Row(
                    InlineKeyboardButton.Create("☕ Mola Başlat", "menu:breakStart"),
                    InlineKeyboardButton.Create("✅ Mola Bitir", "menu:breakEnd")
                )
                .Row(
                    InlineKeyboardButton.Create("❓ Yardım", "help")
                );

        public static InlineKeyboardMarkup BackToMenuOnly()
            => new InlineKeyboardMarkup()
                .Row(InlineKeyboardButton.Create("⬅️ Menüye dön", "menu:main"));


    }


}


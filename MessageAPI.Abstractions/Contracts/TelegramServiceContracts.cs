using MessageAPI.Domain.TelegramDto;
using System;
using System.Collections.Generic;
using System.Text;

namespace MessageAPI.Abstractions.Contracts
{
    public abstract record AppUpdate(long ChatId, long UserId);

    public sealed record AppCallback(
    long ChatId,
    long UserId,
    long MessageId,
    string CallbackId,
    string Data
) : AppUpdate(ChatId, UserId);
    public interface IAppEffect { }
    public interface IUpdateHandler
    {
        Task<IReadOnlyList<IAppEffect>> HandleAsync(AppUpdate update, CancellationToken ct);
    }
    public sealed record ShowMainMenu(long ChatId, long? MessageId = null) : IAppEffect;
    public sealed record SendText(long ChatId, string Text, InlineKeyboardMarkup? Keyboard = null) : IAppEffect;
    public sealed record EditText(long ChatId, long MessageId, string Text, InlineKeyboardMarkup? Keyboard = null) : IAppEffect;
    //public sealed record UiKeyboard(IReadOnlyList<UiRow> Rows);
    //public sealed record UiRow(IReadOnlyList<UiButton> Buttons);
    //public sealed record UiButton(string Text, string CallbackData);
    //public sealed record ShowMainMenu(long ChatId) : IAppEffect;
    public sealed record AckCallback(string CallbackId, string? Text = null, bool ShowAlert = false) : IAppEffect;
    public sealed record AppText(long ChatId, long UserId, string Text) : AppUpdate(ChatId, UserId);
    public sealed record TgError(bool ok, int error_code, string description);



}

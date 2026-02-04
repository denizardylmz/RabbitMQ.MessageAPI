using System;
using System.Collections.Generic;
using System.Text;

namespace TelegramBotService.Contracts
{
    public abstract record AppUpdate(long ChatId, long UserId);

    public sealed record AppText(long ChatId, long UserId, string Text) : AppUpdate(ChatId, UserId);

    public sealed record AppCallback(
        long ChatId,
        long UserId,
        long MessageId,
        string CallbackId,
        string Data
    ) : AppUpdate(ChatId, UserId);

    public interface IAppEffect { }

    public sealed record SendText(long ChatId, string Text) : IAppEffect;
    public sealed record EditText(long ChatId, long MessageId, string Text) : IAppEffect;
    public sealed record ShowMainMenu(long ChatId) : IAppEffect;
    public sealed record AckCallback(string CallbackId, string? Text = null, bool ShowAlert = false) : IAppEffect;

    public interface IUpdateHandler
    {
        Task<IReadOnlyList<IAppEffect>> HandleAsync(AppUpdate update, CancellationToken ct);
    }

}

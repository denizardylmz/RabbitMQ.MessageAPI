using System;
using System.Collections.Generic;
using System.Text;

namespace TelegramBotService.Contracts
{
    public abstract record AppUpdate(long ChatId);

    public sealed record AppText(long ChatId, string Text) : AppUpdate(ChatId);

    public sealed record AppCallback(
        long ChatId,
        long MessageId,
        string CallbackId,
        string Data
    ) : AppUpdate(ChatId);

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

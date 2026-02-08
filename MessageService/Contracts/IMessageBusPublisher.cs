using System;
using System.Collections.Generic;
using System.Text;

namespace MessageService.Contracts
{
    public interface IMessageBusPublisher
    {
        Task PublishAsync<T>(
            string exchange,
            string routingKey,
            T message,
            PublishOptions? options = null,
            CancellationToken ct = default);

        Task PublishRawAsync(
            string exchange,
            string routingKey,
            ReadOnlyMemory<byte> body,
            PublishOptions? options = null,
            CancellationToken ct = default);
    }

    public sealed record PublishOptions(
        string? ContentType = "application/json",
        string? MessageId = null,
        string? CorrelationId = null,
        IDictionary<string, object?>? Headers = null,
        bool Persistent = true,
        bool Confirm = true
    );

}

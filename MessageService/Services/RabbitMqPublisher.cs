using MessageService.Contracts;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace MessageService.Services
{
    public sealed class RabbitMqPublisher : IMessageBusPublisher, IAsyncDisposable
    {
        private readonly ConnectionFactory _factory;
        private IConnection? _connection;
        private readonly SemaphoreSlim _connLock = new(1, 1);
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

        public RabbitMqPublisher(MessageBusSettings settings)
        {
            _factory = new ConnectionFactory
            {
                HostName = settings.HostName,
                Port = settings.Port,
                UserName = settings.UserName,
                Password = settings.Password,
                AutomaticRecoveryEnabled = true,
                TopologyRecoveryEnabled = true
            };
        }

        private async Task<IConnection> GetOrCreateConnectionAsync(CancellationToken ct)
        {
            if (_connection is { IsOpen: true })
                return _connection;

            await _connLock.WaitAsync(ct);
            try
            {
                if (_connection is { IsOpen: true })
                    return _connection;

                if (_connection is not null)
                    await _connection.DisposeAsync();

                _connection = await _factory.CreateConnectionAsync(ct);
                return _connection;
            }
            finally
            {
                _connLock.Release();
            }
        }

        public async Task PublishAsync<T>(string exchange, string routingKey, T message, PublishOptions? options = null, CancellationToken ct = default)
        {
            var body = JsonSerializer.SerializeToUtf8Bytes(message, JsonOptions);
            await PublishRawAsync(exchange, routingKey, body, options with { ContentType = "application/json" }, ct);
        }

        public async Task PublishRawAsync(string exchange, string routingKey, ReadOnlyMemory<byte> body, PublishOptions? options = null, CancellationToken ct = default)
        {
            options ??= new PublishOptions();

            var connection = await GetOrCreateConnectionAsync(ct);

            var channelOptions = new CreateChannelOptions(
                publisherConfirmationsEnabled: true,
                publisherConfirmationTrackingEnabled: true
            );

            await using var channel = await connection.CreateChannelAsync(channelOptions, ct);

            await channel.ExchangeDeclareAsync(exchange: exchange,
                                               type: ExchangeType.Topic,
                                               durable: true,
                                               autoDelete: false,
                                               arguments: null,
                                               cancellationToken: ct);

            var props = new BasicProperties
            {
                ContentType = options.ContentType ?? "application/octet-stream",
                DeliveryMode = options.Persistent ? DeliveryModes.Persistent : DeliveryModes.Transient,
                MessageId = options.MessageId ?? Guid.NewGuid().ToString("N"),
                CorrelationId = options.CorrelationId,
                Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds())
            };

            if (options.Headers is not null)
                props.Headers = new Dictionary<string, object?>(options.Headers);

            await channel.BasicPublishAsync(
                exchange: exchange,
                routingKey: routingKey,
                mandatory: false,
                basicProperties: props,
                body: body.ToArray()
            );

        }

        public async ValueTask DisposeAsync()
        {
            if (_connection is not null)
                await _connection.DisposeAsync();

            _connLock.Dispose();
        }
    }
}

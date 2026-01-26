using MessageService.Contracts;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace MessageService.Services
{
    public class MessageService : IMessageService
    {

        IConnectionFactory _connectionFactory;
        IConnection _connection;
        private readonly MessageBusSettings _settings;

        public MessageService(IOptions<MessageBusSettings> options)
        {

            _settings = options.Value;

            _connectionFactory = new ConnectionFactory
            {
                HostName = _settings.HostName,
                Port = _settings.Port,
                UserName = _settings.UserName,
                Password = _settings.Password
            };
        }
        private async Task<IConnection> GetConnectionAsync()
        {
            if (_connection == null || !_connection.IsOpen)
            {
                _connection = await _connectionFactory.CreateConnectionAsync();
            }
            return _connection;
        }

        public async ValueTask DisposeConnectionAsync()
        {
            if (_connection is not null)
                await _connection.DisposeAsync();
        }

        public async Task<string> SendMessageAsync(string message)
        {
            var connection = await GetConnectionAsync();

            await using var channel = await connection.CreateChannelAsync();

            await channel.ExchangeDeclareAsync("ex.messages", ExchangeType.Direct, durable: true);
            await channel.QueueDeclareAsync("q.messages", durable: true, exclusive: false, autoDelete: false);
            await channel.QueueBindAsync("q.messages", "ex.messages", "messages.create");

            var body = System.Text.Encoding.UTF8.GetBytes(message);

            await channel.BasicPublishAsync(
                exchange: "ex.messages",
                routingKey: "messages.create",
                mandatory: false,
                body: body);

            await DisposeConnectionAsync();
            return "Ok";
        }

    }
}

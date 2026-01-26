using MessageService.Contracts;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Text;
using System.Threading.Channels;

namespace MessageService.Services
{
    public class ReceiverService : BackgroundService
    {
        private readonly ILogger<ReceiverService> _logger;
        IConnectionFactory _connectionFactory;
        IConnection _connection;

        public ReceiverService(ILogger<ReceiverService> logger)
        {
            _connectionFactory = new ConnectionFactory
            {
                HostName = "localhost",
                Port = 5672,
                UserName = "admin",
                Password = "admin"
            };

            _logger = logger;
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
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {

            const string exchangeName = "ex.messages";
            const string queueName = "q.messages";
            const string routingKey = "messages.create";

            try
            {
                _connection = await GetConnectionAsync();
                var channel = await _connection.CreateChannelAsync();

                await channel.ExchangeDeclareAsync(
                    exchange: exchangeName,
                    type: ExchangeType.Direct,
                    durable: true,
                    autoDelete: false,
                    arguments: null);

                await channel.QueueDeclareAsync(
                    queue: queueName,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);

                await channel.QueueBindAsync(
                    queue: queueName,
                    exchange: exchangeName,
                    routingKey: routingKey);

                await channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 1, global: false);

                var consumer = new AsyncEventingBasicConsumer(channel);
                consumer.ReceivedAsync += async (_, ea) =>
                {
                    try
                    {
                        await DoWorkAsync(channel, _, ea, stoppingToken);
                    }
                    catch
                    {
                        await channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: true);
                        throw;
                    }
                };

                await channel.BasicConsumeAsync(
                    queue: queueName,
                    autoAck: false,
                    consumer: consumer);


            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                await DisposeConnectionAsync();
            }
            catch (Exception ex)
            {
                await DisposeConnectionAsync();
                _logger.LogError(ex, "Worker crashed; will continue.");
            }
        }


        private static async Task DoWorkAsync(IChannel channel, object model, BasicDeliverEventArgs ea, CancellationToken ct)
        {
            byte[] body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            Console.WriteLine($" Message Received : {message}");

            await channel.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false);

            Console.WriteLine($" -------- ");

        }
    }
}

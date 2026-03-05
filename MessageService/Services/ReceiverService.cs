using MessageService.Settings;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace MessageService.Services
{
    public class ReceiverService : BackgroundService
    {
        private readonly ILogger<ReceiverService> _logger;
        IConnectionFactory _connectionFactory;
        IConnection _connection;

        public ReceiverService(IOptions<MessageBusSettings> options, ILogger<ReceiverService> logger)
        {
            var settings = options.Value;
            _connectionFactory = new ConnectionFactory
            {
                HostName = settings.HostName,
                Port = settings.Port,
                UserName = settings.UserName,
                Password = settings.Password
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
            const string exchangeName = "ex.erp";
            const string queueName = "q.dev.catchall";
            const string bindingKey = "#";

            IChannel? channel = null;

            try
            {
                _connection = await GetConnectionAsync();
                channel = await _connection.CreateChannelAsync(cancellationToken: stoppingToken);

                await channel.ExchangeDeclareAsync(
                    exchange: exchangeName,
                    type: ExchangeType.Topic,
                    durable: true,
                    autoDelete: false,
                    arguments: null,
                    cancellationToken: stoppingToken);

                await channel.QueueDeclareAsync(
                    queue: queueName,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: new Dictionary<string, object?>
                    {
                        ["x-message-ttl"] = 1000 * 60 * 60 * 24
                    },
                    cancellationToken: stoppingToken);

                await channel.QueueBindAsync(
                    queue: queueName,
                    exchange: exchangeName,
                    routingKey: bindingKey,
                    arguments: null,
                    cancellationToken: stoppingToken);

                await channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 10, global: false, cancellationToken: stoppingToken);

                var consumer = new AsyncEventingBasicConsumer(channel);

                consumer.ReceivedAsync += async (_, ea) =>
                {
                    try
                    {
                        var rk = ea.RoutingKey;
                        var body = System.Text.Encoding.UTF8.GetString(ea.Body.ToArray());
                        _logger.LogInformation("DEV CATCHALL | rk={rk} | msg={msg}", rk, body);

                        await channel.BasicAckAsync(ea.DeliveryTag, multiple: false, cancellationToken: stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "DEV CATCHALL consume failed, requeueing");
                        await channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: true, cancellationToken: stoppingToken);
                    }
                };

                await channel.BasicConsumeAsync(queue: queueName, autoAck: false, consumer: consumer, cancellationToken: stoppingToken);

                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Worker crashed.");
            }
            finally
            {
                if (channel is not null) await channel.DisposeAsync();
                await DisposeConnectionAsync();
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

using MessageAPI.Abstractions;
using MessageService.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace MessageService.Settings
{
    public static class DICollections
    {
        public static IServiceCollection AddMessagingServices( this IServiceCollection services)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            services.AddHostedService<ReceiverService>();
            services.AddSingleton<IMessageService, MessageService.Services.MessageService>();
            services.AddSingleton<IMessageBusPublisher>(sp =>
            {
                var settings = sp.GetRequiredService<IOptions<MessageBusSettings>>().Value;
                return new RabbitMqPublisher(settings);
            });

            return services;
        }
    }
}

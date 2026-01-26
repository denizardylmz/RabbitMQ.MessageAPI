using MessageService.Contracts;
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
            services.AddSingleton<MessageService.Contracts.IMessageService, MessageService.Services.MessageService>();

            return services;
        }
    }
}

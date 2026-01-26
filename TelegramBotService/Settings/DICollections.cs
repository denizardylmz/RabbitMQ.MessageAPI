using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using TelegramBotService.Contracts;
using TelegramBotService.Services;

namespace TelegramBotService.Settings
{
    public static class DICollections
    {
        public static IServiceCollection AddTelegramServices(this IServiceCollection services)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            services.AddHttpClient<TelegramClient>((sp, http) =>
            {
                var opt = sp.GetRequiredService<IOptions<TelegramOptions>>().Value;
                http.BaseAddress = new Uri($"{opt.BaseUrl}/bot{opt.BotToken}/");
                
                http.Timeout = TimeSpan.FromSeconds(10);
            });

            services.AddSingleton<CommandRouter>();
            services.AddHostedService<TelegramPollingWorker>();

            return services;
        }
    }
}

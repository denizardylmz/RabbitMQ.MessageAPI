using MessageAPI.Abstractions.DbContracts;
using MessageAPI.Application.Handlers;
using MessageAPI.Application.Updates;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using TelegramBotService.Contracts;

namespace MessageAPI.Application.Settings
{
    public static class DICollections
    {
        public static IServiceCollection AddApplicaitonServices(this IServiceCollection services)
        {
            services.AddScoped<CreateUserHandler>();
            services.AddScoped<ResolveUserHandler>();
            services.AddScoped<CheckUserPinHandler>();


            services.AddScoped<SendShiftStartHandler>();
            services.AddScoped<SendShiftEndHandler>();
            services.AddScoped<SendBreakStartHandler>();
            services.AddScoped<SendBreakEndHandler>();



            services.AddSingleton<IUpdateHandler, UpdateHandler>();

            return services;
        }
    }
}

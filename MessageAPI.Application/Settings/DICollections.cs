using MessageAPI.Abstractions.DbContracts;
using MessageAPI.Application.Handlers;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace MessageAPI.Application.Settings
{
    public static class DICollections
    {
        public static IServiceCollection AddApplicaitonServices(this IServiceCollection services)
        {
            services.AddScoped<CreateUserHandler>();

            return services;
        }
    }
}

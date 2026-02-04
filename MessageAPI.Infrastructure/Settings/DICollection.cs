using MessageAPI.Abstractions.Contracts;
using MessageAPI.Abstractions.DbContracts;
using MessageAPI.Infrastructure.Context;
using MessageAPI.Infrastructure.Interceptors;
using MessageAPI.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace MessageAPI.Infrastructure.Settings
{
    public static class DICollections
    {
        public static IServiceCollection AddDBServices(this IServiceCollection services)
        {
            services.AddScoped<AuditSaveChangesInterceptor>();
            services.AddMemoryCache();

            services.AddDbContext<AppDbContext>((sp, options) =>
            {
                var dbOptions = sp.GetRequiredService<IOptions<DatabaseOptions>>().Value;

                options.UseNpgsql(
                    dbOptions.ConnectionString,
                    npgsql =>
                    {
                        npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "public");
                    });

                options.AddInterceptors(sp.GetRequiredService<AuditSaveChangesInterceptor>());

            });

            services.AddScoped<IAppDbContext, AppDbContext>();
            services.AddSingleton<IUserContextCache, MemoryUserContextCache>();


            return services;
        }
    }
}

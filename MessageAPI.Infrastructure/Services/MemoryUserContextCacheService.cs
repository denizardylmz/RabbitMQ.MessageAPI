using MessageAPI.Abstractions.Contracts;
using MessageAPI.Domain.DbModels;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Text;

namespace MessageAPI.Infrastructure.Services
{
    public sealed class MemoryUserContextCache : IUserContextCache
    {
        private readonly IMemoryCache _cache;

        public MemoryUserContextCache(IMemoryCache cache)
        {
            _cache = cache;
        }

        private static string Key(long telegramUserId)
            => $"tg:user:{telegramUserId}";

        public Task<User?> GetAsync(long telegramUserId, CancellationToken ct)
        {
            _cache.TryGetValue(Key(telegramUserId), out User? ctx);
            return Task.FromResult(ctx);
        }

        public Task SetAsync(User context, TimeSpan ttl, CancellationToken ct)
        {
            _cache.Set(
                Key(context.TelegramUserId.Value),
                context,
                ttl
            );

            return Task.CompletedTask;
        }

        public Task RemoveAsync(long telegramUserId, CancellationToken ct)
        {
            _cache.Remove(Key(telegramUserId));
            return Task.CompletedTask;
        }
    }
}

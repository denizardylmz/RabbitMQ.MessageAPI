using MessageAPI.Abstractions.Contracts;
using MessageAPI.Abstractions.DbContracts;
using MessageAPI.Domain.DbModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Text;

namespace MessageAPI.Application.Updates
{
    public sealed class UserResolver
    {
        private readonly IUserContextCache _cache;
        private readonly IAppDbContext _db;

        public UserResolver(IUserContextCache cache, IAppDbContext db)
        {
            _cache = cache;
            _db = db;
        }

        public async Task<User?> ResolveAsync(long telegramUserId, CancellationToken ct)
        {

            var ctx = await _cache.GetAsync(telegramUserId, ct);

            if (ctx is not null)
            {
                return ctx;
            }

            var user = await _db.Users.Where(x => x.Id == telegramUserId).AsNoTracking().FirstOrDefaultAsync(ct);

            if (user is null)
            {
                return null;
            }

            await _cache.SetAsync(user, TimeSpan.FromMinutes(30), ct);

            return user;
        }
    }

}

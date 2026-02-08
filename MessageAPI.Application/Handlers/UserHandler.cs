using MessageAPI.Abstractions.DbContracts;
using MessageAPI.Domain.DbModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace MessageAPI.Application.Handlers
{
    public record CreateUserCommand(string Username, string Email);

    public class CreateUserHandler
    {
        private readonly IAppDbContext _db;

        public CreateUserHandler(IAppDbContext db) => _db = db;

        public async Task<string> Handle(CreateUserCommand cmd, CancellationToken ct)
        {
            var existingUser = await _db.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Username == cmd.Username || u.Email == cmd.Email, ct);

            if (existingUser != null)
            {
                existingUser.ActivationPin = GeneratePin();
                existingUser.PinExpireAt = DateTime.UtcNow.AddMinutes(5);

                _db.Users.Update(existingUser);
                await _db.SaveChangesAsync();

                return existingUser.ActivationPin;
            }
            else
            {
                return string.Empty;
            }
        }

        public static string GeneratePin(int length = 6)
        {
            var min = (int)Math.Pow(10, length - 1);
            var max = (int)Math.Pow(10, length) - 1;

            return RandomNumberGenerator
                .GetInt32(min, max + 1)
                .ToString();
        }
    }

    public record CheckUserPinCommand(string pin, long telegramUserId);

    public class CheckUserPinHandler
    {
        private readonly IAppDbContext _db;

        public CheckUserPinHandler(IAppDbContext db) => _db = db;

        public async Task<int> Handle(CheckUserPinCommand cmd, CancellationToken ct)
        {
            var existingUser = await _db.Users
                .Where(u => u.ActivationPin == cmd.pin && u.PinExpireAt > DateTime.UtcNow)
                .FirstOrDefaultAsync(ct);

            if (existingUser != null)
            {
                existingUser.TelegramUserId = cmd.telegramUserId;

                _db.Users.Update(existingUser);

                return await _db.SaveChangesAsync();
            }
            else
            {
                return -1;
            }
        }
    }


    public record ResolveUserCommand(long telegramUserId);

    public class ResolveUserHandler
    {
        private readonly IAppDbContext _db;
        private readonly IMemoryCache _cache;

        public ResolveUserHandler(IAppDbContext db, IMemoryCache cache)
        {
            _db = db;
            _cache = cache;
        }

            public async Task<User?> Handle(ResolveUserCommand cmd, CancellationToken ct)
            {
                var cacheKey = $"tg:user:{cmd.telegramUserId}";

                if (_cache.TryGetValue(cacheKey, out User? ctx) && ctx != null)
                    return ctx;
                    
                var user = await _db.Users.Where(x => x.TelegramUserId == cmd.telegramUserId).AsNoTracking().FirstOrDefaultAsync();

                if (user != null)
                {
                    _cache.Set(cacheKey, user, TimeSpan.FromMinutes(30));
                }

                return user;
            }
    }


   
}

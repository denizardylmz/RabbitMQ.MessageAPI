using MessageAPI.Domain.DbModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace MessageAPI.Abstractions.Contracts
{
    public interface IUserContextCache
    {
        Task<User?> GetAsync(long telegramUserId, CancellationToken ct);
        Task SetAsync(User context, TimeSpan ttl, CancellationToken ct);
        Task RemoveAsync(long telegramUserId, CancellationToken ct);
    }

}

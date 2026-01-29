using MessageAPI.Domain.DbModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace MessageAPI.Abstractions.DbContracts
{
    public interface IAppDbContext
    {
        DbSet<User> Users { get; }
        DbSet<Order> Orders { get; }
        Task<int> SaveChangesAsync(CancellationToken ct = default);
    }
}

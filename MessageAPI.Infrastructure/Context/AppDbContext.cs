using System;
using System.Collections.Generic;
using System.Text;
using MessageAPI.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;


namespace MessageAPI.Infrastructure.Context
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
    : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Order> Orders => Set<Order>();

    }
}

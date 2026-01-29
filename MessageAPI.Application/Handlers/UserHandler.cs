using MessageAPI.Abstractions.DbContracts;
using MessageAPI.Domain.DbModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace MessageAPI.Application.Handlers
{
    public record CreateUserCommand(string Username, string Email);

    public class CreateUserHandler
    {
        private readonly IAppDbContext _db;

        public CreateUserHandler(IAppDbContext db) => _db = db;

        public async Task<int> Handle(CreateUserCommand cmd, CancellationToken ct)
        {
            var user = new User { Username = cmd.Username, Email = cmd.Email };
            _db.Users.Add(user);
            await _db.SaveChangesAsync(ct);
            return user.Id;
        }
    }
}

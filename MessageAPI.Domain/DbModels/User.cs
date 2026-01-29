using System;
using System.Collections.Generic;
using System.Text;

namespace MessageAPI.Domain.DbModels
{
    public class User : EntityBase
    {
        public string Username { get; set; } = null!;
        public string Email { get; set; } = null!;

    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace MessageAPI.Infrastructure.Models
{
    public class User : EntityBase
    {
        public string Username { get; set; } = null!;
        public string Email { get; set; } = null!;

    }
}

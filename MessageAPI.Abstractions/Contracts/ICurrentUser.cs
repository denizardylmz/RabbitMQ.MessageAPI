using System;
using System.Collections.Generic;
using System.Text;

namespace MessageAPI.Abstractions.Contracts
{
    public interface ICurrentUser
    {
        string? Username { get; }
    }

}

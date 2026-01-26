using System;
using System.Collections.Generic;
using System.Text;

namespace MessageService.Contracts
{
    public class MessageBusSettings
    {
        public string HostName { get; set; } = null!;
        public int Port { get; set; }
        public string UserName { get; set; } = null!;
        public string Password { get; set; } = null!;

    }
}

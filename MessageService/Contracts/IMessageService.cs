using System;
using System.Collections.Generic;
using System.Text;

namespace MessageService.Contracts
{
    public interface IMessageService
    {
        public Task<string> SendMessageAsync(string message);
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace TelegramBotService.Contracts
{
    public sealed class TelegramOptions
    {
        public string BotToken { get; set; } = default!;
        public string BaseUrl { get; set; } = "https://api.telegram.org";
    }

}

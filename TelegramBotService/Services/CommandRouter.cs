using System;
using System.Collections.Generic;
using System.Text;

namespace TelegramBotService.Services
{
    public sealed class CommandRouter
    {
        public (string Command, string Args) Parse(string text)
        {
            text = text.Trim();

            if (!text.StartsWith("/"))
                return ("_text", text);

            var firstSpace = text.IndexOf(' ');
            if (firstSpace < 0) return (text, "");

            var cmd = text[..firstSpace];
            var args = text[(firstSpace + 1)..].Trim();
            return (cmd, args);
        }
    }

}

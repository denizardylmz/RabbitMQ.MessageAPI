using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TelegramBotService.Models
{
    public abstract class TelegramException : Exception
    {
        protected TelegramException(string message, Exception? inner = null)
            : base(message, inner)
        {
        }
    }

    public sealed class TelegramApiException : TelegramException
    {
        public int StatusCode { get; }
        public string? TelegramResponse { get; }

        public TelegramApiException(int statusCode, string? telegramResponse)
            : base($"Telegram API error ({statusCode}): {telegramResponse}")
        {
            StatusCode = statusCode;
            TelegramResponse = telegramResponse;
        }
    }
    public sealed class TelegramTransportException : TelegramException
    {
        public TelegramTransportException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }


}


using MessageAPI.Abstractions;
using MessageAPI.Domain.DbModels;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Text;

namespace MessageAPI.Application.Handlers
{
    public record ShiftCommand(long telegramUserId, DateTime date);

    public class SendShiftStartHandler
    {
        private readonly IMemoryCache _cache;
        private readonly IMessageBusPublisher _bus;


        public SendShiftStartHandler(IMemoryCache cache, IMessageBusPublisher messageBusPublisher)
        {
            _cache = cache;


            _bus = messageBusPublisher;
        }

        public Task Handle(ShiftCommand cmd, CancellationToken ct)
        {
            var evt = new ShiftCommand(cmd.telegramUserId, DateTime.Now);

            return _bus.PublishAsync(
                exchange: "ex.erp",
                routingKey: "shift.started",
                message: evt,
                options: new PublishOptions(
                    CorrelationId: $"tg:{evt.telegramUserId}",
                    Headers: new Dictionary<string, object?> { ["source"] = "telegram-bot" }
                ),
                ct: ct);
        }
    }

    public class SendShiftEndHandler
    {
        private readonly IMemoryCache _cache;
        private readonly IMessageBusPublisher _bus;


        public SendShiftEndHandler(IMemoryCache cache, IMessageBusPublisher messageBusPublisher)
        {
            _cache = cache;
            _bus = messageBusPublisher;
        }

        public Task Handle(ShiftCommand cmd, CancellationToken ct)
        {
            var evt = new ShiftCommand(cmd.telegramUserId, DateTime.Now);

            return _bus.PublishAsync(
                exchange: "ex.erp",
                routingKey: "shift.end",
                message: evt,
                options: new PublishOptions(
                    CorrelationId: $"tg:{evt.telegramUserId}",
                    Headers: new Dictionary<string, object?> { ["source"] = "telegram-bot" }
                ),
                ct: ct);
        }
    }

    public class SendBreakStartHandler
    {
        private readonly IMemoryCache _cache;
        private readonly IMessageBusPublisher _bus;




        public SendBreakStartHandler(IMemoryCache cache, IMessageBusPublisher messageBusPublisher)
        {
            _cache = cache;
            _bus = messageBusPublisher;
        }

        public Task Handle(ShiftCommand cmd, CancellationToken ct)
        {
            var evt = new ShiftCommand(cmd.telegramUserId, DateTime.Now);

            return _bus.PublishAsync(
                exchange: "ex.erp.tg",
                routingKey: "shift.break.start",
                message: evt,
                options: new PublishOptions(
                    CorrelationId: $"tg:{evt.telegramUserId}",
                    Headers: new Dictionary<string, object?> { ["source"] = "telegram-bot" }
                ),
                ct: ct);
        }
    }

    public class SendBreakEndHandler
    {
        private readonly IMemoryCache _cache;
        private readonly IMessageBusPublisher _bus;


        public SendBreakEndHandler(IMemoryCache cache, IMessageBusPublisher messageBusPublisher)
        {
            _cache = cache;
            _bus = messageBusPublisher;
        }

        public Task Handle(ShiftCommand cmd, CancellationToken ct)
        {
            var evt = new ShiftCommand(cmd.telegramUserId, DateTime.Now);

            return _bus.PublishAsync(
                exchange: "ex.erp.tg",
                routingKey: "shift.break.end",
                message: evt,
                options: new PublishOptions(
                    CorrelationId: $"tg:{evt.telegramUserId}",
                    Headers: new Dictionary<string, object?> { ["source"] = "telegram-bot" }
                ),
                ct: ct);
        }
    }
}

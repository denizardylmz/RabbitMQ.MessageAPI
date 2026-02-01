using MessageService.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MessageAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MessageController : ControllerBase
    {

        IMessageService _messageService;
        TelegramClient tgClient;

        public MessageController(IMessageService messageService, TelegramClient telegramClient)
        {
            _messageService = messageService;
            tgClient = telegramClient;

        }

        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] string message)
        {
            var result = await _messageService.SendMessageAsync(message);
            return Ok(result);
        }

        [HttpPost("send/tg")]
        public async Task<IActionResult> SendMessageTg([FromBody] string message)
        {
            await tgClient.SendMessageAsync(677462547, message, CancellationToken.None);
            return Ok();
        }

    }
}

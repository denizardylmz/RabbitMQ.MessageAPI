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

        public MessageController(IMessageService messageService )
        {
                _messageService = messageService;
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] string message)
        {
            var result = await _messageService.SendMessageAsync(message);
            return Ok(result);
        }

    }
}

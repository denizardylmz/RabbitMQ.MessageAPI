using MessageAPI.Application.Handlers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MessageAPI.API.Controllers
{
    public record CreateUserRequest(string Username, string Email);

    [ApiController]
    [Route("api/users")]
    public class UsersController : ControllerBase
    {
        private readonly CreateUserHandler _handler;

        public UsersController(CreateUserHandler handler) => _handler = handler;

        [HttpPost]
        public async Task<IActionResult> GetPin(CreateUserRequest req, CancellationToken ct)
        {
            var pin = await _handler.Handle(new CreateUserCommand(req.Username, req.Email), ct);

            if (string.IsNullOrWhiteSpace(pin))
            {
                return StatusCode(StatusCodes.Status401Unauthorized, "User Not Found.");
            }
            var response = new { Pin = pin };
            return Ok(response);
        }

        [HttpGet("{id:int}")]
        public IActionResult GetById(int id) => Ok();
    }
}

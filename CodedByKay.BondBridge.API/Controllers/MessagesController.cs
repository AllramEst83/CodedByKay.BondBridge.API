using CodedByKay.BondBridge.API.Models;
using Microsoft.AspNetCore.Mvc;

namespace CodedByKay.BondBridge.API.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class MessagesController : ControllerBase
    {

        private readonly ILogger<MessagesController> _logger;

        public MessagesController(ILogger<MessagesController> logger)
        {
            _logger = logger;
        }

        [HttpGet("{userId}", Name = "GetMessages")]
        public IEnumerable<Message> Get(Guid userId)
        {
            return new List<Message>
            {
                new() {
                    MessageContent = "Hello there! What are you doing?",
                    MessageId = Guid.NewGuid(),
                    UserId = userId,
                },
                new()
                {
                    MessageContent = "Just checking in to see how you're doing.",
                    MessageId = Guid.NewGuid(),
                    UserId = userId,
                },
                new()
                {
                    MessageContent = "Don't forget our meeting tomorrow!",
                    MessageId = Guid.NewGuid(),
                    UserId = userId,
                }
            };
        }
    }
}

using CodedByKay.BondBridge.API.Model;
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

        [HttpGet("{userId}", Name = "GetMessagesByUserId")]
        public IEnumerable<Messages> Get(Guid userId)
        {
            return new List<Messages>
            {
                new() {
                    Message = "Hello there! What are you doing?",
                    MessageId = Guid.NewGuid(),
                    UserId = userId,
                },
                new()
                {
                    Message = "Just checking in to see how you're doing.",
                    MessageId = Guid.NewGuid(),
                    UserId = userId,
                },
                new()
                {
                    Message = "Don't forget our meeting tomorrow!",
                    MessageId = Guid.NewGuid(),
                    UserId = userId,
                }
            };
        }
    }
}

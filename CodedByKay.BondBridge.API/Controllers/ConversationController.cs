using CodedByKay.BondBridge.API.DBContext;
using Microsoft.AspNetCore.Mvc;

namespace CodedByKay.BondBridge.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConversationController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ConversationController(ApplicationDbContext context)
        {
            _context = context;
        }

    }
}

using CodedByKay.BondBridge.API.DBContext;
using CodedByKay.BondBridge.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CodedByKay.BondBridge.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LogController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public LogController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("latest")]
        [Authorize(Policy = TokenValidationConstants.Policies.CodedByKayBondBridgeApiAdmin)]
        public async Task<IActionResult> GetLatestLogs()
        {
            var since = DateTime.UtcNow.AddHours(-24);
            var logs = await _context.Logs
                .Where(log => log.Timestamp >= since)
                .OrderByDescending(log => log.Timestamp)
                .ToListAsync();

            return Ok(logs);
        }
    }
}

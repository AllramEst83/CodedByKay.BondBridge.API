using CodedByKay.BondBridge.API.DBContext;
using CodedByKay.BondBridge.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CodedByKay.BondBridge.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GroupController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public GroupController(ApplicationDbContext context)
        {
            _context = context;
        }

        [Authorize(Policy = TokenValidationConstants.Policies.CodedByKayBondBridgeApiCommonUser)]
        [HttpGet("groupsbyuserid/{userId}")]
        public IActionResult GroupsByUserId(Guid userId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var groupsWithUsersAndMessages = _context.Groups
                                                  .Where(g => g.UserGroups.Any(ug => ug.UserID == userId))
                                                  .Select(g => new
                                                  {
                                                      GroupID = g.GroupID,
                                                      GroupName = g.GroupName,
                                                      Users = g.UserGroups.Select(ug => new
                                                      {
                                                          UserID = ug.ConversationUser.UserID,
                                                          UserName = ug.ConversationUser.UserName
                                                      }).ToList(),
                                                      Messages = g.Messages.Select(m => new
                                                      {
                                                          MessageText = m.MessageText,
                                                          Timestamp = m.Timestamp,
                                                          UserName = m.User.UserName,
                                                          userId = m.User.UserID
                                                      }).OrderBy(m => m.Timestamp).ToList()
                                                  }).ToList();

            return Ok(new { result = groupsWithUsersAndMessages });
        }
    }
}

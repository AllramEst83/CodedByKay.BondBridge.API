using CodedByKay.BondBridge.API.DBContext;
using CodedByKay.BondBridge.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CodedByKay.BondBridge.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApplicationManagerController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public ApplicationManagerController(
            ApplicationDbContext context, 
            UserManager<IdentityUser> userManager, 
            RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [Authorize(Policy = TokenValidationConstants.Policies.CodedByKayBondBridgeApiAdmin)]
        [HttpGet("ensurecreated")]
        public IActionResult EnsureCreated()
        {
            _context.Database.EnsureCreated();

            return Ok(new { message = "Databae has been cerated" });
        }

        [Authorize(Policy = TokenValidationConstants.Policies.CodedByKayBondBridgeApiAdmin)]
        [HttpPost("addrole/{roleName}")]
        public async Task<IActionResult> AddRole(string roleName)
        {
            //Transform data

            // Check if the role already exists
            var roleExists = await _roleManager.RoleExistsAsync(roleName);
            if (roleExists)
            {
                return BadRequest(new { message = "Role already exists." });
            }

            // Create the role
            var result = await _roleManager.CreateAsync(new IdentityRole(roleName));

            if (!result.Succeeded)
            {
                // Return an error response if role creation failed, detailing why
                return BadRequest(result.Errors);
            }

            return Ok(new { message = "Role has been created." });
        }

        [Authorize(Policy = TokenValidationConstants.Policies.CodedByKayBondBridgeApiAdmin)]
        [HttpPost("adduser")]
        public async Task<IActionResult> AddUser([FromBody] AddUserModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            //Transform data
            //Check email validity

            var user = new IdentityUser { UserName = model.Email, Email = model.Email };
            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                // Return an error response if user creation failed, detailing why
                return BadRequest(result.Errors);
            }

            // Check if the role exists, create if it doesn't
            if (!await _roleManager.RoleExistsAsync(TokenValidationConstants.Roles.CommonUserAccess))
            {
                await _roleManager.CreateAsync(new IdentityRole(TokenValidationConstants.Roles.CommonUserAccess));
            }

            // Assign the user to the role
            var roleResult = await _userManager.AddToRoleAsync(user, TokenValidationConstants.Roles.CommonUserAccess);

            if (!roleResult.Succeeded)
            {
                // Handle errors (e.g., role assignment failed)
                return BadRequest(roleResult.Errors);
            }

            return Ok(new { message = "User has been created and assigned a role." });
        }

        [Authorize(Policy = TokenValidationConstants.Policies.CodedByKayBondBridgeApiAdmin)]
        [HttpPost("addroletouser")]
        public async Task<IActionResult> AddRoleToUser([FromBody] AddRoleToUserModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            //Transform data

            var user = await _userManager.FindByIdAsync(model.UserId);

            if (user == null)
            {
                return NotFound("User does not exist");
            }

            if (!await _roleManager.RoleExistsAsync(model.Role))
            {
                return BadRequest("Role those not exist.");
            }

            var isInRole = await _userManager.IsInRoleAsync(user, model.Role);
            if (isInRole)
            {
                return BadRequest("User alraedy assign to role");
            }

            var roleResult = await _userManager.AddToRoleAsync(user, model.Role);

            if (!roleResult.Succeeded)
            {
                // Handle errors (e.g., role assignment failed)
                return BadRequest(roleResult.Errors);
            }

            return Ok(new { message = "Role has been add to user." });
        }

        [Authorize(Policy = TokenValidationConstants.Policies.CodedByKayBondBridgeApiAdmin)]
        [HttpPost("removerolefromuser")]
        public async Task<IActionResult> RemoveRoleFromUser([FromBody] RemoveRoleFromUserModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            //Transform data

            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null)
            {
                return NotFound("User does not exist");
            }

            if (!await _roleManager.RoleExistsAsync(model.Role))
            {
                return BadRequest("Role those not exist.");
            }

            var isInRole = await _userManager.IsInRoleAsync(user, model.Role);
            if (!isInRole)
            {
                return BadRequest("User is not assign to the role");
            }

            var roleResult = await _userManager.RemoveFromRoleAsync(user, model.Role);
            if (!roleResult.Succeeded)
            {
                // Handle errors (e.g., role assignment failed)
                return BadRequest(roleResult.Errors);
            }

            return Ok(new { message = "Role has been removed from user." });
        }

        [Authorize(Policy = TokenValidationConstants.Policies.CodedByKayBondBridgeApiAdmin)]
        [HttpPost("deleteuser")]
        public async Task<IActionResult> DeleteUser([FromBody] DeleteUserModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            //Transform data

            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null)
            {
                return NotFound("User does not exist");
            }

            var userResult = await _userManager.DeleteAsync(user);
            if (!userResult.Succeeded)
            {
                // Handle errors (e.g., role assignment failed)
                return BadRequest(userResult.Errors);
            }

            return Ok(new { message = "God bless! User has been deleted." });
        }

        [Authorize(Policy = TokenValidationConstants.Policies.CodedByKayBondBridgeApiAdmin)]
        [HttpPost("deleterole")]
        public async Task<IActionResult> DeleteRole([FromBody] DeleteRole model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            //Transform data

            var role = await _roleManager.FindByNameAsync(model.Role);
            if (role == null)
            {
                return BadRequest("Role does not exist.");
            }

            var roleResult = await _roleManager.DeleteAsync(role);
            if (!roleResult.Succeeded)
            {
                return BadRequest(roleResult.Errors);
            }

            return Ok(new { message = "God bless! Role has been deleted." });
        }
    }
}

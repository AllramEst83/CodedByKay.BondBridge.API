using CodedByKay.BondBridge.API.DBContext;
using CodedByKay.BondBridge.API.Models;
using CodedByKay.BondBridge.API.Models.DBModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace CodedByKay.BondBridge.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserManagerController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public UserManagerController(
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

            return Ok(new { message = "God bless! Databae has been cerated" });
        }

        [Authorize(Policy = TokenValidationConstants.Policies.CodedByKayBondBridgeApiCommonUser)]
        [HttpGet("getusers")]
        public async Task<IActionResult> GetUsers()
        {

            var users = _context.ConversationUsers.ToList();

            return Ok(users);
        }
        /// <summary>
        /// Adds a new role to the application.
        /// </summary>
        /// <param name="model">he erle model containing the role name for the new new role.</param>
        /// <returns>A <see cref="IActionResult"/> that indicates the result of the operation.</returns>
        /// <remarks>
        /// This method performs the following operations:
        /// - Checks if the specified role already exists.
        /// - If the role does not exist, it attempts to create the new role.
        /// - Returns a bad request response if the role already exists or if the creation fails, along with the error details.
        /// - Returns an OK response if the role is successfully created.
        /// 
        /// This action requires the caller to be authorized with the 'CodedByKayBondBridgeApiAdmin' policy.
        /// </remarks>
        [Authorize(Policy = TokenValidationConstants.Policies.CodedByKayBondBridgeApiAdmin)]
        [HttpPost("addrole")]
        public async Task<IActionResult> AddRole([FromBody] AddRoleModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            //Transform data

            // Check if the role already exists
            var roleExists = await _roleManager.RoleExistsAsync(model.RoleName);
            if (roleExists)
            {
                return BadRequest(new { message = "Role already exists." });
            }

            // Create the role
            var result = await _roleManager.CreateAsync(new IdentityRole(model.RoleName));

            if (!result.Succeeded)
            {
                // Return an error response if role creation failed, detailing why
                return BadRequest(result.Errors);
            }

            return Ok(new { message = "God bless! Role has been created." });
        }

        /// <summary>
        /// Creates a new user account.
        /// </summary>
        /// <param name="model">The user model containing the email and password for the new user.</param>
        /// <returns>A <see cref="IActionResult"/> that indicates the result of the user creation process.</returns>
        /// <remarks>
        /// This method performs the following operations:
        /// - Validates the incoming model data. Returns a bad request if the model state is invalid.
        /// - Validates the email format for correctness. Returns a bad request if the email format is invalid.
        /// - Checks for the uniqueness of the email address. Returns a bad request if the email is already in use.
        /// - Attempts to create a new user with the specified email and password.
        /// - Checks if the role for common user access exists; creates it if it does not.
        /// - Attempts to assign the newly created user to the common user access role.
        /// - If the role assignment fails, deletes the newly created user to ensure data consistency and returns the encountered errors.
        /// - Returns an OK response if the user is successfully created and assigned the role.
        /// 
        /// This action allows anonymous access.
        /// </remarks>
        [Authorize(Policy = TokenValidationConstants.Policies.CodedByKayBondBridgeApiAppAccess)]
        [HttpPost("adduser")]
        public async Task<IActionResult> AddUser([FromBody] AddUserModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            //Transform data

            if (!new EmailAddressAttribute().IsValid(model.Email))
            {
                return BadRequest("Email format is invalid.");
            }

            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
            {
                return BadRequest("Email is already in use.");
            }

            var user = new IdentityUser { UserName = model.Email, Email = model.Email };
            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            if (!await _roleManager.RoleExistsAsync(TokenValidationConstants.Roles.CommonUserAccess))
            {
                await _roleManager.CreateAsync(new IdentityRole(TokenValidationConstants.Roles.CommonUserAccess));
            }

            var roleResult = await _userManager.AddToRoleAsync(user, TokenValidationConstants.Roles.CommonUserAccess);

            if (!roleResult.Succeeded)
            {
                await _userManager.DeleteAsync(user);

                return BadRequest(roleResult.Errors);
            }

            var conversationUsers = new ConversationUser()
            {
                UserID = Guid.NewGuid(),
                UserName = user.Email,
                ImagePath = "placeholderimage.png"
            };

            await _context.ConversationUsers.AddAsync(conversationUsers);

            await _context.SaveChangesAsync();

            return Ok(new { message = "God bless! User has been created and assigned a role." });
        }

        /// <summary>
        /// Assigns a specified role to a user.
        /// </summary>
        /// <param name="model">The model containing the user ID and the role name to assign to the user.</param>
        /// <returns>A <see cref="IActionResult"/> indicating the result of the role assignment operation.</returns>
        /// <remarks>
        /// This method performs the following operations:
        /// - Validates the incoming model data. If the model state is invalid, a bad request is returned.
        /// - Attempts to find the user by their ID. If the user cannot be found, returns a not found response.
        /// - Checks if the specified role exists. If the role does not exist, returns a bad request.
        /// - Checks if the user is already assigned to the specified role. If so, returns a bad request indicating the user is already in the role.
        /// - Attempts to assign the user to the specified role.
        /// - If the role assignment fails, returns a bad request with the encountered errors.
        /// - Returns an OK response if the role is successfully assigned to the user.
        /// 
        /// Authorization is required to access this action, and the caller must have the 'CodedByKayBondBridgeApiAdmin' policy permission.
        /// </remarks>
        [Authorize(Policy = TokenValidationConstants.Policies.CodedByKayBondBridgeApiAdmin)]
        [HttpPost("addroletouser")]
        public async Task<IActionResult> AddRoleToUser([FromBody] AddRoleToUserModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            //Transform data

            var user = await _userManager.FindByIdAsync(model.UserId.ToString());

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

            return Ok(new { message = "God bless! Role has been add to user." });
        }

        /// <summary>
        /// Removes a specified role from a user.
        /// </summary>
        /// <param name="model">The model containing the user ID and the role name to be removed from the user.</param>
        /// <returns>A <see cref="IActionResult"/> indicating the result of the role removal operation.</returns>
        /// <remarks>
        /// This method performs the following operations:
        /// - Validates the incoming model data. If the model state is invalid, it returns a bad request.
        /// - Attempts to find the user by their ID. If the user cannot be found, it returns a not found response.
        /// - Checks if the specified role exists. If the role does not exist, it returns a bad request.
        /// - Verifies whether the user is currently assigned to the specified role. If the user is not in the role, it returns a bad request.
        /// - Attempts to remove the role from the user.
        /// - If the role removal process fails, it returns a bad request with the encountered errors.
        /// - Returns an OK response if the role is successfully removed from the user.
        /// 
        /// This action requires authorization based on the 'CodedByKayBondBridgeApiAdmin' policy.
        /// </remarks>
        [Authorize(Policy = TokenValidationConstants.Policies.CodedByKayBondBridgeApiAdmin)]
        [HttpPost("removerolefromuser")]
        public async Task<IActionResult> RemoveRoleFromUser([FromBody] RemoveRoleFromUserModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            //Transform data

            var user = await _userManager.FindByIdAsync(model.UserId.ToString());
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

            return Ok(new { message = "God bless! Role has been removed from user." });
        }

        /// <summary>
        /// Deletes a user from the application.
        /// </summary>
        /// <param name="model">The model containing the ID of the user to be deleted.</param>
        /// <returns>A <see cref="IActionResult"/> indicating the result of the user deletion operation.</returns>
        /// <remarks>
        /// This method performs the following operations:
        /// - Validates the incoming model data. If the model state is invalid, a bad request is returned.
        /// - Attempts to find the user by their ID. If the user cannot be found, a not found response is returned.
        /// - Retrieves the roles associated with the user. If the user has any roles, attempts to remove these roles.
        /// - Attempts to delete the user.
        /// - If the role removal or user deletion fails, returns a bad request with the encountered errors.
        /// - Returns an OK response if the user is successfully deleted from the application.
        /// 
        /// Authorization is required to access this action, and the caller must have the 'CodedByKayBondBridgeApiAdmin' policy permission.
        /// </remarks>
        [Authorize(Policy = TokenValidationConstants.Policies.CodedByKayBondBridgeApiAdmin)]
        [HttpPost("deleteuser")]
        public async Task<IActionResult> DeleteUser([FromBody] DeleteUserModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userManager.FindByIdAsync(model.UserId.ToString());
            if (user == null)
            {
                return NotFound("User does not exist");
            }

            var userRoles = await _userManager.GetRolesAsync(user);
            if (userRoles.Count > 0)
            {
                var removalResult = await _userManager.RemoveFromRolesAsync(user, userRoles);
                if (!removalResult.Succeeded)
                {
                    return BadRequest(removalResult.Errors);
                }
            }

            var userResult = await _userManager.DeleteAsync(user);
            if (!userResult.Succeeded)
            {
                return BadRequest(userResult.Errors);
            }

            var conversationUsers = await _context.ConversationUsers.FirstOrDefaultAsync(x => x.UserName == user.UserName);
            if (conversationUsers != null)
            {
                var result = _context.ConversationUsers.Remove(conversationUsers);
            }

            return Ok(new { message = "God bless! User has been deleted." });
        }

        /// <summary>
        /// Deletes a specified role from the application.
        /// </summary>
        /// <param name="model">The model containing the name of the role to be deleted.</param>
        /// <returns>A <see cref="IActionResult"/> indicating the result of the role deletion operation.</returns>
        /// <remarks>
        /// This method performs the following operations:
        /// - Validates the incoming model data. If the model state is invalid, it returns a bad request.
        /// - Attempts to find the role by its name. If the role cannot be found, it returns a bad request indicating the role does not exist.
        /// - Checks if the role name is null, returning a bad request if so, although this condition is unlikely given the role is already found.
        /// - Retrieves all users associated with the role. If there are any, attempts to remove the role from each user.
        /// - Attempts to delete the role.
        /// - If the role deletion process fails, it returns a bad request with the encountered errors.
        /// - Returns an OK response if the role is successfully deleted from the application.
        /// 
        /// Authorization is required to access this action, and the caller must have the 'CodedByKayBondBridgeApiAdmin' policy permission.
        /// </remarks>
        [Authorize(Policy = TokenValidationConstants.Policies.CodedByKayBondBridgeApiAdmin)]
        [HttpPost("deleterole")]
        public async Task<IActionResult> DeleteRole([FromBody] DeleteRole model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Transform data

            var role = await _roleManager.FindByNameAsync(model.Role);
            if (role == null)
            {
                return BadRequest("Role does not exist.");
            }

            if (role.Name == null)
            {
                return BadRequest("Role name cannot be null.");
            }

            var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name);
            if (usersInRole.Count > 0)
            {
                foreach (var user in usersInRole)
                {
                    await _userManager.RemoveFromRoleAsync(user, role.Name);
                }
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

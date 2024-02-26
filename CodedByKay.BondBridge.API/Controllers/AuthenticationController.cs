using CodedByKay.BondBridge.API.Exstensions;
using CodedByKay.BondBridge.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using static CodedByKay.BondBridge.API.Models.TokenValidationConstants;

namespace CodedByKay.BondBridge.API.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly ApplicationSettings _applicationSettings;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        public AuthenticationController(
            ApplicationSettings applicationSettings, 
            UserManager<IdentityUser> userManager, 
            SignInManager<IdentityUser> signInManager)
        {
            _applicationSettings = applicationSettings;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [AllowAnonymous]
        [HttpPost("signin")]
        public async Task<IActionResult> Signin([FromBody] UserSigninModel userLogin)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _signInManager.PasswordSignInAsync(userLogin.Email, userLogin.Password, isPersistent: false, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                var user = await _userManager.FindByEmailAsync(userLogin.Email);
                if (user == null)
                {
                    return NotFound("User does not exist");
                }

                var roles = await _userManager.GetRolesAsync(user);
                if (roles == null)
                {
                    return NotFound("No roles found on user.");
                }

                // Generate a new JWT access token
                var accessToken = JwtAuthExtension.GenerateToken(_applicationSettings.JWTSIGNINGKEY, _applicationSettings.JWTISSUER, _applicationSettings.JWTAUDIENCE, userLogin.Email, [.. roles], user.Id);

                // Generate a new refresh token
                var refreshToken = JwtAuthExtension.GenerateRefreshToken();

                await _userManager.RemoveAuthenticationTokenAsync(user, "RefreshTokenProvider", "RefreshToken");

                // Store the new refresh token securely associated with the user
                await _userManager.SetAuthenticationTokenAsync(user, "RefreshTokenProvider", "RefreshToken", refreshToken);

                // Return both the access and refresh tokens to the client
                return Ok(new { AccessToken = accessToken, RefreshToken = refreshToken });
            }

            return Unauthorized();
        }

        [Authorize(Policy = Policies.CodedByKayBondBridgeApiAdmin)]
        [Authorize(Policy = Policies.CodedByKayBondBridgeApiCommonUser)]
        [HttpPost("refreshtoken")]
        public async Task<IActionResult> RefreshToken([FromBody] TokenRefreshRequest model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Extract user id from the access token
            var userId = JwtAuthExtension.GetUserIdFromExpiredAccessToken(model.AccessToken);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("Invalid access token.");
            }

            // Check if user exists
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound("User does not exist");
            }

            var roles = await _userManager.GetRolesAsync(user);
            if (roles == null)
            {
                return NotFound("No roles found on user.");
            }

            // Verify the refresh token
            var storedRefreshToken = await _userManager.GetAuthenticationTokenAsync(user, "RefreshTokenProvider", "RefreshToken");
            if (storedRefreshToken != model.RefreshToken || string.IsNullOrEmpty(storedRefreshToken))
            {
                return Unauthorized("Invalid refresh token.");
            }

            // Generate new tokens
            var newAccessToken = JwtAuthExtension.GenerateToken(_applicationSettings.JWTSIGNINGKEY, _applicationSettings.JWTISSUER, _applicationSettings.JWTAUDIENCE, user.Email, [.. roles], user.Id);
            var newRefreshToken = JwtAuthExtension.GenerateRefreshToken();

            // Update the stored refresh token with the new one
            // Note: Consider implementing refresh token rotation here
            await _userManager.SetAuthenticationTokenAsync(user, "RefreshTokenProvider", "RefreshToken", newRefreshToken);

            return Ok(new
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken
            });
        }
    }
}

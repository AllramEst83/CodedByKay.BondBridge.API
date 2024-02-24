using CodedByKay.BondBridge.API.Models;
using CodedByKay.BondBridge.JwtAuth;
using CodedByKay.BondBridge.JwtAuth.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Runtime;

namespace CodedByKay.BondBridge.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly ApplicationSettings _applicationSettings;
        public AuthenticationController(ApplicationSettings applicationSettings)
        {
            _applicationSettings = applicationSettings;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public IActionResult Login([FromBody] UserLoginDto userLogin)
        {
            // Validate the user credentials. This is where you check the email and password.
            // For the sake of this example, let's assume a method ValidateUser returns true if valid.
            bool isValidUser = ValidateUser(userLogin.Email, userLogin.Password);

            if (isValidUser)
            {
                // Assume GenerateToken is a method that generates a JWT token.
                var token = JwtAuthExtension.GenerateToken(_applicationSettings.JWTSIGNINGKEY, _applicationSettings.JWTISSUER, _applicationSettings.JWTAUDIENCE, userLogin.Email, TokenValidationConstants.Roles.AdminAccess);
                return Ok(new { Token = token });
            }
            else
            {
                return Unauthorized();
            }
        }
        private bool ValidateUser(string email, string password)
        {
            // Implement your user validation logic here,
            // such as checking against a database.
            return true; // Placeholder
        }

        [Authorize(Policy = TokenValidationConstants.Policies.CodedByKayBondBridgeApiAdmin)]
        [HttpGet("admintest")]
        public IActionResult Admintest()
        {
            return Ok("Nice!");
        }
    }
}

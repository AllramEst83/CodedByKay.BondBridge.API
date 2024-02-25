using CodedByKay.BondBridge.API.Exstensions;
using CodedByKay.BondBridge.API.Model;
using CodedByKay.BondBridge.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CodedByKay.BondBridge.API.Controllers
{

    
    /*
     - Add exception messages
     - Add global exception message on forbbiden JWT token
    - Encrypt sending of toke/all messages to the server and back
     */

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
                var roles = new List<string>() { TokenValidationConstants.Roles.AdminAccess, TokenValidationConstants.Roles.CommonUserAccess };
                // Assume GenerateToken is a method that generates a JWT token.
                var token = JwtAuthExtension.GenerateToken(_applicationSettings.JWTSIGNINGKEY, _applicationSettings.JWTISSUER, _applicationSettings.JWTAUDIENCE, userLogin.Email, roles);
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
        [HttpGet("adminrequesttest")]
        public IActionResult AdminRequestTest()
        {
            return Ok(new { message = "You are a admin user!" });
        }

        [Authorize(Policy = TokenValidationConstants.Policies.CodedByKayBondBridgeApiCommonUser)]
        [HttpGet("userrequesttest")]
        public IActionResult UserRequestTest()
        {
            return Ok(new { message = "You are a common user!" });
        }
    }
}

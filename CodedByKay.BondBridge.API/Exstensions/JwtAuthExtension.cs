using CodedByKay.BondBridge.API.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace CodedByKay.BondBridge.API.Exstensions
{
    /// <summary>
    /// Provides methods for configuring JWT authentication and generating tokens.
    /// </summary>
    public static class JwtAuthExtension
    {
        /// <summary>
        /// Adds JWT-based authentication to the specified IServiceCollection.
        /// </summary>
        /// <param name="services">The IServiceCollection to add services to.</param>
        /// <param name="secretKey">The secret key used for signing the JWT.</param>
        /// <param name="issuer">The issuer of the JWT.</param>
        /// <param name="audience">The audience of the JWT.</param>
        /// <returns>The original IServiceCollection for chaining.</returns>
        /// <remarks>
        /// This method configures JWT authentication using the provided parameters. It sets up the authentication scheme, token validation parameters, 
        /// and authorization policies for different user roles. Ensure the secretKey is securely stored and not hard-coded in production environments.
        /// </remarks>
        public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, string secretKey, string issuer, string audience)
        {
            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.ClaimsIssuer = issuer;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = signingKey,
                    ValidateIssuer = true,
                    ValidIssuer = issuer,
                    ValidateAudience = true,
                    ValidAudience = audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
            });

            services.AddAuthorization(options =>
            {
                // Add policies for different roles to handle various types of users such as admin, user, editUser
                options.AddPolicy(
                    TokenValidationConstants.Policies.CodedByKayBondBridgeApiAdmin,
                    policy => policy.RequireClaim(
                        TokenValidationConstants.Roles.Role,
                        TokenValidationConstants.Roles.AdminAccess));

                options.AddPolicy(
                    TokenValidationConstants.Policies.CodedByKayBondBridgeApiCommonUser,
                    policy => policy.RequireClaim(
                        TokenValidationConstants.Roles.Role,
                        TokenValidationConstants.Roles.CommonUserAccess));

                options.AddPolicy(
                 TokenValidationConstants.Policies.CodedByKayBondBridgeApiAppAccess,
                 policy => policy.RequireClaim(
                     TokenValidationConstants.Roles.Role,
                     TokenValidationConstants.Roles.AppAccess));
            });

            return services;
        }

        /// <summary>
        /// Generates a JWT token for a specified user with claims based on their roles.
        /// </summary>
        /// <param name="secretKey">The secret key used for signing the JWT.</param>
        /// <param name="issuer">The issuer of the JWT.</param>
        /// <param name="audience">The audience of the JWT.</param>
        /// <param name="username">The username of the user for whom the token is being generated.</param>
        /// <param name="roles">A list of roles associated with the user.</param>
        /// <returns>A JWT token as a string.</returns>
        /// <remarks>
        /// Generates a JWT token that includes claims for the user's username and roles. The token is signed with the provided secret key.
        /// </remarks>
        public static string GenerateToken(string secretKey, string issuer, string audience, string username, List<string> roles, string userId, bool generateTokenForApp = false)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, username),
                new Claim(ClaimTypes.NameIdentifier, userId)
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(TokenValidationConstants.Roles.Role, role));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: generateTokenForApp ? DateTime.Now.AddYears(1) : DateTime.Now.AddDays(7), // App = 1 year/User = 7 days
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <summary>
        /// Generates a cryptographically secure secret key.
        /// </summary>
        /// <returns>A Base64 encoded secret key.</returns>
        /// <remarks>
        /// This method generates a 256-bit secret key using a cryptographically secure random number generator. The key is suitable for use in signing JWT tokens.
        /// </remarks>
        public static string GenerateSecretKey()
        {
            var randomBytes = new byte[32]; // 256 bits
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }
            return Convert.ToBase64String(randomBytes);
        }

        /// <summary>
        /// Generates a secure random string to be used as a refresh token.
        /// </summary>
        /// <returns>A base64 encoded string representing a securely generated random value.</returns>
        /// <remarks>
        /// This method generates a 256-bit (32-byte) secure random number using a cryptographic random number generator (RNG).
        /// It then converts this random number into a base64 string, which is suitable for use as a refresh token in authentication flows.
        /// </remarks>
        public static string GenerateRefreshToken()
        {
            var randomNumber = new byte[32]; // 256 bits
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }

        /// <summary>
        /// Extracts the user ID from an expired JWT access token.
        /// </summary>
        /// <param name="accessToken">The JWT access token from which the user ID should be extracted.</param>
        /// <returns>The user ID extracted from the token.</returns>
        /// <exception cref="NullReferenceException">Thrown when the user ID claim cannot be found in the token.</exception>
        /// <remarks>
        /// This method decodes the JWT access token without validating its signature or expiration,
        /// specifically to extract the user ID from expired tokens. It assumes the token contains a NameIdentifier claim,
        /// which is used to store the user ID. If the claim is not found, a NullReferenceException is thrown.
        /// </remarks>
        public static string GetUserIdFromExpiredAccessToken(string accessToken)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var securityToken = tokenHandler.ReadToken(accessToken) as JwtSecurityToken;

            var userIdClaim = securityToken?.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier);

            return userIdClaim?.Value ?? throw new NullReferenceException("user claim value can not be null.");
        }
    }
}

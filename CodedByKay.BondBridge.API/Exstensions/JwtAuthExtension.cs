using CodedByKay.BondBridge.API.Model;
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
        public static string GenerateToken(string secretKey, string issuer, string audience, string username, List<string> roles)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, username)
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
                expires: DateTime.Now.AddDays(7),
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
            return Convert.ToBase64String(randomBytes); // Convert to Base64 for easier use in configurations
        }
    }
}

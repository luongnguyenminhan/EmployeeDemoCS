using EmployeeDemo.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace EmployeeDemo.Application.Utils
{
    public static class GenerateJWTToken
    {
        /// <summary>
        /// Creates a JWT token with user claims and device ID.
        /// </summary>
        /// <param name="authClaims">List of authentication claims (Name, Role, Jti, etc.)</param>
        /// <param name="configuration">Configuration instance with JWT settings</param>
        /// <param name="currentTime">Current UTC time for token expiration calculation</param>
        /// <param name="deviceId">Device identifier to embed as a claim</param>
        /// <returns>Signed JWT security token</returns>
        public static JwtSecurityToken CreateToken(List<Claim> authClaims, IConfiguration configuration, DateTime currentTime, string deviceId)
        {
            var secretKey = configuration["JWT:SecretKey"];
            if (string.IsNullOrWhiteSpace(secretKey))
                throw new InvalidOperationException("JWT:SecretKey is not configured. Set JWT:SecretKey in appsettings.json or environment variables.");

            // Add device ID claim to the token
            authClaims.Add(new Claim(AuthenTools.DeviceIdClaimType, deviceId));

            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            _ = int.TryParse(configuration["JWT:TokenValidityInMinutes"], out int tokenValidityInMinutes);

            var token = new JwtSecurityToken(
                issuer: configuration["JWT:ValidIssuer"],
                audience: configuration["JWT:ValidAudience"],
                expires: currentTime.AddMinutes(tokenValidityInMinutes),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                );

            return token;
        }
    }
}

using EmployeeDemo.Application.Interfaces;
using EmployeeDemo.Application.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace EmployeeDemo.Application.Utils
{
    public static class AuthenTools
    {
        /// <summary>
        /// Standard claim type URI for device ID in JWT tokens.
        /// </summary>
        public const string DeviceIdClaimType = "http://schemas.employeedemo.com/identity/claims/device_id";

        // Accept nullable identity and return nullable string to avoid Nullability warnings
        public static string? GetCurrentAccountId(ClaimsIdentity? identity)
        {
            if (identity != null)
            {
                var userClaims = identity.Claims;
                // prefer NameIdentifier (user id) claim; fall back to Name if not present
                return userClaims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier || x.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value
                       ?? userClaims.FirstOrDefault(x => x.Type == ClaimTypes.Name)?.Value;
            }
            return null;
        }

        /// <summary>
        /// Extracts device ID from JWT claims identity.
        /// </summary>
        public static string? GetDeviceIdFromClaims(ClaimsIdentity? identity)
        {
            if (identity != null)
            {
                return identity.FindFirst(DeviceIdClaimType)?.Value;
            }
            return null;
        }
    }
}

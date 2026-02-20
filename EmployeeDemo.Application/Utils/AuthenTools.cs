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
    }
}

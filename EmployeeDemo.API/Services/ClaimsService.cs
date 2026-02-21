using EmployeeDemo.Application.Interfaces;
using EmployeeDemo.Application.Utils;
using System.Security.Claims;

namespace EmployeeDemo.API.Services
{
    public class ClaimsService : IClaimsService
    {
        public ClaimsService(IHttpContextAccessor httpContextAccessor)
        {
            // Extract numeric user ID from claim (now integer)
            var identity = httpContextAccessor.HttpContext?.User?.Identity as ClaimsIdentity;
            var extractedId = AuthenTools.GetCurrentAccountId(identity);
            if (!string.IsNullOrWhiteSpace(extractedId) && int.TryParse(extractedId, out var parsedId))
            {
                GetCurrentUserId = parsedId;
            }
            else
            {
                GetCurrentUserId = 0;
            }

            // Extract device ID from JWT claims
            GetDeviceId = AuthenTools.GetDeviceIdFromClaims(identity) ?? string.Empty;

            // store principal for role checks
            UserPrincipal = httpContextAccessor.HttpContext?.User;
        }

        private ClaimsPrincipal? UserPrincipal { get; }

        public int GetCurrentUserId { get; }

        public string GetDeviceId { get; }

        public bool IsInRole(string role)
        {
            return UserPrincipal != null && UserPrincipal.IsInRole(role);
        }
    }
}

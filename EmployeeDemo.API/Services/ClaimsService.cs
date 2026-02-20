using EmployeeDemo.Application.Interfaces;
using EmployeeDemo.Application.Utils;
using System.Security.Claims;

namespace EmployeeDemo.API.Services
{
    public class ClaimsService : IClaimsService
    {
        public ClaimsService(IHttpContextAccessor httpContextAccessor)
        {
            // extract numeric user id from claim (now integer)
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
        }

        public int GetCurrentUserId { get; }
    }
}

using EmployeeDemo.Application.Interfaces;
using EmployeeDemo.Application.Utils;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace EmployeeDemo.API.Authorization;

/// <summary>
/// Validates that user session exists in Redis.
/// Returns Fail() if session invalid/expired (logout from another device).
/// </summary>
public class ValidSessionHandler : AuthorizationHandler<ValidSessionRequirement>
{
    private readonly ITokenStore _tokenStore;

    public ValidSessionHandler(ITokenStore tokenStore)
    {
        _tokenStore = tokenStore;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ValidSessionRequirement requirement)
    {
        var identity = context.User.Identity as ClaimsIdentity;
        var userIdStr = AuthenTools.GetCurrentAccountId(identity);
        var deviceId = AuthenTools.GetDeviceIdFromClaims(identity);

        // Missing claims = fail
        if (string.IsNullOrWhiteSpace(userIdStr) || string.IsNullOrWhiteSpace(deviceId))
        {
            context.Fail();
            return;
        }

        if (!int.TryParse(userIdStr, out var userId))
        {
            context.Fail();
            return;
        }

        // Check if session exists in Redis
        var session = await _tokenStore.GetSessionAsync(userId, deviceId);
        if (session == null)
        {
            // Session invalid (logged out, expired, etc.)
            context.Fail();
            return;
        }

        // Session valid
        context.Succeed(requirement);
    }
}

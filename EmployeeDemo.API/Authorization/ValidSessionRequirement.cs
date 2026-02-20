using Microsoft.AspNetCore.Authorization;

namespace EmployeeDemo.API.Authorization;

/// <summary>
/// Authorization requirement: User session must exist in Redis.
/// Ensures logout from one device invalidates session on all devices.
/// </summary>
public class ValidSessionRequirement : IAuthorizationRequirement
{
}

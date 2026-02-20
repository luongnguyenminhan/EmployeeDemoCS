namespace EmployeeDemo.Application.ViewModels;

/// <summary>
/// Response for GET /api/accounts/me endpoint.
/// Contains current user info and active sessions.
/// </summary>
public record CurrentUserSessionResponse(
    UserSessionInfo User,
    SessionModel CurrentSession,
    IReadOnlyList<SessionModel> ActiveSessions
);

/// <summary>
/// Minimal user information for session response (no sensitive data).
/// </summary>
public record UserSessionInfo(
    int Id,
    string Email,
    string Username,
    IReadOnlyList<string> Roles
);

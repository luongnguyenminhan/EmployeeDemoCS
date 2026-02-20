namespace EmployeeDemo.Application.ViewModels;

/// <summary>
/// Represents a user session/device information stored in Redis.
/// Serialized as JSON for compact storage.
/// </summary>
public record SessionModel
{
    public string DeviceId { get; init; } = string.Empty;
    public string DeviceName { get; init; } = string.Empty;
    public string IpAddress { get; init; } = string.Empty;
    public string UserAgent { get; init; } = string.Empty;
    public string OS { get; init; } = string.Empty;
    public string Browser { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime LastActivity { get; init; } = DateTime.UtcNow;
}

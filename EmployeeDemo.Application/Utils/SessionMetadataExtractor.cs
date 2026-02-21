using EmployeeDemo.Application.ViewModels;
using Microsoft.AspNetCore.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace EmployeeDemo.Application.Services;

/// <summary>
/// Extracts and generates device metadata from HTTP context for session tracking.
/// Includes fingerprint generation (deterministic device ID based on User-Agent + IP).
/// </summary>
public static class SessionMetadataExtractor
{
    /// <summary>
    /// Generates a device ID from User-Agent and IP address (deterministic fingerprint).
    /// Same User-Agent + IP = same deviceId (prevents spam login from same device).
    /// </summary>
    public static string GenerateDeviceFingerprintId(string userAgent, string ipAddress)
    {
        if (string.IsNullOrWhiteSpace(userAgent) || string.IsNullOrWhiteSpace(ipAddress))
            return Guid.NewGuid().ToString(); // Fallback if missing

        var combined = $"{userAgent}:{ipAddress}";
        using (var sha = SHA256.Create())
        {
            var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(combined));
            // Convert first 16 bytes to UUID format for consistency
            return new Guid(hash[..16]).ToString();
        }
    }

    /// <summary>
    /// Extracts session metadata from HttpContext.
    /// </summary>
    public static SessionModel ExtractFromHttpContext(HttpContext context, string deviceId)
    {
        var userAgent = context.Request.Headers["User-Agent"].ToString();
        var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

        return new SessionModel
        {
            DeviceId = deviceId,
            DeviceName = GenerateDeviceName(userAgent),
            IpAddress = ipAddress,
            UserAgent = userAgent,
            OS = ExtractOS(userAgent),
            Browser = ExtractBrowser(userAgent),
            CreatedAt = DateTime.UtcNow,
            LastActivity = DateTime.UtcNow
        };
    }

    private static string GenerateDeviceName(string userAgent)
    {
        var browser = ExtractBrowser(userAgent);
        var os = ExtractOS(userAgent);
        return $"{browser} on {os}";
    }

    private static string ExtractOS(string userAgent)
    {
        if (string.IsNullOrEmpty(userAgent)) return "Unknown";

        if (userAgent.Contains("Windows NT 10.0")) return "Windows 10";
        if (userAgent.Contains("Windows NT 11.0")) return "Windows 11";
        if (userAgent.Contains("Windows NT")) return "Windows";
        if (userAgent.Contains("Mac OS X")) return "macOS";
        if (userAgent.Contains("iPhone")) return "iOS";
        if (userAgent.Contains("iPad")) return "iPadOS";
        if (userAgent.Contains("Android")) return "Android";
        if (userAgent.Contains("Linux")) return "Linux";

        return "Unknown";
    }

    private static string ExtractBrowser(string userAgent)
    {
        if (string.IsNullOrEmpty(userAgent)) return "Unknown";

        if (userAgent.Contains("Firefox/")) return "Firefox";
        if (userAgent.Contains("Chrome/") && !userAgent.Contains("Edge")) return "Chrome";
        if (userAgent.Contains("Safari/") && !userAgent.Contains("Chrome")) return "Safari";
        if (userAgent.Contains("Edge/")) return "Edge";
        if (userAgent.Contains("Opera/") || userAgent.Contains("OPR/")) return "Opera";

        return "Unknown";
    }
}

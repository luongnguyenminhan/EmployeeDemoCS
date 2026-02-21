using EmployeeDemo.Application.ViewModels;
using System.Security.Cryptography;
using System.Text;

namespace EmployeeDemo.Application.Utils
{
    public static class DeviceFingerprintService
    {
        public static string GenerateDeviceId(DeviceMetadata metadata)
        {
            if (metadata == null || string.IsNullOrWhiteSpace(metadata.UserAgent) || string.IsNullOrWhiteSpace(metadata.IpAddress))
                return Guid.NewGuid().ToString();

            var combined = $"{metadata.UserAgent}:{metadata.IpAddress}";
            using var sha = SHA256.Create();
            var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(combined));
            return new Guid(hash[..16]).ToString();
        }
    }
}
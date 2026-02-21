namespace EmployeeDemo.Application.ViewModels
{
    /// <summary>
    /// Represents basic client information required for session tracking and
    /// device fingerprinting.  Extracted by the API layer from HttpContext.
    /// </summary>
    public record DeviceMetadata
    {
        public string UserAgent { get; init; } = "";
        public string IpAddress { get; init; } = "";
    }
}

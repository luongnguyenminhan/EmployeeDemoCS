namespace EmployeeDemo.Infrastructure.Settings
{
    /// <summary>
    /// Configuration settings for Redis connection.
    /// Binds to the "Redis" section in appsettings.json.
    /// </summary>
    public class RedisSettings
    {
        /// <summary>
        /// Redis connection string (e.g., "localhost:6379").
        /// Default is "localhost:6379" for local development.
        /// </summary>
        public string ConnectionString { get; set; } = "localhost:6379";
    }
}

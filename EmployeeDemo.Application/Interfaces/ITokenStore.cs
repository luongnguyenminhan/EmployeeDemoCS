namespace EmployeeDemo.Application.Interfaces
{
    /// <summary>
    /// Defines the contract for storing and retrieving refresh tokens.
    /// This interface abstracts the token storage mechanism (e.g., Redis, in-memory cache).
    /// Implementation should handle HMAC-hashed refresh tokens with TTL-based expiration.
    /// </summary>
    public interface ITokenStore
    {
        /// <summary>
        /// Stores a hashed refresh token for a specific user and device.
        /// The token is stored with automatic expiration (TTL).
        /// </summary>
        /// <param name="userId">The user ID (Account.Id)</param>
        /// <param name="deviceId">The device identifier (UUID)</param>
        /// <param name="hashedRefreshToken">The HMAC-SHA256 hashed refresh token</param>
        /// <param name="expiryInSeconds">Token expiration time in seconds (e.g., 604800 for 7 days)</param>
        /// <returns>Completed task</returns>
        Task StoreAsync(int userId, string deviceId, string hashedRefreshToken, int expiryInSeconds);

        /// <summary>
        /// Retrieves the hashed refresh token for a specific user and device.
        /// </summary>
        /// <param name="userId">The user ID (Account.Id)</param>
        /// <param name="deviceId">The device identifier (UUID)</param>
        /// <returns>The hashed refresh token if found; null otherwise</returns>
        Task<string?> GetAsync(int userId, string deviceId);

        /// <summary>
        /// Removes the refresh token for a specific user and device (logout from single device).
        /// </summary>
        /// <param name="userId">The user ID (Account.Id)</param>
        /// <param name="deviceId">The device identifier (UUID)</param>
        /// <returns>Completed task</returns>
        Task RemoveAsync(int userId, string deviceId);

        /// <summary>
        /// Removes all refresh tokens for a specific user across all devices (logout all devices).
        /// </summary>
        /// <param name="userId">The user ID (Account.Id)</param>
        /// <returns>Completed task</returns>
        Task RemoveAllAsync(int userId);
    }
}

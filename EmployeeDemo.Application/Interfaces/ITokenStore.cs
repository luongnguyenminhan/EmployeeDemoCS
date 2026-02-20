using EmployeeDemo.Application.ViewModels;

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

        /// <summary>
        /// Stores session metadata (device info) for a user device.
        /// Uses same TTL as refresh token.
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <param name="deviceId">The device identifier</param>
        /// <param name="session">Session metadata to store</param>
        /// <param name="expiryInSeconds">Session expiration time in seconds</param>
        Task StoreSessionAsync(int userId, string deviceId, SessionModel session, int expiryInSeconds);

        /// <summary>
        /// Retrieves session metadata for a user device.
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <param name="deviceId">The device identifier</param>
        /// <returns>Session metadata if found; null otherwise</returns>
        Task<SessionModel?> GetSessionAsync(int userId, string deviceId);

        /// <summary>
        /// Retrieves all active sessions for a user.
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <returns>List of all active sessions (may be empty)</returns>
        Task<List<SessionModel>> GetAllSessionsAsync(int userId);

        /// <summary>
        /// Updates the LastActivity timestamp for a session.
        /// Called after token refresh to track activity.
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <param name="deviceId">The device identifier</param>
        /// <param name="lastActivityTime">The new last activity time</param>
        Task UpdateLastActivityAsync(int userId, string deviceId, DateTime lastActivityTime);
    }
}

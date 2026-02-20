using EmployeeDemo.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;
using System.Security.Cryptography;
using System.Text;

namespace EmployeeDemo.Infrastructure.Services
{
    /// <summary>
    /// Redis-based implementation of ITokenStore.
    /// Stores refresh tokens with HMAC-SHA256 hashing and automatic TTL expiration.
    /// Key format: refresh:{userId}:{deviceId}
    /// </summary>
    public class RedisTokenStore : ITokenStore
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly IConfiguration _configuration;

        public RedisTokenStore(IConnectionMultiplexer redis, IConfiguration configuration)
        {
            _redis = redis;
            _configuration = configuration;
        }

        /// <summary>
        /// Stores a hashed refresh token in Redis with expiration (EX command).
        /// </summary>
        public async Task StoreAsync(int userId, string deviceId, string hashedRefreshToken, int expiryInSeconds)
        {
            var db = _redis.GetDatabase();
            var key = GenerateKey(userId, deviceId);

            // SET key value EX expiryInSeconds
            await db.StringSetAsync(key, hashedRefreshToken, TimeSpan.FromSeconds(expiryInSeconds));
        }

        /// <summary>
        /// Retrieves the hashed refresh token from Redis.
        /// Returns null if key doesn't exist or has expired.
        /// </summary>
        public async Task<string?> GetAsync(int userId, string deviceId)
        {
            var db = _redis.GetDatabase();
            var key = GenerateKey(userId, deviceId);

            var value = await db.StringGetAsync(key);
            return value.IsNull ? null : value.ToString();
        }

        /// <summary>
        /// Removes a specific device's refresh token (logout from single device).
        /// </summary>
        public async Task RemoveAsync(int userId, string deviceId)
        {
            var db = _redis.GetDatabase();
            var key = GenerateKey(userId, deviceId);

            await db.KeyDeleteAsync(key);
        }

        /// <summary>
        /// Removes all refresh tokens for a user across all devices (logout all devices).
        /// Uses SCAN + UNLINK for efficient pattern-based deletion.
        /// </summary>
        public async Task RemoveAllAsync(int userId)
        {
            var server = _redis.GetServer(_redis.GetEndPoints().First());
            var pattern = $"refresh:{userId}:*";

            // Scan for all keys matching the pattern
            var keys = server.Keys(pattern: pattern);

            if (keys.Any())
            {
                var db = _redis.GetDatabase();
                await db.KeyDeleteAsync(keys.ToArray());
            }
        }

        /// <summary>
        /// Generates the Redis key for a user's device refresh token.
        /// </summary>
        private static string GenerateKey(int userId, string deviceId)
        {
            return $"refresh:{userId}:{deviceId}";
        }

        /// <summary>
        /// Hashes a refresh token using HMAC-SHA256.
        /// Called by AccountRepository before storing.
        /// </summary>
        public static string HashRefreshToken(string refreshToken, string hashKey)
        {
            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(hashKey)))
            {
                var hashedBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(refreshToken));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        /// <summary>
        /// Verifies a refresh token against its hash.
        /// Called by AccountRepository during token refresh.
        /// </summary>
        public static bool VerifyRefreshToken(string refreshToken, string storedHash, string hashKey)
        {
            var computedHash = HashRefreshToken(refreshToken, hashKey);
            return computedHash == storedHash;
        }
    }
}

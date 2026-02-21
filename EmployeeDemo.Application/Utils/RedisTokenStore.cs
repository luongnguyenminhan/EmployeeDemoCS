using EmployeeDemo.Application.ViewModels;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace EmployeeDemo.Application.Utils
{
    /// <summary>
    /// Redis-based helper for refresh token operations.
    /// Stores refresh tokens with HMAC-SHA256 hashing and automatic TTL expiration.
    /// Key format: refresh:{userId}:{deviceId}
    /// </summary>
    public class RedisTokenStore
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
        /// Removes a specific device's refresh token and session (logout from single device).
        /// </summary>
        public async Task RemoveAsync(int userId, string deviceId)
        {
            var db = _redis.GetDatabase();
            var refreshKey = GenerateKey(userId, deviceId);
            var sessionKey = GenerateSessionKey(userId, deviceId);

            await db.KeyDeleteAsync(new RedisKey[] { refreshKey, sessionKey });
        }

        /// <summary>
        /// Removes all refresh tokens and sessions for a user across all devices (logout all devices).
        /// Uses SCAN + DELETE for efficient pattern-based deletion.
        /// </summary>
        public async Task RemoveAllAsync(int userId)
        {
            var server = _redis.GetServer(_redis.GetEndPoints().First());
            var refreshPattern = $"refresh:{userId}:*";
            var sessionPattern = $"session:{userId}:*";

            // Scan for all refresh tokens
            var refreshKeys = server.Keys(pattern: refreshPattern);
            // Scan for all sessions
            var sessionKeys = server.Keys(pattern: sessionPattern);

            var allKeys = new List<RedisKey>();
            allKeys.AddRange(refreshKeys);
            allKeys.AddRange(sessionKeys);

            if (allKeys.Any())
            {
                var db = _redis.GetDatabase();
                await db.KeyDeleteAsync(allKeys.ToArray());
            }
        }

        /// <summary>
        /// Stores session metadata as JSON for a user device.
        /// </summary>
        public async Task StoreSessionAsync(int userId, string deviceId, SessionModel session, int expiryInSeconds)
        {
            var db = _redis.GetDatabase();
            var key = GenerateSessionKey(userId, deviceId);
            var json = JsonSerializer.Serialize(session);

            await db.StringSetAsync(key, json, TimeSpan.FromSeconds(expiryInSeconds));
        }

        /// <summary>
        /// Retrieves session metadata from Redis.
        /// </summary>
        public async Task<SessionModel?> GetSessionAsync(int userId, string deviceId)
        {
            var db = _redis.GetDatabase();
            var key = GenerateSessionKey(userId, deviceId);

            var value = await db.StringGetAsync(key);
            return value.IsNull ? null : JsonSerializer.Deserialize<SessionModel>(value.ToString());
        }

        /// <summary>
        /// Retrieves all active sessions for a user by scanning session keys.
        /// </summary>
        public async Task<List<SessionModel>> GetAllSessionsAsync(int userId)
        {
            var server = _redis.GetServer(_redis.GetEndPoints().First());
            var pattern = $"session:{userId}:*";
            var keys = server.Keys(pattern: pattern);

            if (!keys.Any())
                return [];

            var db = _redis.GetDatabase();
            var sessions = new List<SessionModel>();

            foreach (var key in keys)
            {
                var value = await db.StringGetAsync(key);
                if (!value.IsNull)
                {
                    var session = JsonSerializer.Deserialize<SessionModel>(value.ToString());
                    if (session != null)
                        sessions.Add(session);
                }
            }

            return sessions;
        }

        /// <summary>
        /// Updates LastActivity timestamp for a session.
        /// </summary>
        public async Task UpdateLastActivityAsync(int userId, string deviceId, DateTime lastActivityTime)
        {
            var db = _redis.GetDatabase();
            var key = GenerateSessionKey(userId, deviceId);

            var value = await db.StringGetAsync(key);
            if (!value.IsNull)
            {
                var session = JsonSerializer.Deserialize<SessionModel>(value.ToString());
                if (session != null)
                {
                    var updated = session with { LastActivity = lastActivityTime };
                    var json = JsonSerializer.Serialize(updated);
                    var ttl = await db.KeyTimeToLiveAsync(key);
                    await db.StringSetAsync(key, json, ttl);
                }
            }
        }

        /// <summary>
        /// Generates the Redis key for a user's device session.
        /// </summary>
        private static string GenerateSessionKey(int userId, string deviceId)
        {
            return $"session:{userId}:{deviceId}";
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

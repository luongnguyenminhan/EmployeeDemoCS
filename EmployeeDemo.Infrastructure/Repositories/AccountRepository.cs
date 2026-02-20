using EmployeeDemo.Application.Interfaces;
using EmployeeDemo.Application.Repositories;
using EmployeeDemo.Application.Utils;
using EmployeeDemo.Application.ViewModels;
using EmployeeDemo.Application.ViewModels.AccountViewModels;
using EmployeeDemo.Application.ViewModels.ResponseModels;
using EmployeeDemo.Domain.Entities;
using EmployeeDemo.Domain.Enums;
using EmployeeDemo.Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace EmployeeDemo.Infrastructure.Repositories
{
    public class AccountRepository : IAccountRepository
    {
        private readonly AppDbContext _dbContext;
        private readonly ICurrentTime _timeService;
        private readonly IPasswordHasher<Account> _passwordHasher;
        private readonly IConfiguration _configuration;
        private readonly IClaimsService _claimsService;
        private readonly ITokenStore _tokenStore;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AccountRepository(AppDbContext dbContext,
            ICurrentTime timeService,
            IClaimsService claimsService,
            IPasswordHasher<Account> passwordHasher,
            IConfiguration configuration,
            ITokenStore tokenStore,
            IHttpContextAccessor httpContextAccessor)
        {
            _dbContext = dbContext;
            _claimsService = claimsService;
            _timeService = timeService;
            _passwordHasher = passwordHasher;
            _configuration = configuration;
            _tokenStore = tokenStore;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<ResponseModel> AddAccount(AccountLoginDTO account, RoleEnums role)
        {
            try
            {
                var existing = await _dbContext.Accounts.FirstOrDefaultAsync(a => a.Email == account.Email);
                if (existing != null)
                    return new ResponseModel { Status = false, Message = "Username already exists" };

                // create domain profile (Account) and hash password
                var domainAccount = new Account
                {
                    Email = account.Email,
                    UserName = account.Email,
                    CreatedBy = _claimsService.GetCurrentUserId,
                    CreationDate = _timeService.GetCurrentTime(),
                    SecurityStamp = Guid.NewGuid().ToString()
                };

                domainAccount.PasswordHash = _passwordHasher.HashPassword(domainAccount, account.Password);
                _dbContext.Accounts.Add(domainAccount);
                await _dbContext.SaveChangesAsync();

                // ensure requested role exists and assign it to the account (using domain Role / AccountRole)
                var roleName = role.ToString();
                var roleEntity = await _dbContext.Roles.FirstOrDefaultAsync(r => r.Name == roleName);
                if (roleEntity == null)
                {
                    roleEntity = new Role { Name = roleName };
                    _dbContext.Roles.Add(roleEntity);
                    await _dbContext.SaveChangesAsync();
                }

                _dbContext.AccountRoles.Add(new AccountRole { AccountId = domainAccount.Id, RoleId = roleEntity.Id });

                // // ensure USER role exists and assign as default (unless it is the same as the requested role)
                // var userRole = await _dbContext.Roles.FirstOrDefaultAsync(r => r.Name == RoleEnums.USER.ToString());
                // if (userRole == null)
                // {
                //     userRole = new Role { Name = RoleEnums.USER.ToString() };
                //     _dbContext.Roles.Add(userRole);
                //     await _dbContext.SaveChangesAsync();
                // }
                // _dbContext.AccountRoles.Add(new AccountRole { AccountId = domainAccount.Id, RoleId = userRole.Id });

                return new ResponseModel { Status = true, Message = "Your account is ready. Try to login now." };
            }
            catch (Exception ex)
            {
                return new ResponseModel { Status = false, Message = "Error: " + ex.Message };
            }
        }

        public async Task<Account?> GetAccountByEmailAsync(string email)
        {
            // return domain Account (profile) by email
            return await _dbContext.Accounts.FirstOrDefaultAsync(a => a.Email == email);
        }

        public async Task<ResponseLoginModel> GetUserByEmailAndPassword(AccountLoginDTO account)
        {
            var accountEntity = await _dbContext.Accounts
                .Include(a => a.AccountRoles).ThenInclude(ar => ar.Role)
                .FirstOrDefaultAsync(a => a.Email == account.Email);

            if (accountEntity == null)
                return new ResponseLoginModel { Status = false, Message = "Incorrect email or password" };

            var verify = _passwordHasher.VerifyHashedPassword(accountEntity, accountEntity.PasswordHash, account.Password);
            if (verify != PasswordVerificationResult.Success)
                return new ResponseLoginModel { Status = false, Message = "Incorrect email or password" };

            var roles = accountEntity.AccountRoles?.Select(ar => ar.Role.Name).ToList() ?? new List<string>();

            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, accountEntity.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };
            foreach (var r in roles) authClaims.Add(new Claim(ClaimTypes.Role, r));

            // Generate device fingerprint based on User-Agent + IP (deterministic, prevents spam)
            var httpContext = _httpContextAccessor.HttpContext;
            var userAgent = httpContext?.Request.Headers["User-Agent"].ToString() ?? "";
            var ipAddress = httpContext?.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var deviceId = SessionMetadataExtractor.GenerateDeviceFingerprintId(userAgent, ipAddress);

            // Check if this device already has an active session
            var existingSession = await _tokenStore.GetSessionAsync(accountEntity.Id, deviceId);

            // Generate JWT access token with deviceId embedded as a claim
            var token = GenerateJWTToken.CreateToken(authClaims, _configuration, _timeService.GetCurrentTime(), deviceId);

            // Generate refresh token (raw string)
            var refreshToken = TokenTools.GenerateRefreshToken();

            // Hash refresh token for Redis storage
            var hashKey = _configuration["JWT:RefreshTokenHashKey"];
            if (string.IsNullOrWhiteSpace(hashKey))
                return new ResponseLoginModel { Status = false, Message = "JWT:RefreshTokenHashKey not configured" };

            var hashedRefreshToken = RedisTokenStore.HashRefreshToken(refreshToken, hashKey);

            // Calculate TTL in seconds (RefreshTokenValidityInDays * 86400)
            _ = int.TryParse(_configuration["JWT:RefreshTokenValidityInDays"], out int refreshTokenValidityInDays);
            int expiryInSeconds = (refreshTokenValidityInDays > 0 ? refreshTokenValidityInDays : 7) * 86400;

            // Store hashed token in Redis with TTL
            try
            {
                await _tokenStore.StoreAsync(accountEntity.Id, deviceId, hashedRefreshToken, expiryInSeconds);

                // Store/update session metadata (device info) with same TTL
                if (httpContext != null)
                {
                    // If session exists, preserve CreatedAt; if new, set current time
                    var session = existingSession ?? SessionMetadataExtractor.ExtractFromHttpContext(httpContext, deviceId);
                    await _tokenStore.StoreSessionAsync(accountEntity.Id, deviceId, session, expiryInSeconds);
                }
            }
            catch (Exception ex)
            {
                return new ResponseLoginModel { Status = false, Message = $"Token storage failed: {ex.Message}" };
            }

            return new ResponseLoginModel
            {
                Status = true,
                Message = "Login successfully",
                JWT = new JwtSecurityTokenHandler().WriteToken(token),
                Expired = token.ValidTo,
                JWTRefreshToken = refreshToken,
                DeviceId = deviceId
            };
        }

        public async Task<ResponseLoginModel> RefreshToken(TokenModel token)
        {
            if (token is null)
            {
                return new ResponseLoginModel
                {
                    Status = false,
                    Message = "Token is null"
                };
            }

            string? accessToken = token.AccessToken;
            string? refreshToken = token.RefreshToken;

            var principal = TokenTools.GetPrincipalFromExpiredToken(accessToken, _configuration);
            if (principal == null)
            {
                return new ResponseLoginModel
                {
                    Status = false,
                    Message = "Invalid access token or refresh token!"
                };
            }

            var accountIdStr = principal.Identity?.Name;
            if (!int.TryParse(accountIdStr, out var accountId))
            {
                return new ResponseLoginModel { Status = false, Message = "Invalid account id in token." };
            }

            // Extract device ID from JWT claims
            var identity = principal.Identity as ClaimsIdentity;
            var deviceId = AuthenTools.GetDeviceIdFromClaims(identity);
            if (string.IsNullOrWhiteSpace(deviceId))
            {
                return new ResponseLoginModel
                {
                    Status = false,
                    Message = "Device ID not found in token. Token may be malformed or expired."
                };
            }

            // Retrieve stored hash from Redis
            var storedHash = await _tokenStore.GetAsync(accountId, deviceId);
            if (storedHash == null)
            {
                return new ResponseLoginModel
                {
                    Status = false,
                    Message = "Invalid access token or refresh token!"
                };
            }

            // Hash incoming refresh token and compare
            var hashKey = _configuration["JWT:RefreshTokenHashKey"];
            if (string.IsNullOrWhiteSpace(hashKey))
                return new ResponseLoginModel { Status = false, Message = "JWT:RefreshTokenHashKey not configured" };

            bool isValid = RedisTokenStore.VerifyRefreshToken(refreshToken, storedHash, hashKey);
            if (!isValid)
            {
                return new ResponseLoginModel
                {
                    Status = false,
                    Message = "Invalid access token or refresh token!"
                };
            }

            // Generate new access token
            var newAccessToken = GenerateJWTToken.CreateToken(principal.Claims.ToList(), _configuration, _timeService.GetCurrentTime(), deviceId);

            // Generate new refresh token
            var newRefreshToken = TokenTools.GenerateRefreshToken();

            // Hash new refresh token
            var hashedNewRefreshToken = RedisTokenStore.HashRefreshToken(newRefreshToken, hashKey);

            // Calculate TTL in seconds
            _ = int.TryParse(_configuration["JWT:RefreshTokenValidityInDays"], out int refreshTokenValidityInDays);
            int expiryInSeconds = (refreshTokenValidityInDays > 0 ? refreshTokenValidityInDays : 7) * 86400;

            // Store new hashed token in Redis (overwrites old one)
            try
            {
                await _tokenStore.StoreAsync(accountId, deviceId, hashedNewRefreshToken, expiryInSeconds);
                
                // Update last activity timestamp for the session
                await _tokenStore.UpdateLastActivityAsync(accountId, deviceId, DateTime.UtcNow);
            }
            catch (Exception ex)
            {
                return new ResponseLoginModel { Status = false, Message = $"Token storage failed: {ex.Message}" };
            }

            return new ResponseLoginModel
            {
                Status = true,
                Message = "Refresh Token successfully!",
                JWT = new JwtSecurityTokenHandler().WriteToken(newAccessToken),
                Expired = newAccessToken.ValidTo,
                JWTRefreshToken = newRefreshToken,
                DeviceId = deviceId
            };
        }

        /// <summary>
        /// Logout from a specific device (removes refresh token for that device).
        /// </summary>
        public async Task LogoutAsync(int userId, string deviceId)
        {
            try
            {
                await _tokenStore.RemoveAsync(userId, deviceId);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Logout failed: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Logout from all devices (removes all refresh tokens for the user).
        /// </summary>
        public async Task LogoutAllDevicesAsync(int userId)
        {
            try
            {
                await _tokenStore.RemoveAllAsync(userId);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Logout all failed: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Gets current user info with all active sessions.
        /// </summary>
        public async Task<CurrentUserSessionResponse?> GetCurrentUserWithSessionsAsync(int userId, string currentDeviceId)
        {
            var account = await _dbContext.Accounts
                .AsNoTracking()
                .Include(a => a.AccountRoles).ThenInclude(ar => ar.Role)
                .FirstOrDefaultAsync(a => a.Id == userId && !a.IsDeleted);

            if (account == null)
                return null;

            var roles = account.AccountRoles?.Select(ar => ar.Role.Name).ToList() ?? [];
            var userInfo = new UserSessionInfo(account.Id, account.Email ?? "", account.UserName ?? "", roles.AsReadOnly());

            var currentSession = await _tokenStore.GetSessionAsync(userId, currentDeviceId);
            var allSessions = await _tokenStore.GetAllSessionsAsync(userId);

            return new CurrentUserSessionResponse(userInfo, currentSession, allSessions.AsReadOnly());
        }
    }
}

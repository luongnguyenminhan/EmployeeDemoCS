using EmployeeDemo.Application.Interfaces;
using EmployeeDemo.Application.Repositories;
using EmployeeDemo.Application.Utils;
using EmployeeDemo.Application.ViewModels;
using EmployeeDemo.Application.ViewModels.AccountViewModels;
using EmployeeDemo.Application.ViewModels.ResponseModels;
using EmployeeDemo.Domain.Entities;
using EmployeeDemo.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.WebSockets;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace EmployeeDemo.Infrastructure.Repositories
{
    public class AccountRepository : IAccountRepository
    {
        private readonly AppDbContext _dbContext;
        private readonly ICurrentTime _timeService;
        private readonly UserManager<Account> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<Account> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly IClaimsService _claimsService;

        public AccountRepository(AppDbContext dbContext,
            ICurrentTime timeService,
            IClaimsService claimsService,
            UserManager<Account> userManager,
            IConfiguration configuration,
            RoleManager<IdentityRole> roleManager,
            SignInManager<Account> signInManager)
        {
            _dbContext = dbContext;
            _claimsService = claimsService;
            _timeService = timeService;
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _configuration = configuration;
        }
        public async Task<ResponseModel> AddAccount(AccountLoginDTO account, RoleEnums role)
        {
            try
            {
                var userExist = await _userManager.FindByEmailAsync(account.Email);
                if (userExist != null)
                {
                    return new ResponseModel { Status = false, Message = "Username is already exist" };
                }
                var user = new Account
                {
                    Email = account.Email,
                    UserName = account.Email,
                    CreatedBy = _claimsService.GetCurrentUserId,
                    CreationDate = _timeService.GetCurrentTime(),
                };

                //var result = await _userManager.CreateAsync(user, account.Password);
                if (role.Equals(RoleEnums.ADMIN) || role.Equals(RoleEnums.STAFF) || role.Equals(RoleEnums.USER))
                {
                    await _userManager.CreateAsync(user, account.Password);

                    if (!await _roleManager.RoleExistsAsync(role.ToString()))
                    {
                        await _roleManager.CreateAsync(new IdentityRole(role.ToString()));
                    }

                    if (await _roleManager.RoleExistsAsync(role.ToString()))
                    {
                        await _userManager.AddToRoleAsync(user, role.ToString());
                    }

                    if (!await _roleManager.RoleExistsAsync(RoleEnums.USER.ToString()))
                        await _roleManager.CreateAsync(new IdentityRole(RoleEnums.USER.ToString()));

                    if (await _roleManager.RoleExistsAsync(RoleEnums.USER.ToString()))
                    {
                        await _userManager.AddToRoleAsync(user, RoleEnums.USER.ToString());
                    }

                    return new ResponseModel { Status = true, Message = "Your account is ready. Try to login now." };
                }

                return new ResponseModel { Status = true, Message = "Error" };

            }
            catch (Exception ex)
            {
                return new ResponseModel { Status = false, Message = "Error: " + ex.Message };
            }

        }

        public async Task<Account> GetAccountByEmailAsync(string email)
        {
            return await _userManager.FindByEmailAsync(email);
        }

        public async Task<ResponseLoginModel> GetUserByEmailAndPassword(AccountLoginDTO account)
        {
            var result = await _signInManager.PasswordSignInAsync(account.Email, account.Password, false, false);
            var accountExist = await _userManager.FindByNameAsync(account.Email);

            if (result.Succeeded)
            {
                var roles = await _userManager.GetRolesAsync(accountExist);

                var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, accountExist.Id),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                };

                foreach (var role in roles)
                {
                    authClaims.Add(new Claim(ClaimTypes.Role, role));
                }

                //generate refresh token
                var refreshToken = TokenTools.GenerateRefreshToken();

                _ = int.TryParse(_configuration["JWT:RefreshTokenValidityInDays"], out int refreshTokenValidityInDays);

                accountExist.RefreshToken = refreshToken;
                accountExist.RefreshTokenExpiryTime = DateTime.Now.AddDays(refreshTokenValidityInDays);

                await _userManager.UpdateAsync(accountExist);

                var token = GenerateJWTToken.CreateToken(authClaims, _configuration, _timeService.GetCurrentTime());

                return new ResponseLoginModel
                {
                    Status = true,
                    Message = "Login successfully",
                    JWT = new JwtSecurityTokenHandler().WriteToken(token),
                    Expired = token.ValidTo,
                    JWTRefreshToken = refreshToken,
                };
            }
            else
            {
                return new ResponseLoginModel
                {
                    Status = false,
                    Message = "Incorrect email or password",
                };
            }
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

            string accountId = principal.Identity.Name;

            var account = await _userManager.FindByIdAsync(accountId);

            if (account == null || account.RefreshToken != refreshToken || account.RefreshTokenExpiryTime <= DateTime.Now)
            {
                return new ResponseLoginModel
                {
                    Status = false,
                    Message = "Invalid access token or refresh token!"
                };
            }

            var newAccessToken = GenerateJWTToken.CreateToken(principal.Claims.ToList(), _configuration, _timeService.GetCurrentTime());
            var newRefreshToken = TokenTools.GenerateRefreshToken();

            account.RefreshToken = newRefreshToken;
            await _userManager.UpdateAsync(account);

            return new ResponseLoginModel
            {
                Status = true,
                Message = "Refresh Token successfully!",
                JWT = new JwtSecurityTokenHandler().WriteToken(newAccessToken),
                Expired = newAccessToken.ValidTo,
                JWTRefreshToken = newRefreshToken
            };
        }
    }
}

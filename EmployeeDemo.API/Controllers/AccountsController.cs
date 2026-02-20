using EmployeeDemo.Application.Interfaces;
using EmployeeDemo.Application.ViewModels;
using EmployeeDemo.Application.ViewModels.AccountViewModels;
using EmployeeDemo.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Security.Claims;

namespace EmployeeDemo.API.Controllers
{
    [Route("api/accounts")]
    [ApiController]
    public class AccountsController : ControllerBase
    {
        private readonly IAccountService _accountService;
        private readonly IClaimsService _claimsService;

        public AccountsController(IAccountService accountService, IClaimsService claimsService)
        {
            _accountService = accountService;
            _claimsService = claimsService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterAsync(AccountLoginDTO accountLoginDTO, [FromQuery] RoleEnums role)
        {
            try
            {
                return Ok(await _accountService.ResigerAsync(accountLoginDTO, role));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync(AccountLoginDTO account)
        {
            try
            {
                var result = await _accountService.LoginAsync(account);
                if (result.Status)
                {
                    return Ok(result);
                }
                return Unauthorized(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("refresh-token")]
        [Authorize]
        public async Task<IActionResult> RefreshToken([FromBody] TokenModel token)
        {
            try
            {
                var result = await _accountService.RefreshToken(token);
                if (result.Status)
                {
                    return Ok(result);
                }
                return Unauthorized(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> LogoutAsync()
        {
            try
            {
                var userId = _claimsService.GetCurrentUserId;
                var deviceId = _claimsService.GetDeviceId;

                if (userId == 0 || string.IsNullOrWhiteSpace(deviceId))
                    return Unauthorized(new { Status = false, Message = "User or device not authenticated" });

                await _accountService.LogoutAsync(userId, deviceId);
                return Ok(new { Status = true, Message = "Logged out successfully from device" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Status = false, Message = ex.Message });
            }
        }

        [HttpPost("logout-all-devices")]
        [Authorize]
        public async Task<IActionResult> LogoutAllDevicesAsync()
        {
            try
            {
                var userId = _claimsService.GetCurrentUserId;
                if (userId == 0)
                    return Unauthorized(new { Status = false, Message = "User not authenticated" });

                await _accountService.LogoutAllDevicesAsync(userId);
                return Ok(new { Status = true, Message = "Logged out successfully from all devices" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Status = false, Message = ex.Message });
            }
        }
    }
}

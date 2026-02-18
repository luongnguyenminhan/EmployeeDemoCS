using EmployeeDemo.Application.Interfaces;
using EmployeeDemo.Application.ViewModels;
using EmployeeDemo.Application.ViewModels.AccountViewModels;
using EmployeeDemo.Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace EmployeeDemo.API.Controllers
{
    [Route("api/accounts")]
    [ApiController]
    public class AccountsController : ControllerBase
    {
        private readonly IAccountService _accountService;

        public AccountsController(IAccountService accountService)
        {
            _accountService = accountService;
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
        public async Task<IActionResult> RefreshToken(TokenModel token)
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
    }
}

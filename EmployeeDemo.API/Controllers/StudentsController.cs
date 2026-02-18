using EmployeeDemo.Application.Interfaces;
using EmployeeDemo.Application.Services;
using EmployeeDemo.Application.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeDemo.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudentsController : ControllerBase
    {
        private readonly IStudentService _studentService;

        public StudentsController(IStudentService studentService) 
        {
            _studentService = studentService;
        }
        [HttpPost]
        public async Task<IActionResult> LoginWithGoogleAsync(string email)
        {
            try
            {
                var result = await _studentService.LoginWithEmailAsync(email);
                if (!result.Status)
                {
                    return Unauthorized(result);
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("refresh-token-student")]
        public async Task<IActionResult> RefreshToken(TokenModel token)
        {
            try
            {
                var result = await _studentService.RefreshToken(token);
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

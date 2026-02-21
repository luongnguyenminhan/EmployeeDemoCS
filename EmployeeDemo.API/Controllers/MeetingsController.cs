using EmployeeDemo.Application.Interfaces;
using EmployeeDemo.Application.ViewModels.MeetingViewModels;
using EmployeeDemo.Application.ViewModels.ResponseModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace EmployeeDemo.API.Controllers
{
    [Route("api/meetings")]
    [ApiController]
    public class MeetingsController : ControllerBase
    {
        private readonly IMeetingService _meetingService;

        public MeetingsController(IMeetingService meetingService)
        {
            _meetingService = meetingService;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateAsync(MeetingCreateDTO dto)
        {
            try
            {
                var result = await _meetingService.CreateMeetingAsync(dto);
                if (result.Status)
                {
                    return Ok(result);
                }
                else
                {
                    // validation or business error
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                // unexpected error
                return BadRequest(new ResponseModel { Status = false, Message = ex.Message });
            }
        }
    }
}
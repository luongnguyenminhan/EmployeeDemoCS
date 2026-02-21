using EmployeeDemo.Application.Interfaces;
using EmployeeDemo.Application.ViewModels.MeetingViewModels;
using EmployeeDemo.Application.ViewModels.ResponseModels;
using EmployeeDemo.Application.Commons;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
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

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetPaginationAsync([FromQuery] PaginationParameter paginationParameter)
        {
            try
            {
                var result = await _meetingService.GetMeetingPaginationAsync(paginationParameter);
                var metadata = new
                {
                    result.TotalCount,
                    result.PageSize,
                    result.CurrentPage,
                    result.TotalPages,
                    result.HasNext,
                    result.HasPrevious
                };
                Response.Headers["X-Pagination"] = JsonConvert.SerializeObject(metadata);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseModel { Status = false, Message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            try
            {
                var result = await _meetingService.GetMeetingByIdAsync(id);
                if (result.Status)
                {
                    return Ok(result);
                }
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseModel { Status = false, Message = ex.Message });
            }
        }
    }
}
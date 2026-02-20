using EmployeeDemo.Application.Interfaces;
using EmployeeDemo.Application.Repositories;
using EmployeeDemo.Application.ViewModels.MeetingViewModels;
using EmployeeDemo.Application.ViewModels.ResponseModels;
using EmployeeDemo.Domain.Entities;
using System;
using System.Threading.Tasks;

namespace EmployeeDemo.Application.Services
{
    public class MeetingService : IMeetingService
    {
        private readonly IMeetingRepository _meetingRepository;
        private readonly IClaimsService _claimsService;
        private readonly ICurrentTime _currentTime;

        public MeetingService(
            IMeetingRepository meetingRepository,
            IClaimsService claimsService,
            ICurrentTime currentTime)
        {
            _meetingRepository = meetingRepository;
            _claimsService = claimsService;
            _currentTime = currentTime;
        }

        public async Task<ResponseModel> CreateMeetingAsync(MeetingCreateDTO dto)
        {
            // basic validation
            if (string.IsNullOrWhiteSpace(dto.Title))
            {
                return new ResponseModel { Status = false, Message = "Title is required" };
            }

            var now = _currentTime.GetCurrentTime();
            if (dto.StartTime <= now)
            {
                return new ResponseModel { Status = false, Message = "StartTime must be in the future" };
            }

            if (dto.EndTime.HasValue && dto.EndTime.Value <= dto.StartTime)
            {
                return new ResponseModel { Status = false, Message = "EndTime must be after StartTime" };
            }

            // host determination
            var userId = _claimsService.GetCurrentUserId;
            if (userId == 0)
            {
                return new ResponseModel { Status = false, Message = "User is not authenticated" };
            }

            // construct entity
            var meeting = new Meeting
            {
                Title = dto.Title.Trim(),
                Description = dto.Description,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime ?? dto.StartTime,
                HostId = userId,
                CreatedBy = userId,
                CreationDate = now
            };

            var saved = await _meetingRepository.AddMeetingAsync(meeting);

            var responseDto = new MeetingResponseDTO
            {
                Id = saved.Id,
                Title = saved.Title,
                Description = saved.Description,
                StartTime = saved.StartTime,
                EndTime = saved.EndTime,
                HostId = saved.HostId,
                CreationDate = saved.CreationDate
            };

            return new ResponseModel { Status = true, Message = "Meeting created", Data = responseDto };
        }
    }
}

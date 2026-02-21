using EmployeeDemo.Application.Interfaces;
using EmployeeDemo.Application.Repositories;
using EmployeeDemo.Application.ViewModels.MeetingViewModels;
using EmployeeDemo.Application.ViewModels.ResponseModels;
using EmployeeDemo.Domain.Entities;
using EmployeeDemo.Application;
using System;
using System.Linq;
using System.Threading.Tasks;
using EmployeeDemo.Application.Commons;

namespace EmployeeDemo.Application.Services
{
    public class MeetingService : IMeetingService
    {
        private readonly IMeetingRepository _meetingRepository;
        private readonly IClaimsService _claimsService;
        private readonly ICurrentTime _currentTime;
        private readonly IUnitOfWork _unitOfWork;

        public MeetingService(
            IMeetingRepository meetingRepository,
            IClaimsService claimsService,
            ICurrentTime currentTime,
            IUnitOfWork unitOfWork)
        {
            _meetingRepository = meetingRepository;
            _claimsService = claimsService;
            _currentTime = currentTime;
            _unitOfWork = unitOfWork;
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
            // end time no longer supplied by client; service will treat meeting as instantaneous or compute later

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
                EndTime = dto.StartTime, // default to start time (no duration)
                HostId = userId,
                CreatedBy = userId,
                CreationDate = now
            };

            await _meetingRepository.AddAsync(meeting);
            await _unitOfWork.SaveChangeAsync();
            var saved = meeting;

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

        public async Task<Pagination<MeetingResponseDTO>> GetMeetingPaginationAsync(PaginationParameter paginationParameter)
        {
            var userId = _claimsService.GetCurrentUserId;
            bool isAdmin = _claimsService.IsInRole("ADMIN");

            Pagination<Meeting> queryResult;
            if (isAdmin)
            {
                queryResult = await _meetingRepository.ToPagination(paginationParameter);
            }
            else
            {
                queryResult = await _meetingRepository.GetUserMeetingPaginationAsync(userId, paginationParameter);
            }

            // map entities to DTOs
            var dtoItems = queryResult.Select(m => new MeetingResponseDTO
            {
                Id = m.Id,
                Title = m.Title,
                Description = m.Description,
                StartTime = m.StartTime,
                EndTime = m.EndTime,
                HostId = m.HostId,
                CreationDate = m.CreationDate
            }).ToList();

            return new Pagination<MeetingResponseDTO>(dtoItems, queryResult.TotalCount, queryResult.CurrentPage, queryResult.PageSize);
        }

        public async Task<ResponseModel> GetMeetingByIdAsync(int meetingId)
        {
            var meeting = await _meetingRepository.GetByIdWithParticipantsAsync(meetingId);
            if (meeting == null)
            {
                return new ResponseModel { Status = false, Message = "Meeting not found" };
            }

            var userId = _claimsService.GetCurrentUserId;
            bool isAdmin = _claimsService.IsInRole("ADMIN");
            if (!isAdmin)
            {
                bool isHost = meeting.HostId == userId;
                bool isParticipant = meeting.Participants.Any(p => p.AccountId == userId);
                if (!isHost && !isParticipant)
                {
                    return new ResponseModel { Status = false, Message = "Unauthorized" };
                }
            }

            var dto = new MeetingResponseDTO
            {
                Id = meeting.Id,
                Title = meeting.Title,
                Description = meeting.Description,
                StartTime = meeting.StartTime,
                EndTime = meeting.EndTime,
                HostId = meeting.HostId,
                CreationDate = meeting.CreationDate
            };

            return new ResponseModel { Status = true, Message = "", Data = dto };
        }
    }
}

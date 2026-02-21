using EmployeeDemo.Application.Commons;
using EmployeeDemo.Application.ViewModels.MeetingViewModels;
using EmployeeDemo.Application.ViewModels.ResponseModels;

namespace EmployeeDemo.Application.Interfaces
{
    public interface IMeetingService
    {
        Task<ResponseModel> CreateMeetingAsync(MeetingCreateDTO dto);

        Task<Pagination<MeetingResponseDTO>> GetMeetingPaginationAsync(PaginationParameter paginationParameter);
        Task<ResponseModel> GetMeetingByIdAsync(int meetingId);
    }
}

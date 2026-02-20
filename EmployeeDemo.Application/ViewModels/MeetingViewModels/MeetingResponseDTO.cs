using System;

namespace EmployeeDemo.Application.ViewModels.MeetingViewModels
{
    public class MeetingResponseDTO
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public int HostId { get; set; }
        public DateTime CreationDate { get; set; }
    }
}
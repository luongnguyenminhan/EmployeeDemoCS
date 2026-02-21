using System;

namespace EmployeeDemo.Application.ViewModels.MeetingViewModels
{
    public class MeetingCreateDTO
    {
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public DateTime StartTime { get; set; }
        // end time intentionally omitted; meeting lasts zero by default or determined later
    }
}
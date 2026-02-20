using System;
using System.Collections.Generic;

namespace EmployeeDemo.Domain.Entities
{
    public class Meeting : BaseEntity
    {
        // Required fields
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        // Host FK (1 Meeting has 1 Host)
        public int HostId { get; set; }
        public Account Host { get; set; } = null!;

        // Participants (M2M through MeetingParticipant)
        public ICollection<MeetingParticipant> Participants { get; set; } = new List<MeetingParticipant>();
    }
}

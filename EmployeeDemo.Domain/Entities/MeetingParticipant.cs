using System;

namespace EmployeeDemo.Domain.Entities
{
    public class MeetingParticipant
    {
        // Composite PK + FKs
        public int MeetingId { get; set; }
        public int AccountId { get; set; }

        // Navigation properties
        public Meeting Meeting { get; set; } = null!;
        public Account Account { get; set; } = null!;

        // Metadata (for query optimization)
        public bool IsHost { get; set; } = false;
    }
}

using System;
using System.ComponentModel.DataAnnotations;

namespace EmployeeDemo.Domain.Entities
{
    // Domain Account (inherits BaseEntity for audit trail)
    public class Account : BaseEntity
    {
        public string? Email { get; set; }
        public string? UserName { get; set; }

        // authentication fields (we use PasswordHasher<Account>)
        public string PasswordHash { get; set; } = null!;
        public string? SecurityStamp { get; set; }
        public bool EmailConfirmed { get; set; }

        // roles (many-to-many)
        public ICollection<AccountRole> AccountRoles { get; set; } = new List<AccountRole>();

        // meetings (as host and participant)
        public ICollection<Meeting> MeetingsAsHost { get; set; } = new List<Meeting>();
        public ICollection<MeetingParticipant> MeetingParticipations { get; set; } = new List<MeetingParticipant>();
    }
}

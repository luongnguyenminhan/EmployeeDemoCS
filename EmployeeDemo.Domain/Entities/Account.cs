using System;
using System.ComponentModel.DataAnnotations;

namespace EmployeeDemo.Domain.Entities
{
    // Domain Account (no longer inherits IdentityUser)
    public class Account
    {
        [Key]
        public int Id { get; set; }

        public string? Email { get; set; }
        public string? UserName { get; set; }

        // authentication fields (we use PasswordHasher<Account>)
        public string PasswordHash { get; set; } = null!;
        public string? SecurityStamp { get; set; }
        public bool EmailConfirmed { get; set; }

        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }

        public DateTime CreationDate { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? ModificationDate { get; set; }
        public int? ModificationBy { get; set; }
        public DateTime? DeletionDate { get; set; }
        public int? DeleteBy { get; set; }
        public bool IsDeleted { get; set; } = false;

        // roles (many-to-many)
        public ICollection<AccountRole> AccountRoles { get; set; } = new List<AccountRole>();
    }
}

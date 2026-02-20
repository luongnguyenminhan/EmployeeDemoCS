using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EmployeeDemo.Domain.Entities
{
    public class Role
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public ICollection<AccountRole> AccountRoles { get; set; } = new List<AccountRole>();
    }
}
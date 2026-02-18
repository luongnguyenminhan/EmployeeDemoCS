using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmployeeDemo.Domain.Entities
{
    public class Student : BaseEntity
    {
        public string StudentName { get; set; } = "";

        public string StudentPhone { get; set; } = "";

        public string StudentEmail { get; set; } = "";

        public string? RefreshToken { get; set; }

        public DateTime? RefreshTokenExpiryTime { get; set; }
    }
}

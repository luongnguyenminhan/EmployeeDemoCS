using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmployeeDemo.Domain.Entities
{
    public abstract class BaseEntity
    {
        // integer identity primary key (auto-increment)
        public int Id { get; set; }

        public DateTime CreationDate { get; set; }

        // references to actor IDs — now int to match identity change
        public int? CreatedBy { get; set; }

        public DateTime? ModificationDate { get; set; }

        public int? ModificationBy { get; set; }

        public DateTime? DeletionDate { get; set; }

        public int? DeleteBy { get; set; }

        public bool IsDeleted { get; set; } = false;
    }
}

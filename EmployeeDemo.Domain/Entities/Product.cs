using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmployeeDemo.Domain.Entities
{
    public class Product : BaseEntity
    {
        public string ProductName { get; set; } = "";
        public string ProductDescription { get; set; } = "";
        public int ProductPrice { get; set; }
    }
}

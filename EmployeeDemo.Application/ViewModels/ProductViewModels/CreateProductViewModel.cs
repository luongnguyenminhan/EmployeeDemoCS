using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmployeeDemo.Application.ViewModels.ProductViewModels
{
    public class CreateProductViewModel
    {
        public string ProductName { get; set; } = "";
        public string ProductDescription { get; set; } = "";
        public int ProductPrice { get; set; }
    }
}

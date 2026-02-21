using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmployeeDemo.Application.ViewModels.ResponseModels
{
    public class ResponseModel
    {
        public bool Status { get; set; } = false;
        public string Message { get; set; } = "";
        
        // optional payload for success responses; typically a DTO
        public object? Data { get; set; }
    }
}

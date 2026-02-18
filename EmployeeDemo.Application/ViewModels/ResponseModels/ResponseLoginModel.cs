using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmployeeDemo.Application.ViewModels.ResponseModels
{
    public class ResponseLoginModel
    {
        public bool Status { get; set; } = false;

        public string Message { get; set; } = "";

        public string JWT { get; set; } = "";

        public DateTime? Expired {  get; set; }

        public string? JWTRefreshToken { get; set; } = "";
    }
}

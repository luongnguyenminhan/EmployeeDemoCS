using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmployeeDemo.Application.ViewModels
{
    public class TokenModel
    {
        public string AccessToken { get; set; } = null!;

        public string RefreshToken { get; set; } = null!;

        public string DeviceId { get; set; } = null!;
    }
}

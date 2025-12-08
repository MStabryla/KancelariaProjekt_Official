using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SWI2.Models.Response
{
    public class LoginResponseViewModel
    {
        public string token { get; set; }
        public DateTime expiration { get; set; }
        public string username { get; set; }
        public string userRole { get; set; }
    }
}

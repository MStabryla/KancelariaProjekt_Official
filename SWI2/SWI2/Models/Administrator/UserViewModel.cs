using SWI2DB.Models.Account;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SWI2.Models.Administrator
{
    public class UserViewModel
    {
        public string Id { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public string UserRole { get; set; }
        public bool IsActive { get; set; }
        public DateTime Created { get; set; }
    }
}

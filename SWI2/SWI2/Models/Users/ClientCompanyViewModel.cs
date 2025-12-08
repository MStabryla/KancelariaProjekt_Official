using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SWI2.Models.Users
{
    public class ClientCompanyViewModel
    {
        public long Id { get; set; }
        public long ClientId { get; set; }
        public long CompanyId { get; set; }
        public bool IsInBoard { get; set; }
    }
}

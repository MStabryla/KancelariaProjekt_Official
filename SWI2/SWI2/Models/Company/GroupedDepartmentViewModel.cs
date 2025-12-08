using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SWI2.Models.Company
{
    public class GroupedDepartmentViewModel
    {
        public CompanyViewModel Company { get; set; }
        public List<DepartmentViewModel> Deparments { get; set; }
    }
}

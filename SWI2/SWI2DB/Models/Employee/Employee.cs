using SWI2DB.Models.Authentication;
using System.Collections.Generic;

namespace SWI2DB.Models.Employee
{
    public class Employee : BaseModel
    {
        public virtual List<Department.Department> Departments { get; set; }
        public virtual List<Letter> Letters { get; set; }
        public virtual User User { get; set; }

    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SWI2DB.Models.Department
{
    public class Department : BaseModel
    {
        [StringLength(200, MinimumLength = 1, ErrorMessage = "Notes should be minimum 1 characters and a maximum of 200 characters")]
        public string Name { get; set; }
        [Range(0, 100, ErrorMessage = "Type must be in range 1 to 100")]
        public int Type { get; set; }
        [StringLength(255, MinimumLength = 1, ErrorMessage = "Notes should be minimum 1 characters and a maximum of 255 characters")]
        public string FolderName { get; set; }
        public virtual List<Employee.Employee> Employees { get; set; }
        public virtual Company.Company Company { get; set; }
        public virtual List<Client.Client> Clients { get; set; }
        [DataType(DataType.DateTime)]
        public DateTime Created { get; set; }
        [DataType(DataType.DateTime)]
        public DateTime? Updated { get; set; }


    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SWI2.Models.Company
{
    public class DepartmentViewModel
    {
        [Required]
        public long Id { get; set; }
        [Required]
        public string Name { get; set; }
        public int Type { get; set; }
        [Required]
        public string FolderName { get; set; }
        public string CompanyName { get; set; }
        public DateTime Created { get; set; }
    }
}

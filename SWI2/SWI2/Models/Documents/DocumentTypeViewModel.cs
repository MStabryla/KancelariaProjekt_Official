using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SWI2.Models.Documents
{
    public class DocumentTypeViewModel
    {
        public long Id { get; set; }
        [Required]
        public string Name { get; set; }
        public bool CanBeDeleted { get; set; }
        public DateTime Created { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SWI2.Models
{
    public class QueryViewModel
    {
        [Required]
        [Range(1,int.MaxValue)]
        public int Page { get; set; }

        [Required]
        [Range(1, 50)]
        public int ElementsPerPage { get; set; }
        [Range(0, int.MaxValue)]
        public int Offset { get; set; }
    }
}

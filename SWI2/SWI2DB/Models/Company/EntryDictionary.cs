using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SWI2DB.Models.Company
{
    public class EntryDictionary : BaseModel
    {
        [StringLength(255, MinimumLength = 1, ErrorMessage = "Name should be minimum 1 characters and a maximum of 100 characters")]
        public string Name { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }
        [Range(-200, 100, ErrorMessage = "Please enter valid integer Number")]
        public int VAT { get; set; }
        [Range(0, 100, ErrorMessage = "Please enter valid integer Number")]
        public int? GTU { get; set; }
        // public string Code { get; set; } // nie uzywana dana narazie
        public virtual Company Company { get; set; }
        [DataType(DataType.DateTime)]
        public DateTime Created { get; set; }
        [DataType(DataType.DateTime)]
        public DateTime? Updated { get; set; }

    }
}

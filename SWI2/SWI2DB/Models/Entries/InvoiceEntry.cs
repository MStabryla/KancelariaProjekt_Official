using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SWI2DB.Models.Entries
{
    public class InvoiceEntry : BaseModel
    {
        [StringLength(1024, MinimumLength = 1, ErrorMessage = "Title should be minimum 1 characters and a maximum of 100 characters")]
        public string Name { get; set; }
        [Range(0, 1000000000, ErrorMessage = "Please enter valid integer Number")]
        public int Quantity { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }
        [Range(-200, 100, ErrorMessage = "Please enter valid integer Number")]
        public int VAT { get; set; }
        [Range(0, 100, ErrorMessage = "Please enter valid integer Number")]
        public int? GTU { get; set; }
        public virtual Invoice.Invoice Invoice { get; set; }
        [DataType(DataType.DateTime)]
        public DateTime Created { get; set; }
        [DataType(DataType.DateTime)]
        public DateTime? Updated { get; set; }

    }
}

using System;
using System.ComponentModel.DataAnnotations;

namespace SWI2DB.Models.Invoice
{
    public class InvoiceSended : BaseModel
    {
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
        public virtual Invoice Invoice { get; set; }
        public virtual Authentication.User User { get; set; }
        [DataType(DataType.DateTime)]
        public DateTime Created { get; set; }
        [DataType(DataType.DateTime)]
        public DateTime? Updated { get; set; }

    }
}

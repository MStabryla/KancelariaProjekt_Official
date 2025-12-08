using SWI2DB.Models.Contractor;
using System;
using System.ComponentModel.DataAnnotations;

namespace SWI2DB.Models.Invoice
{
    public class InvoiceMailTemplate : BaseModel
    {
        [EnumDataType(typeof(MailLanguage))]
        public MailLanguage MailLanguage { get; set; }
        [StringLength(200, MinimumLength = 0, ErrorMessage = "First Name should be minimum 1 characters and a maximum of 200 characters")]
        public string Title { get; set; }
        [StringLength(100000, MinimumLength = 0, ErrorMessage = "Message Name should be minimum 1 characters and a maximum of 100000 characters")]
        public string Message { get; set; }
        public virtual Company.Company Company { get; set; }
        [DataType(DataType.DateTime)]
        public DateTime Created { get; set; }
        [DataType(DataType.DateTime)]
        public DateTime? Updated { get; set; }

    }
}

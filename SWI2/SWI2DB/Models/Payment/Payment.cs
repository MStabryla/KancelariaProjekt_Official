using SWI2DB.Models.Contractor;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SWI2DB.Models.Payment
{
    public class Payment : BaseModel
    {
        [Column(TypeName = "decimal(18,2)")]
        public decimal PaymentValue { get; set; }
        [DataType(DataType.Currency)]
        public string Currency { get; set; }
        [StringLength(200 , MinimumLength =0, ErrorMessage = "First Name should be minimum 1 characters and a maximum of 200 characters")]
        public string Topic { get; set; }
        [DataType(DataType.Date)]
        public DateTime PaymentDate { get; set; }
        public virtual ContractorBankAccount ContractorBankAccount { get; set; }
        public virtual List<PaymentForInvoice> PaymentsForInvoices { get; set; }
        [DataType(DataType.DateTime)]
        public DateTime Created { get; set; }
        [DataType(DataType.DateTime)]
        public DateTime? Updated { get; set; }

    }
}

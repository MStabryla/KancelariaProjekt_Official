using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SWI2DB.Models.Contractor
{
    public class ContractorBankAccount : BaseModel
    {
        [StringLength(100, MinimumLength = 1, ErrorMessage = "BankName should be minimum 1 characters and a maximum of 100 characters")]
        public string BankName { get; set; }
        [RegularExpression("^((([A-Z| ]{0,10})(?:[0-9]{26}|[0-9]{2}( [0-9]{4}){6})|[0-9]{2}(-[0-9]{4}){6})|(unknown))$")]
        public string AccountNumber { get; set; }
        public virtual Contractor Contractor { get; set; }
        public virtual List<Payment.Payment> Payments { get; set; }
        [DataType(DataType.DateTime)]
        public DateTime Created { get; set; }
        [DataType(DataType.DateTime)]
        public DateTime? Updated { get; set; }


    }
}

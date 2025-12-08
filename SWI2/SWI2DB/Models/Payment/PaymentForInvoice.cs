using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace SWI2DB.Models.Payment
{
    public class PaymentForInvoice : BaseModel
    {
        public virtual Invoice.Invoice Invoice { get; set; }
        public virtual Payment Payment { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public virtual decimal PaymentValueForInvoice { get; set; }

    }
}

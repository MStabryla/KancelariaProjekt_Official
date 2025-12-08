using SWI2DB.Models.Contractor;
using System;
using System.Collections.Generic;
using SWI2DB.Models.Invoice;
using SWI2DB;
using SWI2DB.Models.Payment;

namespace SWI2.Models.Invoice
{
    public class PaymentViewModel
    {
        public long Id { get; set; }
        public List<PaymentsForInvoicesViewModel> PaymentsForInvoices { get; set; }
        public long ContractorId { get; set; }
        public string ContractorName { get; set; }
        public string ContractorNip { get; set; }
        public string contractorBankAccountName { get; set; }
        public string ContractorBankAccountNumber { get; set; }
        public long ContractorBankAccountId { get; set; }
        public decimal PaymentValue { get; set; }
        public string Currency { get; set; }
        public string Topic { get; set; }
        public DateTime PaymentDate { get; set; }
        public DateTime Created { get; set; }

    }
    public class PaymentsForInvoicesViewModel
    {
        public long Id { get; set; }
        public PaymentInvoiceViewModel Invoice { get; set; }
        public decimal PaymentValueForInvoice { get; set; }

    }
    public class PaymentInvoiceViewModel
    {
        public long Id { get; set; }
        public string Number { get; set; }
        public decimal BruttoWorth { get; set; }


    }
    public class PaymentMachingModel : BaseModel
    {
        public decimal PaymentValue { get; set; }
        public string Currency { get; set; }
        public string Topic { get; set; }
        public string Addressee { get; set; }
        public DateTime PaymentDate { get; set; }
        public virtual ContractorBankAccount ContractorBankAccount { get; set; }
        public virtual List<PaymentForInvoice> PaymentsForInvoices { get; set; }

    }

}
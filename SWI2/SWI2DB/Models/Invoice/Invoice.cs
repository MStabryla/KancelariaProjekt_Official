using Microsoft.EntityFrameworkCore;
using SWI2DB.Models.Entries;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SWI2DB.Models.Invoice
{
    public enum PaymentStatus
    {
        Notpaid,
        Paid,
        Overpaid
    }
    public class Invoice : BaseModel
    {
        [StringLength(50, MinimumLength = 1, ErrorMessage = "First Name should be minimum 1 characters and a maximum of 50 characters")]
        public string Number { get; set; }  
        [StringLength(50, MinimumLength = 0, ErrorMessage = "First Name should be minimum 1 characters and a maximum of 50 characters")]
        public string Correcting { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal NettoWorth { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal BruttoWorth { get; set; }
        [DataType(DataType.Currency)]
        public string PaymentCurrency { get; set; }
        [EnumDataType(typeof (PaymentStatus))]
        public PaymentStatus PaymentStatus { get; set; }
        public virtual List<InvoiceSended> InvoiceSendeds { get; set; }
        public virtual Client.Client Client { get; set; }
        public virtual Company.Company Company { get; set; }
        public virtual List<Payment.PaymentForInvoice> PaymentsForInvoices { get; set; }
        public bool Mpp { get; set; }//split payment method
        public bool IsTransferType { get; set; }
        [StringLength(100, MinimumLength = 0, ErrorMessage = "First Name should be minimum 1 characters and a maximum of 100 characters")]
        public string PaymentBank { get; set; }
        [RegularExpression("^(([A-Z| ]{0,10})(?:[0-9]{26}|[0-9]{2}( [0-9]{4}){6})|[0-9]{2}(-[0-9]{4}){6})$")]
        public string PaymentAccountNumber { get; set; }
        [StringLength(100, MinimumLength = 0, ErrorMessage = "First Name should be minimum 1 characters and a maximum of 100 characters")]
        public string CreationPlace { get; set; }
        [DataType(DataType.DateTime)]
        public DateTime SellDate { get; set; }
        [DataType(DataType.DateTime)]
        public DateTime PaymentDate { get; set; }
        public SellDateName SellDateName { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal Rate { get; set; }
        [StringLength(1000, MinimumLength = 0, ErrorMessage = "First Name should be minimum 1 characters and a maximum of 1000 characters")]
        public string Note { get; set; }
        public long InvoiceContractorId { get; set; }
        public virtual InvoiceContractor InvoiceContractor { get; set; }
        public virtual InvoiceIssuer InvoiceIssuer { get; set; }
        public virtual List<InvoiceEntry> InvoiceEntries { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime Created { get; set; }
        [DataType(DataType.DateTime)]
        public DateTime? Updated { get; set; }

    }
}

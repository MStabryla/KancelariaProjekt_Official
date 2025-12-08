using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SWI2DB.Models.Invoice
{
/*  public enum PaymentStatus
  {
    Notpaid,
    Paid,
    Overpaid
  }*/
  public class InvoiceHeader /*: BaseModel*/
  {
    public string Number { get; set; }
    public string Correcting { get; set; }
    public decimal NettoWorth { get; set; }
    public decimal BruttoWorth { get; set; }
    public string PaymentCurrency { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public virtual Invoice Invoice { get; set; }
    public virtual List<InvoiceSended> InvoiceSendeds { get; set; }
    public virtual Client.Client Client { get; set; }
    public virtual Company.Company Company { get; set; }
    public virtual List<Payment.Payment> Payments { get; set; }

  }
}

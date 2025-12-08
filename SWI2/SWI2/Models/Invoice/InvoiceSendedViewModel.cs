using System;

namespace SWI2.Models.Invoice
{
  public class InvoiceSendedViewModel
    {
        public long Id { get; set; }
        public string Email { get; set; }
        public string Number { get; set; }
        public string UserName { get; set; }
        public DateTime Created { get; set; }

    }
}

using SWI2DB.Models.Company;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace SWI2DB.Models.Client
{
  public class Client : BaseModel
  {
    public virtual Authentication.User User { get; set; }
    public virtual List<ClientCompany> ClientCompany { get; set; }
    public virtual Department.Department Department { get; set; }
    public virtual List<Invoice.Invoice> Invoices { get; set; }
  }
}

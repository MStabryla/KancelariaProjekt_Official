using System;
using System.Collections.Generic;
using System.Text;

namespace SWI2DB.Models.Company
{
  public class ClientCompany : BaseModel
  {
    public Company Company { get; set; }
    public Client.Client Client { get; set; }
    public bool IsInBoard { get; set; }
  }
}

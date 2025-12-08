using Microsoft.AspNetCore.Identity;
using SWI2DB.Models.Account;
using SWI2DB.Models.Messages;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace SWI2DB.Models.Authentication
{
  public class User : IdentityUser
  {
    public bool IsActive { get; set; }
    public bool ForcePasswordChange { get; set; }
    public virtual List<MessageReceiver> ReceivedMessages { get; set; }
    public virtual List<MessageSender> SendedMessages { get; set; }
    public virtual List<UserMessageTemplate> UserMessageTemplates { get; set; }
    public virtual Client.Client Client { get; set; }
    [ForeignKey("Client.Client")]
    public long? ClientId { get; set; }
    public virtual Employee.Employee Employee { get; set; }
    [ForeignKey("Employee.Employee")]
    public long? EmployeeId { get; set; }

    //Zastanowiłbym się nad zmianą,a by dokumenty przypisywać do pracowników
    public virtual List<Document> Documents { get; set; }
    public virtual UserDetails UserDetails { get; set; }
    [ForeignKey("UserDetails")]
    public long UserDetailsId { get; set; }
    public virtual List<UserEmail> UserEmails { get; set; }
    public virtual List<Invoice.InvoiceSended> InvoiceSendeds { get; set; }

    //potrzeban do migracjii
    public long OldId { get; set; }
  }
}

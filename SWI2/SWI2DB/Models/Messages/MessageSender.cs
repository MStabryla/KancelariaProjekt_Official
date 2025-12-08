using SWI2DB.Models.Authentication;

namespace SWI2DB.Models.Messages
{
  public class MessageSender : BaseModel
  {
    public virtual Message Message { get; set; }
    public virtual User User { get; set; }

  }
}

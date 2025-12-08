using SWI2DB.Models.Authentication;
using System;
using System.ComponentModel.DataAnnotations;

namespace SWI2DB.Models.Account
{
    public class UserEmail : BaseModel
    {
        [EmailAddress]
        public string Email { get; set; }
        public virtual User User { get; set; }
        [DataType(DataType.DateTime)]
        public DateTime Created { get; set; }
        [DataType(DataType.DateTime)]
        public DateTime? Updated { get; set; }

    }
}

using SWI2DB.Models.Authentication;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SWI2DB.Models.Messages
{
    public class UserMessageTemplate : BaseModel
    {
        [StringLength(200, MinimumLength = 0, ErrorMessage = "Title should be minimum 1 characters and a maximum of 200 characters")]
        public string Title { get; set; }
        [StringLength(10000, MinimumLength =0, ErrorMessage = "Message should be minimum 1 characters and a maximum of 100000 characters")]
        public string Message { get; set; }
        public virtual List<User> Users { get; set; }
        [DataType(DataType.DateTime)]
        public DateTime Created { get; set; }
        [DataType(DataType.DateTime)]
        public DateTime? Updated { get; set; }

    }
}

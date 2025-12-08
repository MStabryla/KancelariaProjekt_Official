using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SWI2DB.Models.Employee
{
    public class LetterRecipent : BaseModel
    {
        [StringLength(200, MinimumLength = 1, ErrorMessage = "Name should be minimum 1 characters and a maximum of 200 characters")]
        public string Name { get; set; }
        public virtual List<Letter> Letters { get; set; }
        [DataType(DataType.DateTime)]
        public DateTime Created { get; set; }
        [DataType(DataType.DateTime)]
        public DateTime? Updated { get; set; }
        public long OldId { get; set; }


    }
}

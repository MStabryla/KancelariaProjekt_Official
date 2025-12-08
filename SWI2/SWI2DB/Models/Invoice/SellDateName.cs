using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace SWI2DB.Models.Invoice
{
    public class SellDateName : BaseModel
    {
        [StringLength(100, MinimumLength = 1, ErrorMessage = "First Name should be minimum 1 characters and a maximum of 100 characters")]
        public string Name { get; set;}
    }
}

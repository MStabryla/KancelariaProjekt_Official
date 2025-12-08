using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SWI2.Models.Company
{
    public class PaymentMethodViewModel
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string AccountNumber { get; set; }
        [Required]
        [MaxLength(5)]
        public string Currency { get; set; }
    }
}

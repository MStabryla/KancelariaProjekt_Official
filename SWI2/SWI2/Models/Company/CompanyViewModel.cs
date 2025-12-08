using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SWI2.Models.Company
{
    public class CompanyViewModel
    {
        
        public long Id { get; set; }
        [Required]
        public string Name { get; set; }
        public string Country { get; set; }
        public string City { get; set; }
        public string Postoffice { get; set; }
        public string Postalcode { get; set; }
        public string Street { get; set; }
        public int? Housenumber { get; set; }
        public DateTime Created { get; set; }
        public string ApartamentNumber { get; set; }
        [Required]
        public string Nip { get; set; }
        public string CreationPlace { get; set; }
        public string InvoiceDescription { get; set; }
        public string DefaultWNAAccount { get; set; }
        public string DefaultMAAccount { get; set; }
        public string DefaultMAVatAccount { get; set; }
    }
}

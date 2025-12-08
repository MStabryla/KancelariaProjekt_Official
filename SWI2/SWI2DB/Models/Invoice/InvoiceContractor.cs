using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SWI2DB.Models.Invoice
{
    public class InvoiceContractor : BaseModel
    {
        [StringLength(150, MinimumLength = 1, ErrorMessage = "Name should be minimum 1 characters and a maximum of 150 characters")]
        public string Name { get; set; }
        [StringLength(60, MinimumLength = 0, ErrorMessage = "Country should be minimum 1 characters and a maximum of 60 characters")]
        public string Country { get; set; }
        [StringLength(100, MinimumLength = 0, ErrorMessage = "City should be minimum 1 characters and a maximum of 100 characters")]
        public string City { get; set; }
        [StringLength(100, MinimumLength = 0, ErrorMessage = "Postoffice should be minimum 1 characters and a maximum of 100 characters")]
        public string Postoffice { get; set; }
        [StringLength(20, MinimumLength = 0, ErrorMessage = "Postalcode should be minimum 1 characters and a maximum of 20 characters")]
        public string Postalcode { get; set; }
        [StringLength(100, MinimumLength = 0, ErrorMessage = "Street should be minimum 1 characters and a maximum of 100 characters")]
        public string Street { get; set; }
        [StringLength(15, MinimumLength = 0, ErrorMessage = "HouseNumber should be minimum 1 characters and a maximum of 15 characters")]
        public string HouseNumber { get; set; }
        [StringLength(15, MinimumLength = 0, ErrorMessage = "ApartamentNumber should be minimum 1 characters and a maximum of 15 characters")]
        public string ApartamentNumber { get; set; }
        [StringLength(24, MinimumLength = 0, ErrorMessage = "Nip should be minimum 1 characters and a maximum of 15 characters")]
        public string Nip { get; set; }
        public virtual Contractor.Contractor Contractor { get; set; }
        public virtual List<Invoice> Invoices { get; set; }
        [DataType(DataType.DateTime)]
        public DateTime Created { get; set; }
        [DataType(DataType.DateTime)]
        public DateTime? Updated { get; set; }
    }
}

















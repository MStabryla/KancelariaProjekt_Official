using SWI2DB.Models.Invoice;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SWI2DB.Models.Contractor
{
    public enum MailLanguage
    {
        PL,
        EN,
        IT
    }
    public class Contractor : BaseModel
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
        [StringLength(15, MinimumLength = 0, ErrorMessage = "Nip should be minimum 1 characters and a maximum of 15 characters")]
        public string Nip { get; set; }
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
        [StringLength(24, MinimumLength = 0, ErrorMessage = "Nip should be minimum 1 characters and a maximum of 24 characters")]
        public string WNAccount { get; set; }
        [EnumDataType(typeof(MailLanguage))]
        public MailLanguage MailLanguage { get; set; }
        public virtual Company.Company Company { get; set; }
        public virtual List<InvoiceContractor> InvoiceContractors { get; set; }
        public virtual List<ContractorBankAccount> ContractorBankAccounts { get; set; }
        [DataType(DataType.DateTime)]
        public DateTime Created { get; set; }
        [DataType(DataType.DateTime)]
        public DateTime? Updated { get; set; }

        //public InvoiceSended InvoiceSended { get; set; }
        //zmienna potrzeba wyłącznie do migracji swi1toswi2
        public long oldId { get; set; }
    }
}

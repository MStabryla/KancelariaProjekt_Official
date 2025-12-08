using SWI2DB.Models.Account;
using SWI2DB.Models.Invoice;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace SWI2DB.Models.Company
{
    public class Company : BaseModel
    {
        [StringLength(100, MinimumLength = 1, ErrorMessage = "Name should be minimum 1 characters and a maximum of 100 characters")]
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
        [StringLength(100, MinimumLength = 0, ErrorMessage = "CreationPlace should be minimum 1 characters and a maximum of 100 characters")]
        public string CreationPlace { get; set; }
        [StringLength(200, MinimumLength = 0, ErrorMessage = "InvoiceDescription should be minimum 1 characters and a maximum of 200 characters")]
        public string InvoiceDescription { get; set; }
        [StringLength(20, MinimumLength = 0, ErrorMessage = "DefaultWNAAccount should be minimum 1 characters and a maximum of 20 characters")]
        public string DefaultWNAAccount { get; set; }
        [StringLength(20, MinimumLength = 0, ErrorMessage = "DefaultMAAccount should be minimum 1 characters and a maximum of 20 characters")]
        public string DefaultMAAccount { get; set; }
        [StringLength(20, MinimumLength = 0, ErrorMessage = "DefaultMAVatAccount should be minimum 1 characters and a maximum of 20 characters")]
        public string DefaultMAVatAccount { get; set; }
        [EmailAddress]
        public string ReplayToMail { get; set; }
        public virtual List<Department.Department> Departments { get; set; }
        public virtual List<Document> Documents { get; set; }
        public virtual List<PaymentMethodDictionary> PaymentMethodsDictionary { get; set; }
        public virtual List<Contractor.Contractor> Contractors { get; set; }
        public virtual List<Invoice.Invoice> Invoices { get; set; }
        public virtual List<InvoiceIssuer> InvoiceIssuers { get; set; }
        public virtual List<EntryDictionary> EntriesDictionary { get; set; }
        public virtual List<ClientCompany> ClientCompany { get; set; }
        public virtual List<InvoiceMailTemplate> InvoiceMailTemplates { get; set; }
        [DataType(DataType.DateTime)]
        public DateTime Created { get; set; }
        [DataType(DataType.DateTime)]
        public DateTime? Updated { get; set; }
    }
}

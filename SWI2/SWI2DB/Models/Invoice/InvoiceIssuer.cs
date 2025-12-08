using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SWI2DB.Models.Invoice
{
    public class InvoiceIssuer : BaseModel
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
        //public long CompanyId { get; set; }
        public virtual Company.Company Company { get; set; }
        public virtual List<Invoice> Invoices { get; set; }
        [DataType(DataType.DateTime)]
        public DateTime Created { get; set; }
        [DataType(DataType.DateTime)]
        public DateTime? Updated { get; set; }

        /*        public bool Equals(InvoiceIssuer other)
                {

                    //Check whether the compared object is null.
                    if (Object.ReferenceEquals(other, null)) return false;

                    //Check whether the compared object references the same data.
                    if (Object.ReferenceEquals(this, other)) return true;

                    //Check whether the products' properties are equal.
                    return Name.Equals(other.Name) && Country.Equals(other.Country) && City.Equals(other.City) && Postoffice.Equals(other.Postoffice) 
                        && Postalcode.Equals(other.Postalcode) && Street.Equals(other.Street) && Housenumber.Equals(other.Housenumber) && ApartamentNumber.Equals(other.ApartamentNumber) && Nip.Equals(other.Nip);
                }

                // If Equals() returns true for a pair of objects
                // then GetHashCode() must return the same value for these objects.

                public override int GetHashCode()
                {
                    //Get hash code for the Code field.

                    int hashProductName =  Name.GetHashCode();
                    int hashProductCountry = Country.GetHashCode();
                    int hashProductCity = City.GetHashCode();
                    int hashProductPostoffice = Postoffice.GetHashCode();
                    int hashProductPostalcode = Postalcode.GetHashCode();
                    int hashProductStreet = Street.GetHashCode();
                    int hashProductHousenumber = Housenumber.GetHashCode();
                    int hashProductApartamentNumber = ApartamentNumber.GetHashCode();
                    int hashProductNip = Nip.GetHashCode();

                    //Calculate the hash code for the product.
                    return hashProductName ^ hashProductCountry ^ hashProductCity ^ hashProductPostoffice ^ hashProductPostalcode ^ hashProductStreet ^ hashProductHousenumber ^ hashProductApartamentNumber ^ hashProductNip;
                }*/

    }
}

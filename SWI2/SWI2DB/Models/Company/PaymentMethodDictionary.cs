using System;
using System.ComponentModel.DataAnnotations;
//using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace SWI2DB.Models.Company
{
    public class PaymentMethodDictionary : BaseModel
    {
        [StringLength(100, MinimumLength = 1, ErrorMessage = "Name should be minimum 1 characters and a maximum of 100 characters")]
        public string Name { get; set; }
        [RegularExpression("^((([A-Z| ]{0,10})(?:[0-9]{26}|[0-9]{2}( [0-9]{4}){6})|[0-9]{2}(-[0-9]{4}){6})|(unknown))$")]
        public string AccountNumber { get; set; }
        [DataType(DataType.Currency)]
        public string Currency { get; set; }
        [JsonIgnore]
        public virtual Company Company { get; set; }
        [DataType(DataType.DateTime)]
        public DateTime Created { get; set; }
        [DataType(DataType.DateTime)]
        public DateTime? Updated { get; set; }
    }
}

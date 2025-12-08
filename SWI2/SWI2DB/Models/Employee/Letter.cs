using System;
using System.ComponentModel.DataAnnotations;

namespace SWI2DB.Models.Employee
{
    public class Letter : BaseModel
    {
        public bool OutLetter { get; set; }
        [StringLength(1000, MinimumLength = 0, ErrorMessage = "Notes should be minimum 1 characters and a maximum of 1000 characters")]
        public string Notes { get; set; }
        [StringLength(255, MinimumLength = 0, ErrorMessage = "Notes should be minimum 1 characters and a maximum of 255 characters")]
        public string Path { get; set; }
        [StringLength(1000, MinimumLength = 0, ErrorMessage = "RegisteredNumbr should be minimum 1 characters and a maximum of 1000 characters")]
        public string RegisteredNumbr { get; set; }
        [DataType(DataType.DateTime)]
        public DateTime Added { get; set; }
        public bool IsPaid { get; set; }
        public bool IsRegistered { get; set; }
        public bool? IsEmail { get; set; }
        public bool? IsNormal { get; set; }
        public bool? IsCourier { get; set; }
        public bool WithConfirm { get; set; }
        public virtual Employee Employee { get; set; }
        public virtual LetterRecipent LetterRecipent { get; set; }
        [DataType(DataType.DateTime)]
        public DateTime Created { get; set; }
        [DataType(DataType.DateTime)]
        public DateTime? Updated { get; set; }


    }
}


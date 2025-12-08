using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SWI2.Models.Letter
{
    public class LetterViewModel
    {
        public long Id { get; set; }
        public bool OutLetter { get; set; }
        public string Notes { get; set; }
        public string Path { get; set; }
        public string RegisteredNumbr { get; set; }
        public DateTime Added { get; set; }
        public bool IsPaid { get; set; }
        public bool IsRegistered { get; set; }
        public bool? IsEmail { get; set; }
        public bool? IsNormal { get; set; }
        public bool? IsCourier { get; set; }
        public bool WithConfirm { get; set; }
        public bool HasLetterFile { get; set; }
        public string LetterFileType { get; set; }
        [Required]
        public long LetterRecipientId { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
    }
}

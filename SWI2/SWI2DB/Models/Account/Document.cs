using SWI2DB.Models.Authentication;
using System;
using System.ComponentModel.DataAnnotations;

namespace SWI2DB.Models.Account
{
    public class Document : BaseModel
    {
        public bool OutDocument { get; set; }
        [StringLength(255, MinimumLength = 0, ErrorMessage = "Path should be minimum 1 characters and a maximum of 255 characters")]
        public string Path { get; set; }
        [StringLength(10000, MinimumLength = 0, ErrorMessage = "Notes should be minimum 1 characters and a maximum of 10000 characters")]
        public string Notes { get; set; }
        [StringLength(1000, MinimumLength = 0, ErrorMessage = "Comment should be minimum 1 characters and a maximum of 1000 characters")]
        public string Comment { get; set; }
        [DataType(DataType.DateTime)]
        public DateTime Added { get; set; }
        public bool IsProtocol { get; set; }
        public virtual User User { get; set; }
        public virtual Company.Company Company { get; set; }
        public virtual DocumentType DocumentType { get; set; }
        public virtual Employee.Employee Employee { get; set; }
        [DataType(DataType.DateTime)]
        public DateTime Created { get; set; }
        [DataType(DataType.DateTime)]
        public DateTime? Updated { get; set; }
    }
}

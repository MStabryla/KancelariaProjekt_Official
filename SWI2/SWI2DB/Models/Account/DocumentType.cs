using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SWI2DB.Models.Account
{
    public class DocumentType : BaseModel
    {
        public long OldId { get; set; }
        [StringLength(200, MinimumLength = 0, ErrorMessage = "Path should be minimum 1 characters and a maximum of 200 characters")]
        public string Name { get; set; }
        public virtual List<Document> Documents { get; set; }
        [DataType(DataType.DateTime)]
        public DateTime Created { get; set; }
        [DataType(DataType.DateTime)]
        public DateTime? Updated { get; set; }
    }
}

using SWI2DB.Models.Authentication;
using System;
using System.ComponentModel.DataAnnotations;

namespace SWI2DB.Models.Account
{
    public class UserDetails : BaseModel
    {
        [StringLength(255, MinimumLength = 1, ErrorMessage = "Name should be minimum 1 characters and a maximum of 255 characters")]
        public string Name { get; set; }
        [StringLength(255, MinimumLength = 0, ErrorMessage = "Surname should be minimum 1 characters and a maximum of 255 characters")]
        public string Surname { get; set; }
        [DataType(DataType.DateTime)]
        public DateTime? LastSeen { get; set; }
        [DataType(DataType.DateTime)]
        public DateTime? Registered { get; set; }
        [StringLength(3, MinimumLength = 0, ErrorMessage = "Language should be minimum 1 characters and a maximum of 3 characters")]
        public string Language { get; set; }
        [StringLength(255, MinimumLength = 0, ErrorMessage = "FolderName should be minimum 1 characters and a maximum of 255 characters")]
        public string FolderName { get; set; }
        public virtual User User { get; set; }

    }
}

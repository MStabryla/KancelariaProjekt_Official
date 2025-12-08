using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SWI2.Models.Users
{
    public class UserDetailsViewModel
    {
        public long Id { get; set; }
        public string UserId { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Email { get; set; }
        [DataType(DataType.DateTime)]
        public DateTime? LastSeen { get; set; }
        [DataType(DataType.DateTime)]
        public DateTime? Registered { get; set; }
        public string Language { get; set; }
        public string FolderName { get; set; }
    }
}

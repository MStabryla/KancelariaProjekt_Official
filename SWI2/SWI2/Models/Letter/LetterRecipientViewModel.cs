using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SWI2.Models.Letter
{
    public class LetterRecipientViewModel
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public DateTime Created { get; set; }
        public bool CanBeDeleted { get; set; }
    }
}

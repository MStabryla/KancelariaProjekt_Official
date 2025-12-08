using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SWI2.Models
{
    public class TableViewModel<T> where T : class
    {
        public int totalCount { get; set; }
        public IList<T> elements { get; set; }
    }
}

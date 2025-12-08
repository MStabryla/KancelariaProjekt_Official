using System.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;

namespace SWI2.Models
{
    public class TableParamsModel
    {
        public TableParamsModel()
        {
            PageNumber = 0;
            PageSize = 25;
            Sort = "created";
        }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string Sort { get; set; }
        public List<FilterModel> Filters { get; set; }
    }

    public class FilterModel
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public string Type { get; set; }
    }

}

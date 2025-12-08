using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SWI2.Models.Response
{
    public class ErrorResponseViewModel
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public ErrorResponseViewModel(string message)
        {
            Message = message;
            Success = false;
        }
    }
}

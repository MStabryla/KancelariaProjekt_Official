using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SWI2.Models.Response
{
    public class OperationSuccesfullViewModel<T>
    {
        public bool Success { get; set; }
        public T Data { get; set; }
        public OperationSuccesfullViewModel(T data)
        {
            Data = data;
            Success = true;
        }
        public OperationSuccesfullViewModel()
        {

        }
    }
}

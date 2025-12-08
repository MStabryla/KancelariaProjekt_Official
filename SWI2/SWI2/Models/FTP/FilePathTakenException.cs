using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SWI2.Models.FTP
{
    public class FilePathTakenException : Exception
    {
        public FilePathTakenException(string path) : base("File " + path + " cannot be created")
        { }
    }
}

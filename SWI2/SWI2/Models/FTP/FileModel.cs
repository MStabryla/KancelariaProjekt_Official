using FluentFTP;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SWI2.Models.FTP
{
    public class FileModel
    {
        public DateTime Created { get; set; }
        public DateTime Modified { get; set; }
        public string Name { get; set; }
        public string FullName { get; set; }
        public FtpFileSystemObjectType Type { get; set; }
        public long Size { get; set; }
        public Stream Data { get; set; }
        public string MIMEType { get; set; }
    }
}

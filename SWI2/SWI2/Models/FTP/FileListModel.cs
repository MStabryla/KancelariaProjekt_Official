using FluentFTP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SWI2.Models.FTP
{
    public class FileListModel
    {
        public string Path { get; set; }
        public FtpListItem[] ChildFiles { get; set; }
    }
}

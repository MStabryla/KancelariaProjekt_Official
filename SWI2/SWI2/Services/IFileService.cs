using FluentFTP;
using SWI2.Models.FTP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SWI2.Services
{
    public interface IFileService
    {
        public FtpListItem GetFileStatus(string path);
        public bool CreateFolder(string path);
        public bool DeleteFile(string path);
        public bool DeleteFolder(string path);
        public FileModel GetFile(string path);
        public byte[] DownloadFile(string path);
        public IEnumerable<FtpListItem> GetFiles(string path);
        public bool Rename(string path, string newPath);
        public FileModel SendFile(FileModel file, string path);



    }
}

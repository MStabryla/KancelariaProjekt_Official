using SWI2.Models.FTP;
using FluentFTP;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using SWI2.Services.Static;

namespace SWI2.Services
{
    public class FTPService : IFileService
    {
        public FTPService(IConfiguration configuration)
        {
            data = configuration.GetSection("FTP").Get<FTPConnData>();
            Connect();
        }
        private IFtpClient _client;
        private readonly FTPConnData data;

        private Dictionary<string, string> MIMETypesDictionary = new Dictionary<string, string>
{ {"ai","application/postscript"}, {"aif","audio/x-aiff"}, {"aifc","audio/x-aiff"}, {"aiff","audio/x-aiff"}, {"asc","text/plain"}, {"atom","application/atom+xml"}, {"avi","video/x-msvideo"}, {"bin","application/octet-stream"}, {"bmp","image/bmp"}, {"class","application/octet-stream"}, {"css","text/css"}, {"dll","application/octet-stream"}, {"dmg","application/octet-stream"}, {"doc","application/msword"}, {"docx","application/vnd.openxmlformats-officedocument.wordprocessingml.document"}, {"dotx","application/vnd.openxmlformats-officedocument.wordprocessingml.template"}, {"docm","application/vnd.ms-word.document.macroEnabled.12"}, {"dotm","application/vnd.ms-word.template.macroEnabled.12"}, {"dtd","application/xml-dtd"}, {"eps","application/postscript"}, {"exe","application/octet-stream"}, {"gif","image/gif"}, {"gram","application/srgs"}, {"grxml","application/srgs+xml"}, {"hqx","application/mac-binhex40"}, {"htm","text/html"}, {"html","text/html"}, {"ico","image/x-icon"}, {"ics","text/calendar"}, {"ief","image/ief"}, {"ifb","text/calendar"}, {"jnlp","application/x-java-jnlp-file"}, {"jp2","image/jp2"}, {"jpe","image/jpeg"}, {"jpeg","image/jpeg"}, {"jpg","image/jpeg"}, {"js","application/x-javascript"}, {"m3u","audio/x-mpegurl"}, {"m4a","audio/mp4a-latm"}, {"m4b","audio/mp4a-latm"}, {"m4p","audio/mp4a-latm"}, {"m4u","video/vnd.mpegurl"}, {"m4v","video/x-m4v"}, {"mac","image/x-macpaint"}, {"man","application/x-troff-man"}, {"mid","audio/midi"}, {"midi","audio/midi"}, {"mov","video/quicktime"}, {"movie","video/x-sgi-movie"}, {"mp2","audio/mpeg"}, {"mp3","audio/mpeg"}, {"mp4","video/mp4"}, {"mpe","video/mpeg"}, {"mpeg","video/mpeg"}, {"mpg","video/mpeg"}, {"mpga","audio/mpeg"}, {"ogg","application/ogg"}, {"pbm","image/x-portable-bitmap"}, {"pct","image/pict"}, {"pdf","application/pdf"}, {"pic","image/pict"}, {"pict","image/pict"}, {"png","image/png"}, {"pnt","image/x-macpaint"}, {"pntg","image/x-macpaint"}, {"ppm","image/x-portable-pixmap"}, {"ppt","application/vnd.ms-powerpoint"}, {"pptx","application/vnd.openxmlformats-officedocument.presentationml.presentation"}, {"potx","application/vnd.openxmlformats-officedocument.presentationml.template"}, {"ppsx","application/vnd.openxmlformats-officedocument.presentationml.slideshow"}, {"ppam","application/vnd.ms-powerpoint.addin.macroEnabled.12"}, {"pptm","application/vnd.ms-powerpoint.presentation.macroEnabled.12"}, {"potm","application/vnd.ms-powerpoint.template.macroEnabled.12"}, {"ppsm","application/vnd.ms-powerpoint.slideshow.macroEnabled.12"}, {"ps","application/postscript"}, {"qt","video/quicktime"}, {"qti","image/x-quicktime"}, {"qtif","image/x-quicktime"}, {"ram","audio/x-pn-realaudio"}, {"rgb","image/x-rgb"}, {"rm","application/vnd.rn-realmedia"}, {"rtf","text/rtf"}, {"rtx","text/richtext"}, {"sh","application/x-sh"}, {"src","application/x-wais-source"}, {"sv4cpio","application/x-sv4cpio"}, {"sv4crc","application/x-sv4crc"}, {"svg","image/svg+xml"}, {"swf","application/x-shockwave-flash"}, {"tar","application/x-tar"}, {"tif","image/tiff"}, {"tiff","image/tiff"}, {"txt","text/plain"}, {"vcd","application/x-cdlink"}, {"wav","audio/x-wav"}, {"xbm","image/x-xbitmap"}, {"xht","application/xhtml+xml"}, {"xhtml","application/xhtml+xml"}, {"xls","application/vnd.ms-excel"}, {"xml","application/xml"}, {"xlsx","application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"}, {"xltx","application/vnd.openxmlformats-officedocument.spreadsheetml.template"}, {"xlsm","application/vnd.ms-excel.sheet.macroEnabled.12"}, {"xltm","application/vnd.ms-excel.template.macroEnabled.12"}, {"xlam","application/vnd.ms-excel.addin.macroEnabled.12"}, {"xlsb","application/vnd.ms-excel.sheet.binary.macroEnabled.12"}, {"xslt","application/xslt+xml"}, {"xul","application/vnd.mozilla.xul+xml"}, {"xwd","image/x-xwindowdump"}, {"zip","application/zip"}, {"flv","video/x-flv"}, {"rar","application/x-rar-compressed"}, {"7z","application/x-7z-compressed"}};

        private bool Connect()
        {
            _client = new FtpClient(data.Address,data.User, data.Password);
            try { _client.Connect(); return true; }
            catch { return false; }
            
        }
        private bool Connect(int port)
        {
            _client = new FtpClient(data.Address, port, data.User, data.Password);
            try { _client.Connect(); return true; }
            catch { return false; }
        }
        /// <summary>
        /// Sprawdza dostep do serwera FTP
        /// </summary>
        private void Check()
        {
            if (!_client.IsConnected)
                if (Connect())
                    throw new Exception("Problem with connecting to FTP Server");
        }
        /// <summary>
        /// Sprawdza dostęp do serwera FTP oraz do pliku
        /// </summary>
        /// <param name="path">Ścieszka do pliku</param>
        private void Check(string path)
        {
            if (!_client.IsConnected)
                if (Connect())
                    throw new Exception("Problem with connecting to FTP Server");
            if (!_client.FileExists(path) && !_client.DirectoryExists(path))
                throw new FileNotFoundException(path);
        }
        /// <summary>
        /// Sprawdza dostęp do serwera FTP oraz czy można stworzyć plik
        /// </summary>
        /// <param name="path">Ścieszka do pliku</param>
        /// <param name="restrictPath">true - nie zmieniaj nazwy pliku i wywal błąd, gdy ścieszka jest zajęta, false - w przypadku zajętej nazwy pliku zmień nazwę</param>
        /// <returns>path verified to inserting file or folder</returns>
        private string Check(string path,bool restrictPath)
        {
            if (!_client.IsConnected)
                if (Connect())
                    throw new Exception("Problem with connecting to FTP Server");
            var splittedPath = path.Split("/");
            var filePath = splittedPath.Last(); var fileName = filePath.Split(".").First();
            var fileExtention = filePath.Split(".").Count() > 1 ? "." + filePath.Split(".")[1] : "";
            var folderPath = string.Join("/",splittedPath.Take(splittedPath.Count() - 1));
            if (_client.FileExists(path) && restrictPath)
                throw new FilePathTakenException(path);
            else if(_client.FileExists(path) && !restrictPath)
            {
                var rand = new Random();
                int incrementer = 1; var actNewPath = "";
                do
                {
                    actNewPath = folderPath + "/" + fileName + incrementer.ToString() + fileExtention;
                    incrementer++;
                    if (incrementer >= 1000)
                        incrementer = rand.Next(1001,int.MaxValue);
                }
                while (_client.FileExists(actNewPath));
                return actNewPath;
            }
            return path;
        }

        public FtpListItem GetFileStatus(string path)
        {
            Check(path);
            return _client.GetObjectInfo(path);
        }

        public bool CreateFolder(string path)
        {
            path = Check(path, true);
            return _client.CreateDirectory(path);
        }

        public bool DeleteFile(string path)
        {
            Check(path);
            try { _client.DeleteFile(path); return true; }
            catch { return false; }
        }

        public bool DeleteFolder(string path)
        {
            Check(path);
            try { _client.DeleteDirectory(path); return true; }
            catch { return false; }
        }

        public FileModel GetFile(string path)
        {
            Check(path);
            Stream stream = new MemoryStream();
            if (!_client.Download(stream, path))
                throw new Exception("Error with getting data from file " + path);
            var data = _client.GetObjectInfo(path);
            var model = new FileModel();
            ModelOperations.CopyValues(model, data);
            model.Data = stream;
            if(data.Name.Split(".").Length > 1)
            {
                var fileType = data.Name.Split(".")[1];
                model.MIMEType = MIMETypesDictionary.ContainsKey(fileType) ? MIMETypesDictionary[fileType] : "text/plain";
            }
            else
                model.MIMEType = "text/plain";
            return model;
        }

        public IEnumerable<FtpListItem> GetFiles(string path)
        {
            if (path != "/")
                Check(path);
            else
                Check();
            return _client.GetListing(path);
        }

        public bool Rename(string path,string newPath)
        {
            Check(path);
            try { _client.Rename(path,newPath); return true; }
            catch { return false; }
        }

        public FileModel SendFile(FileModel file,string path)
        {
            path = Check(path,false);
            FtpStatus status = _client.Upload(file.Data, path);
            if (status == FtpStatus.Success)
                return file;
            else
                throw new Exception("Error with uploading file " + path);
        }

        public byte[] DownloadFile(string path)
        {
            Check(path);
            var info = _client.GetObjectInfo(path); ;
            byte[] buffer = new byte[info.Size];
            if (!_client.Download(out buffer, path))
                throw new Exception("Error with getting data from file " + path);
            return buffer;
        }
    }
}

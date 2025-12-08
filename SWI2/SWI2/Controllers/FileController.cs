using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using SWI2DB.Models.Authentication;
using SWI2.Persistence;
using SWI2DB.Models.Employee;
using SWI2DB.Models.Client;
using System.Security.Claims;
using SWI2.Services;
using SWI2.Services.Static;
using SWI2.Models.FTP;
using System.IO;
using SWI2DB.Models.Company;
using Microsoft.AspNetCore.Http;
using SWI2.Models.Response;
using FluentFTP;
using System.Net.Mime;
using Microsoft.Win32.SafeHandles;
using Microsoft.Extensions.Logging;

namespace SWI2.Controllers
{
    [ApiController]
    [Authorize(Roles = "Administrator,Employee,Client")]
    [Route("api/filepanel")]
    public class FileController : Controller
    {
        private readonly ILogger<FileController> logger;
        private readonly IStore<Client> clientStore;
        private readonly IStore<Employee> employeeStore;
        private readonly IStore<Company> companyStore;
        private readonly IFileService fileService;
        private readonly UserManager<User> userStore;


        public FileController(
            ILogger<FileController> _logger,
            IStore<Client> _clientStore,
            IStore<Employee> _employeeStore,
            IStore<Company> _companyStore,
            UserManager<User> _userStore,
            IFileService _fileService
        )
        {
            logger = _logger;
            clientStore = _clientStore;
            employeeStore = _employeeStore;
            companyStore = _companyStore;
            userStore = _userStore;
            fileService = _fileService;
        }

        private async Task<User> ActUser()
        {
            var MainClaim = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);
            return await userStore.FindByNameAsync(MainClaim.Value);
        }
        private async Task<IQueryable<Company>> ActUserCompanies()
        {
            var user = await ActUser();
            if (User.IsInRole("Client") || User.IsInRole("Employee"))
            {
                var companiesIdendity = ((ClaimsIdentity)HttpContext.User.Identity).FindFirst("companys").Value;
                var companiesListId = companiesIdendity.Split("_").Select(x => x.Split("|")[0]);
                var companies = companiesListId.Select(x => companyStore.GetById(long.Parse(x))).AsQueryable();
                return companies;
            }
            else
                return null;
        }
        /// <summary>
        /// Weryfikacja operacji na pliku FTP - czy odnosi się do prawidłowego pliku/folderu w zależności od typu operacji
        /// </summary>
        /// <param name="path">Ścieszka pliku</param>
        /// <param name="operation">Rodzaj operacji READ, CREATE, EDIT, DELETE</param>
        /// <returns></returns>
        private bool CheckOperation(string path, string operation)
        {
            if (Regex.Match(path, @"^/users/").Success)
            {
                if (operation == "READ" || operation == "CREATE")
                {
                    return path.Split('/').Length >= 2;
                }
                else
                {
                    return path.Split('/').Length >= 3;
                }
            }
            else if (Regex.Match(path, @"^/companies/").Success)
            {
                if (operation == "READ")
                {
                    return path.Split('/').Length >= 3;
                }
                else if(operation == "CREATE")
                {
                    return path.Split('/').Length >= 4;
                }
                else
                {
                    return path.Split('/').Length >= 5;
                }
            }
            return false;
        }
        /// <summary>
        /// Weryfikacja operacji na pliku FTP - czy odnosi się do prawidłowego pliku/folderu w zależności od typu operacji. 
        /// Administrator ma dostęp do odczytu najwyższych folderów.
        /// </summary>
        /// <param name="path">Ścieszka pliku</param>
        /// <param name="operation">Rodzaj operacji READ, CREATE, EDIT, DELETE</param>
        /// <returns></returns>
        private bool CheckOperationAdmin(string path, string operation)
        {
            if (Regex.Match(path, @"^/users/").Success)
            {
                if (operation == "READ")
                    return path.Split("/").Length >= 1;
                else if (operation == "CREATE")
                    return path.Split('/').Length >= 2;
                else
                    return path.Split('/').Length >= 3;
            }
            else if (Regex.Match(path, @"^/companies/").Success)
            {
                if (operation == "READ")
                    return path.Split("/").Length >= 1;
                else if (operation == "CREATE")
                    return path.Split('/').Length >= 4;
                else
                    return path.Split('/').Length >= 5;
            }
            return false;
        }
        private async Task<IActionResult> CheckUserAccess(User user, string path,string operation, Func<IActionResult> callback)
        {
            if (Regex.Match(path, @"^/users/").Success)
            {
                if (await userStore.IsInRoleAsync(user, "Administrator"))
                {
                    try
                    {
                        if (CheckOperationAdmin(path, operation))
                            return callback.Invoke();
                        else
                            return Forbid();
                    }
                    catch (FileNotFoundException)
                    {
                        return NotFound();
                    }
                }
                else
                {
                    var match = Regex.Match(path, @"^/users/" + user.UserName);
                    if (!match.Success)
                        return Forbid();
                    try
                    {
                        if (CheckOperation(path, operation))
                            return callback.Invoke();
                        else
                            return Forbid();
                    }
                    catch (FileNotFoundException)
                    {
                        return NotFound();
                    }
                }
            }
            else if (Regex.Match(path, @"^/companies/").Success)
            {
                if (await userStore.IsInRoleAsync(user, "Administrator"))
                {
                    try
                    {
                        if (CheckOperationAdmin(path, operation))
                            return callback.Invoke();
                        else
                            return Forbid();
                    }
                    catch (FileNotFoundException)
                    {
                        return NotFound();
                    }
                }
                else
                {
                    //założenie, że path jest w postaci /fol1/fol2
                    long companyId;
                    if (!Int64.TryParse(path.Split("/")[2].Split("-")[0], out companyId))
                        return NotFound();
                    if (!(await ActUserCompanies()).Any(x => x.Id == companyId))
                        return Forbid();
                    try
                    {
                        if (CheckOperation(path, operation))
                            return callback.Invoke();
                        else
                            return Forbid();
                    }
                    catch (FileNotFoundException)
                    {
                        return NotFound();
                    }
                }
            }
            return Forbid();
        }
        private async Task<IActionResult> CheckUserAccess(User user, string path, string operation, Func<Task<IActionResult>> callback)
        {
            if (Regex.Match(path, @"^/users/").Success)
            {
                if (await userStore.IsInRoleAsync(user, "Administrator"))
                {
                    try
                    {
                        if (CheckOperation(path, operation))
                            return await callback.Invoke();
                        else
                            return Forbid();
                    }
                    catch (FileNotFoundException)
                    {
                        return NotFound();
                    }
                }
                else
                {
                    var match = Regex.Match(path, @"^/users/" + user.UserName);
                    if (!match.Success)
                        return Forbid();
                    try
                    {
                        if (CheckOperation(path, operation))
                            return await callback.Invoke();
                        else
                            return Forbid();
                    }
                    catch (FileNotFoundException)
                    {
                        return NotFound();
                    }
                }
            }
            else if (Regex.Match(path, @"^/companies/").Success)
            {
                if (await userStore.IsInRoleAsync(user, "Administrator"))
                {
                    try
                    {
                        if (CheckOperation(path, operation))
                            return await callback.Invoke();
                        else
                            return Forbid();
                    }
                    catch (FileNotFoundException)
                    {
                        return NotFound();
                    }
                }
                else
                {
                    //założenie, że path jest w postaci /fol1/fol2
                    long companyId;
                    if (!Int64.TryParse(path.Split("/")[2].Split("-")[0], out companyId))
                        return NotFound();
                    if (!(await ActUserCompanies()).Any(x => x.Id == companyId))
                        return Forbid();
                    try
                    {
                        if (CheckOperation(path, operation))
                            return await callback.Invoke();
                        else
                            return Forbid();
                    }
                    catch (FileNotFoundException)
                    {
                        return NotFound();
                    }
                }
            }
            return Forbid();
        }

        //Wysyła zawartość folderu domowego dla danego użytkownika
        [HttpGet("home")]
        public async Task<IActionResult> GetHomeFiles()
        {
            var user = await ActUser();
            var path = "/users/" + user.UserName;
            var files = fileService.GetFiles(path);
            return Ok(new FileListModel() { ChildFiles = files.ToArray(), Path = path });
        }
        [HttpGet]
        public async Task<IActionResult> GetFiles([FromQuery] string path)
        {
            var user = await ActUser();
            var realPath = Encoding64.Base64Decode(path); realPath = Regex.Match(realPath, "^home").Success ? "/users/" + user.UserName : realPath;
            return await CheckUserAccess(user, realPath, "READ", () =>
            {
                var files = fileService.GetFiles(realPath);
                return Ok(new FileListModel() { ChildFiles = files.ToArray(), Path = realPath });
            });
        }
        [HttpGet("download")]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> DownloadFile([FromQuery] string path)
        {
            var user = await ActUser();
            var realPath = Encoding64.Base64Decode(path); realPath = Regex.Match(realPath, "^home").Success ? "/users/" + user.UserName : realPath;
            return await CheckUserAccess(user, realPath, "READ", () =>
            {
                var files = fileService.GetFile(realPath);
                var buffer = fileService.DownloadFile(realPath);
                logger.LogInformation(new EventId(181, "DownloadFile"), "user: " + string.Join(",", user.UserName, user.Id) + "; file: " + realPath);
                return File(buffer, files.MIMEType);
            });
        }

        [HttpPost("folder")]
        public async Task<IActionResult> CreateFolder([FromQuery] string path,[FromQuery] string folderName)
        {
            var user = await ActUser();
            var realPath = Encoding64.Base64Decode(path); realPath = Regex.Match(realPath,"^home").Success ? "/users/" + user.UserName : realPath;
            return await CheckUserAccess(user, realPath, "CREATE", () =>
            {
                try
                {
                    if (fileService.CreateFolder(realPath + "/" + folderName))
                    {
                        logger.LogInformation(new EventId(182, "CreateFolder"), "user: " + string.Join(",", user.UserName, user.Id) + "; folder: " + realPath);
                        return Ok(fileService.GetFileStatus(realPath + "/" + folderName));
                    }
                    else
                        return StatusCode(500, "Problem with creating folder");
                }
                catch (FilePathTakenException)
                {
                    return BadRequest("Use another name");
                }
            });
        }
        [HttpPost]
        [DisableRequestSizeLimit]
        public async Task<IActionResult> SendFile([FromQuery] string path,[FromForm] IFormFile file)
        {
            var user = await ActUser();
            var realPath = Encoding64.Base64Decode(path); realPath = Regex.Match(realPath, "^home").Success ? "/users/" + user.UserName : realPath;
            return await CheckUserAccess(user, realPath, "CREATE", async () =>
            {
                if (file.Length > 0)
                {
                    var filePath = Path.GetTempFileName();
                    var fileModel = new FileModel() { Name = file.FileName, Created = DateTime.Now, Modified = DateTime.Now, Size = file.Length, Data = System.IO.File.Create(filePath) };
                    await file.CopyToAsync(fileModel.Data);
                    var returnModel = fileService.SendFile(fileModel, realPath + "/" + fileModel.Name); returnModel.Data = null;
                    logger.LogInformation(new EventId(183, "SendFile"), "user: " + string.Join(",", user.UserName, user.Id) + "; file: " + realPath);
                    return Ok(returnModel);
                }
                else
                    return BadRequest();
            });
        }

        [HttpDelete("folder")]
        public async Task<IActionResult> RemoveFolder([FromQuery] string path)
        {
            var user = await ActUser();
            var realPath = Encoding64.Base64Decode(path); realPath = Regex.Match(realPath, "^home").Success ? "/users/" + user.UserName : realPath;
            return await CheckUserAccess(user, realPath, "DELETE", () =>
            {
                if (fileService.DeleteFolder(realPath))
                {
                    logger.LogInformation(new EventId(184, "RemoveFolder"), "user: " + string.Join(",", user.UserName, user.Id) + "; folder: " + realPath);
                    return Ok();
                }
                else
                    return StatusCode(500, "Problem with removing file");
            });
        }
        [HttpDelete]
        public async Task<IActionResult> RemoveFile([FromQuery] string path)
        {
            var user = await ActUser();
            var realPath = Encoding64.Base64Decode(path); realPath = Regex.Match(realPath, "^home").Success ? "/users/" + user.UserName : realPath;
            return await CheckUserAccess(user, realPath, "DELETE", () =>
            {
                if (fileService.DeleteFile(realPath))
                {
                    logger.LogInformation(new EventId(185, "RemoveFile"), "user: " + string.Join(",", user.UserName, user.Id) + "; file: " + realPath);
                    return Ok();
                } 
                else
                    return StatusCode(500, "Problem with removing file");
            });
        }

        [HttpPut]
        public async Task<IActionResult> Rename([FromQuery] string path,[FromQuery] string newName)
        {
            var user = await ActUser();
            var realPath = Encoding64.Base64Decode(path); realPath = Regex.Match(realPath, "^home").Success ? "/users/" + user.UserName : realPath;
            return await CheckUserAccess(user, realPath, "EDIT", () =>
            {
                var filePath = realPath.Split("/");
                filePath[filePath.Length - 1] = newName;
                var newPath = filePath.Aggregate((a, b) => a + "/" + b);
                if (fileService.Rename(realPath, newPath))
                {
                    logger.LogInformation(new EventId(186, "Rename"), "user: " + string.Join(",", user.UserName, user.Id) + "; file: " + realPath);
                    return Ok(new OperationSuccesfullViewModel<FtpListItem>(fileService.GetFileStatus(newPath)));
                }
                else
                    return StatusCode(500, "Problem with renaming file");
            });
        }


        [HttpGet("search")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> SearchFiles([FromQuery] string path, [FromQuery] string query)
        {
            var user = await ActUser();
            var realPath = Encoding64.Base64Decode(path); realPath = Regex.Match(realPath, "^home").Success ? "/users/" + user.UserName : realPath;
            string realQuery = Encoding64.Base64Decode(query);
            return await CheckUserAccess(user, realPath, "READ", () =>
            {
                var files = fileService.GetFiles(realPath);
                files = files.Where(x => x.Name.Contains(realQuery));
                return Ok(new FileListModel() { ChildFiles = files.ToArray(), Path = realPath });
            });
        }
    }
}

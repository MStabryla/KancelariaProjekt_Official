using BrunoZell.ModelBinding;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SWI2.Extensions;
using SWI2.Models;
using SWI2.Models.Documents;
using SWI2.Models.FTP;
using SWI2.Models.Letter;
using SWI2.Models.Response;
using SWI2.Persistence;
using SWI2.Services;
using SWI2.Services.Static;
using SWI2DB.Models.Account;
using SWI2DB.Models.Authentication;
using SWI2DB.Models.Company;
using SWI2DB.Models.Employee;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace SWI2.Controllers
{
    [ApiController]
    [Authorize(Roles = "Employee,Administrator")]
    [Route("api/documentpanel")]
    public class DocumentController : Controller
    {
        private readonly ILogger<DocumentController> logger;
        private readonly UserManager<User> userManager;
        private readonly IStore<Document> employeeStore;
        private readonly IStore<Document> documentStore;
        private readonly IStore<Letter> letterStore;
        private readonly IStore<Company> companyStore;
        private readonly IStore<DocumentType> documentTypeStore;
        private readonly IStore<LetterRecipent> letterRecipientStore;
        private readonly IFileService fileService;


        public DocumentController(
            ILogger<DocumentController> _logger,
            UserManager<User> _userManager,
            IStore<Company> _companyStore,
            IStore<Document> _employeeStore,
            IStore<Letter> _letterStore,
            IStore<Document> _documentStore,
            IStore<DocumentType> _documentTypeStore,
            IStore<LetterRecipent> _letterRecipientStore,
            IFileService _fileService)
        {
            logger = _logger;
            userManager = _userManager;
            employeeStore = _employeeStore;
            companyStore = _companyStore;
            letterStore = _letterStore;
            documentStore = _documentStore;
            documentTypeStore = _documentTypeStore;
            letterRecipientStore = _letterRecipientStore;
            fileService = _fileService;
        }

        private async Task<User> ActUser()
        {
            var MainClaim = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);
            return await userManager.FindByNameAsync(MainClaim.Value);
        }

        #region MapyEncji
        //Filtry własne dla typów istniejących w klasie ViewModel, a nie istniejących w encji bazowej
        private static Dictionary<string, Func<IQueryable<Document>, FilterModel, IQueryable<Document>>> docCustomFilters = new Dictionary<string, Func<IQueryable<Document>, FilterModel, IQueryable<Document>>>()
        {
            { "companyName", (query,fm) => query.Where(x => x.Company != null & x.Company.Name.Contains(fm.Value)) },
            { "hasDocumentFile", (query,fm) => query.Where(x => (x.Path != null && x.Path != "") == (fm.Value == "true")) },
            { "documentType", (query,fm) => query.Where(x => x.DocumentType != null & x.DocumentType.Name.Contains(fm.Value)) },
            { "documentFileType", (query,fm) => query.Where(x => x.Path != null && x.Path.Contains("." + fm.Value)) },
        };
        private static Dictionary<string, Func<IQueryable<Document>, bool, IQueryable<Document>>> docCustomSorters = new Dictionary<string, Func<IQueryable<Document>, bool, IQueryable<Document>>>()
        {
            { "companyName", (query,desc) => !desc ? query.Where(x => x.Company != null).OrderBy(x => x.Company.Name) : query.Where(x => x.Company != null).OrderByDescending(x => x.Company.Name)},
            { "hasDocumentFile", (query,desc) => !desc ? query.OrderBy(x => (x.Path != null && x.Path != "")) : query.OrderByDescending(x => (x.Path != null && x.Path != ""))},
            { "documentType", (query,desc) => !desc ? query.Where(x => x.DocumentType != null).OrderBy(x => x.DocumentType.Name) : query.Where(x => x.DocumentType != null).OrderByDescending(x => x.DocumentType.Name)},
            { "documentFileType", (query,desc) => !desc ? query.Where(x => x.Path != null).OrderBy(x => x.Path.Substring(x.Path.IndexOf(".")) ) : query.Where(x => x.Path != null).OrderByDescending(x => x.Path.Substring(x.Path.IndexOf("."))) }
        };

        private static Dictionary<string, Func<IQueryable<Letter>, FilterModel, IQueryable<Letter>>> letCustomFilters = new Dictionary<string, Func<IQueryable<Letter>, FilterModel, IQueryable<Letter>>>()
        {
            { "HasLetterFile", (query,fm) => query.Where(x => (x.Path != null && x.Path != "") == (fm.Value == "true")) },
        };
        private static Dictionary<string, Func<IQueryable<Letter>, bool, IQueryable<Letter>>> letCustomSorters = new Dictionary<string, Func<IQueryable<Letter>, bool, IQueryable<Letter>>>()
        {
            { "HasLetterFile", (query,desc) => !desc ? query.OrderBy(x => (x.Path != null && x.Path != "")) : query.OrderByDescending(x => (x.Path != null && x.Path != "")) }
        };
        #endregion


        //Obsługa listów do kancelarii
        [HttpGet]
        [Route("letter")]
        public IActionResult GetLetters([FromQuery] string query)
        {
            TableParamsModel tableParams = query != null ? JsonConvert.DeserializeObject<TableParamsModel>(Encoding64.Base64Decode(query)) : new TableParamsModel() { Sort = "created", PageNumber = 0, PageSize = 25, Filters = new List<FilterModel>() };
            IQueryable<Letter> IQuery = letterStore.AsQueryable();
            foreach (FilterModel fm in tableParams.Filters)
            {
                if (letCustomFilters.ContainsKey(fm.Name))
                {
                    IQuery = letCustomFilters[fm.Name](IQuery, fm);
                    continue;
                }
                switch (fm.Type)
                {
                    case "string":
                        IQuery = IQuery.Where(fm.Name + ".Contains(@0)", fm.Value);
                        break;
                    case "date":
                        IQuery = IQuery.Where($"" + fm.Name + "@0", Convert.ToDateTime(fm.Value));
                        break;
                    default:
                        IQuery = IQuery.Where(fm.Name + ".Contains(@0)", fm.Value);
                        break;
                }
            }
            //Sortowanie na podstawie parametrów encji bazowej
            if ((typeof(Letter)).GetProperties().Select(x => x.Name.ToLower()).Contains(tableParams.Sort.ToLower().Split(" ")[0]))
                //Sortowanie na podstawie parametrów encji bazowej
                IQuery = IQuery.OrderBy(tableParams.Sort);
            else if (letCustomSorters.ContainsKey(tableParams.Sort.Split(" ")[0]))
                //Sortowanie na podstawie parametrów encji nie istniejących w bazie
                IQuery = letCustomSorters[tableParams.Sort.Split(" ")[0]](IQuery, tableParams.Sort.Split(" ").Length > 1 ? tableParams.Sort.Split(" ")[1] == "desc" : false);
            var pagedResult = IQuery.GetPaged(tableParams.PageNumber, tableParams.PageSize);
            var elements = pagedResult.Results.Select(x =>
            {
                var m = new LetterViewModel();
                ModelOperations.CopyValues(m, x, new string[] { });
                m.HasLetterFile = x.Path != null && x.Path != "";
                return m;
            });
            
            return Ok(new TableViewModel<LetterViewModel>
            {
                totalCount = pagedResult.RowCount,
                elements = elements.ToList()
            });
        }
        [HttpGet]
        [Route("letter/{id}")]
        public async Task<IActionResult> GetLetter(long? id)
        {
            var data = await letterStore.GetByIdAsync(id);
            if (data == null)
                return NotFound();
            var m = new LetterViewModel();
            m.HasLetterFile = data.Path != null && data.Path != "";
            m.LetterFileType = data.Path != null && data.Path.Split(".").Length > 1 ? data.Path.Split(".")[1] : "";
            m.HasLetterFile = m.HasLetterFile ? fileService.GetFile("/letters/" + data.Id + "." + m.LetterFileType) != null : false;
            ModelOperations.CopyValues(m, data, new string[] { });
            return Ok(m);
        }
        [HttpGet("letter/{id}/download")]
        [IgnoreAntiforgeryToken]
        public IActionResult DownloadLetter(long? id)
        {
            var model = letterStore.GetById(id);
            if (model == null)
                return NotFound();
            var realPath = model.Path;
            if (realPath == null || realPath == "")
                return BadRequest("Letter doesn't contain any file");
            var files = fileService.GetFile(realPath);
            var buffer = fileService.DownloadFile(realPath);
            return File(buffer, files.MIMEType);
        }
        [HttpPost]
        [Route("letter")]
        public async Task<IActionResult> InsertLetter([FromForm] string modelJson, [FromForm] IFormFile file)
        {
            LetterViewModel model = JsonConvert.DeserializeObject<LetterViewModel>(modelJson);
            var letter = new Letter();
            ModelOperations.CopyValues(letter, model);
            letter.Created = DateTime.Now;
            letter.Updated = DateTime.Now;
            var user = await ActUser();
            letter.Employee = User.IsInRole("Employee") ? user.Employee : null;
            var letterRecipient = letterRecipientStore.GetById(model.LetterRecipientId);
            if (letterRecipient == null)
                return NotFound("letterRecipient");
            letter.LetterRecipent = letterRecipient;
            if (!await letterStore.InsertAsync(letter))
                return StatusCode(500, "Problem with creating letter");
            ModelOperations.CopyValues(model, letter, new string[] { });
            if (file != null && file.Length > 0)
            {
                var extention = file.Name.Split(".").Length > 1 ? file.Name.Split(".")[1] : null;
                var filePath = Path.GetTempFileName();
                var fileModel = new FileModel() { Name = letter.Id.ToString() + (extention != null ? extention : ""), Created = DateTime.Now, Modified = DateTime.Now, Size = file.Length, Data = System.IO.File.Create(filePath) };
                await file.CopyToAsync(fileModel.Data);
                var path = "/letters/" + letter.Id.ToString() + "." + (file.FileName.Split(".").Length > 1 ? file.FileName.Split(".")[1] : "");
                try { var returnModel = fileService.SendFile(fileModel, path); letter.Path = path; await letterStore.Update(letter); }
                catch (FilePathTakenException) { await letterStore.DeleteAsync(letter); return BadRequest("File with name " + fileModel.Name + " already exist."); }
                catch (Exception ex) { await letterStore.DeleteAsync(letter); throw ex; }
            }
            logger.LogInformation(new EventId(161, "InsertLetter"), "user: " + string.Join(",", user.UserName, user.Id) + "; letter: " + letter.Id );
            return Ok(new OperationSuccesfullViewModel<LetterViewModel>(model));
        }
        [HttpGet]
        [Route("letter/recipients")]
        public IActionResult GetLetterRecipient([FromQuery] string query)
        {
            TableParamsModel tableParams = query != null ? JsonConvert.DeserializeObject<TableParamsModel>(Encoding64.Base64Decode(query)) : new TableParamsModel() { Sort = "Created", PageNumber = 0, PageSize = 25, Filters = new List<FilterModel>() };
            IQueryable<LetterRecipent> IQuery = letterRecipientStore.AsQueryable().Include(x => x.Letters);
            foreach (FilterModel fm in tableParams.Filters)
            {
                if (!(typeof(LetterRecipent)).GetProperties().Select(x => x.Name).Contains(tableParams.Sort))
                    return BadRequest("Wrong property");
                switch (fm.Type)
                {
                    case "string":
                        IQuery = IQuery.Where(fm.Name + ".Contains(@0)", fm.Value);
                        break;
                    case "date":
                        IQuery = IQuery.Where($"" + fm.Name + "@0", Convert.ToDateTime(fm.Value));
                        break;
                    default:
                        IQuery = IQuery.Where(fm.Name + ".Contains(@0)", fm.Value);
                        break;
                }
            }
            if ((typeof(LetterRecipent)).GetProperties().Select(x => x.Name).Contains(tableParams.Sort))
                //Sortowanie na podstawie parametrów encji bazowej
                IQuery = IQuery.OrderBy(tableParams.Sort);
            var pagedResult = IQuery.GetPaged(tableParams.PageNumber, tableParams.PageSize);
            var elements = pagedResult.Results.Select(x =>
            {
                var m = new LetterRecipientViewModel();
                ModelOperations.CopyValues(m, x, new string[] { });
                m.CanBeDeleted = x.Letters.Count() == 0;
                return m;
            });
            return Ok(new TableViewModel<LetterRecipientViewModel>
            {
                totalCount = pagedResult.RowCount,
                elements = elements.ToList()
            });
        }
        [HttpPost]
        [Route("letter/recipients")]
        public async Task<IActionResult> InsertLetterRecipient(LetterRecipientViewModel model)
        {
            var user = await ActUser();
            var letterRec = new LetterRecipent();
            ModelOperations.CopyValues(letterRec, model);
            if (!await letterRecipientStore.InsertAsync(letterRec))
                return StatusCode(500, "Problem with creating letter recipent");
            ModelOperations.CopyValues(model, letterRec, new string[] { });
            logger.LogInformation(new EventId(162, "InsertLetterRecipient"), "user: " + string.Join(",", user.UserName, user.Id) + "; letter recipient: " + string.Join(",", letterRec.Name, letterRec.Id));
            return Ok(new OperationSuccesfullViewModel<LetterRecipientViewModel>(model));
        }
        //Obłusga dokumentów
        [HttpGet("document")]
        public IActionResult GetDocuments([FromQuery] string query)
        {
            TableParamsModel tableParams = query != null ? JsonConvert.DeserializeObject<TableParamsModel>(Encoding64.Base64Decode(query)) : new TableParamsModel() { Sort = "Created", PageNumber = 0, PageSize = 25, Filters = new List<FilterModel>() };
            IQueryable<Document> IQuery = documentStore.AsQueryable().Include(x => x.Company).Include(x => x.DocumentType);
            foreach (FilterModel fm in tableParams.Filters)
            {
                if (docCustomFilters.ContainsKey(fm.Name))
                {
                    IQuery = docCustomFilters[fm.Name](IQuery, fm);
                    continue;
                }
                switch (fm.Type)
                {
                    case "string":
                        IQuery = IQuery.Where(fm.Name + ".Contains(@0)", fm.Value);
                        break;
                    case "date":
                        IQuery = IQuery.Where($"" + fm.Name + "@0", Convert.ToDateTime(fm.Value));
                        break;
                    default:
                        IQuery = IQuery.Where(fm.Name + ".Contains(@0)", fm.Value);
                        break;
                }
            }
            //Sortowanie na podstawie parametrów encji bazowej
            if ((typeof(Document)).GetProperties().Select(x => x.Name.ToLower()).Contains(tableParams.Sort.ToLower().Split(" ")[0]))
                //Sortowanie na podstawie parametrów encji bazowej
                IQuery = IQuery.OrderBy(tableParams.Sort);
            else if (docCustomSorters.ContainsKey(tableParams.Sort.Split(" ")[0]))
                //Sortowanie na podstawie parametrów encji nie istniejących w bazie
                IQuery = docCustomSorters[tableParams.Sort.Split(" ")[0]](IQuery, tableParams.Sort.Split(" ").Length > 1 ? tableParams.Sort.Split(" ")[1] == "desc" : false);
            var pagedResult = IQuery.GetPaged(tableParams.PageNumber, tableParams.PageSize);
            var elements = pagedResult.Results.Select(x =>
            {
                var m = new DocumentViewModel();
                ModelOperations.CopyValues(m, x, new string[] { "DocumentType" });
                m.HasDocumentFile = x.Path != null && x.Path != "";
                m.CompanyName = x.Company.Name;
                m.DocumentType = x.DocumentType.Name;
                m.DocumentFileType = x.Path != null && x.Path.Split(".").Length > 1 ? x.Path.Split(".")[1] : "";
                return m;
            });
            return Ok(new TableViewModel<DocumentViewModel>
            {
                totalCount = pagedResult.RowCount,
                elements = elements.ToList()
            });
        }
        [HttpGet("document/{id}")]
        public async Task<IActionResult> GetDocument(long? id)
        {
            var model = await documentStore.GetByIdAsync(id);
            if (model == null)
                return NotFound();
            var m = new DocumentViewModel();
            ModelOperations.CopyValues(m, model, new string[] { "DocumentType" });
            m.HasDocumentFile = model.Path != null && model.Path != "";
            m.CompanyName = model.Company.Name;
            m.DocumentType = model.DocumentType.Name;
            m.DocumentFileType = model.Path != null && model.Path.Split(".").Length > 1 ? model.Path.Split(".")[1] : "";
            m.HasDocumentFile = m.HasDocumentFile ? fileService.GetFile("/documents/" + model.Id + "." + m.DocumentFileType) != null : false;
            return Ok(m);
        }
        [HttpGet("document/{id}/download")]
        [IgnoreAntiforgeryToken]
        public IActionResult DownloadDocument(long? id)
        {
            var model = documentStore.GetById(id);
            if (model == null)
                return NotFound();
            var realPath = model.Path;
            if (realPath == null || realPath == "")
                return BadRequest("Document doesn't contain any file");
            var files = fileService.GetFile(realPath);
            var buffer = fileService.DownloadFile(realPath);
            return File(buffer, files.MIMEType);
        }
        [HttpPost("document")]
        public async Task<IActionResult> InsertDocument([FromForm] string modelJson, [FromForm] IFormFile file)
        {
            var user = await ActUser();
            DocumentViewModel model = JsonConvert.DeserializeObject<DocumentViewModel>(modelJson);
            var document = new Document();
            ModelOperations.CopyValues(document, model);
            document.Created = DateTime.Now;
            document.Updated = DateTime.Now;
            document.User = await ActUser();
            if (companyStore.GetById(model.CompanyId) == null)
                return NotFound("company");
            document.Company = companyStore.GetById(model.CompanyId);
            if (documentTypeStore.GetById(model.DocumentTypeId) == null)
                return NotFound("documentType");
            document.DocumentType = documentTypeStore.GetById(model.DocumentTypeId);
            if (!await documentStore.InsertAsync(document))
                return StatusCode(500, "Problem with creating document");
            ModelOperations.CopyValues(model, document, new string[] { "DocumentType"});
            if (file != null && file.Length > 0)
            {
                var extention = file.Name.Split(".").Length > 1 ? file.Name.Split(".")[1] : null;
                var filePath = Path.GetTempFileName();
                var fileModel = new FileModel() { Name = document.Id.ToString() + (extention != null ? extention : ""), Created = DateTime.Now, Modified = DateTime.Now, Size = file.Length, Data = System.IO.File.Create(filePath) };
                await file.CopyToAsync(fileModel.Data);
                var path = "/documents/" + document.Id.ToString() + "." + (file.FileName.Split(".").Length > 1 ? file.FileName.Split(".")[1] : "");
                try { var returnModel = fileService.SendFile(fileModel, path); document.Path = path; await documentStore.Update(document); }
                catch (FilePathTakenException) { await documentStore.DeleteAsync(document); return BadRequest("File with name " + fileModel.Name + " already exist."); }
                catch (Exception ex) { await documentStore.DeleteAsync(document); throw ex; }
            }
            logger.LogInformation(new EventId(163, "InsertDocument"), "user: " + string.Join(",", user.UserName, user.Id) + "; document: " + document.Id);
            return Ok(new OperationSuccesfullViewModel<DocumentViewModel>(model));
        }
        [HttpGet("document/types")]
        public IActionResult GetDocumentTypes()
        {
            return Ok(documentTypeStore.AsQueryable().Include(x => x.Documents).ToList().Select(x =>
            {
                var m = new DocumentTypeViewModel();
                ModelOperations.CopyValues(m, x, new string[] { });
                m.CanBeDeleted = x.Documents.Count() == 0;
                return m;
            }));
        }
        [HttpPost]
        [Route("document/types")]
        public async Task<IActionResult> InsertDocumentType(DocumentTypeViewModel model)
        {
            var user = await ActUser();
            DocumentType docType = new DocumentType();
            docType.Created = DateTime.Now;
            ModelOperations.CopyValues(docType, model);
            if (!await documentTypeStore.InsertAsync(docType))
                return StatusCode(500, "Error with inserting document type");
            ModelOperations.CopyValues(model, docType, new string[] { });
            logger.LogInformation(new EventId(164, "InsertDocumentType"), "user: " + string.Join(",", user.UserName, user.Id) + "; document type: " + string.Join(",", docType.Name, docType.Id));
            return Ok(new OperationSuccesfullViewModel<DocumentTypeViewModel>(model));
        }
        [HttpGet("document/companies")]
        public IActionResult GetCompaniesToCreateDocument([FromQuery] string query)
        {
            TableParamsModel tableParams = query != null ? JsonConvert.DeserializeObject<TableParamsModel>(Encoding64.Base64Decode(query)) : new TableParamsModel() { Sort = "created", PageNumber = 0, PageSize = 25, Filters = new List<FilterModel>() };
            FilterModel fm = tableParams.Filters.FirstOrDefault(x => x.Name == "name");
            var companies = companyStore.AsQueryable().Where(x => x.Name.Contains(fm.Value ?? "")).Take(20);
            return Ok(companies.Select(x => new { x.Id, x.Name }));
        }
        
        //Sekcja Administratora

        //usuwanie listów
        [HttpDelete("letter/{id}")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> RemoveLetter(long? id)
        {
            var user = await ActUser();
            var letter = letterStore.GetById(id);
            if (letter == null)
                return NotFound();
            await letterStore.DeleteAsync(letter);
            logger.LogInformation(new EventId(166, "RemoveLetter"), "user: " + string.Join(",", user.UserName, user.Id) + "; letter: " + letter.Id);
            return Ok(new OperationSuccesfullViewModel<Letter>(letter));
        }
        [HttpDelete("letter/recipients/{id}")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> RemoveLetterRecipient(long? id)
        {
            var user = await ActUser();
            var letterRec = letterRecipientStore.GetById(id);
            if (letterRec == null)
                return NotFound();
            if (letterRec.Letters.Count > 0)
                return BadRequest("This letter recipient is associated with one or more letters. Remove this letters first.");
            await letterRecipientStore.DeleteAsync(letterRec);
            logger.LogInformation(new EventId(167, "RemoveLetterRecipient"), "user: " + string.Join(",", user.UserName, user.Id) + "; letter recipient: " + letterRec.Id);
            return Ok(new OperationSuccesfullViewModel<LetterRecipent>(letterRec));
        }
        //usuwanie dokumentów
        [HttpDelete("document/{id}")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> RemoveDocument(long? id)
        {
            var user = await ActUser();
            var document = documentStore.GetById(id);
            if (document == null)
                return NotFound();
            if(document.Path != null && document.Path != "")
                if (!fileService.DeleteFile(document.Path))
                    return StatusCode(500, "problem with removing file");
            await documentStore.DeleteAsync(document);
            logger.LogInformation(new EventId(168, "RemoveDocument"), "user: " + string.Join(",", user.UserName, user.Id) + "; ldocument: " + document.Id);
            return Ok(new OperationSuccesfullViewModel<Document>(document));
        }
        [HttpDelete("document/types/{id}")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> RemoveDocumentType(long? id)
        {
            var user = await ActUser();
            var documentType = documentTypeStore.GetById(id);
            if (documentType == null)
                return NotFound();
            if (documentType.Documents.Count > 0)
                return BadRequest("This document type is associated with one or more of documents. Remove this documents first.");
            await documentTypeStore.DeleteAsync(documentType);
            logger.LogInformation(new EventId(169, "RemoveDocumentType"), "user: " + string.Join(",", user.UserName, user.Id) + "; document type: " + documentType.Id);
            return Ok(new OperationSuccesfullViewModel<DocumentType>(documentType));
        }
    }
}

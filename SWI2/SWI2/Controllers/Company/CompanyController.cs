using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SWI2.Extensions;
using SWI2.Models;
using SWI2.Models.Company;
using SWI2.Models.Response;
using SWI2.Persistence;
using SWI2.Services;
using SWI2.Services.AuthorityHelper;
using SWI2.Services.Static;
using SWI2DB.Models.Authentication;
using SWI2DB.Models.Client;
using SWI2DB.Models.Company;
using SWI2DB.Models.Employee;
using SWI2DB.Models.Invoice;
using Microsoft.Extensions.Logging;

namespace SWI2.Controllers
{
    [ApiController]
    [Route("api/companypanel")]
    [Authorize]
    public class CompanyController : Controller
    {
        private readonly ILogger<CompanyController> logger;
        private readonly IStore<Client> clientStore;
        private readonly IStore<Company> companyStore;
        private readonly IStore<Employee> employeeStore;
        private readonly IStore<InvoiceMailTemplate> invoiceMailTemplateStore;
        private readonly IStore<InvoiceIssuer> invoiceStore;
        private readonly IStore<PaymentMethodDictionary> paymentMethodStore;
        private readonly UserManager<User> userStore;
        private readonly IFileService fileService;


        public CompanyController(
            ILogger<CompanyController> _logger,
            IStore<Client> _clientStore,
            IStore<Employee> _employeeStore,
            IStore<InvoiceMailTemplate> _invoiceMailTemplateStore,
            IStore<Company> _companyStore,
            IStore<InvoiceIssuer> _invoiceStore,
            IStore<PaymentMethodDictionary> _paymentMethodStore,
            UserManager<User> _userStore,
            IFileService _fileService
        )
        {
            logger = _logger;
            clientStore = _clientStore;
            companyStore = _companyStore;
            employeeStore = _employeeStore;
            paymentMethodStore = _paymentMethodStore;
            invoiceMailTemplateStore = _invoiceMailTemplateStore;
            userStore = _userStore;
            fileService = _fileService;
            invoiceStore = _invoiceStore;
        }

        private async Task<User> ActUser()
        {
            var MainClaim = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);
            return await userStore.FindByNameAsync(MainClaim.Value);
        }
        private async Task<IQueryable<Company>> ActUserCompanies()
        {
            var user = await ActUser();
            if (User.IsInRole("Client"))
            {
                var clientDetails = await clientStore.Table.Include(x => x.ClientCompany).FirstOrDefaultAsync(x => x.Id == user.ClientId);
                var companies = companyStore.AsQueryable().Where(x => x.ClientCompany.Any(y => y.Client == clientDetails));
                return companies;
            }
            else if (User.IsInRole("Employee"))
            {
                var employeeDetails = await employeeStore.Table.Include(x => x.Departments).FirstOrDefaultAsync(x => x.Id == user.EmployeeId);
                var companies = companyStore.AsQueryable().Where(x => x.Departments.Any(y => employeeDetails.Departments.Any(z => z == y)));
                return companies;
            }
            return null;
        }
        private async Task<IQueryable<Company>> ActUserCompanies(string id)
        {
            var user = await userStore.FindByIdAsync(id);
            if (await userStore.IsInRoleAsync(user, "Client"))
            {
                var clientDetails = await clientStore.Table.Include(x => x.ClientCompany).FirstOrDefaultAsync(x => x.Id == user.ClientId);
                var companies = companyStore.AsQueryable().Where(x => x.ClientCompany.Any(y => y.Client == clientDetails));
                return companies;
            }
            else if (await userStore.IsInRoleAsync(user, "Employee"))
            {
                var employeeDetails = await employeeStore.Table.Include(x => x.Departments).FirstOrDefaultAsync(x => x.Id == user.EmployeeId);
                var companies = companyStore.AsQueryable().Where(x => x.Departments.Any(y => employeeDetails.Departments.Any(z => z == y)));
                return companies;
            }
            return null;
        }
        private readonly Func<Company, CompanyViewModel> _convertFunc = (x) =>
        {
            var model = new CompanyViewModel();
            ModelOperations.CopyValues(model, x, new string[] { });
            return model;
        };

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCompany(long? id)
        {
            if (User.IsInRole("Employee") || User.IsInRole("Client"))
            {
                var company = companyStore.GetById(id);
                if (company == null)
                    return NotFound();
                var clientCompanies = await ActUserCompanies();
                if (!clientCompanies.Any(x => x.Id == id))
                    return Forbid();
                var returnModel = new CompanyViewModel();
                ModelOperations.CopyValues(returnModel, company, new string[] { });
                return Ok(returnModel);
            }
            else if (User.IsInRole("Administrator"))
            {
                var company = companyStore.GetById(id);
                if (company == null)
                    return NotFound();
                var returnModel = new CompanyViewModel();
                ModelOperations.CopyValues(returnModel, company, new string[] { });
                return Ok(returnModel);
            }
            return Forbid();
        }
        [HttpGet("")]
        public async Task<IActionResult> GetCompanies([FromQuery] string query)
        {
            TableParamsModel tableParams = query != null ? JsonConvert.DeserializeObject<TableParamsModel>(Encoding64.Base64Decode(query)) : new TableParamsModel() { Sort = "created", PageNumber = 0, PageSize = 25, Filters = new List<FilterModel>() };
            IQueryable<Company> IQuery;
            if (User.IsInRole("Employee") || User.IsInRole("Client"))
            {
                IQuery = (await ActUserCompanies()).OrderBy(tableParams.Sort).AsQueryable();

            }
            else if (User.IsInRole("Administrator"))
            {
                IQuery = companyStore.AsQueryable();

            }
            else
                return Forbid();
            foreach (FilterModel fm in tableParams.Filters)
            {
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
            var pagedResult = IQuery.GetPaged(tableParams.PageNumber, tableParams.PageSize);
            return Ok(new TableViewModel<CompanyViewModel>
            {
                totalCount = pagedResult.RowCount,
                elements = pagedResult.Results.Select(_convertFunc).ToArray()
            });
        }

        [HttpPost("")]
        public async Task<IActionResult> InsertCompany([FromBody] CompanyViewModel model)
        {
            var user = await ActUser();
            if (User.IsInRole("Client"))
            {
                var clientDetails = await clientStore.Table.FirstOrDefaultAsync(x => x.Id == user.ClientId);
                //if (clientDetails.Company != default)
                //    return BadRequest(new ErrorResponseViewModel("client is associated with existing company"));
                var company = new Company();
                ModelOperations.CopyValues(company, model);
                company.ClientCompany = new List<ClientCompany>
                {
                    new ClientCompany(){Client = clientDetails, Company = company, IsInBoard = true }
                };
                company.Created = DateTime.UtcNow;
                company.Updated = DateTime.UtcNow;
                await companyStore.InsertAsync(company);
                if (!fileService.CreateFolder("/companies/" + company.Id))
                {
                    await companyStore.DeleteAsync(company);
                    return StatusCode(500, "Problem with creating ftp folder");
                }
                ModelOperations.CopyValues(model, company, new string[] { });
                logger.LogInformation(new EventId(141, "InsertCompany"), "user: " + string.Join(",", user.UserName, user.Id) + "; company: " + string.Join(",", company.Name, company.Id));
                return Ok(new OperationSuccesfullViewModel<CompanyViewModel>(model));
            }
            else if (User.IsInRole("Administrator"))
            {
                var company = new Company();
                ModelOperations.CopyValues(company, model);
                company.Created = DateTime.UtcNow;
                company.Updated = DateTime.UtcNow;
                await companyStore.InsertAsync(company);
                if (!fileService.CreateFolder("/companies/" + company.Id))
                {
                    await companyStore.DeleteAsync(company);
                    return StatusCode(500, "Problem with creating ftp folder");
                }
                ModelOperations.CopyValues(model, company, new string[] { });
                logger.LogInformation(new EventId(141, "InsertCompany"), "user: " + string.Join(",", user.UserName, user.Id) + "; company: " + string.Join(",", company.Name, company.Id));
                return Ok(new OperationSuccesfullViewModel<CompanyViewModel>(model));
            }
            return Forbid();
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> EditCompany(long? id, [FromBody] CompanyViewModel model)
        {
            var user = await ActUser();
            if (/*User.IsInRole("Employee") || */User.IsInRole("Client"))
            {
                if (id != model.Id)
                    return BadRequest(new ErrorResponseViewModel("Id fields are not equals"));
                var company = companyStore.GetById(id);
                if (company == null)
                    return NotFound();
                var clientCompanies = await ActUserCompanies();
                if (!clientCompanies.Any(x => x.Id == id))
                    return Forbid();
                ModelOperations.CopyValues(company, model);
                company.Updated = DateTime.UtcNow;
                await companyStore.Update(company);
                //dodanie do InvoiceIssuer przez transakcję czy ręcznie tutaj?
                InvoiceIssuer ii = new InvoiceIssuer();
                ModelOperations.CopyValues(ii, company, new string[] { "Id", "Created", "Updated" });
                ii.Created = (DateTime)(ii.Updated = DateTime.Now);
                ii.Company = company;
                if (!await invoiceStore.InsertAsync(ii))
                    return StatusCode(500, "Problem with inserting invoiceIssuer");
                logger.LogInformation(new EventId(142, "EditCompany"), "user: " + string.Join(",", user.UserName, user.Id) + "; company: " + string.Join(",", company.Name, company.Id));
                return Ok(model);
            }
            // Czy pracownik ma edytować?
            else if (User.IsInRole("Administrator"))
            {
                if (id != model.Id)
                    return BadRequest(new ErrorResponseViewModel("Id fields are not equals"));
                var company = companyStore.GetById(id);
                if (company == null)
                    return NotFound();
                ModelOperations.CopyValues(company, model);
                company.Updated = DateTime.UtcNow;
                await companyStore.Update(company);
                //dodanie do InvoiceIssuer przez transakcję czy ręcznie tutaj?
                InvoiceIssuer ii = new InvoiceIssuer();
                ModelOperations.CopyValues(ii, company, new string[] { "Id", "Created", "Updated" });
                ii.Created = (DateTime)(ii.Updated = DateTime.Now);
                ii.Company = company;
                if (!await invoiceStore.InsertAsync(ii))
                    return StatusCode(500, "Problem with inserting invoiceIssuer");
                logger.LogInformation(new EventId(142, "EditCompany"), "user: " + string.Join(",", user.UserName, user.Id) + "; company: " + string.Join(",", company.Name, company.Id));
                return Ok(model);
            }
            return Forbid();
        }
        //Zwraca paymentmethods dla danej firmy
        [HttpGet("{id}/paymentmethod")]
        public async Task<IActionResult> GetPaymentMethods(long? id, [FromQuery] string query)
        {
            TableParamsModel tableParams = query != null ? JsonConvert.DeserializeObject<TableParamsModel>(Encoding64.Base64Decode(query)) : new TableParamsModel() { Sort = "created", PageNumber = 0, PageSize = 25, Filters = new List<FilterModel>() };
            IQueryable<PaymentMethodDictionary> IQuery;

            if (User.IsInRole("Employee") || User.IsInRole("Client"))
            {
                var company = companyStore.GetById(id);
                if (company == null)
                    return NotFound();
                var clientCompanies = await ActUserCompanies();
                if (!clientCompanies.Any(x => x.Id == id))
                    return Forbid();

                IQuery = company.PaymentMethodsDictionary.AsQueryable();
                //return Ok(paymentMethods.Results.ToArray());
            }
            else if (User.IsInRole("Administrator"))
            {
                var company = companyStore.GetById(id);
                if (company == null)
                    return NotFound();
                IQuery = company.PaymentMethodsDictionary.AsQueryable();
                //return Ok(paymentMethods.Results.ToArray());
            }
            else
                return Forbid();
            foreach (FilterModel fm in tableParams.Filters)
            {
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
            var pagedResult = IQuery.GetPaged(tableParams.PageNumber, tableParams.PageSize);
            return Ok(new TableViewModel<PaymentMethodDictionary>
            {
                totalCount = pagedResult.RowCount,
                elements = pagedResult.Results.ToArray()
            });
        }

        [HttpPost("{id}/paymentmethod")]
        public async Task<IActionResult> InsertPaymentMethods(long? id, PaymentMethodViewModel model)
        {
            var user = await ActUser();
            if (User.IsInRole("Employee") || User.IsInRole("Client"))
            {
                var company = companyStore.GetById(id);
                if (company == null)
                    return NotFound();
                var clientCompanies = await ActUserCompanies();
                if (!clientCompanies.Any(x => x.Id == id))
                    return Forbid();
                var paymentMethod = new PaymentMethodDictionary();
                ModelOperations.CopyValues(paymentMethod, model);
                paymentMethod.Company = company;
                await paymentMethodStore.InsertAsync(paymentMethod);
                logger.LogInformation(new EventId(143, "InsertPaymentMethod"), "user: " + string.Join(",", user.UserName, user.Id) + "; company: " + string.Join(",", company.Name, company.Id) + "; paymentMethod: " + string.Join(",", paymentMethod.Name, paymentMethod.Id));
                return Ok(new OperationSuccesfullViewModel<PaymentMethodDictionary>(paymentMethod));
            }
            else if (User.IsInRole("Administrator"))
            {
                var company = companyStore.GetById(id);
                if (company == null)
                    return NotFound();
                var paymentMethod = new PaymentMethodDictionary();
                ModelOperations.CopyValues(paymentMethod, model);
                paymentMethod.Company = company;
                await paymentMethodStore.InsertAsync(paymentMethod);
                logger.LogInformation(new EventId(143, "InsertPaymentMethod"), "user: " + string.Join(",", user.UserName, user.Id) + "; company: " + string.Join(",", company.Name, company.Id) + "; paymentMethod: " + string.Join(",", paymentMethod.Name, paymentMethod.Id));
                return Ok(new OperationSuccesfullViewModel<PaymentMethodDictionary>(paymentMethod));
            }
            return Forbid();
        }
        [HttpPut("paymentmethod/{id}")]
        public async Task<IActionResult> EditPaymentMethods(long? id, PaymentMethodViewModel model)
        {
            var user = await ActUser();
            if (User.IsInRole("Employee") || User.IsInRole("Client"))
            {
                var company = await ActUserCompanies();
                if (company.Count() == 0)
                    return BadRequest(new ErrorResponseViewModel("User is not associated with any company"));
                var paymentMethod = paymentMethodStore.GetById(id);
                if (paymentMethod == null)
                    return NotFound();
                if (paymentMethod.Company == null || !company.Any(x => x.Id == paymentMethod.Company.Id))
                    return Forbid();
                ModelOperations.CopyValues(paymentMethod, model);
                paymentMethod.Updated = DateTime.UtcNow;
                await paymentMethodStore.Update(paymentMethod);
                logger.LogInformation(new EventId(144, "EditPaymentMethod"), "user: " + string.Join(",", user.UserName, user.Id) + "; paymentMethod: " + string.Join(",", paymentMethod.Name, paymentMethod.Id));
                return Ok(new OperationSuccesfullViewModel<PaymentMethodDictionary>(paymentMethod));
            }
            else if (User.IsInRole("Administrator"))
            {
                var paymentMethod = paymentMethodStore.GetById(id);
                if (paymentMethod == default)
                    return NotFound();
                ModelOperations.CopyValues(paymentMethod, model);
                paymentMethod.Updated = DateTime.UtcNow;
                await paymentMethodStore.Update(paymentMethod);
                logger.LogInformation(new EventId(144, "EditPaymentMethod"), "user: " + string.Join(",", user.UserName, user.Id) + "; paymentMethod: " + string.Join(",", paymentMethod.Name, paymentMethod.Id));
                return Ok(new OperationSuccesfullViewModel<PaymentMethodDictionary>(paymentMethod));
            }
            return Forbid();

        }

        [HttpDelete("paymentmethod/{id}")]
        public async Task<IActionResult> RemovePaymentMethods(long? id)
        {
            var user = await ActUser();
            if (User.IsInRole("Employee") || User.IsInRole("Client"))
            {
                var company = await ActUserCompanies();
                if (company.Count() == 0)
                    return BadRequest(new ErrorResponseViewModel("Client is not associated with any company"));
                var paymentMethod = paymentMethodStore.GetById(id);
                if (paymentMethod == null)
                    return NotFound();
                if (paymentMethod.Company == null || !company.Any(x => x.Id == paymentMethod.Company.Id))
                    return Forbid();
                await paymentMethodStore.DeleteAsync(paymentMethod);
                logger.LogInformation(new EventId(145, "RemovePaymentMethod"), "user: " + string.Join(",", user.UserName, user.Id) + "; paymentMethod: " + string.Join(",", paymentMethod.Name, paymentMethod.Id));
                return Ok(new OperationSuccesfullViewModel<PaymentMethodDictionary>(paymentMethod));
            }
            else if (User.IsInRole("Administrator"))
            {
                var paymentMethod = paymentMethodStore.GetById(id);
                if (paymentMethod == default)
                    return NotFound();
                await paymentMethodStore.DeleteAsync(paymentMethod);
                logger.LogInformation(new EventId(145, "RemovePaymentMethod"), "user: " + string.Join(",", user.UserName, user.Id) + "; paymentMethod: " + string.Join(",", paymentMethod.Name, paymentMethod.Id));
                return Ok(new OperationSuccesfullViewModel<PaymentMethodDictionary>(paymentMethod));
            }
            return Forbid();
        }

        #region OldKlient
        //Podstawowe informacje o firmie
        //[HttpGet("")]
        //[Authorize(Roles = "client")]
        //public async Task<IActionResult> GetCompanies()
        //{
        //    var company = await ActUserCompanies();
        //    if (company == null)
        //        return NotFound(new ErrorResponseViewModel("Client is not associated with any company"));
        //    return Ok(company);
        //}
        //Edycja danych firmy pracownika
        //[HttpPut("")]
        //[Authorize(Roles = "client")]
        //public async Task<IActionResult> EditCompany([FromBody]CompanyViewModel model)
        //{
        //    /*var user = await ActUser();
        //    var clientDetails = await clientStore.Table.Include(x => x.Company).FirstAsync(x => x.Id == user.ClientId);*/
        //    var company = await ActUserCompanies();
        //    if (company == null)
        //        return BadRequest(new ErrorResponseViewModel("Client is not associated with any company"));
        //    ModelOperations.CopyValues(company, model);
        //    company.Updated = DateTime.UtcNow;
        //    await companyStore.Update(company);
        //    //dodanie do InvoiceIssuer przez transakcję czy ręcznie tutaj?
        //    return Ok(model);

        //}
        //Zwraca paymentmethods dla firmy klienta
        /*[HttpGet("paymentmethod")]
        [Authorize(Roles = "client")]
        public async Task<IActionResult> GetPaymentMethods()
        {
            var company = await ActUserCompany();
            if (company == null)
                return BadRequest(new ErrorResponseViewModel("Client is not associated with any company"));
            var paymentMethods = company.PaymentMethodsDictionary.Take(20);
            return Ok(paymentMethods.ToArray());
        }*/
        //[HttpGet("paymentmethod")]
        //[Authorize(Roles = "client")]
        //public async Task<IActionResult> GetPaymentMethods([FromQuery] int pageNumber = 0, [FromQuery] int pageSize = 20)
        //{
        //    var company = await ActUserCompanies();
        //    company = await companyStore.GetById(company.Id);
        //    if (company == null)
        //        return BadRequest(new ErrorResponseViewModel("Client is not associated with any company"));
        //    //var query = new QueryViewModel() { Page = Page, ElementsPerPage = ElementsPerPage, Offset = Offset };
        //    //var paymentMethods = ModelOperations.ExecuteQuery(company.PaymentMethodsDictionary, query);
        //    var paymentMethods = company.PaymentMethodsDictionary.AsQueryable().GetPaged(pageNumber, pageSize);
        //    return Ok(paymentMethods.Results.ToArray());
        //}
        //Dodaje paymentmethod do firmy klienta
        //[HttpPost("paymentmethod")]
        //[Authorize(Roles = "client")]
        //public async Task<IActionResult> InsertPaymentMethods(PaymentMethodViewModel model)
        //{
        //    var company = await ActUserCompanies();
        //    if (company == null)
        //        return BadRequest(new ErrorResponseViewModel("Client is not associated with any company"));
        //    var paymentMethod = new PaymentMethodDictionary();
        //    ModelOperations.CopyValues(paymentMethod, model, new string[] { });
        //    paymentMethod.Company = company;
        //    await paymentMethodStore.Insert(paymentMethod);
        //    return Ok(new OperationSuccesfullViewModel<PaymentMethodDictionary>(paymentMethod));
        //}
        #endregion

        //Podstawowe informacje o konretnej firmie
        #region Administrator
        [HttpGet("client/{clientId}")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> GetCompaniesForClient(string clientId, [FromQuery] string query)
        {
            TableParamsModel tableParams = query != null ? JsonConvert.DeserializeObject<TableParamsModel>(Encoding64.Base64Decode(query)) : new TableParamsModel() { Sort = "created", PageNumber = 0, PageSize = 25, Filters = new List<FilterModel>() };
            var client = userStore.FindByIdAsync(clientId);
            if (client == null)
                return NotFound();
            IQueryable<Company> IQuery = await ActUserCompanies(clientId);

            foreach (FilterModel fm in tableParams.Filters)
            {
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
            var pagedResult = IQuery.GetPaged(tableParams.PageNumber, tableParams.PageSize);
            return Ok(new TableViewModel<CompanyViewModel>
            {
                totalCount = pagedResult.RowCount,
                elements = pagedResult.Results.Select(_convertFunc).ToArray()
            });
        }


        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> RemoveCompany(long? id)
        {
            var company = companyStore.GetById(id);
            if (company == null)
                return NotFound();
            if (company.ClientCompany.Count > 0)
            {
                return Conflict(new ErrorResponseViewModel(String.Format("This company has {0} associated clients. Detach these clients from this company before removing company.", company.ClientCompany.Count)));
            }
            try
            {
                var context = companyStore.Context;
                //Usuwanie departamentów firmy
                context.Departments.RemoveRange(company.Departments);
                //Usuwanie dokumentów (na pewno?)
                context.Document.RemoveRange(company.Documents);
                context.PaymentMethodDictionary.RemoveRange(company.PaymentMethodsDictionary);
                //Usuwanie kontraktorów (w InvoiceContractor zostają)
                context.Contractors.RemoveRange(company.Contractors);
                context.EntryDictionary.RemoveRange(company.EntriesDictionary);
                context.InvoiceMailTemplate.RemoveRange(company.InvoiceMailTemplates);
                //Faktury usuwać?
                //Usuwanie folderu FTP
                if (!fileService.DeleteFolder("/companies/" + company.Id))
                {
                    return StatusCode(500, "Problem with removing ftp folder");
                }
                int result = await context.SaveChangesAsync();
                await companyStore.DeleteAsync(company);
                return Ok(new OperationSuccesfullViewModel<int>(result));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ErrorResponseViewModel(ex.Message));
            }
        }
        //Aktywuje daną firmę
        [HttpPost]
        [Authorize(Roles = "Administrator")]
        [Route("{id}/activate")]
        public async Task<IActionResult> ActivateCompany(long? id)
        {
            throw new NotImplementedException();
        }

        //Aktywuje daną firmę
        [HttpGet]
        [Route("mailtemplates/{id}")]
        public async Task<IActionResult> GetInvoiceMailTamplates(long id)
        {
            if (AuthorityHelper.CheckIfHasPermitionForCompany(User.Claims, id))
            {
                return Ok(invoiceMailTemplateStore.Table.Where(imt => imt.Company.Id == id));
            }
            else
            {
                return Unauthorized("Brak autoryzacji do zasobów firmy");
            }

        }
        //Aktywuje daną firmę
        [HttpPost]
        [Route("mailtemplates/{id}")]
        public async Task<IActionResult> EditInvoiceMailTamplate(long id, InvoiceMailTemplate invoiceMailTemplateEdit)
        {
            if (AuthorityHelper.CheckIfHasPermitionForCompany(User.Claims, id))
            {

                if (invoiceMailTemplateEdit.Id != 0)
                {
                    var invoiceMailTemplate = invoiceMailTemplateStore.Table.FirstOrDefault(imt => imt.Id == invoiceMailTemplateEdit.Id && imt.Company.Id == id);
                    if (invoiceMailTemplate != null)
                    {
                        ModelOperations.CopyValues(invoiceMailTemplate, invoiceMailTemplateEdit, new string[] { "Id", "Created", "Updated" });
                        invoiceMailTemplate.Updated = DateTime.Now;
                        await invoiceMailTemplateStore.Update(invoiceMailTemplate);
                        return Ok(invoiceMailTemplate);
                    }
                    else
                    {
                        return Unauthorized("Brak autoryzacji do zasobów firmy");
                    }
                }
                else
                {
                    if (await invoiceMailTemplateStore.InsertAsync(invoiceMailTemplateEdit))
                    {
                        return Ok(invoiceMailTemplateEdit);
                    }
                    else {
                        return BadRequest("Błąd podczas dodawania szablonu mailowego");
                    }
                }
            }
            else
            {
                return Unauthorized("Brak autoryzacji do zasobów firmy");
            }

        }
        #endregion

    }
}

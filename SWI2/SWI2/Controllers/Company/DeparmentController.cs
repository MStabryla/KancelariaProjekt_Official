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
using SWI2.Models.FTP;
using SWI2.Models.Response;
using SWI2.Persistence;
using SWI2.Services;
using SWI2.Services.Static;
using SWI2DB.Models.Authentication;
using SWI2DB.Models.Client;
using SWI2DB.Models.Company;
using SWI2DB.Models.Department;
using SWI2DB.Models.Employee;
using Microsoft.Extensions.Logging;

namespace SWI2.Controllers
{
    [ApiController]
    [Route("api/departmentpanel")]
    [Authorize]
    public class DeparmentController : Controller
    {
        private readonly ILogger<DeparmentController> logger;
        private readonly UserManager<User> userStore;
        private readonly IStore<Department> depStore;
        private readonly IStore<Client> clientStore;
        private readonly IStore<Employee> employeeStore;
        private readonly IStore<Company> companyStore;
        private readonly IFileService fileService;

        public DeparmentController(
            ILogger<DeparmentController> _logger,
            UserManager<User> _userStore,
            IStore<Department> _depStore,
            IStore<Client> _clientStore,
            IStore<Employee> _employeeStore,
            IStore<Company> _companyStore,
            IFileService _fileService
        )
        {
            logger = _logger;
            userStore = _userStore;
            depStore = _depStore;
            clientStore = _clientStore;
            employeeStore = _employeeStore;
            companyStore = _companyStore;
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
            if (User.IsInRole("Client"))
            {
                var clientDetails = await clientStore.Table.Include(x => x.ClientCompany).FirstOrDefaultAsync(x => x.Id == user.ClientId);
                var companies = companyStore.AsQueryable().Include(x => x.Departments).ThenInclude(x => x.Company).Where(x => x.ClientCompany.Any(y => y.Client == clientDetails));
                return companies;
            }
            else if (User.IsInRole("Employee"))
            {
                //await employeeStore.AsQueryable().
                //user.Employee.Departments.AsQueryable().Select(x => x.Company).GroupBy(x => x).
                var employeeDetails = await employeeStore.Table.Include(x => x.Departments).FirstOrDefaultAsync(x => x.Id == user.EmployeeId);
                var companies = companyStore.AsQueryable().Include(x => x.Departments).ThenInclude(x => x.Company).Where(x => x.Departments.Any(y => employeeDetails.Departments.Any(z => z == y)));
                return companies;
            }
            return null;
        }

        #region MapyEncji
        //Filtry własne dla typów istniejących w klasie ViewModel, a nie istniejących w encji bazowej
        private static Dictionary<string, Func<IQueryable<Department>, FilterModel, IQueryable<Department>>> customFilters = new Dictionary<string, Func<IQueryable<Department>, FilterModel, IQueryable<Department>>>()
        {
            { "companyName", (query,fm) => query.Where(x => x.Company != null & x.Company.Name.Contains(fm.Value)) }
        };
        private static Dictionary<string, Func<IQueryable<Department>,bool, IQueryable<Department>> > customSorters = new Dictionary<string, Func<IQueryable<Department>, bool, IQueryable<Department>>>()
        {
            { "companyName", (query,desc) => !desc ? query.Where(x => x.Company != null).OrderBy(x => x.Company.Name) : query.Where(x => x.Company != null).OrderByDescending(x => x.Company.Name)}
        };
        #endregion

        #region OldKlient
        //Pobranie informacji o departamentach danej firmy
        //[HttpGet("")]
        //[Authorize(Roles = "client")]
        //public async Task<IActionResult> GetDeparmentsForClient([FromQuery] int pageNumber = 0, [FromQuery] int pageSize = 20)
        //{
        //    //var query = new QueryViewModel() { Page = Page, ElementsPerPage = ElementsPerPage, Offset = Offset };
        //    var company = await ActUserCompanies();
        //    if (company == null)
        //        return NotFound(new ErrorResponseViewModel("Client is not associated with any company"));
        //    var depCollection = company.Departments.AsQueryable().GetPaged(pageNumber, pageSize);
        //    return Ok(depCollection.Results.Select(x =>
        //    {
        //        var model = new DepartmentViewModel();
        //        ModelOperations.CopyValues(model, x,new string[] { });
        //        return model;
        //    }).ToArray());
        //}
        //[HttpGet("amount")]
        //[Authorize(Roles = "client")]
        //public async Task<IActionResult> GetDeparmentsForClientAmount()
        //{
        //    //var query = new QueryViewModel() { Page = Page, ElementsPerPage = ElementsPerPage, Offset = Offset };
        //    var company = await ActUserCompanies();
        //    if (company == null)
        //        return NotFound(new ErrorResponseViewModel("Client is not associated with any company"));
        //    return Ok(company.Departments.AsQueryable().Count());
        //}
        //[HttpPost("")]
        //[Authorize(Roles = "client")]
        //public async Task<IActionResult> InsertDeparment([FromBody] DepartmentViewModel model)
        //{
        //    var company = await ActUserCompanies();
        //    if (company == null)
        //        return NotFound(new ErrorResponseViewModel("Client is not associated with any company"));
        //    var department = new Department();
        //    ModelOperations.CopyValues(department, model);
        //    department.Company = company;
        //    await depStore.Insert(department);
        //    ModelOperations.CopyValues(model, department, new string[] { });
        //    return Ok(new OperationSuccesfullViewModel<DepartmentViewModel>(model));
        //}
        #endregion

        //Pobranie informacji o departamentach
        [HttpGet("company/{companyid}")]
        public async Task<IActionResult> GetDeparments(long companyid, [FromQuery] string query)
        {
            TableParamsModel tableParams = query != null ? JsonConvert.DeserializeObject<TableParamsModel>(Encoding64.Base64Decode(query)) : new TableParamsModel() { Sort = "created", PageNumber = 0, PageSize = 25, Filters = new List<FilterModel>() };
            IQueryable<Department> IQuery;

            if (User.IsInRole("Employee") || User.IsInRole("Client"))
            {
                var company = companyStore.GetById(companyid);
                if (company == null)
                    return NotFound();
                var clientCompanies = await ActUserCompanies();
                if (!clientCompanies.Any(x => x.Id == companyid))
                    return Forbid();
                IQuery = company.Departments.AsQueryable();
            }
            else if (User.IsInRole("Administrator"))
            {
                var company = companyStore.GetById(companyid);
                if (company == null)
                    return NotFound();
                IQuery = company.Departments.AsQueryable();
            }
            else
                return Forbid();

            IQuery = IQuery.Include(x => x.Company);

            foreach (FilterModel fm in tableParams.Filters)
            {
                if (customFilters.ContainsKey(fm.Name))
                {
                    IQuery = customFilters[fm.Name](IQuery, fm);
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
            if ((typeof(Department)).GetProperties().Select(x => x.Name).Contains(tableParams.Sort))
                //Sortowanie na podstawie parametrów encji bazowej
                IQuery = IQuery.OrderBy(tableParams.Sort);
            else if (customSorters.ContainsKey(tableParams.Sort.Split(" ")[0]))
                //Sortowanie na podstawie parametrów encji nie istniejących w bazie
                IQuery = customSorters[tableParams.Sort.Split(" ")[0]](IQuery, tableParams.Sort.Split(" ").Length > 1 ? tableParams.Sort.Split(" ")[1] == "desc" : false);
            var pagedResult = IQuery.GetPaged(tableParams.PageNumber, tableParams.PageSize);
            var elements = pagedResult.Results.Select(x =>
            {
                var model = new DepartmentViewModel();
                ModelOperations.CopyValues(model, x, new string[] { });
                if (x.Company != null)
                    model.CompanyName = x.Company.Name;
                return model;
            });
            return Ok(new TableViewModel<DepartmentViewModel>
            {
                totalCount = pagedResult.RowCount,
                elements = elements.ToArray()
            });

        }

        //Pobranie informacji o wszystkich departamentach
        [HttpGet("")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> GetDeparments([FromQuery] string query)
        {
            TableParamsModel tableParams = query != null ? JsonConvert.DeserializeObject<TableParamsModel>(Encoding64.Base64Decode(query)) : new TableParamsModel() { Sort = "created", PageNumber = 0, PageSize = 25, Filters = new List<FilterModel>() };
            IQueryable<Department> IQuery;
            //Dołączanie pozostałych tabel do encji, jeśli są potrzebne do generowania klasy ViewModel
            IQuery = depStore.AsQueryable().Include(x => x.Company).AsQueryable();
            
            foreach (FilterModel fm in tableParams.Filters)
            {
                if(customFilters.ContainsKey(fm.Name))
                {
                    IQuery = customFilters[fm.Name](IQuery, fm);
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
            if ((typeof(Department)).GetProperties().Select(x => x.Name).Contains(tableParams.Sort))
                //Sortowanie na podstawie parametrów encji bazowej
                IQuery = IQuery.OrderBy(tableParams.Sort);
            else if (customSorters.ContainsKey(tableParams.Sort.Split(" ")[0]))
                //Sortowanie na podstawie parametrów encji nie istniejących w bazie
                IQuery = customSorters[tableParams.Sort.Split(" ")[0]](IQuery, tableParams.Sort.Split(" ").Length > 1 ? tableParams.Sort.Split(" ")[1] == "desc" : false);
            var pagedResult = IQuery.GetPaged(tableParams.PageNumber, tableParams.PageSize);
            var elements = pagedResult.Results.Select(x =>
            {
                var model = new DepartmentViewModel();
                ModelOperations.CopyValues(model, x, new string[] { });
                if (x.Company != null)
                    model.CompanyName = x.Company.Name;
                return model;
            });
            return Ok(new TableViewModel<DepartmentViewModel>
            {
                totalCount = pagedResult.RowCount,
                elements = elements.ToArray()
            });

        }

        //Pobranie informacji o wszystkich departamentach
        [HttpGet("employee/{employeeId}")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> GetDeparmentsForEmployee(string employeeId, [FromQuery] string query)
        {
            TableParamsModel tableParams = query != null ? JsonConvert.DeserializeObject<TableParamsModel>(Encoding64.Base64Decode(query)) : new TableParamsModel() { Sort = "created", PageNumber = 0, PageSize = 25, Filters = new List<FilterModel>() };
            var employee = await userStore.FindByIdAsync(employeeId);
            if (employee == null)
                return NotFound();
            IQueryable<Department> IQuery;
            //Dołączanie pozostałych tabel do encji, jeśli są potrzebne do generowania klasy ViewModel
            IQuery = depStore.AsQueryable().Include(x => x.Company).Include(x => x.Employees).Where(x => x.Employees.Any(y => y.User == employee)).AsQueryable();

            foreach (FilterModel fm in tableParams.Filters)
            {
                if (customFilters.ContainsKey(fm.Name))
                {
                    IQuery = customFilters[fm.Name](IQuery, fm);
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
            if ((typeof(Department)).GetProperties().Select(x => x.Name).Contains(tableParams.Sort))
                //Sortowanie na podstawie parametrów encji bazowej
                IQuery = IQuery.OrderBy(tableParams.Sort);
            else if (customSorters.ContainsKey(tableParams.Sort.Split(" ")[0]))
                //Sortowanie na podstawie parametrów encji nie istniejących w bazie
                IQuery = customSorters[tableParams.Sort.Split(" ")[0]](IQuery, tableParams.Sort.Split(" ").Length > 1 ? tableParams.Sort.Split(" ")[1] == "desc" : false);
            var pagedResult = IQuery.GetPaged(tableParams.PageNumber, tableParams.PageSize);
            var elements = pagedResult.Results.Select(x =>
            {
                var model = new DepartmentViewModel();
                ModelOperations.CopyValues(model, x, new string[] { });
                if (x.Company != null)
                    model.CompanyName = x.Company.Name;
                return model;
            });
            return Ok(new TableViewModel<DepartmentViewModel>
            {
                totalCount = pagedResult.RowCount,
                elements = elements.ToArray()
            });

        }

        [HttpPost("{companyid}")]
        public async Task<IActionResult> InsertDeparment(long? companyid, [FromBody] DepartmentViewModel model)
        {
            var user = await ActUser();
            if (User.IsInRole("Employee") || User.IsInRole("Client"))
            {
                var company = companyStore.GetById(companyid);
                if (company == null)
                    return NotFound();
                var clientCompanies = await ActUserCompanies();
                if (!clientCompanies.Any(x => x.Id == companyid))
                    return Forbid();
                var department = new Department();
                ModelOperations.CopyValues(department, model);
                department.Company = company;
                await depStore.InsertAsync(department);
                try
                {
                    fileService.CreateFolder("/companies/" + companyid + (company.Name != "" ? "-" + company.Name : "") + "/" + department.FolderName);
                }
                catch (FilePathTakenException)
                {
                    await depStore.DeleteAsync(department);
                    return BadRequest("Name taken. Choose another name.");
                }
                catch
                {
                    await depStore.DeleteAsync(department);
                    return StatusCode(500, "Problem with creating ftp folder");
                }
                ModelOperations.CopyValues(model, department, new string[] { });
                logger.LogInformation(new EventId(151, "InsertDepartment"), "user: " + string.Join(",", user.UserName, user.Id) + "; department: " + string.Join(",", department.Name, department.Id));
                return Ok(new OperationSuccesfullViewModel<DepartmentViewModel>(model));
            }
            else if (User.IsInRole("Administrator"))
            {
                var company = companyStore.GetById(companyid);
                if (company == null)
                    return NotFound();
                var department = new Department();
                ModelOperations.CopyValues(department, model);
                department.Company = company;
                await depStore.InsertAsync(department);
                try
                {
                    fileService.CreateFolder("/companies/" + companyid + (company.Name != "" ? "-" + company.Name : "") + "/" + department.FolderName);
                }
                catch(FilePathTakenException)
                {
                    await depStore.DeleteAsync(department);
                    return BadRequest("Name taken. Choose another name.");
                }
                catch
                {
                    await depStore.DeleteAsync(department);
                    return StatusCode(500, "Problem with creating ftp folder");
                }
                ModelOperations.CopyValues(model, department, new string[] { });
                logger.LogInformation(new EventId(151, "InsertDepartment"), "user: " + string.Join(",", user.UserName, user.Id) + "; department: " + string.Join(",", department.Name, department.Id));
                return Ok(new OperationSuccesfullViewModel<DepartmentViewModel>(model));
            }
            return Forbid();
        }
        //Pobranie informacji o departamentcie
        [HttpGet("{id}")]
        public async Task<IActionResult> GetDeparment(long? id)
        {
            if (User.IsInRole("Employee") || User.IsInRole("Client"))
            {
                var department = depStore.GetById(id);
                if (department == null)
                    return NotFound();
                var actusercompanies = await ActUserCompanies();
                if (!actusercompanies.ToArray().Any(x => x.Id == department.Company.Id))
                    return Forbid();
                var depViewModel = new DepartmentViewModel();
                ModelOperations.CopyValues(depViewModel, department, new string[] { });
                depViewModel.CompanyName = department.Company.Name;
                return Ok(depViewModel);
            }
            else if (User.IsInRole("Administrator"))
            {
                var department = depStore.GetById(id);
                if (department == null)
                    return NotFound();
                var depViewModel = new DepartmentViewModel();
                ModelOperations.CopyValues(depViewModel, department, new string[] { });
                depViewModel.CompanyName = department.Company.Name;
                return Ok(depViewModel);
            }
            return Forbid();
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> EditDeparment(long? id, [FromBody] DepartmentViewModel model)
        {
            var user = await ActUser();
            //Czy pracownik może edytować departamenty?
            if (User.IsInRole("Employee") || User.IsInRole("Client"))
            {
                var department = depStore.GetById(id);
                if (department == null)
                    return NotFound();
                var actusercompanies = await ActUserCompanies();
                if (!actusercompanies.ToArray().Any(x => x.Id == department.Company.Id))
                    return Forbid();
                //var depViewModel = new DepartmentViewModel();
                if (model.FolderName != department.FolderName)
                {
                    try
                    {
                        fileService.Rename("/companies/" + department.Company.Id + (department.Company.Name != "" ? "-" + department.Company.Name : "") + "/" + department.FolderName, "/companies/" + department.Company.Id + "/" + model.FolderName);
                    }
                    catch (FilePathTakenException)
                    {
                        await depStore.DeleteAsync(department);
                        return BadRequest("Name taken. Choose another name.");
                    }
                    catch
                    {
                        await depStore.DeleteAsync(department);
                        return StatusCode(500, "Problem with creating ftp folder");
                    }
                }
                ModelOperations.CopyValues(department, model,new string[] { "Id" });
                await depStore.Update(department);
                ModelOperations.CopyValues(model, department, new string[] { });
                logger.LogInformation(new EventId(152, "EditDeparment"), "user: " + string.Join(",", user.UserName, user.Id) + "; department: " + string.Join(",", department.Name, department.Id));
                return Ok(new OperationSuccesfullViewModel<DepartmentViewModel>(model));
            }
            else if (User.IsInRole("Administrator"))
            {
                var department = depStore.GetById(id);
                if (department == null)
                    return NotFound();
                if (model.FolderName != department.FolderName)
                {
                    try
                    {
                        fileService.Rename("/companies/" + department.Company.Id + (department.Company.Name != "" ? "-" + department.Company.Name : "") + "/" + department.FolderName, "/companies/" + department.Company.Id + "/" + model.FolderName);
                    }
                    catch (FilePathTakenException)
                    {
                        await depStore.DeleteAsync(department);
                        return BadRequest("Name taken. Choose another name.");
                    }
                    catch
                    {
                        await depStore.DeleteAsync(department);
                        return StatusCode(500, "Problem with creating ftp folder");
                    }
                }
                ModelOperations.CopyValues(department, model, new string[] { "Id" });
                await depStore.Update(department);
                ModelOperations.CopyValues(model, department, new string[] { });
                logger.LogInformation(new EventId(152, "EditDeparment"), "user: " + string.Join(",", user.UserName, user.Id) + "; department: " + string.Join(",", department.Name, department.Id));
                return Ok(new OperationSuccesfullViewModel<DepartmentViewModel>(model));
            }
            return Forbid();
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> RemoveDeparment(long? id)
        {
            var user = await ActUser();
            //Czy pracownik może usuwać departamenty?
            if (User.IsInRole("Employee") || User.IsInRole("Client"))
            {
                var department = depStore.GetById(id);
                if (department == null)
                    return NotFound();
                var actusercompanies = await ActUserCompanies();
                if (!actusercompanies.ToArray().Any(x => x.Id == department.Company.Id))
                    return Forbid();
                //Usuwanie folderów i plików z FTP

                if (!fileService.DeleteFolder("/companies/" + department.Company.Id + (department.Company.Name != "" ? "-" + department.Company.Name : "") + "/" + department.FolderName))
                {
                    return StatusCode(500, "Problem with removing ftp folder");
                }
                await depStore.DeleteAsync(department);
                logger.LogInformation(new EventId(153, "RemoveDeparment"), "user: " + string.Join(",", user.UserName, user.Id) + "; department: " + string.Join(",", department.Name, department.Id));
                return Ok(new OperationSuccesfullViewModel<Department>(department));
            }
            else if (User.IsInRole("Administrator"))
            {
                var department = depStore.GetById(id);
                if (department == null)
                    return NotFound();
                //Usuwanie folderów i plików z FTP
                if (!fileService.DeleteFolder("/companies/" + department.Company.Id + (department.Company.Name != "" ? "-" + department.Company.Name : "") + "/" + department.FolderName))
                {
                    return StatusCode(500, "Problem with removing ftp folder");
                }
                await depStore.DeleteAsync(department);
                logger.LogInformation(new EventId(153, "RemoveDeparment"), "user: " + string.Join(",", user.UserName, user.Id) + "; department: " + string.Join(",", department.Name, department.Id));
                return Ok(new OperationSuccesfullViewModel<Department>(department));
            }
            return Forbid();
        }

        //Ustawia dostęp do danego departamentu dla danych użytkowników - przesłanych w data 
        [HttpPost("access")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> AddAccessToDepartment([FromBody]DepartmentAccessViewModel model)
        {
            var user = await ActUser();
            var department = depStore.GetById(model.DepartmentId);
            if (department == null)
                return NotFound();
            var employee = employeeStore.GetById(model.EmployeeId);
            if (employee == null)
                return NotFound();
            if (department.Employees.Any(x => x.Id == employee.Id))
                return BadRequest(new ErrorResponseViewModel("Employee has received access to this department"));
            department.Employees.Add(employee);
            await depStore.Update(department);
            logger.LogInformation(new EventId(154, "AddAccessToDepartment"), "user: " + string.Join(",", user.UserName, user.Id) + "; department: " + string.Join(",", department.Name, department.Id) + "; employee: " + string.Join(",", employee.User.UserName, employee.User.Id));
            return Ok(new OperationSuccesfullViewModel<DepartmentAccessViewModel>(model));
        }
        //Usuwa dostęp do danego departamentu dla danych użytkowników - przesłanych w data 
        [HttpDelete("access")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> RemoveAccessFromDepartment([FromHeader]DepartmentAccessViewModel model)
        {
            var user = await ActUser();
            var department = depStore.GetById(model.DepartmentId);
            if (department == null)
                return NotFound();
            var employee = employeeStore.GetById(model.EmployeeId);
            if (employee == null)
                return NotFound();
            var employeeFromDep = department.Employees.First(x => x.Id == model.EmployeeId);
            if (employeeFromDep == null)
                return NotFound();
            department.Employees.Remove(employeeFromDep);
            await depStore.Update(department);
            logger.LogInformation(new EventId(155, "RemoveAccessFromDepartment"), "user: " + string.Join(",", user.UserName, user.Id) + "; department: " + string.Join(",", department.Name, department.Id) + "; employee: " + string.Join(",", employee.User.UserName, employee.User.Id));
            return Ok(new OperationSuccesfullViewModel<DepartmentAccessViewModel>(model));
        }
    }
}

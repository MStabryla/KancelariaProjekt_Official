using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SWI2.Models.Users;
using SWI2.Models.Company;
using SWI2.Models.Response;
using SWI2.Persistence;
using SWI2.Services.Static;
using SWI2DB.Models.Authentication;
using SWI2DB.Models.Company;
using SWI2DB.Models.Department;
using SWI2DB.Models.Employee;
using Microsoft.Extensions.Logging;

namespace SWI2.Controllers
{
    [ApiController]
    [Authorize(Roles = "Employee,Administrator")]
    [Route("api/employeepanel")]
    public class EmployeeController : Controller
    {
        private readonly ILogger<EmployeeController> logger;
        private readonly IStore<Employee> employeeStore;
        private readonly IStore<Department> departmentStore;
        private readonly IStore<Company> companyStore;
        private readonly UserManager<User> userStore;
        public EmployeeController(
            ILogger<EmployeeController> _logger,
            UserManager<User> _userStore,
            IStore<Employee> _clientStore,
            IStore<Company> _companyStore,
            IStore<Department> _departmentStore)
        {
            logger = _logger;
            employeeStore = _clientStore;
            userStore = _userStore;
            companyStore = _companyStore;
            departmentStore = _departmentStore;
        }
        private async Task<User> ActUser()
        {
            var MainClaim = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);
            return await userStore.FindByNameAsync(MainClaim.Value);
        }
        private async Task<IQueryable<Department>> ActUserDepartments()
        {
            var user = await ActUser();
            var employeeDetails = await employeeStore.Table.Include(x => x.Departments).ThenInclude(x => x.Company).FirstOrDefaultAsync(x => x.Id == user.EmployeeId);
            /*var companies = companyStore.AsQueryable().Where(x => x.Departments.Any(y => employeeDetails.Departments.Any(z => z == y)));
            return companies;*/
            return employeeDetails.Departments.AsQueryable();
        }

        //Zwraca dostepne departamenty dla danego pracownika
        [HttpGet("access")]
        [Authorize(Roles = "Employee")]
        public async Task<IActionResult> GetAccessToDepartments()
        {
            var groupedDepartments = (await ActUserDepartments()).GroupBy(x => x.Company);
            var model = new List<GroupedDepartmentViewModel>();
            foreach(var companyDepartments in groupedDepartments)
            {
                if (companyDepartments.Key == null)
                    continue;
                var gdmodel = new GroupedDepartmentViewModel()
                {
                    Company = new CompanyViewModel(),
                    Deparments = new List<DepartmentViewModel>()
                };
                ModelOperations.CopyValues(gdmodel.Company, companyDepartments.Key, new string[] { });
                foreach (var dep in companyDepartments)
                {
                    if (dep == null)
                        continue;
                    var newDep = new DepartmentViewModel();
                    ModelOperations.CopyValues(newDep, dep, new string[] { });
                    gdmodel.Deparments.Add(newDep);
                }
                model.Add(gdmodel);
            }
            return Ok(model);
        }
        //Zwraca dostepne departamenty dla wybranego pracownika
        [HttpGet("access/{employeeId}")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> GetAccessToDepartment(string employeeId)
        {
            var user = await userStore.FindByIdAsync(employeeId);
            if (user == null)
                return NotFound();
            var employeeDetails = await employeeStore.Table.Include(x => x.Departments).ThenInclude(x => x.Company).FirstOrDefaultAsync(x => x.Id == user.EmployeeId);

            var groupedDepartments = employeeDetails.Departments.AsQueryable().GroupBy(x => x.Company);
            var model = new List<GroupedDepartmentViewModel>();
            foreach (var companyDepartments in groupedDepartments)
            {
                if (companyDepartments.Key == null)
                    continue;
                var gdmodel = new GroupedDepartmentViewModel()
                {
                    Company = new CompanyViewModel(),
                    Deparments = new List<DepartmentViewModel>()
                };
                ModelOperations.CopyValues(gdmodel.Company, companyDepartments.Key, new string[] { });
                foreach (var dep in companyDepartments)
                {
                    if (dep == null)
                        continue;
                    var newDep = new DepartmentViewModel();
                    ModelOperations.CopyValues(newDep, dep, new string[] { });
                    gdmodel.Deparments.Add(newDep);
                }
                model.Add(gdmodel);
            }
            return Ok(model);
        }
        

        //Sekcja Administratora

        [HttpPost("{id}")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> SetUserAsEmployee(string id)
        {
            var user = await userStore.FindByIdAsync(id);
            if (user == null)
                return NotFound();
            var employeeDetails = await employeeStore.Table.Include(x => x.User).FirstOrDefaultAsync(x => x.Id == user.EmployeeId);
            var roles = await userStore.GetRolesAsync(user);
            if (employeeDetails != default && roles.Contains("Employee"))
                return BadRequest(new ErrorResponseViewModel("User is already an employee"));
            if (roles.Contains("Client") || roles.Contains("Administrator"))
                return BadRequest(new ErrorResponseViewModel("User cannot be employee"));
            if (roles.Contains("ArchivedEmployee") && employeeDetails != default)
            {
                foreach (var role in roles)
                    await userStore.RemoveFromRoleAsync(user, role);
                await userStore.AddToRoleAsync(user, "Employee");
                var aeViewModel = new EmployeeViewModel()
                {
                    Id = employeeDetails.Id,
                    UserId = employeeDetails.User.Id,
                };
                logger.LogInformation(new EventId(171, "SetUserAsEmployee"), "user: " + string.Join(",", user.UserName, user.Id));
                return Ok(new OperationSuccesfullViewModel<EmployeeViewModel>(aeViewModel));
            }
            var employee = new Employee()
            {
                User = user,
            };
            await employeeStore.InsertAsync(employee);
            foreach (var role in roles)
                await userStore.RemoveFromRoleAsync(user, role);
            await userStore.AddToRoleAsync(user, "Employee");
            var eViewModel = new EmployeeViewModel()
            {
                Id = employee.Id,
                UserId = employee.User.Id,
            };
            logger.LogInformation(new EventId(171, "SetUserAsEmployee"), "user: " + string.Join(",", user.UserName, user.Id));
            return Ok(new OperationSuccesfullViewModel<EmployeeViewModel>(eViewModel));
        }

        //Ustawia dostęp do danego departamentu departamenty dla danego pracownika
        [HttpPost("access/{employeeId}/{departmentid}")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> SetAccessToDepartment(string employeeId,long? departmentid)
        {
            var user = await userStore.FindByIdAsync(employeeId);
            if (user == null)
                return NotFound();
            var departmentDetails = departmentStore.GetById(departmentid);
            if (departmentDetails == null)
                return NotFound();
            var employeeDetails = await employeeStore.Table.Include(x => x.Departments).FirstOrDefaultAsync(x => x.Id == user.EmployeeId);

            var departments = employeeDetails.Departments.AsQueryable();
            if (employeeDetails.Departments.Any(x => x.Id == departmentDetails.Id))
                return BadRequest(new ErrorResponseViewModel("User is already received access to this department"));
            employeeDetails.Departments.Add(departmentDetails);
            await employeeStore.Update(employeeDetails);

            var model = new EmployeeDepartmentViewModel() {
                DepartmentId = departmentDetails.Id,
                EmployeeId = user.Id
            };
            logger.LogInformation(new EventId(172, "SetAccessToDepartment"), "user: " + string.Join(",", user.UserName, user.Id) +"; department: " + string.Join(",", departmentDetails.Name, departmentDetails.Id));
            return Ok(new OperationSuccesfullViewModel<EmployeeDepartmentViewModel>(model));
        }
        //Usuwa dostęp do danego departamentu departamenty dla danego pracownika
        [HttpDelete("access/{employeeId}/{departmentid}")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> RemoveAccessFromDepartment(string employeeId, long? departmentid)
        {
            var user = await userStore.FindByIdAsync(employeeId);
            if (user == null)
                return NotFound();
            var departmentDetails = departmentStore.GetById(departmentid);
            if (departmentDetails == null)
                return NotFound();
            var employeeDetails = await employeeStore.Table.Include(x => x.Departments).FirstOrDefaultAsync(x => x.Id == user.EmployeeId);

            var departments = employeeDetails.Departments.AsQueryable();
            if (!employeeDetails.Departments.Any(x => x.Id == departmentDetails.Id))
                return BadRequest(new ErrorResponseViewModel("User has not get access to this department"));
            employeeDetails.Departments.Remove(departmentDetails);
            await employeeStore.Update(employeeDetails);

            var model = new EmployeeDepartmentViewModel()
            {
                DepartmentId = departmentDetails.Id,
                EmployeeId = user.Id
            };
            logger.LogInformation(new EventId(172, "RemoveAccessFromDepartment"), "user: " + string.Join(",", user.UserName, user.Id) + "; department: " + string.Join(",", departmentDetails.Name, departmentDetails.Id));
            return Ok(new OperationSuccesfullViewModel<EmployeeDepartmentViewModel>(model));
        }

    }
}

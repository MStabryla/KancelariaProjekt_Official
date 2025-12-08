using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SWI2.Models.Administrator;
using SWI2.Persistence;
using SWI2.Models;
using SWI2.Extensions;
using SWI2.Services.Static;
using SWI2DB.Models.Authentication;
using SWI2.Models.Response;
using SWI2.Models.Authentication;
using SWI2DB.Models.Account;
using SWI2DB.Models.Client;
using SWI2DB.Models.Employee;
using SWI2DB.Models.Company;
using Newtonsoft.Json;
using System.Linq.Dynamic.Core;
using Microsoft.Extensions.Logging;

namespace SWI2.Controllers
{
    //Czy takie role?
    [Route("/api/adminpanel")]
    [Authorize(Roles = "Administrator")]
    [ApiController]
    public class AdministratorController : Controller
    {
        private readonly ILogger<AdministratorController> logger;
        private readonly UserManager<User> userManager;
        private readonly IStore<UserDetails> userDetailsStore;
        private readonly IStore<Client> clientDetailsStore;
        private readonly IStore<Employee> employeeDetailsStore;
        private readonly IStore<ClientCompany> clientComStore;

        public AdministratorController(
            ILogger<AdministratorController> _logger,
            UserManager<User> _userManager,
            IStore<UserDetails> _userDetailsStore,
            IStore<Client> _clientDetailsStore,
            IStore<Employee> _employeeDetailsStore,
            IStore<ClientCompany> _clientComStore
            )
        {
            logger = _logger;
            userManager = _userManager;
            userDetailsStore = _userDetailsStore;
            clientDetailsStore = _clientDetailsStore;
            employeeDetailsStore = _employeeDetailsStore;
            clientComStore = _clientComStore;
        }

        private readonly Func<User, UserViewModel> _convertFunc = (x) =>
        {
            var model = new UserViewModel();
            ModelOperations.CopyValues(model, x, new string[] { });
            return model;
        };

        //Pobranie informacji o użytkownikach
        [HttpGet("users")]
        public async Task<IActionResult> GetUsers([FromQuery] string query)
        {
            TableParamsModel tableParams = query != null ? JsonConvert.DeserializeObject<TableParamsModel>(Encoding64.Base64Decode(query)) : new TableParamsModel() { Sort = "created", PageNumber = 0, PageSize = 25, Filters = new List<FilterModel>() };
            var users = await userManager.GetUsersAsync();
            IQueryable<UserViewModel> IQuery = users.ToList().Select(async (x) =>
            {
                var model = new UserViewModel();
                ModelOperations.CopyValues(model, x, new string[] { });
                model.UserRole = (await userManager.GetRolesAsync(x)).FirstOrDefault();
                return model;
            }).Select(x => x.Result).AsQueryable();
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
                    case "role":
                        IQuery = IQuery.Where($"" + fm.Name + "== @0", fm.Value);
                        break;
                    default:
                        IQuery = IQuery.Where(fm.Name + ".Contains(@0)", fm.Value);
                        break;
                }
            }
            var usersPageResult = IQuery.GetPaged(tableParams.PageNumber, tableParams.PageSize);
            return Ok(new TableViewModel<UserViewModel>() {elements = usersPageResult.Results,totalCount = usersPageResult.RowCount });
        }
        [HttpGet]
        [Route("user/{id}")]
        public async Task<IActionResult> GetUser(string id)
        {
            var user = (await userManager.GetUsersAsync()).FirstOrDefault(x => x.Id == id);
            if (user == default)
                return NotFound();
            var model = new UserViewModel();
            ModelOperations.CopyValues(model, user, new string[] { });
            model.UserRole = (await userManager.GetRolesAsync(user)).FirstOrDefault();

            return Ok(model);
        }
        //Należy przesłać dane (id usera) użytkownika do aktywacji
        [HttpPost]
        [Route("{id}/activate")]
        public async Task<IActionResult> ActivateUser(string id)
        {
            var user = (await userManager.GetUsersAsync()).FirstOrDefault(x => x.Id == id);
            if (user == default)
                return NotFound();
            if (user.IsActive)
                return BadRequest("User is already active");
            user.IsActive = true;
            var result = await userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MinValue);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description);
                return StatusCode(500, new { Errors = errors });
            }
            result = await userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description);
                return StatusCode(500, new { Errors = errors });
            }

            var model = new UserViewModel();
            ModelOperations.CopyValues(model, user, new string[] { });
            model.UserRole = (await userManager.GetRolesAsync(user)).FirstOrDefault();
            logger.LogInformation(new EventId(121, "AddUser"), "user: " + string.Join(",", user.UserName, user.Id));
            return Ok(new OperationSuccesfullViewModel<UserViewModel>(model));
        }
        //Należy zaimplementować ViewModel odpowiedzialny za dane logowania
        [HttpPost]
        [Route("")]
        public async Task<IActionResult> AddUser([FromBody] RegisterViewModel model)
        {
            if (model == null || !ModelState.IsValid)
                return BadRequest();

            UserDetails userDetails = new UserDetails { Name = model.Name, Surname = model.Surname, Registered = DateTime.Now, Language = model.Language };
            User user = new User { UserName = model.Login, Email = model.Email, UserDetails = userDetails, IsActive = false, LockoutEnabled = true};
            var result = await userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description);
                return StatusCode(500, new { Errors = errors });
            }
            try
            {
                result = await userManager.AddToRoleAsync(user, "Guest");
                if (!result.Succeeded)
                {
                    var errors = result.Errors.Select(e => e.Description);
                    return StatusCode(500, new { Errors = errors });
                }
                result = await userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
                if (!result.Succeeded)
                {
                    var errors = result.Errors.Select(e => e.Description);
                    return StatusCode(500, new { Errors = errors });
                }
            }
            //wycofanie zmian
            catch
            {

                await userManager.DeleteAsync(user);
                return StatusCode(500, "Error with setting role");
            }
            logger.LogInformation(new EventId(122, "AddUser"), "user: " + string.Join(",", user.UserName, user.Id));
            return Ok(new OperationSuccesfullViewModel<string>(user.Id));
        }
        //Należy zaimplementować ViewModel odpowiedzialny za zmiane danych usera
        //ZASTANOWIĆ SIĘ NAD ELEMENTAMI, KTÓRE MOŻE ZMIENIĆ ADMINISTRATOR
        [HttpPut]
        [Route("{id}")]
        public async Task<IActionResult> EditUser(string id, object data)
        {
            throw new NotImplementedException();
        }
        //Usuwanie usera o podanym id
        //TRZEBA POPRACOWAĆ NAD KASKADOWOŚCIĄ USUWANIA - Czy poza usuwaniem Client, Employee i UserDetails coś trzeba jeszcze zrobić?
        [HttpDelete]
        [Route("{id}")]
        public async Task<IActionResult> RemoveUser(string id)
        {
            var user = userManager.Users.Include(x => x.UserDetails).FirstOrDefault(x => x.Id == id);
            var userDetails = user.UserDetails;
            var clientDetails = clientDetailsStore.AsQueryable().Include(x => x.ClientCompany).FirstOrDefault( x => x.User == user);
            var employeeDetails = employeeDetailsStore.AsQueryable().FirstOrDefault(x => x.User == user);
            if (user == default)
                return NotFound();
            if (user.IsActive)
                return BadRequest("User should be locked first");
            var model = new UserViewModel();
            ModelOperations.CopyValues(model, user, new string[] { });
            model.UserRole = (await userManager.GetRolesAsync(user)).FirstOrDefault();
            var result = await userManager.DeleteAsync(user);
            await userDetailsStore.DeleteAsync(userDetails);
            if (clientDetails != default)
            {
                await clientComStore.DeleteAsync(clientDetails.ClientCompany);
                await clientDetailsStore.DeleteAsync(clientDetails);
            }
            if (employeeDetails != default) await employeeDetailsStore.DeleteAsync(employeeDetails);
            logger.LogInformation(new EventId(123, "RemoveUser"), "user: " + string.Join(",", user.UserName,user.Id));
            return Ok(new OperationSuccesfullViewModel<UserViewModel>(model));
        }
        //Zablokowanie usera o podanym id
        [HttpPost]
        [Route("{id}/lock")]
        public async Task<IActionResult> LockUser(string id)
        {
            var user = (await userManager.GetUsersAsync()).FirstOrDefault(x => x.Id == id);
            if (user == default)
                return NotFound();
            if (!user.IsActive)
                return BadRequest("User is already blocked");
            user.IsActive = false;
            var result = await userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description);
                return StatusCode(500, new { Errors = errors });
            }
            result = await userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description);
                return StatusCode(500, new { Errors = errors });
            }

            var model = new UserViewModel();
            ModelOperations.CopyValues(model, user, new string[] { });
            model.UserRole = (await userManager.GetRolesAsync(user)).FirstOrDefault();
            logger.LogInformation(new EventId(124, "LockUser"), "user: " + string.Join(",", user.UserName, user.Id));
            return Ok(new OperationSuccesfullViewModel<UserViewModel>(model));
        }

        #region OnlyToLocalTest

        public void AddAdministrator()
        {

        }

        #endregion
    }
}

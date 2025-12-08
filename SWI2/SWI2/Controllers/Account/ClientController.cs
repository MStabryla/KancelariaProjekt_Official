using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SWI2.Models.Response;
using SWI2.Models.Users;
using SWI2.Persistence;
using SWI2.Services.Static;
using SWI2DB.Models.Authentication;
using SWI2DB.Models.Client;
using SWI2DB.Models.Company;
using Microsoft.Extensions.Logging;

namespace SWI2.Controllers
{
    [ApiController]
    [Authorize(Roles = "Client,Administrator")]
    [Route("api/clientpanel")]
    public class ClientController : Controller
    {
        private readonly ILogger<ClientController> logger;
        private readonly IStore<Client> clientStore;
        private readonly IStore<Company> companyStore;
        private readonly IStore<ClientCompany> clientComStore;
        private readonly UserManager<User> userStore;
        public ClientController(
            ILogger<ClientController> _logger,
            UserManager<User> _userStore,
            IStore<Client> _clientStore,
            IStore<Company> _companyStore,
            IStore<ClientCompany> _clientComStore)
        {
            logger = _logger;
            clientStore = _clientStore;
            userStore = _userStore;
            companyStore = _companyStore;
            clientComStore = _clientComStore;
        }
        private async Task<User> ActUser()
        {
            var MainClaim = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);
            return await userStore.FindByNameAsync(MainClaim.Value);
        }
        private async Task<IQueryable<Company>> ActUserCompanies()
        {
            var user = await ActUser();
            var clientDetails = await clientStore.Table.Include(x => x.ClientCompany).FirstOrDefaultAsync(x => x.Id == user.ClientId);
            var companies = companyStore.AsQueryable().Where(x => x.ClientCompany.Any(y => y.Client == clientDetails));
            return companies;
        }

        //Ustawienie jako członka zarządu - companyid można pobrać przy pomocy użytkownika
        [HttpPut("{clientId}/{companyId}")]
        public async Task<IActionResult> SetAsBoardMember(string clientId, long companyId)
        {
            var actUser = await ActUser();
            var user = await userStore.FindByIdAsync(clientId);
            if (user == null)
                return NotFound();
            var clientDetails = await clientStore.AsQueryable().Include(x => x.ClientCompany).Include(x => x.User).FirstOrDefaultAsync(x => x.Id == user.ClientId);
            if (clientDetails == default)
                return StatusCode(500, new ErrorResponseViewModel("Inconsistient database - client doesn't have Client entity"));
            var companyDetails = companyStore.GetById(companyId);
            if (companyDetails == null)
                return NotFound();
            if (User.IsInRole("Client"))
            {
                var actUserClientDetails = clientStore.GetById(actUser.ClientId);
                var clientCompanies = await ActUserCompanies();
                if (!clientCompanies.Any(x => x.Id == companyId))
                    return Forbid();
                //sprawdzanie, czy aktualny użytkownik jest w zarządzie firmy
                if (!(await actUserClientDetails.ClientCompany.AsQueryable().FirstOrDefaultAsync(x => x.Company.Id == companyId)).IsInBoard)
                    return Forbid();
                //sprawdzanie, czy docelowy użytkownik jest już przypisany do firmy
                var clientCompanyEntity = await clientComStore.AsQueryable().FirstOrDefaultAsync(x => x.Company.Id == companyId && x.Client.Id == clientDetails.Id);
                if (clientCompanyEntity == default)
                    return BadRequest(new ErrorResponseViewModel("User is not associated with this company"));

                clientCompanyEntity.IsInBoard = true;
                await clientComStore.Update(clientCompanyEntity);

                var cViewModel = new ClientCompanyViewModel()
                {
                    ClientId = clientDetails.Id,
                    CompanyId = companyDetails.Id
                };
                logger.LogInformation(new EventId(131, "SetBoardMember"), "user: " + string.Join(",", user.UserName, user.Id) + "; company: " + string.Join(",", companyDetails.Name, companyDetails.Id) + "; board member: " + string.Join(",", companyDetails.Name, companyDetails.Id));
                return Ok(new OperationSuccesfullViewModel<ClientCompanyViewModel>(cViewModel));

            }
            else if(User.IsInRole("Administrator"))
            {
                //sprawdzanie, czy docelowy użytkownik jest już przypisany do firmy
                var clientCompanyEntity = await clientComStore.AsQueryable().FirstOrDefaultAsync(x => x.Company.Id == companyId && x.Client.Id == clientDetails.Id);
                if (clientCompanyEntity == default)
                    return BadRequest(new ErrorResponseViewModel("User is not associated with this company"));

                clientCompanyEntity.IsInBoard = true;
                await clientComStore.Update(clientCompanyEntity);

                var cViewModel = new ClientCompanyViewModel()
                {
                    ClientId = clientDetails.Id,
                    CompanyId = companyDetails.Id
                };
                logger.LogInformation(new EventId(131, "SetBoardMember"), "user: " + string.Join(",", user.UserName, user.Id) + "; company: " + string.Join(",", companyDetails.Name, companyDetails.Id) + "; board member: " + string.Join(",", companyDetails.Name, companyDetails.Id));
                return Ok(new OperationSuccesfullViewModel<ClientCompanyViewModel>(cViewModel));
            }
            return Forbid();
        }

        //Sekcja Administratora


        //Ustawienie usera jako klienta
        [HttpPost("{id}")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> SetAsClient(string id)
        {
            var user = await userStore.FindByIdAsync(id);
            if (user == null)
                return NotFound();
            var clientDetails = await clientStore.Table.Include(x => x.User).FirstOrDefaultAsync(x => x.Id == user.ClientId);
            var roles = await userStore.GetRolesAsync(user);
            if (clientDetails != default && roles.Contains("Client"))
                return BadRequest(new ErrorResponseViewModel("User is already a client"));
            if (roles.Contains("Employee") || roles.Contains("Administrator"))
                return BadRequest(new ErrorResponseViewModel("User cannot be client"));
            if(roles.Contains("ArchivedClient") && clientDetails != default)
            {
                foreach(var role in roles)
                    await userStore.RemoveFromRoleAsync(user, role);
                await userStore.AddToRoleAsync(user, "Client");
                var acViewModel = new ClientViewModel()
                {
                    Id = clientDetails.Id,
                    UserId = clientDetails.User.Id,
                };
                logger.LogInformation(new EventId(132, "SetAsClient"), "user: " + string.Join(",", user.UserName, user.Id));
                return Ok(new OperationSuccesfullViewModel<ClientViewModel>(acViewModel));
            }
            var client = new Client()
            {
                User = user
            };
            await clientStore.InsertAsync(client);
            foreach (var role in roles)
                await userStore.RemoveFromRoleAsync(user, role);
            await userStore.AddToRoleAsync(user, "Client");
            var cViewModel = new ClientViewModel()
            {
                Id = client.Id,
                UserId = client.User.Id,
            };
            logger.LogInformation(new EventId(132, "SetAsClient"), "user: " + string.Join(",", user.UserName, user.Id));
            return Ok(new OperationSuccesfullViewModel<ClientViewModel>(cViewModel));
        }
        [HttpPost("{clientId}/{companyId}")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> AddClientToCompany(string clientId, long companyId)
        {
            var user = await userStore.FindByIdAsync(clientId);
            if (user == null)
                return NotFound();
            if (user.ClientId == null)
                return NotFound();
            var clientDetails = await clientStore.AsQueryable().Include(x => x.ClientCompany).FirstOrDefaultAsync(x => x.Id == user.ClientId);
            if (clientDetails == default)
                return NotFound();
            var companyDetails = companyStore.GetById(companyId);
            if(companyDetails == null)
                return NotFound();

            var clientCompany = new ClientCompany() { Client = clientDetails, Company = companyDetails };
            clientDetails.ClientCompany.Add(clientCompany);
            await clientStore.Update(clientDetails);
            var model = new ClientCompanyViewModel() { ClientId = clientDetails.Id, CompanyId = companyDetails.Id,IsInBoard = false };
            logger.LogInformation(new EventId(133, "AddClientToCompany"), "user: " + string.Join(",", user.UserName, user.Id) + "company: " + string.Join(",", companyDetails.Name, companyDetails.Id));
            return Ok(new OperationSuccesfullViewModel<ClientCompanyViewModel>(model));
        }
        [HttpDelete("{clientId}/{companyId}")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> RemoveClientFromCompany(string clientId, long companyId)
        {
            var user = await userStore.FindByIdAsync(clientId);
            if (user == null)
                return NotFound();
            if (user.ClientId == null)
                return NotFound();
            var clientDetails = await clientStore.AsQueryable().Include(x => x.ClientCompany).FirstOrDefaultAsync(x => x.Id == user.ClientId);
            if (clientDetails == default)
                return NotFound();
            var companyDetails = companyStore.GetById(companyId);
            if (companyDetails == null)
                return NotFound();

            var clientCompany = clientDetails.ClientCompany.FirstOrDefault(x => x.Company == companyDetails);
            if (clientCompany == default)
                return BadRequest("Client is not associated with this company");
            clientDetails.ClientCompany.Remove(clientCompany);
            await clientStore.Update(clientDetails);
            var model = new ClientCompanyViewModel() { ClientId = clientDetails.Id, CompanyId = companyDetails.Id, IsInBoard = false };
            logger.LogInformation(new EventId(134, "RemoveClientFromCompany"), "user: " + string.Join(",", user.UserName, user.Id) + "company: " + string.Join(",", companyDetails.Name, companyDetails.Id));
            return Ok(new OperationSuccesfullViewModel<ClientCompanyViewModel>(model));
        }
    }
}

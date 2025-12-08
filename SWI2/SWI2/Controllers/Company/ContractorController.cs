using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SWI2DB.Models.Contractor;
using Microsoft.Extensions.Logging;
using SWI2.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using SWI2.Services.AuthorityHelper;
using Newtonsoft.Json.Linq;
using SWI2DB.Models.Company;
using SWI2.Models;
using System.Linq.Dynamic.Core;
using SWI2.Extensions;
using Newtonsoft.Json;
using SWI2.Services.Static;
using System.Collections.Generic;

namespace SWI2.Controllers
{
    [ApiController]
    [Route("api/contractor")]
    [Authorize]
    public class ContractorController : Controller
    {
        private readonly ILogger<ContractorController> _logger;
        private readonly IStore<Contractor> _contractor;
        private readonly IStore<Company> _company;
        private readonly IStore<ContractorBankAccount> _contractorBankAccount;


        public ContractorController(ILogger<ContractorController> logger,
            IStore<Company> company,
            IStore<Contractor> contractor,
            IStore<ContractorBankAccount> contractorBankAccount)
        {
            _logger = logger;
            _contractor = contractor;
            _company = company;
            _contractorBankAccount = contractorBankAccount;
        }
        [HttpGet]
        [Route("{id}")]
        public IActionResult GetContractors(long id, [FromQuery] string query)
        {
            if (AuthorityHelper.CheckIfHasPermitionForCompany(User.Claims, id))
            {
                TableParamsModel tableParams = query != null ? JsonConvert.DeserializeObject<TableParamsModel>(Encoding64.Base64Decode(query)) : new TableParamsModel() { Sort = "created", PageNumber = 0, PageSize = 25, Filters = new List<FilterModel>() };
                IQueryable<Contractor> contractors = _contractor.AsQueryable().Where(ih => ih.Company.Id == id).OrderBy(tableParams.Sort);
                if (tableParams.PageSize != -1)
                {
                    foreach (FilterModel fm in tableParams.Filters)
                    {
                        switch (fm.Type)
                        {
                            case "string":
                                contractors = contractors.Where(fm.Name + ".Contains(@0)", fm.Value);
                                break;
                            case "date":
                                contractors = contractors.Where($"" + fm.Name + "@0", Convert.ToDateTime(fm.Value));
                                break;
                            case "enum":
                                contractors = contractors.Where(fm.Name + "==" + fm.Value);
                                break;
                            default:
                                contractors = contractors.Where(fm.Name + ".Contains(@0)", fm.Value);
                                break;
                        }
                    }

                    var pagedResult = contractors.Include(c => c.ContractorBankAccounts).GetPaged(tableParams.PageNumber, tableParams.PageSize);
                    _logger.LogInformation(new EventId(40, "GetContractors"), "contractors: " + string.Join(",", pagedResult.Results.Select(i => i.Id).ToArray()) + " with companyId: " + id);
                    return Ok(new TableViewModel<Contractor>
                    {
                        totalCount = pagedResult.RowCount,
                        elements = pagedResult.Results
                    });
                }

                _logger.LogInformation(new EventId(40, "GetContractors"), "all contractors companyId: " + id);
                return Ok(contractors.Include(c => c.ContractorBankAccounts));

            }
            _logger.LogWarning(new EventId(40, "GetContractors"), "acces denaied with companyId: " + id);
            return Forbid("Brak autoryzacji do zasobów firmy");
        }
        [HttpPost]
        [Route("{companyid}")]
        public async Task<IActionResult> InsertContractor(long companyid, Contractor contractor)
        {
            if (AuthorityHelper.CheckIfHasPermitionForCompany(User.Claims, companyid))
            {
                if (_contractor.Table.FirstOrDefault(c => c.Name == contractor.Name) == null)
                {
                    contractor.ContractorBankAccounts = null;
                    contractor.Company = _company.Table.FirstOrDefault(c => c.Id == companyid);
                    contractor.ContractorBankAccounts = new List<ContractorBankAccount>() { new ContractorBankAccount() { Id = 0, AccountNumber = "unknown", BankName = "unknown", Created = DateTime.Now, } };
                    contractor.Created = DateTime.Now;
                    if (await _contractor.InsertAsync(contractor))
                    {
                        _logger.LogInformation(new EventId(41, "InsertContractor"), "added " + contractor.Id + " companyId: " + companyid);
                        return Ok(contractor);

                    }
                    _logger.LogError(new EventId(41, "InsertContractor"), "error db companyId: " + companyid);

                    return BadRequest("Błąd Podczas dodawania kontrachenta");
                }
                _logger.LogError(new EventId(41, "InsertContractor"), "error contarctor with this name exist companyId: " + companyid);
                return BadRequest("Kontrachent o takie nazwie już istnieje");

            }
            _logger.LogWarning(new EventId(41, "InsertContractor"), "acces denaied with companyId: " + companyid);
            return Forbid("Brak autoryzacji do zasobów firmy");
        }
        [HttpPut]
        [Route("{companyid}")]
        public async Task<IActionResult> EditContractor(long companyid, JObject changedData)
        {
            if (AuthorityHelper.CheckIfHasPermitionForCompany(User.Claims, companyid))
            {
                var contractor = _contractor.Table.FirstOrDefault(c => c.Id == (long)changedData.Property("id").Value && c.Company.Id == companyid);
                if (contractor != null)
                {
                    if (changedData.Property("name") != null)
                    {
                        if (_contractor.Table.FirstOrDefault(c => c.Name == (string)changedData.Property("name").Value && c.Company.Id == companyid) == null)
                        {

                            contractor = await _contractor.Update(contractor, changedData);
                            _logger.LogInformation(new EventId(42, "EditContractor"), "updated " + contractor.Id + " changedData:" + changedData.ToString() + "  companyId: " + companyid);
                            return Ok(contractor);
                        }
                        _logger.LogError(new EventId(42, "EditContractor"), "error contarctor with tis name exist companyId: " + companyid);
                        return BadRequest("Kontrachent o takie nazwie już istnieje");

                    }
                    else
                    {
                        contractor = await _contractor.Update(contractor, changedData);
                        _logger.LogInformation(new EventId(42, "EditContractor"), "updated " + contractor.Id + " changedData:" + changedData.ToString() + "  companyId: " + companyid);
                        return Ok(contractor);

                    }
                }
                _logger.LogWarning(new EventId(42, "EditContractor"), "no contractors with changedData:" + changedData.ToString() + "  companyId: " + companyid);
                return BadRequest("Próba wyciagniecia danych kontachemntów innych firm");

            }
            _logger.LogWarning(new EventId(42, "EditContractor"), "acces denaied with companyId: " + companyid);
            return Forbid("Brak autoryzacji do zasobów firmy");
        }
        [HttpDelete]
        [Route("{id}")]
        public async Task<IActionResult> RemoveContractor(long id)
        {
            var contractor = await _contractor.Table.Include(con => con.Company).FirstOrDefaultAsync(i => i.Id == id);
            if (contractor != null && AuthorityHelper.CheckIfHasPermitionForCompany(User.Claims, contractor.Company.Id))
            {
                bool isInserted = _contractor.Delete(contractor);
                if (isInserted)
                {
                    _logger.LogInformation(new EventId(43, "RemoveContractor"), "removed " + contractor.Id + " companyId: " + contractor.Company.Id);
                    return Ok(contractor);
                }
                _logger.LogError(new EventId(43, "RemoveContractor"), "error db " + contractor.Company.Id);
                return BadRequest("błąd przy usuwaniu faktury z bazy danych");
            }
            _logger.LogWarning(new EventId(43, "RemoveContractor"), "acces denaied with contractorId: " + id);
            return Forbid("Brak autoryzacji do danej faktury");
        }
        [HttpGet]
        [Route("contractorbankaccounts/{id}")]
        public async Task<IActionResult> GetContractorBankAccount(long id, long contractorId)
        {
            if (AuthorityHelper.CheckIfHasPermitionForCompany(User.Claims, id))
            {
                _logger.LogInformation(new EventId(44, "GetContractorBankAccount"), "contractorId: " + id + " companyId: " + id);
                return Ok(_contractorBankAccount.Table.Where(cba => cba.Contractor.Company.Id == id && cba.Contractor.Id == contractorId).ToList());
            }
            _logger.LogWarning(new EventId(44, "GetContractorBankAccount"), "acces denaied with companyId: " + contractorId);
            return Forbid("Brak autoryzacji do zasobów firmy");
        }
        [HttpPost]
        [Route("bankaccount/{contractorid}")]
        public async Task<IActionResult> InsertContractorBankAccount(long contractorId, ContractorBankAccount contractorBankAccount)
        {
            var contractor = await _contractor.Table.Include(con => con.Company).FirstOrDefaultAsync(i => i.Id == contractorId);
            if (AuthorityHelper.CheckIfHasPermitionForCompany(User.Claims, contractor.Company.Id))
            {
                if (contractorBankAccount.AccountNumber != "unknown")
                {
                    contractorBankAccount.Created = DateTime.Now;
                    contractorBankAccount.Contractor = contractor;
                    if (await _contractorBankAccount.InsertAsync(contractorBankAccount))
                    {
                        _logger.LogInformation(new EventId(45, "InsertContractorBankAccount"), "added " + contractorBankAccount.Id + " companyId: " + contractorId);
                        return Ok(contractorBankAccount);
                    }
                    _logger.LogError(new EventId(45, "InsertContractorBankAccount"), "error db companyId: " + contractorId);

                    return BadRequest("Błąd Podczas dodawania konta bankowego");
                }
                _logger.LogError(new EventId(45, "InsertContractorBankAccount"), "error unknown account cannot be doubled companyId: " + contractorId);
                return BadRequest("Nie możesz dodac konta o takim numerze");

            }
            _logger.LogWarning(new EventId(45, "InsertContractorBankAccount"), "acces denaied with companyId: " + contractorId);
            return Forbid("Brak autoryzacji do zasobów firmy");
        }
        [HttpPut]
        [Route("bankaccount/{companyid}")]
        public async Task<IActionResult> EditContractorBankAccount(long contractorId, JObject changedData)
        {
            if (AuthorityHelper.CheckIfHasPermitionForCompany(User.Claims, contractorId))
            {
                var contractorBankAccount = await _contractorBankAccount.Table.FirstOrDefaultAsync(cba => cba.Id == (long)changedData.Property("id").Value && cba.Contractor.Company.Id == contractorId);

                if (contractorBankAccount != null)
                {

                    if (contractorBankAccount.AccountNumber != "unknown")
                    {
                        contractorBankAccount.Updated = DateTime.Now;
                        contractorBankAccount = await _contractorBankAccount.Update(contractorBankAccount, changedData);
                        _logger.LogInformation(new EventId(46, "EditContractorBankAccount"), "updated: " + contractorBankAccount.Id + " with changedData: " + changedData.ToString() + " companyId: " + contractorId);
                        return Ok(contractorBankAccount);
                    }
                    _logger.LogError(new EventId(46, "EditContractorBankAccount"), "error unknown account cannot be edited companyId: " + contractorId);
                    return BadRequest("nie możesz edytować tego konta bankowego");
                }
                _logger.LogWarning(new EventId(46, "EditContractorBankAccount"), "no contractorsBankAccounts with changedData:" + changedData.ToString() + "  companyId: " + contractorId);
                return BadRequest("Próba wyciagniecia danych kontachemntów innych firm");

            }
            _logger.LogWarning(new EventId(46, "EditContractorBankAccount"), "acces denaied with companyId: " + contractorId);
            return Forbid("Brak autoryzacji do zasobów firmy");
        }
        [HttpDelete]
        [Route("bankaccount/{id}")]
        public async Task<IActionResult> RemoveContractorBankAccount(long id)
        {
            var contractorBankAccount = await _contractorBankAccount.Table.Include(cba => cba.Contractor).ThenInclude(con => con.Company).FirstOrDefaultAsync(cba => cba.Id == id);
            if (contractorBankAccount != null && AuthorityHelper.CheckIfHasPermitionForCompany(User.Claims, contractorBankAccount.Contractor.Company.Id))
            {
                if (contractorBankAccount.AccountNumber != "unknown")
                {
                    bool isDelated = _contractorBankAccount.Delete(contractorBankAccount);
                    if (isDelated)
                    {
                        _logger.LogInformation(new EventId(47, "RemoveContractorBankAccount"), "delated: " + contractorBankAccount.Id + " with companyId: " + contractorBankAccount.Contractor.Company.Id);
                        return Ok(contractorBankAccount);
                    }
                    _logger.LogError(new EventId(47, "RemoveContractorBankAccount"), "error db with companyId: " + contractorBankAccount.Contractor.Company.Id);
                    return BadRequest("błąd przy usuwaniu faktury z bazy danych");

                }
                _logger.LogError(new EventId(47, "RemoveContractorBankAccount"), "error unknown bankaccount cannot be delate companyId: " + contractorBankAccount.Contractor.Company.Id);
                return BadRequest("nie możesz usunąc tego tego konta bankowego");

            }
            _logger.LogWarning(new EventId(47, "RemoveContractorBankAccount"), "acces denaied with contractorBankAccountId: " + id);
            return Forbid("Brak autoryzacji do danej faktury");
        }
    }
}

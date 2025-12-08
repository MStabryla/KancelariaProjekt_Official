using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SWI2.Persistence;
using SWI2DB.Models.Payment;
using SWI2.Models;
using System.Security.Claims;
using SWI2.Services.AuthorityHelper;
using SWI2.Models.Invoice;
using System.Linq.Dynamic.Core;
using SWI2.Extensions;
using Newtonsoft.Json;
using SWI2.Services.Static;
using SWI2DB.Models.Contractor;
using SWI2DB.Models.Invoice;
using Newtonsoft.Json.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace SWI2.Controllers
{
    [ApiController]
    [Route("api/payment")]
    [Authorize]
    public class PaymentController : Controller
    {
        private readonly ILogger<ContractorController> _logger;
        private readonly IStore<Payment> _payment;
        private readonly IStore<ContractorBankAccount> _contractorBankAccount;
        private readonly IStore<Invoice> _invoice;

        public PaymentController(ILogger<ContractorController> logger,
            IStore<Payment> payment,
            IStore<Invoice> invoice,
            IStore<ContractorBankAccount> contractorBankAccount)
        {
            _logger = logger;
            _payment = payment;
            _invoice = invoice;
            _contractorBankAccount = contractorBankAccount;
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> GetPeyments(long id, [FromQuery] string query)
        {
            if (AuthorityHelper.CheckIfHasPermitionForCompany(User.Claims, id))
            {
                TableParamsModel tableParams = query != null ? JsonConvert.DeserializeObject<TableParamsModel>(Encoding64.Base64Decode(query)) : new TableParamsModel() { Sort = "created", PageNumber = 0, PageSize = 25, Filters = new List<FilterModel>() };
                IQueryable<PaymentViewModel> payments = _payment.AsQueryable().Where(p => p.ContractorBankAccount.Contractor.Company.Id == id).Select(p =>
                 new PaymentViewModel
                 {
                     Id = p.Id,
                     PaymentsForInvoices = p.PaymentsForInvoices.Select(i => new PaymentsForInvoicesViewModel() { Id = i.Id, Invoice = new PaymentInvoiceViewModel() { Id = i.Invoice.Id, Number = i.Invoice.Number, BruttoWorth = i.Invoice.BruttoWorth }, PaymentValueForInvoice = i.PaymentValueForInvoice }).ToList(),
                     ContractorName = p.ContractorBankAccount.Contractor.Name,
                     ContractorNip = p.ContractorBankAccount.Contractor.Nip,
                     ContractorId = p.ContractorBankAccount.Contractor.Id,
                     contractorBankAccountName = p.ContractorBankAccount.BankName,
                     ContractorBankAccountId = p.ContractorBankAccount.Id,
                     ContractorBankAccountNumber = p.ContractorBankAccount.AccountNumber,
                     Topic = p.Topic,
                     PaymentValue = p.PaymentValue,
                     Currency = p.Currency,
                     PaymentDate = p.PaymentDate,
                     Created = p.Created,
                 }).OrderBy(tableParams.Sort);

                foreach (FilterModel fm in tableParams.Filters)
                {
                    switch (fm.Type)
                    {
                        //dodać wyszukiwaniepo stringu dla wielu

                        case "string":
                            payments = payments.Where(fm.Name + ".Contains(@0)", fm.Value);
                            break;
                        case "date":
                            payments = payments.Where($"" + fm.Name + "@0", Convert.ToDateTime(fm.Value));
                            break;
                        case "int":
                            payments = payments.Where(fm.Name + fm.Value);
                            break;
                        case "enum":
                            string[] nestesProperties = fm.Name.Split('.');
                            payments = payments.Where(nestesProperties[0] + ".Any(" + nestesProperties[1] + '.' + nestesProperties[2] + ".Contains(@0))", fm.Value);
                            break;
                        default:
                            payments = payments.Where(fm.Name + ".Contains(@0)", fm.Value);
                            break;
                    }
                }


                var pagedResult = payments.GetPaged(tableParams.PageNumber, tableParams.PageSize);

                _logger.LogWarning(new EventId(30, "GetPeyments"), "payments: " + string.Join(",", pagedResult.Results.Select(i => i.Id).ToArray()) + " with companyId: " + id);
                return Ok(new TableViewModel<PaymentViewModel>
                {
                    totalCount = pagedResult.RowCount,
                    elements = pagedResult.Results
                });

            }
            _logger.LogWarning(new EventId(30, "GetPeyments"), "acces denaied with companyId: " + id);
            return Forbid("Brak autoryzacji do zasobów firmy");
        }

        [HttpPost]
        [Route("{id}")]
        public async Task<IActionResult> InsertPayment(long id, Payment payment)
        {
            if (AuthorityHelper.CheckIfHasPermitionForCompany(User.Claims, id))
            {
                payment.Created = DateTime.Now;
                payment.ContractorBankAccount = _contractorBankAccount.Table.Where(cba => cba.Id == payment.ContractorBankAccount.Id && cba.Contractor.Company.Id == id).FirstOrDefault();
                var paymentsForInvoices = new List<PaymentForInvoice>();
                if (payment.PaymentsForInvoices != null)
                {
                    foreach (var pfi in payment.PaymentsForInvoices)
                    {
                        if (pfi != null)
                        {
                            paymentsForInvoices.Add(new PaymentForInvoice() { PaymentValueForInvoice = pfi.PaymentValueForInvoice, Invoice = _invoice.Table.FirstOrDefault(i => i.Id == pfi.Invoice.Id) });
                        }
                    }
                    payment.PaymentsForInvoices = paymentsForInvoices;
                }

                if (await _payment.InsertAsync(payment))
                {
                    if (payment.PaymentsForInvoices != null)
                    {
                        foreach (var pfi in payment.PaymentsForInvoices)
                        {
                            await UpdateInvoiceStatus(pfi.Invoice.Id);
                        }
                    }
                    _logger.LogInformation(new EventId(31, "InsertPayment"), "added " + payment.Id + " companyId" + id);
                    return Ok(payment);
                }
                _logger.LogError(new EventId(31, "InsertPayment"), "error db companyId:" + id);
                return BadRequest("Błąd w trakcie dodawania płatności");

            }
            _logger.LogWarning(new EventId(31, "InsertPayment"), "acces denaied with companyId: " + id);
            return Forbid("Brak autoryzacji do zasobów firmy");
        }
        [HttpPut]
        [Route("{id}")]
        public async Task<IActionResult> EditPayment(long id, JObject changedData)
        {
            if (AuthorityHelper.CheckIfHasPermitionForCompany(User.Claims, id))
            {
                var payment = await _payment.Table.Include(p => p.PaymentsForInvoices).ThenInclude(pfi => pfi.Invoice).FirstOrDefaultAsync(cba => cba.Id == (long)changedData.Property("id").Value && cba.ContractorBankAccount.Contractor.Company.Id == id);

                if (payment != null)
                {
                    payment.Updated = DateTime.Now;
                    payment = await _payment.Update(payment, changedData);
                    foreach (var pfi in payment.PaymentsForInvoices)
                    {
                        await UpdateInvoiceStatus(pfi.Invoice.Id);
                    }
                    _logger.LogWarning(new EventId(32, "EditPayment"), "updated " + id + " with changedData:" + changedData.ToString() + " companyId: " + id);
                    return Ok(payment);
                }
                _logger.LogWarning(new EventId(32, "EditPayment"), "no payments with changedData:" + changedData.ToString() + "  companyId: " + id);
                return BadRequest("Próba wyciagniecia danych kontachemntów innych firm");
            }
            _logger.LogWarning(new EventId(32, "EditPayment"), "acces denaied with companyId: " + id);
            return Forbid("Brak autoryzacji do zasobów firmy");
        }
        [HttpDelete]
        [Route("{id}")]
        public async Task<IActionResult> RemovePayment(long id)
        {
            var payment = await _payment.Table.Include(cba => cba.ContractorBankAccount).ThenInclude(con => con.Contractor).ThenInclude(con => con.Company).Include(p => p.PaymentsForInvoices).ThenInclude(pfi => pfi.Invoice).FirstOrDefaultAsync(cba => cba.Id == id);
            if (payment != null && AuthorityHelper.CheckIfHasPermitionForCompany(User.Claims, payment.ContractorBankAccount.Contractor.Company.Id))
            {
                bool isDelated = _payment.Delete(payment);
                if (isDelated)
                {
                    foreach (var pfi in payment.PaymentsForInvoices)
                    {
                        await UpdateInvoiceStatus(pfi.Invoice.Id);
                    }
                    _logger.LogWarning(new EventId(33, "RemovePayment"), "removed " + id);
                    return Ok(payment);
                }
                _logger.LogError(new EventId(33, "RemovePayment"), "error db companyId: " + id);
                return BadRequest("błąd przy usuwaniu wpłaty z bazy danych");
            }
            _logger.LogWarning(new EventId(33, "RemovePayment"), "acces denaied with companyId: " + id);
            return Forbid("Brak autoryzacji do danej wpłaty");
        }
        private async Task<bool> UpdateInvoiceStatus(long id)
        {
            var invoice = await _invoice.AsQueryable().Include(i => i.PaymentsForInvoices).FirstOrDefaultAsync(i => i.Id == id);

            if (invoice != null)
            {
                var sum = invoice.PaymentsForInvoices.Select(pfi => pfi.PaymentValueForInvoice).Sum();
                if (sum == invoice.BruttoWorth)
                {
                    invoice.PaymentStatus = PaymentStatus.Paid;
                }
                else if (sum < invoice.BruttoWorth)
                {
                    invoice.PaymentStatus = PaymentStatus.Notpaid;
                }
                else
                {
                    invoice.PaymentStatus = PaymentStatus.Overpaid;
                }

                if (await _invoice.Update(invoice) != null)
                {
                    return true;
                }
                return false;

            }
            return false;
        }
    }
}

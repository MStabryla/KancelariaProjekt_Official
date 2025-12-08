using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SWI2DB.Models.Invoice;
using SWI2.Persistence;
using Microsoft.Extensions.Logging;
using SWI2.Extensions;
using System.Linq.Dynamic.Core;
using SWI2DB.Models.Contractor;
using Microsoft.EntityFrameworkCore;
using SWI2DB.Models.Company;
using System.Security.Claims;
using SWI2.Services.AuthorityHelper;
using Newtonsoft.Json.Linq;
using SWI2DB.Models.Client;
using SWI2.Services.Email;
using SWI2.Models.Email;
using Microsoft.AspNetCore.Identity;
using SWI2DB.Models.Authentication;
using System.IO;
using Microsoft.AspNetCore.Http;
using SWI2.Models;
using SWI2.Models.Invoice;
using Newtonsoft.Json;
using SWI2.Services.Static;
using System.Collections.Generic;
using SWI2DB.Models.Payment;

namespace SWI2.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/invoice")]
    public class InvoiceController : Controller
    {
        private readonly ILogger<InvoiceController> _logger;
        private readonly IEmailService _emailService;
        private readonly UserManager<User> _userManager;
        private readonly IStore<Invoice> _invoice;
        private readonly IStore<InvoiceIssuer> _invoiceIssuer;
        private readonly IStore<InvoiceContractor> _invoiceContractor;
        private readonly IStore<EntryDictionary> _entryDictionary;
        private readonly IStore<Company> _company;
        private readonly IStore<Client> _client;
        private readonly IStore<Contractor> _contractor;
        private readonly IStore<SellDateName> _sellDateName;
        private readonly IStore<InvoiceSended> _invoiceSended;


        public InvoiceController(ILogger<InvoiceController> logger,
            IStore<Invoice> invoice,
            IStore<InvoiceContractor> invoiceContractor,
            IStore<InvoiceIssuer> invoiceIssuer,
            IStore<SellDateName> sellDateName,
            IStore<Company> company,
            IEmailService emailService,
            IStore<Client> client,
            IStore<Contractor> contractor,
            IStore<InvoiceSended> invoiceSended,
            UserManager<User> userManager,
            IStore<EntryDictionary> entryDictionary)
        {
            _logger = logger;
            _invoice = invoice;
            _invoiceIssuer = invoiceIssuer;
            _invoiceContractor = invoiceContractor;
            _company = company;
            _client = client;
            _contractor = contractor;
            _entryDictionary = entryDictionary;
            _sellDateName = sellDateName;
            _emailService = emailService;
            _invoiceSended = invoiceSended;
            _userManager = userManager;

        }
        [HttpGet]
        [Route("getinvoices")]
        public IActionResult GetInvoicesForCompany(long companyId, string filter, string numericFilter, string sort, DateTime? startDateFilter, DateTime? endDateFilter, int pageNumber, int pageSize)
        {
            if (AuthorityHelper.CheckIfHasPermitionForCompany(User.Claims, companyId))
            {
                var invoice = _invoice.TableNoTracking.Where(ih => ih.Company.Id == companyId);

                if (!startDateFilter.Equals(null)) invoice = invoice.Where(s => s.Created > startDateFilter);
                if (!endDateFilter.Equals(null)) invoice = invoice.Where(s => s.Created < endDateFilter);
                invoice = invoice.OrderBy(sort);
                if (!String.IsNullOrEmpty(filter))
                {
                    string[] allString = filter.Split("|");
                    foreach (string f in allString)
                    {
                        string[] fliterKeyValue = f.Split("!=!");


                        if (fliterKeyValue[0] == "PaymentStatus")
                        {
                            var p = fliterKeyValue[1] == "0" ? PaymentStatus.Notpaid : fliterKeyValue[1] == "1" ? PaymentStatus.Paid : PaymentStatus.Overpaid;
                            invoice = invoice.Where(nw => nw.PaymentStatus.Equals(p));
                        }
                        else
                            invoice = invoice.Where(fliterKeyValue[0] + ".Contains(@0)", fliterKeyValue[1]);
                    }
                }
                if (!String.IsNullOrEmpty(numericFilter))
                {
                    string[] allString = numericFilter.Split("|");
                    foreach (string f in allString)
                    {
                        string[] fliterKeyValue = f.Split("!=!");
                        if (fliterKeyValue[0].Contains("Brutto"))
                        {
                            if (fliterKeyValue[0].Contains("From"))
                            {
                                invoice = invoice.Where("BruttoWorth >" + Int32.Parse(fliterKeyValue[1]));
                            }
                            else
                            {
                                invoice = invoice.Where("BruttoWorth <" + Int32.Parse(fliterKeyValue[1]));
                            }
                        }
                        else
                        {
                            if (fliterKeyValue[0].Contains("From"))
                            {
                                invoice = invoice.Where("NettoWorth >" + Int32.Parse(fliterKeyValue[1]));
                            }
                            else
                            {
                                invoice = invoice.Where("NettoWorth <" + Int32.Parse(fliterKeyValue[1]));
                            }
                        }
                    }
                }
                invoice = invoice.Include(i => i.InvoiceContractor).ThenInclude(ic => ic.Contractor).ThenInclude(c => c.ContractorBankAccounts).Include(i => i.InvoiceEntries).Include(i => i.InvoiceIssuer).Include(i => i.InvoiceSendeds).ThenInclude(isn => isn.User).Include(i => i.SellDateName).Include(i => i.PaymentsForInvoices);
                var pagedResult = invoice.GetPaged(pageNumber, pageSize);
                _logger.LogInformation(new EventId(11, "GetInvoicesForCompany"), "invoices: " + string.Join(",", pagedResult.Results.Select(i => i.Id).ToArray()) + " with companyId: " + companyId);

                return Ok(new
                {
                    totalCount = pagedResult.RowCount,
                    invoice = pagedResult.Results
                });

            }
            else
            {
                _logger.LogWarning(new EventId(11, "GetInvoicesForCompany"), "acces denaied with companyId:" + companyId);
                return Unauthorized("Brak autoryzacji do zasobów firmy");
            }

        }

        [HttpGet]
        [Route("issuers")]
        public async Task<IActionResult> GetInvoiceIssuers(long companyId)
        {

            if (AuthorityHelper.CheckIfHasPermitionForCompany(User.Claims, companyId))
            {
                var invoiceIssuer = _invoiceIssuer.AsQueryable().Where(ih => ih.Company.Id == companyId);
                _logger.LogInformation(new EventId(12, "GetInvoiceIssuers"), "issuers: " + string.Join(",", invoiceIssuer.Select(i => i.Id).ToArray()) + " companyId:" + companyId);
                return Ok(await invoiceIssuer.ToListAsync());
            }
            else
            {
                _logger.LogWarning(new EventId(12, "GetInvoiceIssuers"), "acces denaied with companyId:" + companyId);
                return Unauthorized("Brak autoryzacji do zasobów firmy");
            }
        }

        [HttpPost]
        [Route("{companyId}")]
        public async Task<IActionResult> InsertInvoice(long companyId, Invoice invoice)
        {
            if (AuthorityHelper.CheckIfHasPermitionForCompany(User.Claims, companyId))
            {
                invoice.Company = _company.Table.FirstOrDefault(ic => ic.Id == companyId);
                invoice.Client = _client.Table.FirstOrDefault(ic => ic.User.Id == User.FindAll(ClaimTypes.NameIdentifier).LastOrDefault().Value);

                if (invoice.InvoiceContractor != null && invoice.InvoiceContractor.Id == 0)
                {
                    if (invoice.InvoiceContractor.Contractor == null || invoice.InvoiceContractor.Contractor.Id == 0)
                    {
                        invoice.InvoiceContractor.Contractor = null;
                    }
                    else
                    {
                        var existingInvoiceContractor = _invoiceContractor.Table.Where(ic => ic.Contractor.Id == invoice.InvoiceContractor.Contractor.Id).Include(ic => ic.Contractor).FirstOrDefault(c =>
                             c.Street == invoice.InvoiceContractor.Street &&
                             c.HouseNumber == invoice.InvoiceContractor.HouseNumber &&
                             c.ApartamentNumber == invoice.InvoiceContractor.ApartamentNumber &&
                             c.City == invoice.InvoiceContractor.City &&
                             c.Postalcode == invoice.InvoiceContractor.Postalcode &&
                             c.Postoffice == invoice.InvoiceContractor.Postoffice &&
                             c.Country == invoice.InvoiceContractor.Country &&
                             c.Nip == invoice.InvoiceContractor.Nip &&
                             c.Name == invoice.InvoiceContractor.Name);
                        if (existingInvoiceContractor != null)
                        {
                            invoice.InvoiceContractor = existingInvoiceContractor;
                        }
                        else
                        {
                            invoice.InvoiceContractor.Contractor = _contractor.Table.FirstOrDefault(c => c.Id == invoice.InvoiceContractor.Contractor.Id && c.Company.Id == companyId);
                        }
                    }
                }
                else
                {
                    invoice.InvoiceContractor = _invoiceContractor.Table.FirstOrDefault(ic => ic.Id == invoice.InvoiceContractor.Id && ic.Contractor.Company.Id == companyId);
                }
                //impelemntacja odnajdywania issuera po SellDate
                invoice.InvoiceIssuer = _invoiceIssuer.Table.OrderBy(iis => iis.Created).LastOrDefault();
                invoice.SellDateName = _sellDateName.Table.FirstOrDefault(sdn => sdn.Id == invoice.SellDateName.Id);
                bool isInserted = await _invoice.InsertAsync(invoice);
                if (isInserted)
                {
                    _logger.LogInformation(new EventId(13, "InsertInvoice"), "added " + invoice.Id + " companyId:" + companyId);
                    return Ok(invoice);

                }
                else
                {
                    _logger.LogError(new EventId(13, "InsertInvoice"), "db error companyId:" + companyId);
                    return BadRequest("błąd przy dodawaniu faktury do bazy danych");
                }

            }
            else
            {
                _logger.LogWarning(new EventId(13, "InsertInvoice"), "acces denaied with companyId:" + companyId);
                return Unauthorized("Brak autoryzacji do danej faktury");
            }
        }

        [HttpPut]
        [Route("{id}")]
        public async Task<IActionResult> UpdateInvoice(long id, JObject changedData)
        {
            var invoice = await _invoice.Table.Include(i => i.InvoiceContractor).ThenInclude(ic => ic.Contractor).Include(i => i.InvoiceContractor).Include(i => i.Company).Include(i => i.InvoiceEntries).Include(i => i.InvoiceIssuer).Include(i => i.InvoiceSendeds).Include(i => i.SellDateName).FirstOrDefaultAsync(i => i.Id == id);

            if (invoice != null && AuthorityHelper.CheckIfHasPermitionForCompany(User.Claims, invoice.Company.Id))
            {
                var invoiceContractor = changedData.Property("invoiceContractor");
                if (invoiceContractor != null)
                {
                    var contractor = ((JObject)invoiceContractor.Value).Property("contractor");
                    var invoiceContractorId = ((JObject)invoiceContractor.Value).Property("id");
                    if (contractor != null)
                    {
                        long contractorIdV = (long)((JObject)contractor.Value).Property("id").Value;
                        if (contractorIdV != 0)
                        {
                            var selectedContartcor = _contractor.Table.FirstOrDefault(con => con.Id == contractorIdV && con.Company.Id == invoice.Company.Id);
                            //check if client is getting wright contractor
                            if (selectedContartcor == null)
                            {
                                ((JObject)invoiceContractor.Value).Remove("contractor");
                                _logger.LogWarning(new EventId(14, "UpdateInvoice"), "no contractors with changedData:" + changedData.ToString() + "  companyId: " + id);
                                return BadRequest("Próba wyciagniecia danych kontachemntów innych firm");
                            }
                            else
                            {
                                //check if thee is invoicecontractor in db
                                var newInvoiceContractor = invoiceContractor.Value.ToObject<InvoiceContractor>();
                                var existingInvoiceContractor = _invoiceContractor.Table.Where(ic => ic.Contractor.Id == contractorIdV && ic.Contractor.Company.Id == invoice.Company.Id).FirstOrDefault(c =>
                                     c.Street == newInvoiceContractor.Street &&
                                     c.HouseNumber == newInvoiceContractor.HouseNumber &&
                                     c.ApartamentNumber == newInvoiceContractor.ApartamentNumber &&
                                     c.City == newInvoiceContractor.City &&
                                     c.Postalcode == newInvoiceContractor.Postalcode &&
                                     c.Postoffice == newInvoiceContractor.Postoffice &&
                                     c.Country == newInvoiceContractor.Country &&
                                     c.Nip == newInvoiceContractor.Nip &&
                                     c.Name == newInvoiceContractor.Name);
                                if (existingInvoiceContractor != null)
                                {
                                    ((JObject)invoiceContractor.Value).RemoveAll();
                                    ((JObject)invoiceContractor.Value).Add("id", existingInvoiceContractor.Id);
                                }
                                else
                                {
                                    if (newInvoiceContractor != null)
                                    {
                                        newInvoiceContractor.Contractor = selectedContartcor;
                                    }
                                    foreach (var nic in newInvoiceContractor.GetType().GetProperties())
                                    {

                                        if (nic.GetValue(newInvoiceContractor) == null && nic.Name != "Invoices" && nic.Name != "Created" && nic.Name != "Updated" && nic.Name != "Contractor")
                                        {
                                            var value = nic.GetValue(invoice.InvoiceContractor).ToString();
                                            if (value != null)
                                            {
                                                ((JObject)invoiceContractor.Value).Add(nic.Name, value);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {

                        if (invoiceContractorId != null)
                        {
                            //checked if cient dosnt changing invoice contractor
                            if (((long)invoiceContractorId.Value) == invoice.InvoiceContractor.Id && _invoice.Table.Count(i => i.InvoiceContractor.Id == invoice.InvoiceContractor.Id) != 1)
                            {
                                invoiceContractorId.Value = 0;
                                foreach (var icv in invoice.InvoiceContractor.GetType().GetProperties())
                                {
                                    string name = icv.Name.Substring(0, 1).ToLower() + icv.Name.Remove(0, 1);
                                    if (((JObject)invoiceContractor.Value).Property(name) == null && name != "invoices" && name != "created" && name != "updated")
                                    {
                                        if (name == "contractor")
                                        {
                                            var company = icv.GetValue(invoice.InvoiceContractor);
                                            ((JObject)invoiceContractor.Value).Add(new JProperty(name, new JObject(new JProperty("id", company.GetType().GetProperty("Id").GetValue(company).ToString()))));
                                        }
                                        else
                                        {
                                            ((JObject)invoiceContractor.Value).Add(name, icv.GetValue(invoice.InvoiceContractor).ToString());

                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                /*                var invoiceIssuer = changedData.Property("invoiceIssuer");
                                if (invoiceIssuer != null)
                                {
                                    if (invoiceIssuer.Select(ic => ic["id"]).First().ToString() == invoice.InvoiceContractor.Id.ToString())
                                    {
                                        changedData.Remove("invoiceIssuer");
                                    }
                implementacja odnajdywania w bazie odpowiedniego invoice issuera za pomoca daty utworzenia
                                }*/
                invoice = await _invoice.Update(invoice, changedData);
                _logger.LogInformation(new EventId(14, "UpdateInvoice"), "updated " + id + " with changedData:" + changedData.ToString() + " companyId: " + invoice.Company.Id);
                return Ok(invoice);
            }
            _logger.LogWarning(new EventId(14, "UpdateInvoice"), "acces denaied with invoiceId:" + id);
            return Unauthorized("Brak autoryzacji do danej faktury");
        }
        [HttpDelete]
        [Route("{id}")]
        public async Task<IActionResult> RemoveInvoice(long id)
        {
            var invoice = await _invoice.Table.Include(i => i.Company).Include(i => i.InvoiceEntries).Include(i => i.InvoiceSendeds).FirstOrDefaultAsync(i => i.Id == id);

            if (invoice != null && AuthorityHelper.CheckIfHasPermitionForCompany(User.Claims, invoice.Company.Id))
            {
                bool isInserted = _invoice.Delete(invoice);
                if (isInserted)
                {
                    return Ok(invoice);
                }
                _logger.LogInformation(new EventId(15, "RemoveInvoice"), "removed " + id);
                return BadRequest("błąd przy usuwaniu faktury z bazy danych");
            }
                _logger.LogWarning(new EventId(15, "RemoveInvoice"), "acces denaied with invoiceId:" + id);
                return Unauthorized("Brak autoryzacji do danej faktury");
        }
        [HttpPost]
        [Route("{id}/email")]
        public async Task<IActionResult> SendByEmail(long id, JObject pdf)
        {
            var invoice = await _invoice.Table.Include(i => i.Company).ThenInclude(c => c.InvoiceMailTemplates).Include(i => i.InvoiceContractor).ThenInclude(ic => ic.Contractor).Include(i => i.InvoiceEntries).Include(i => i.InvoiceSendeds).FirstOrDefaultAsync(i => i.Id == id);

            if (invoice != null && AuthorityHelper.CheckIfHasPermitionForCompany(User.Claims, invoice.Company.Id))
            {
                if (invoice.InvoiceContractor.Contractor != null)
                {
                    var pdfBinary = Convert.FromBase64String(pdf.Property("pdf").ToObject<string>());
                    var dir = Path.Combine("~/DataDump");


                    if (!Directory.Exists(dir))
                        Directory.CreateDirectory(dir);

                    var fileName = dir + "\\PDFnMail-" + DateTime.Now.ToString("yyyyMMdd-HHMMss") + ".pdf";

                    using (var stream = new MemoryStream(pdfBinary))
                    {
                        var file = new FormFile(stream, 0, stream.Length, null, invoice.Number.Replace("\\", "_") + ".pdf")
                        {
                            Headers = new HeaderDictionary(),
                            ContentType = "application/pdf"
                        };
                        var fileCollection = new FormFileCollection();
                        fileCollection.Add(file);
                        var expressionWords = new List<string>() { "[[my_name]]", "[[contractor_name]]", "[[contractor_balance]]", "[[invoice_number]]", "[[issue_place]]", "[[invoice_date]]", "[[payment_date]]", "[[currency]]", "[[total_worth]]", "[[net_worth]]" };
                        var mailTample = invoice.Company.InvoiceMailTemplates.FirstOrDefault(imt => imt.MailLanguage == invoice.InvoiceContractor.Contractor.MailLanguage);
                        foreach (string eword in expressionWords)
                        {
                            var replaceFraze = String.Empty;
                            switch (eword)
                            {
                                case "[[my_name]]":
                                    replaceFraze = invoice.Company.Name;
                                    break;
                                case "[[contractor_name]]":
                                    replaceFraze = invoice.InvoiceContractor.Name;
                                    break;
                                case "[[contractor_balance]]":
                                    /*                                decimal contractorSum = _contractor.Table.Where(c => c.Id == invoice.InvoiceContractor.Contractor.Id).Select(c => (c.InvoiceContractors.Select(ic => ic.Invoices.Select(i => i.BruttoWorth)))).Sum();
                                                                    decimal paymentSum = _invoice.Table.Where(i => i.InvoiceContractor.Contractor.Id == invoice.InvoiceContractor.Contractor.Id).Select(i => (i.PaymentsForInvoices.Select(p => p.PaymentValueForInvoice))).Sum();*/
                                    replaceFraze = (invoice.BruttoWorth * invoice.Rate).ToString();
                                    break;
                                case "[[invoice_number]]":
                                    replaceFraze = invoice.Number;
                                    break;
                                case "[[issue_place]]":
                                    replaceFraze = invoice.CreationPlace;
                                    break;
                                case "[[invoice_date]]":
                                    replaceFraze = invoice.SellDate.ToString();
                                    break;
                                case "[[payment_date]]":
                                    replaceFraze = invoice.PaymentDate.ToString();
                                    break;
                                case "[[currency]]":
                                    replaceFraze = invoice.PaymentCurrency;
                                    break;
                                case "[[total_worth]]":
                                    replaceFraze = invoice.BruttoWorth.ToString();
                                    break;
                                case "[[net_worth]]":
                                    replaceFraze = invoice.NettoWorth.ToString();
                                    break;
                                default:
                                    break;
                            }
                            mailTample.Message = mailTample.Message.Replace(eword, replaceFraze);
                            mailTample.Title = mailTample.Title.Replace(eword, replaceFraze);

                        }
                        if (mailTample == null)
                        {
                            mailTample = new InvoiceMailTemplate();
                            mailTample.Title = "Faktura Klienta " + User.Identity.Name + " o numerze " + invoice.Number;
                            mailTample.Message = "Wiadomość wygenerowan przez system SWI2, brak szoblonu wysyłkowego, prosze nie odpisywać.</br> Fatktura w załączniku";
                        }
                        var message = new EmailMessage(new string[] { invoice.InvoiceContractor.Contractor.Email }, mailTample.Title, mailTample.Message, fileCollection);

                        if (await _emailService.SendEmailAsync(message))
                        {
                            var invoiceSended = new InvoiceSended()
                            {
                                Created = DateTime.Now,
                                Email = invoice.InvoiceContractor.Contractor.Email,
                                Invoice = invoice,
                                User = await _userManager.FindByIdAsync(User.FindAll(ClaimTypes.NameIdentifier).LastOrDefault().Value)
                            };
                            if (await _invoiceSended.InsertAsync(invoiceSended))
                            {
                                _logger.LogInformation(new EventId(16, "SendByEmail"), "sended to " + invoice.InvoiceContractor.Contractor.Email + " " + id);
                                return Ok(invoiceSended);
                            }
                            else
                            {
                                _logger.LogError(new EventId(16, "SendByEmail"), "cannot add invoiceSended" + id);
                                return BadRequest("Email wysłany ,problem w dodaniu rekordu do bazy");
                            }
                        }

                        _logger.LogError(new EventId(16, "SendByEmail"), "cannot sned mail " + id);
                        return BadRequest("Email nie wysłany ,sprawdź czy emial kontrachenta jest aktualny");
                    }

                }
                _logger.LogWarning(new EventId(16, "SendByEmail"), "no contractor " + id);
                return BadRequest("Faktura nie posiada kontrachenta w systemie");
            }
            _logger.LogWarning(new EventId(16, "SendByEmail"), "acces denaied with invoiceId:" + id);
            return Unauthorized("Brak autoryzacji do danej faktury");
        }
        [HttpGet]
        [Route("{companyId}/lastindex")]
        public async Task<IActionResult> GetIndexOfLastInvoiceInMonth(long companyId)
        {
            if (AuthorityHelper.CheckIfHasPermitionForCompany(User.Claims, companyId))
            {
                var todayMonth = DateTime.Today.AddDays(-DateTime.Today.Day).Month;
                var lastindex = await _invoice.Table.OrderBy(i => i.Id).Where(i => i.Company.Id == companyId).Select(i => i.Number).LastOrDefaultAsync();
                if (lastindex != null && lastindex.Split('/')[1] == todayMonth.ToString())
                {
                    _logger.LogInformation(new EventId(17, "SendByEmail"), "index " + lastindex.Split('/')[0] + " " + companyId);
                    return Ok(lastindex.Split('/')[0]);

                }
                else
                {
                    _logger.LogInformation(new EventId(17, "SendByEmail"), "index " + 0.ToString() + " " + companyId);
                    return Ok(0);
                }
            }
            _logger.LogWarning(new EventId(17, "SendByEmail"), "acces denaied with companyId:" + companyId);
            return Unauthorized("Brak autoryzacji do danej faktury");
        }

        //Zwraca entries dictorany dla danej firmy
        [HttpGet]
        [Route("etriedictionary")]
        public async Task<IActionResult> GetEntiresDictonary(long companyId)
        {
            if (AuthorityHelper.CheckIfHasPermitionForCompany(User.Claims, companyId))
            {
                var entryDictionary = _entryDictionary.AsQueryable().Where(ih => ih.Company.Id == companyId);
                _logger.LogInformation(new EventId(18, "GetEntiresDictonary"), "sended dictionary " + companyId);
                return Ok(await entryDictionary.ToListAsync());
            }
            _logger.LogWarning(new EventId(18, "GetEntiresDictonary"), "acces denaied with companyId:" + companyId);
            return Unauthorized("Brak autoryzacji do zasobów firmy");

        }
        [HttpPost]
        [Route("etriedictionary/{companyId}")]
        public async Task<IActionResult> InsertEntireDictonary(long companyId, EntryDictionary invoiceEntry)
        {
            if (AuthorityHelper.CheckIfHasPermitionForCompany(User.Claims, companyId))
            {
                invoiceEntry.Company = _company.Table.First(c => c.Id == companyId);
                if (invoiceEntry.Company != null)
                {
                    var isAdded = await _entryDictionary.InsertAsync(invoiceEntry);
                    if (isAdded)
                    {
                        _logger.LogInformation(new EventId(19, "InsertEntireDictonary"), "added " + invoiceEntry.Id + " companyId:" + companyId);
                        return Ok(invoiceEntry);
                    }
                    _logger.LogError(new EventId(19, "InsertEntireDictonary"), "error db companyId:" + companyId);
                    return BadRequest("Błąd w trakcie dodawania do słownika");
                }
                _logger.LogError(new EventId(19, "InsertEntireDictonary"), "no company companyId:" + companyId);
                return BadRequest("Firma nie istnieje");

            }
            _logger.LogWarning(new EventId(19, "InsertEntireDictonary"), "acces denaied with companyId:" + companyId);
            return Unauthorized("Brak autoryzacji do zasobów firmy");

        }
        [HttpPut]
        [Route("etriedictionary/{entryId}")]
        public async Task<IActionResult> UpdateEntireDictonary(long entryId, JObject data)
        {
            var entryDictionary = await _entryDictionary.Table.Include(e => e.Company).FirstOrDefaultAsync(i => i.Id == entryId);

            if (AuthorityHelper.CheckIfHasPermitionForCompany(User.Claims, entryDictionary.Company.Id))
            {
                entryDictionary = await _entryDictionary.Update(entryDictionary, data);
                _logger.LogInformation(new EventId(20, "UpdateEntireDictonary"), "updated " + entryId + " with dataChanged" + data.ToString() + " companyId: " + entryDictionary.Company.Id);
                return Ok(entryDictionary);
            }

            _logger.LogWarning(new EventId(20, "UpdateEntireDictonary"), "acces denaied with entriDictionaryId:" + entryId);
            return BadRequest("Brak autoryzacji do zasobów firmy");

        }

        [HttpGet]
        [Route("selldatename")]
        public async Task<IActionResult> GetSellDateNames()
        {
            _logger.LogInformation(new EventId(21, "GetSellDateNames"), "sneded");
            return Ok(await _sellDateName.AsQueryable().ToListAsync());
        }

        [HttpGet]
        [Route("sendedinvoices/{id}")]
        public IActionResult GetAllSendedInvoices(long id, [FromQuery] string query)
        {
            if (AuthorityHelper.CheckIfHasPermitionForCompany(User.Claims, id))
            {
                TableParamsModel tableParams = query != null ? JsonConvert.DeserializeObject<TableParamsModel>(Encoding64.Base64Decode(query)) : new TableParamsModel() { Sort = "created", PageNumber = 0, PageSize = 25, Filters = new List<FilterModel>() };
                IQueryable<InvoiceSendedViewModel> invoiceSendedViewModel = _invoiceSended.Table.Where(ins => ins.Invoice.Company.Id == id).Select(ins =>
                new InvoiceSendedViewModel()
                {
                    Id = ins.Id,
                    Email = ins.Email,
                    UserName = ins.User.UserName,
                    Number = ins.Invoice.Number,
                    Created = ins.Created
                }).OrderBy(tableParams.Sort);
                foreach (FilterModel fm in tableParams.Filters)
                {
                    switch (fm.Type)
                    {
                        case "string":
                            invoiceSendedViewModel = invoiceSendedViewModel.Where(fm.Name + ".Contains(@0)", fm.Value);
                            break;
                        case "date":
                            invoiceSendedViewModel = invoiceSendedViewModel.Where($"" + fm.Name + "@0", Convert.ToDateTime(fm.Value));
                            break;
                        case "int":
                            invoiceSendedViewModel = invoiceSendedViewModel.Where(fm.Name + fm.Value);
                            break;
                        default:
                            invoiceSendedViewModel = invoiceSendedViewModel.Where(fm.Name + ".Contains(@0)", fm.Value);
                            break;
                    }
                }


                var pagedResult = invoiceSendedViewModel.GetPaged(tableParams.PageNumber, tableParams.PageSize);

                _logger.LogInformation(new EventId(22, "GetAllSendedInvoices"), "sendedInvoices: " + string.Join(",", pagedResult.Results.Select(i => i.Id).ToArray()) + " with companyId:" + id);
                return Ok(new TableViewModel<InvoiceSendedViewModel>
                {
                    totalCount = pagedResult.RowCount,
                    elements = pagedResult.Results
                });

            }
            _logger.LogWarning(new EventId(22, "GetAllSendedInvoices"), "acces denaied with companyId: " + id);

            return Unauthorized("Brak autoryzacji do zasobów firmy");
        }

        [HttpPost]
        [Route("findmachinginvoices/{companyId}")]
        public async Task<IActionResult> FindMachingInvoice(long companyId, [FromBody] IEnumerable<PaymentMachingModel> payments, [FromQuery] bool ifSeriesSettelement, [FromQuery] bool ifFromTheLastSetteld, [FromQuery] string accountNumber, [FromQuery] string currency, [FromQuery] bool byTitle = false, [FromQuery] bool byAddressee = false, [FromQuery] bool byValue = false)
        {
            if (AuthorityHelper.CheckIfHasPermitionForCompany(User.Claims, companyId))
            {
                IQueryable<Invoice> invoices = null;
                IQueryable<Contractor> contractors = _contractor.Table.Where(c => c.Company.Id == companyId);
                IQueryable<Contractor> serchedContractors = null;
                Contractor contractor = null;
                foreach (var payment in payments)
                {
                    serchedContractors = contractors.Where(i => i.InvoiceContractors.Any(ic =>
                        payment.Topic.Contains(ic.Name) && ic.Name != String.Empty ||
                        payment.Topic.Contains(ic.Nip) && ic.Nip != String.Empty ||
                        payment.Addressee.Contains(ic.Name) && ic.Name != String.Empty));



                    if (serchedContractors.Count() == 0)
                    {
                        serchedContractors = contractors.Where(i => i.InvoiceContractors.Any(ic => ic.Invoices.Any(i => payment.Topic.Contains(i.Number))));
                        if (serchedContractors.Count() == 0)
                        {
                            payment.ContractorBankAccount.Contractor = serchedContractors.FirstOrDefault();
                        }
                        serchedContractors = contractors.Where(i => i.InvoiceContractors.Any(ic =>
                            payment.Addressee.Contains(ic.Postalcode) &&
                            payment.Addressee.Contains(ic.Street) &&
                            payment.Addressee.Contains(ic.HouseNumber)));

                        if (serchedContractors.Count() == 0)
                        {
                            var contractorList = contractors.Include(c => c.InvoiceContractors).ToArray().Where(c => c.InvoiceContractors != null && c.InvoiceContractors.Any(ic =>
                            ic.Name != null &&
                            ic.Name != String.Empty &&
                            ((String.Join("", ic.Name.Intersect(payment.Addressee)).Count() > ic.Name.Count() * 4 / 5) ||
                             (String.Join("", ic.Name.Intersect(payment.Topic)).Count() > ic.Name.Count() * 4 / 5))));
                            if (contractorList != null)
                            {
                                payment.ContractorBankAccount.Contractor = contractorList.FirstOrDefault();
                            }
                        }
                        else
                        {
                            payment.ContractorBankAccount.Contractor = serchedContractors.FirstOrDefault();

                        }
                    }
                    else
                    {
                        payment.ContractorBankAccount.Contractor = serchedContractors.FirstOrDefault();
                    }

                    if (serchedContractors.Count() == 0)
                    {
                        serchedContractors = contractors.Where(i => i.ContractorBankAccounts.Any(cba => cba.AccountNumber == payment.ContractorBankAccount.AccountNumber));
                        if (serchedContractors.Count() != 0)
                        {
                            if (payment.ContractorBankAccount.Contractor == null)
                            {
                                payment.ContractorBankAccount = serchedContractors.Include(c => c.ContractorBankAccounts).ThenInclude(cba => cba.Contractor).FirstOrDefault().ContractorBankAccounts.FirstOrDefault(cba => cba.AccountNumber == payment.ContractorBankAccount.AccountNumber);
                            }
                            else
                            {
                                var serchedContractor = serchedContractors.Include(c => c.ContractorBankAccounts).ThenInclude(cba => cba.Contractor).FirstOrDefault(c => c.Id == payment.ContractorBankAccount.Contractor.Id);
                                if (serchedContractor != null)
                                {
                                    payment.ContractorBankAccount = serchedContractor.ContractorBankAccounts.FirstOrDefault(cba => cba.AccountNumber == payment.ContractorBankAccount.AccountNumber);
                                }
                            }
                        }

                    }
                    else
                    {
                        var ContractorBankAccount = serchedContractors.Include(c => c.ContractorBankAccounts).ThenInclude(cba => cba.Contractor).FirstOrDefault().ContractorBankAccounts.FirstOrDefault(cba => cba.AccountNumber == payment.ContractorBankAccount.AccountNumber);
                        if (ContractorBankAccount != null)
                        {
                            payment.ContractorBankAccount = ContractorBankAccount;
                        }
                    }
                }
                var grouptPayments = payments.GroupBy(p => p.ContractorBankAccount.Contractor);
                foreach (var grouptPayment in grouptPayments)
                {
                    invoices = _invoice.Table.Where(i => i.Company.Id == companyId && i.PaymentCurrency == currency);
                    if (grouptPayment.Key != null)
                    {
                        invoices = invoices.Where(i => i.InvoiceContractor.Contractor.Id == grouptPayment.Key.Id);
                        if (invoices.Where(i => i.PaymentStatus == PaymentStatus.Notpaid).Any())
                        {
                            if (ifSeriesSettelement)
                            {
                                var invoicePaidDateFilter = DateTime.Now;
                                if (ifFromTheLastSetteld)
                                {
                                    invoicePaidDateFilter = invoices.Where(i => i.PaymentStatus == PaymentStatus.Paid || i.PaymentStatus == PaymentStatus.Overpaid).Max(i => i.PaymentDate);
                                    invoicePaidDateFilter = invoices.Where(i => i.PaymentStatus == PaymentStatus.Notpaid && i.PaymentDate >= invoicePaidDateFilter).Min(i => i.PaymentDate);
                                }
                                else
                                {
                                    invoicePaidDateFilter = invoices.Where(i => i.PaymentStatus == PaymentStatus.Notpaid).Min(i => i.PaymentDate);
                                }
                                invoices = invoices.Where(i => i.PaymentStatus == PaymentStatus.Notpaid && i.PaymentDate > invoicePaidDateFilter).OrderBy(i => i.PaymentDate).Include(i => i.PaymentsForInvoices);
                                int currentInvoiceId = 0;
                                int invoicesLength = invoices.Count();
                                decimal slidingPamentValue = 0, invoiceUnpaidValue = 0, newPaymentForInvoiceValueSum = 0;
                                Invoice currentInvoice = null;
                                if (invoicesLength != 0)
                                {
                                    foreach (var payment in grouptPayment)
                                    {
                                        payment.PaymentsForInvoices = new List<PaymentForInvoice>();
                                        if (invoicesLength >= currentInvoiceId)
                                        {
                                            slidingPamentValue = payment.PaymentValue;
                                            do
                                            {
                                                if (invoicesLength > currentInvoiceId)
                                                {
                                                    currentInvoice = invoices.Skip(currentInvoiceId).First();
                                                    invoiceUnpaidValue = currentInvoice.BruttoWorth - currentInvoice.PaymentsForInvoices.Sum(p => p.PaymentValueForInvoice) - newPaymentForInvoiceValueSum;
                                                    slidingPamentValue -= invoiceUnpaidValue;

                                                    if (slidingPamentValue > 0)
                                                    {
                                                        newPaymentForInvoiceValueSum = 0;
                                                        payment.PaymentsForInvoices.Add(new PaymentForInvoice() { Invoice = currentInvoice, PaymentValueForInvoice = invoiceUnpaidValue });
                                                        currentInvoiceId++;
                                                    }
                                                    else if (slidingPamentValue < 0)
                                                    {
                                                        newPaymentForInvoiceValueSum = invoiceUnpaidValue + slidingPamentValue;
                                                        payment.PaymentsForInvoices.Add(new PaymentForInvoice() { Invoice = currentInvoice, PaymentValueForInvoice = invoiceUnpaidValue + slidingPamentValue });
                                                        break;
                                                    }
                                                    else
                                                    {
                                                        newPaymentForInvoiceValueSum = 0;
                                                        payment.PaymentsForInvoices.Add(new PaymentForInvoice() { Invoice = currentInvoice, PaymentValueForInvoice = invoiceUnpaidValue });
                                                        currentInvoiceId++;
                                                        break;
                                                    }
                                                }
                                                else
                                                {
                                                    break;
                                                }
                                            }
                                            while (slidingPamentValue > 0);
                                        }
                                        else
                                        {
                                            break;

                                        }
                                    }
                                }
                            }
                            else
                            {
                                invoices = invoices.Where(i => i.PaymentStatus == PaymentStatus.Notpaid);
                                IQueryable<Invoice> serchedInvoices = null;
                                Invoice invoice = null;
                                foreach (var payment in grouptPayment)
                                {

                                    if (grouptPayment.Key != null)
                                    {
                                        invoices = invoices.Where(i => i.InvoiceContractor.Contractor.Id == grouptPayment.Key.Id);
                                    }
                                    if (accountNumber != null && invoices.Count() > 1)
                                    {
                                        serchedInvoices = invoices.Where(i => i.PaymentAccountNumber == accountNumber);
                                        if (serchedInvoices.Count() > 0)
                                        {
                                            invoices = serchedInvoices;
                                        }

                                    }
                                    if (byValue && invoices.Count() > 1)
                                    {
                                        serchedInvoices = invoices.Where(i => i.BruttoWorth == payment.PaymentValue);
                                        if (serchedInvoices.Count() > 0)
                                        {
                                            invoices = serchedInvoices;
                                        }
                                    }
                                    if (byTitle && invoices.Count() > 1)
                                    {
                                        serchedInvoices = invoices.Where(i => payment.Topic.Contains(i.Number));
                                        if (serchedInvoices.Count() > 0)
                                        {
                                            invoices = serchedInvoices;
                                        }
                                        else
                                        {
                                            if (byAddressee)
                                            {
                                                var invoicesList = invoices.Include(i => i.InvoiceContractor).ThenInclude(ic => ic.Contractor).ToArray().Where(i =>
                                                i.InvoiceContractor.Contractor.Street != null && i.InvoiceContractor.Contractor.Street != String.Empty &&
                                                (String.Join("", i.InvoiceContractor.Contractor.Street.Intersect(payment.Topic)).Count() > i.InvoiceContractor.Contractor.Street.Count() * 4 / 5)
                                                &&
                                                i.InvoiceContractor.Contractor.Postalcode != null && i.InvoiceContractor.Contractor.Postalcode != String.Empty &&
                                                (String.Join("", i.InvoiceContractor.Contractor.Postalcode.Intersect(payment.Topic)).Count() > i.InvoiceContractor.Contractor.Postalcode.Count() * 4 / 5)
                                                &&
                                                i.InvoiceContractor.Contractor.HouseNumber != null && i.InvoiceContractor.Contractor.HouseNumber != String.Empty &&
                                                (String.Join("", i.InvoiceContractor.Contractor.HouseNumber.Intersect(payment.Topic)).Count() > i.InvoiceContractor.Contractor.HouseNumber.Count() * 4 / 5)
                                                ||
                                                i.InvoiceContractor.Contractor.Name != null && i.InvoiceContractor.Contractor.Name != String.Empty &&
                                                (String.Join("", i.InvoiceContractor.Contractor.Name.Intersect(payment.Topic)).Count() > i.InvoiceContractor.Contractor.Name.Count() * 4 / 5) && i.InvoiceContractor.Contractor.Name.Contains(String.Join("", i.InvoiceContractor.Contractor.Name.Intersect(payment.Topic))));

                                                if (invoicesList.Count() > 0)
                                                {
                                                    invoice = invoicesList.FirstOrDefault();
                                                }
                                            }
                                            else
                                            {
                                                invoices = null;
                                            }
                                        }
                                    }


                                    if (invoice == null && invoices.Count() < 5)
                                    {
                                        invoice = invoices.FirstOrDefault();
                                    }
                                    if (invoice != null)
                                    {
                                        payment.PaymentsForInvoices = new List<PaymentForInvoice>();
                                        payment.PaymentsForInvoices.Add(new PaymentForInvoice() { Invoice = invoice, PaymentValueForInvoice = payment.PaymentValue });
                                    }


                                }
                            }

                        }

                    }
                }
                _logger.LogInformation(new EventId(23, "FindMachingInvoice"), "finded invoices: " + string.Join(",", payments.Select(p => p.PaymentsForInvoices.Select(pfi => pfi.Invoice.Id)).Cast<double>().ToArray()) + " with companyId:" + companyId);
                return Ok(payments);
            }
            _logger.LogWarning(new EventId(23, "FindMachingInvoice"), "acces denaied with companyId: " + companyId);
            return Unauthorized("Brak autoryzacji do zasobów firmy");
        }
        [HttpGet]
        [Route("contractorInvoices/{companyId}")]
        public async Task<IActionResult> GetContractorInvoices(long companyId, long contractorId)
        {
            if (AuthorityHelper.CheckIfHasPermitionForCompany(User.Claims, companyId))
            {
                _logger.LogInformation(new EventId(24, "GetContractorInvoices"), "contractorId: " + contractorId + " companyId: " + companyId);
                return Ok(_invoice.Table.Where(i => i.Company.Id == companyId && i.InvoiceContractor.Contractor.Id == contractorId));
            }

            _logger.LogWarning(new EventId(24, "GetContractorInvoices"), "acces denaied with companyId: " + companyId);
            return Unauthorized("Brak autoryzacji do zasobów firmy");
        }

        #region helpers 
        #endregion
    }
}

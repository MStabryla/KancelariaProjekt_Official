using Microsoft.AspNetCore.Mvc.Testing;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using System.Linq;
using System.Net.Http.Json;
using SWI2DB.Models.Company;
using SWI2.Models.Company;
using SWI2.Models.Authentication;
using SWI2.Models.Response;
using XUnit.Project.Attributes;
using SWI2.Models;
using SWI2.Services.Static;
using Newtonsoft.Json;

namespace SWIIntegarationTests
{
    [TestCaseOrderer("XUnit.Project.Orderers.TestPriority", "XUnit.Project")]
    public class CompanyControllerTest : BasicTests
    {
        public CompanyControllerTest(WebApplicationFactory<SWI2.Startup> factory) : base(factory)
        {

        }

        #region ClientCompany
        [Fact, TestPriority(0,TestType.Get)]
        public async Task GetCompanyInfoForClient()
        {
            var client = await GetAuthorizedHttpClient("clientII");
            //await RefreshToken("/api/companypanel", client);

            string url = "/api/companypanel";
            var response = await client.GetAsync(url);
            await CheckOk(response);
            var content = await response.Content.ReadFromJsonAsync<TableViewModel<CompanyViewModel>>();
            Assert.True(content.elements.Count() > 0);
        }
        [Fact, TestPriority(0, TestType.Edit)]
        public async Task EditCompanyForClient()
        {
            var client = await GetAuthorizedHttpClient("clientII");

            var tableobject = new TableParamsModel() { PageSize = 25, PageNumber = 0, Sort="created", Filters = new List<FilterModel> { } };
            var query = "?query=" + Encoding64.Base64Encode(JsonConvert.SerializeObject(tableobject));
            var url = "api/companypanel" + query;

            var response = await client.GetAsync(url);
            await CheckOk(response);
            var content = await response.Content.ReadFromJsonAsync<TableViewModel<CompanyViewModel>>();
            var company = content.elements.First();
            Assert.InRange(company.Id, -1, long.MaxValue);

            url = "/api/companypanel/" + company.Id;

            company.Name = "Test Name " + DateTime.Now.Day;
            company.Street = "test street";
            company.Postoffice = "Kraków";
            company.InvoiceDescription = "test invoice description";

            await ActivateToken(client);

            var contentSend = JsonContent.Create(company);
            response = await client.PutAsync(url, contentSend);
            await CheckOk(response);
        }
        #endregion

        #region EmployeeCompany
        [Theory, TestPriority(1, TestType.Get)]
        [InlineData("6")]
        [InlineData("10")]
        public async Task GetCompanyInfoByIdForEmployee(string id)
        {
            var client = await GetAuthorizedHttpClient("employee");
            //await RefreshToken("/api/companypanel", client);

            string url = "/api/companypanel/" + id;

            var response = await client.GetAsync(url);
            var content = await response.Content.ReadFromJsonAsync<Company>();
            await CheckOk(response);
            Assert.Equal(id, content.Id.ToString());
        }
        [Theory, TestPriority(1, TestType.Get)]
        [InlineData("4")]
        [InlineData("8")]
        public async Task GetCompanyInfoByIdForEmployeeIncorrect(string id)
        {
            var client = await GetAuthorizedHttpClient("employee");
            //await RefreshToken("/api/companypanel", client);

            string url = "/api/companypanel/" + id;

            var response = await client.GetAsync(url);
            Assert.Equal(System.Net.HttpStatusCode.Forbidden, response.StatusCode);
        }
        #endregion

        #region AdminCompany
        [Fact, TestPriority(1, TestType.Get)]
        public async Task GetCompaniesForAdmin()
        {
            var client = await GetAuthorizedHttpClient("admin");
            var tableobject = new TableParamsModel() { PageSize = 25, PageNumber = 0, Filters = new List<FilterModel> { } };
            var query = "?query=" + Encoding64.Base64Encode(JsonConvert.SerializeObject(tableobject));
            var url = "api/companypanel" + query;
            var response = await client.GetAsync(url);
            await CheckOk(response);
            var responseContent = await response.Content.ReadFromJsonAsync<TableViewModel<CompanyViewModel>>();
            Assert.True(responseContent.elements.Count() == 25);
        }
        [Fact, TestPriority(1, TestType.Get)]
        public async Task GetCompaniesForAdminWithFilters()
        {
            var client = await GetAuthorizedHttpClient("admin");
            var tableobject = new TableParamsModel() { PageSize = 25, PageNumber = 0, Filters = new List<FilterModel> { new FilterModel() { Name = "Name", Type = "string", Value = "a" } } };
            var query = "?query=" + Encoding64.Base64Encode(JsonConvert.SerializeObject(tableobject));
            var url = "api/companypanel" + query;
            var response = await client.GetAsync(url);
            await CheckOk(response);
            var responseContent = await response.Content.ReadFromJsonAsync<TableViewModel<CompanyViewModel>>();
            Assert.True(responseContent.elements.Count() > 0);
        }
        [Fact, TestPriority(1, TestType.Insert)]
        public async Task InsertCompanyByAdmin()
        {
            var client = await GetAuthorizedHttpClient("admin");
            var url = "api/companypanel";

            var model = new CompanyViewModel()
            {
                Name = "test",
                Country = "Poland",
                City = "Kraków",
                Postoffice = "Kraków",
                Postalcode = "30-111",
                Street = "Poliszynela",
                Housenumber = 1,
                ApartamentNumber = "2",
                Nip = "1234561288",
                CreationPlace = "Kraków",
                InvoiceDescription = "",
                DefaultMAAccount = "1",
                DefaultMAVatAccount = "2",
                DefaultWNAAccount = "3"
            };

            var content = JsonContent.Create(model);
            var response = await client.PostAsync(url, content);
            await CheckOk(response);
            var responseData = await response.Content.ReadFromJsonAsync<OperationSuccesfullViewModel<CompanyViewModel>>();
            Assert.Equal(model.Name, responseData.Data.Name);
        }
        [Theory, TestPriority(1, TestType.Edit)]
        [InlineData("4")]
        public async Task EditCompanyForAdmin(string id)
        {
            var client = await GetAuthorizedHttpClient("admin");


            string url = "/api/companypanel/" + id;

            var response = await client.GetAsync(url);
            await CheckOk(response);
            var content = await response.Content.ReadFromJsonAsync<CompanyViewModel>();
            
            Assert.InRange(content.Id, -1, long.MaxValue);

            content.Name = "Test Name " + DateTime.Now.Day;
            content.Street = "test street";
            content.Postoffice = "Kraków";
            content.InvoiceDescription = "test invoice description";

            await ActivateToken(client);

            var contentSend = JsonContent.Create(content);
            response = await client.PutAsync(url, contentSend);
            await CheckOk(response);
        }
        [Fact, TestPriority(1, TestType.Remove)]
        public async Task RemoveCompany()
        {
            var client = await GetAuthorizedHttpClient("admin");
            var url = "api/companypanel";

            var model = new CompanyViewModel()
            {
                Name = "test",
                Country = "Poland",
                City = "Kraków",
                Postoffice = "Kraków",
                Postalcode = "30-111",
                Street = "Poliszynela",
                Housenumber = 1,
                ApartamentNumber = "2",
                Nip = "1234561288",
                CreationPlace = "Kraków",
                InvoiceDescription = "",
                DefaultMAAccount = "1",
                DefaultMAVatAccount = "2",
                DefaultWNAAccount = "3"
            };

            var content = JsonContent.Create(model);
            var response = await client.PostAsync(url, content);
            await CheckOk(response);
            var responseData = await response.Content.ReadFromJsonAsync<OperationSuccesfullViewModel<CompanyViewModel>>();
            Assert.Equal(model.Name, responseData.Data.Name);

            url = "api/companypanel/" + responseData.Data.Id;
            response = await client.DeleteAsync(url);
            await CheckOk(response);
        }
        #endregion

        #region ClientPaymentMethod
        [Fact, TestPriority(2, TestType.Get)]
        public async Task GetCompanyPaymentMethodInfoForClient()
        {
            var client = await GetAuthorizedHttpClient("clientII");


            string url = "/api/companypanel";
            var response = await client.GetAsync(url);
            await CheckOk(response);
            var content = await response.Content.ReadFromJsonAsync<TableViewModel<CompanyViewModel>>();
            var company = content.elements.First();

            url = "/api/companypanel/" + company.Id + "/paymentmethod";
            response = await client.GetAsync(url);
            await CheckOk(response);
            var content2 = await response.Content.ReadFromJsonAsync<TableViewModel<PaymentMethodDictionary>>();

            Assert.True(content2.elements.Count() >= 0);
        }
        [Fact, TestPriority(2, TestType.Get)]
        public async Task GetCompanyPaymentMethodWithQueryInfoForClient()
        {
            var client = await GetAuthorizedHttpClient("clientII");

            string url = "/api/companypanel";
            var response = await client.GetAsync(url);
            await CheckOk(response);
            var content = await response.Content.ReadFromJsonAsync<TableViewModel<CompanyViewModel>>();
            var company = content.elements.First();

            url = "/api/companypanel/" + company.Id + "/paymentmethod?pageNumber2&pageSize=1";

            response = await client.GetAsync(url);
            await CheckOk(response);

            var content2 = await response.Content.ReadFromJsonAsync<TableViewModel<PaymentMethodDictionary>>();

            Assert.True(content.elements.Count() > 0);
            Assert.Equal("PLN", content2.elements.ElementAt(0).Currency);
        }
        [Fact, TestPriority(2, TestType.Insert)]
        public async Task InsertCompanyPaymentMethodForClient()
        {
            var client = await GetAuthorizedHttpClient("clientII");

            string url = "/api/companypanel";
            var response = await client.GetAsync(url);
            await CheckOk(response);
            var content = await response.Content.ReadFromJsonAsync<TableViewModel<CompanyViewModel>>();
            var company = content.elements.First();

            url = "/api/companypanel/" + company.Id + "/paymentmethod";

            var model = new PaymentMethodViewModel()
            {
                Name = "Test Payment Method",
                Currency = "PLN",
                AccountNumber = "1234567890"
            };

            var content2 = JsonContent.Create(model);
            response = await client.PostAsync(url, content2);
            await CheckOk(response);
            var responseContent = await response.Content.ReadFromJsonAsync<OperationSuccesfullViewModel<PaymentMethodDictionary>>();

            Assert.Equal(model.Name,responseContent.Data.Name);
        }
        [Fact, TestPriority(2, TestType.Edit)]
        public async Task EditCompanyPaymentMethodForClient()
        {
            await InsertCompanyPaymentMethodForClient();

            var client = await GetAuthorizedHttpClient("clientII");
            string url = "/api/companypanel";
            var response = await client.GetAsync(url);
            await CheckOk(response);
            var content = await response.Content.ReadFromJsonAsync<TableViewModel<CompanyViewModel>>();
            var company = content.elements.First();

            url = "/api/companypanel/" + company.Id + "/paymentmethod";
            response = await client.GetAsync(url);
            await CheckOk(response);
            var content2 = await response.Content.ReadFromJsonAsync<TableViewModel<PaymentMethodDictionary>>();
            Assert.True(content2.elements.Count() > 0);
            var paymentMethod = content2.elements.Last();


            url = "/api/companypanel/paymentmethod/" + paymentMethod.Id;

            var model = new PaymentMethodViewModel()
            {
                Name = paymentMethod.Name + "t",
                Currency = paymentMethod.Currency,
                AccountNumber = paymentMethod.AccountNumber + "1"
            };

            var postContent = JsonContent.Create(model);
            response = await client.PutAsync(url, postContent);
            await CheckOk(response);
            var responseContent = await response.Content.ReadFromJsonAsync<OperationSuccesfullViewModel<PaymentMethodDictionary>>();

            Assert.Equal(model.Name, responseContent.Data.Name);
        }
        [Fact, TestPriority(2, TestType.Remove)]
        public async Task RemoveCompanyPaymentMethodForClient()
        {
            await InsertCompanyPaymentMethodForClient();
            var client = await GetAuthorizedHttpClient("clientII");
            string url = "/api/companypanel";
            var response = await client.GetAsync(url);
            await CheckOk(response);
            var content = await response.Content.ReadFromJsonAsync<TableViewModel<CompanyViewModel>>();
            var company = content.elements.First();
            url = "/api/companypanel/" + company.Id + "/paymentmethod";
            response = await client.GetAsync(url);
            await CheckOk(response);
            var content2 = await response.Content.ReadFromJsonAsync<TableViewModel<PaymentMethodDictionary>>();
            Assert.True(content.elements.Count() > 0);
            var paymentMethod = content2.elements.Last();
            url = "/api/companypanel/paymentmethod/" + paymentMethod.Id;

            response = await client.DeleteAsync(url);
            await CheckOk(response);
            var responseContent = await response.Content.ReadFromJsonAsync<OperationSuccesfullViewModel<PaymentMethodDictionary>>();
            Assert.Equal(paymentMethod.Id, responseContent.Data.Id);
        }
        #endregion

        #region AdminPaymentMethod
        [Fact, TestPriority(3, TestType.Get)]
        public async Task GetCompaniesPaymentMethodsFormAdmin()
        {
            var client = await GetAuthorizedHttpClient("admin");
            var tableobject = new TableParamsModel() { PageSize = 25, PageNumber = 0, Sort = "created", Filters = new List<FilterModel> {  } };
            var query = "?query=" + Encoding64.Base64Encode(JsonConvert.SerializeObject(tableobject));
            var url = "api/companypanel" + query;
            var response = await client.GetAsync(url);
            await CheckOk(response);
            var responseContent = await response.Content.ReadFromJsonAsync<TableViewModel<CompanyViewModel>>();
            Assert.True(responseContent.elements.Count() == 25);

            var company = responseContent.elements.Last();
            tableobject = new TableParamsModel() { PageSize = 25, PageNumber = 0, Sort = "created", Filters = new List<FilterModel> { } };
            query = "?query=" + Encoding64.Base64Encode(JsonConvert.SerializeObject(tableobject));
            url = "api/companypanel/" + company.Id + "/paymentmethod" + query;
            response = await client.GetAsync(url);
            await CheckOk(response);
        }
        [Fact, TestPriority(3, TestType.Insert)]
        public async Task InsertCompanyPaymentMethodByAdmin()
        {
            var client = await GetAuthorizedHttpClient("admin");
            var query = "?pageSize=25";
            var url = "api/companypanel" + query;
            var response = await client.GetAsync(url);
            await CheckOk(response);
            var responseContent = await response.Content.ReadFromJsonAsync<TableViewModel<CompanyViewModel>>();
            Assert.True(responseContent.elements.Count() == 25);
            var company = responseContent.elements.Last();

            
            url = "/api/companypanel/" + company.Id + "/paymentmethod";
            var model = new PaymentMethodViewModel()
            {
                Name = "Test Payment Method",
                Currency = "PLN",
                AccountNumber = "1234567890"
            };
            var content = JsonContent.Create(model);
            response = await client.PostAsync(url, content);
            await CheckOk(response);
            var responseContent2 = await response.Content.ReadFromJsonAsync<OperationSuccesfullViewModel<PaymentMethodDictionary>>();
            Assert.Equal(model.Name, responseContent2.Data.Name);
        }
        [Fact, TestPriority(3, TestType.Edit)]
        public async Task EditCompanyPaymentMethodByAdmin()
        {
            await InsertCompanyPaymentMethodByAdmin();
            var client = await GetAuthorizedHttpClient("admin");
            var tableobject = new TableParamsModel() { PageSize = 25, PageNumber = 0, Sort="created", Filters = new List<FilterModel> { } };
            var query = "?query=" + Encoding64.Base64Encode(JsonConvert.SerializeObject(tableobject));
            var url = "api/companypanel" + query;
            var response = await client.GetAsync(url);
            await CheckOk(response);
            var responseContent = await response.Content.ReadFromJsonAsync<TableViewModel<Company>>();
            Assert.True(responseContent.elements.Count() == 25);
            var company = responseContent.elements.Last();

            tableobject = new TableParamsModel() { PageSize = 25, PageNumber = 0, Sort = "created", Filters = new List<FilterModel> { } };
            query = "?query=" + Encoding64.Base64Encode(JsonConvert.SerializeObject(tableobject));
            url = "api/companypanel/" + company.Id + "/paymentmethod" + query;
            response = await client.GetAsync(url);
            await CheckOk(response);
            var responseContent2 = await response.Content.ReadFromJsonAsync<TableViewModel<PaymentMethodDictionary>>();
            var paymentMethod = responseContent2.elements.Last();

            url = "api/companypanel/paymentmethod/" + paymentMethod.Id;
            var model = new PaymentMethodViewModel()
            {
                Name = paymentMethod.Name + "t",
                Currency = paymentMethod.Currency,
                AccountNumber = paymentMethod.AccountNumber + "1"
            };
            var postContent = JsonContent.Create(model);
            response = await client.PutAsync(url, postContent);
            await CheckOk(response);
            var responseContent3 = await response.Content.ReadFromJsonAsync<OperationSuccesfullViewModel<PaymentMethodDictionary>>();

            Assert.Equal(model.Name, responseContent3.Data.Name);
        }
        [Fact, TestPriority(3, TestType.Remove)]
        public async Task RemoveCompanyPaymentMethodByAdmin()
        {
            await InsertCompanyPaymentMethodByAdmin();
            var client = await GetAuthorizedHttpClient("admin");
            var query = "?pageSize=25";
            var url = "api/companypanel" + query;
            var response = await client.GetAsync(url);
            await CheckOk(response);
            var responseContent = await response.Content.ReadFromJsonAsync<TableViewModel<CompanyViewModel>>();
            Assert.True(responseContent.elements.Count() == 25);
            var company = responseContent.elements.Last();

            url = "api/companypanel/" + company.Id + "/paymentmethod";
            response = await client.GetAsync(url);
            await CheckOk(response);
            var responseContent2 = await response.Content.ReadFromJsonAsync<TableViewModel<PaymentMethodDictionary>>();
            var paymentMethod = responseContent2.elements.Last();


            url = "api/companypanel/paymentmethod/" + paymentMethod.Id;

            response = await client.DeleteAsync(url);
            await CheckOk(response);

            var responseContent3 = await response.Content.ReadFromJsonAsync<OperationSuccesfullViewModel<PaymentMethodDictionary>>();

            Assert.Equal(paymentMethod.Id, responseContent3.Data.Id);
        }
        #endregion
    }
}

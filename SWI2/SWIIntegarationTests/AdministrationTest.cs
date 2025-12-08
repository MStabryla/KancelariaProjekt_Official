using Microsoft.AspNetCore.Mvc.Testing;
using SWI2.Models;
using SWI2.Models.Administrator;
using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using System.Linq;
using SWI2.Models.Authentication;
using SWI2.Models.Response;
using SWI2.Models.Users;
using SWI2.Models.Company;
using Newtonsoft.Json;
using SWI2.Services.Static;

namespace SWIIntegarationTests
{
    [TestCaseOrderer("XUnit.Project.Orderers.TestPriority", "XUnit.Project")]
    public class AdministrationTest : BasicTests
    {
        public AdministrationTest(WebApplicationFactory<SWI2.Startup> factory) : base(factory)
        {

        }
        [Fact]
        public async Task GetUsers()
        {
            var client = await GetAuthorizedHttpClient("admin");
            var url = "/api/adminpanel/users";
            var response = await client.GetAsync(url);
            await CheckOk(response);
            var content = await response.Content.ReadFromJsonAsync<TableViewModel<UserViewModel>>();
            Assert.True(content.elements.Count() == 25);
            Assert.True(content.totalCount > 25);
        }
        [Theory]
        [InlineData("Administrator")]
        [InlineData("Employee")]
        [InlineData("Client")]
        public async Task GetUsersByRole(string role)
        {
            var client = await GetAuthorizedHttpClient("admin");
            var tableobject = new TableParamsModel() { PageSize = 25, PageNumber = 0, Filters = new List<FilterModel> { new FilterModel() { Name = "userRole", Type = "role", Value = role } } };
            var query = "?query=" + Encoding64.Base64Encode(JsonConvert.SerializeObject(tableobject));
            var url = "/api/adminpanel/users/" + query;
            var response = await client.GetAsync(url);
            await CheckOk(response);
            var content = await response.Content.ReadFromJsonAsync<TableViewModel<UserViewModel>>();
            Assert.True(content.elements.Count() > 0);
            Assert.True(content.totalCount > 0);
            Assert.True(content.elements.All(x => x.UserRole == role));
        }
        [Fact]
        public async Task CreateAndDeleteUser()
        {
            var client = await GetAuthorizedHttpClient("admin");
            var url = "/api/adminpanel/";

            var date = DateTime.Now;
            long number = date.Ticks;
            var model = new RegisterViewModel()
            {
                Name = "Test",
                Surname = "Admin",
                Email = "testadmin." + number + "@swi.com.pl",
                Login = "test_admin_" + number,
                Password = "zaq1@WSX",
                ConfirmPassword = "zaq1@WSX",
                ClientURI = "https://localhost:5001/authentication/emailconfirmation"
            };
            var content = JsonContent.Create(model);
            var response = await client.PostAsync(url,content);
            await CheckOk(response);
            var responseContent = await response.Content.ReadFromJsonAsync<OperationSuccesfullViewModel<string>>();
            Assert.True(responseContent.Data.Length > 0);
            var id = responseContent.Data;

            await ActivateToken(client);
            url = "/api/adminpanel/" + id;
            response = await client.DeleteAsync(url);
            await CheckOk(response);
            var responseContent2 = await response.Content.ReadFromJsonAsync<OperationSuccesfullViewModel<UserViewModel>>();
            Assert.Equal("Guest",responseContent2.Data.UserRole);
        }
        [Fact]
        public async Task ActivateAndLockUser()
        {
            var client = await GetAuthorizedHttpClient("admin");
            var url = "/api/adminpanel/";

            var date = DateTime.Now;
            long number = date.Ticks;
            var model = new RegisterViewModel()
            {
                Name = "Test",
                Surname = "Admin",
                Email = "testadmin." + number + "@swi.com.pl",
                Login = "test_admin_" + number,
                Password = "zaq1@WSX",
                ConfirmPassword = "zaq1@WSX",
                ClientURI = "https://localhost:5001/authentication/emailconfirmation"
            };
            var content = JsonContent.Create(model);
            var response = await client.PostAsync(url, content);
            await CheckOk(response);
            var responseContent = await response.Content.ReadFromJsonAsync<OperationSuccesfullViewModel<string>>();
            Assert.True(responseContent.Data.Length > 0);
            var id = responseContent.Data;

            await ActivateToken(client);
            url = "/api/adminpanel/" + id + "/activate";
            response = await client.PostAsync(url, null);
            await CheckOk(response);

            await ActivateToken(client);
            url = "/api/adminpanel/" + id + "/lock";
            response = await client.PostAsync(url, null);
            await CheckOk(response);

            //Clearing database
            await ActivateToken(client);
            url = "/api/adminpanel/" + id;
            response = await client.DeleteAsync(url);
            await CheckOk(response);
        }

        [Fact]
        public async Task TryLoginOnNewUser()
        {
            var client = await GetAuthorizedHttpClient("admin");
            var url = "/api/adminpanel/";

            var date = DateTime.Now;
            long number = date.Ticks;
            var model = new RegisterViewModel()
            {
                Name = "Test",
                Surname = "Admin",
                Email = "testadmin." + number + "@swi.com.pl",
                Login = "test_admin_" + number,
                Password = "zaq1@WSX",
                ConfirmPassword = "zaq1@WSX",
                ClientURI = "https://localhost:5001/authentication/emailconfirmation"
            };
            var content = JsonContent.Create(model);
            var response = await client.PostAsync(url, content);
            await CheckOk(response);
            var responseContent = await response.Content.ReadFromJsonAsync<OperationSuccesfullViewModel<string>>();
            Assert.True(responseContent.Data.Length > 0);
            var id = responseContent.Data;

            await ActivateToken(client);
            url = "/api/adminpanel/" + id + "/activate";
            response = await client.PostAsync(url, null);
            await CheckOk(response);

            url = "api/authentication/login";
            var clientHttp = await GetHttpClient();
            var loginModel = new LoginViewModel()
            {
                Login = model.Login,
                Password = SWI2.Services.Static.Encoding64.Base64Encode("zaq1@WSX")

            };
            content = JsonContent.Create(loginModel);
            response = await clientHttp.PostAsync(url, content);
            await CheckOk(response);

            //Clearing database
            await ActivateToken(client);
            url = "/api/adminpanel/" + id + "/lock";
            response = await client.PostAsync(url, null);
            await CheckOk(response);
            
            await ActivateToken(client);
            url = "/api/adminpanel/" + id;
            response = await client.DeleteAsync(url);
            await CheckOk(response);
        }

        [Fact]
        public async Task TryLoginOnNewUserFalse()
        {
            var client = await GetAuthorizedHttpClient("admin");
            var url = "/api/adminpanel/";

            var date = DateTime.Now;
            long number = date.Ticks;
            var model = new RegisterViewModel()
            {
                Name = "Test",
                Surname = "Admin",
                Email = "testadmin." + number + "@swi.com.pl",
                Login = "test_admin_" + number,
                Password = "zaq1@WSX",
                ConfirmPassword = "zaq1@WSX",
                ClientURI = "https://localhost:5001/authentication/emailconfirmation"
            };
            var content = JsonContent.Create(model);
            var response = await client.PostAsync(url, content);
            await CheckOk(response);
            var responseContent = await response.Content.ReadFromJsonAsync<OperationSuccesfullViewModel<string>>();
            Assert.True(responseContent.Data.Length > 0);
            var id = responseContent.Data;

            url = "api/authentication/login";
            var clientHttp = await GetHttpClient();
            var loginModel = new LoginViewModel()
            {
                Login = model.Login,
                Password = SWI2.Services.Static.Encoding64.Base64Encode("zaq1@WSX")
                
            };
            content = JsonContent.Create(loginModel);
            response = await clientHttp.PostAsync(url, content);
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);

            //Clearing database
            await ActivateToken(client);
            url = "/api/adminpanel/" + id;
            response = await client.DeleteAsync(url);
            await CheckOk(response);
        }

        [Fact]
        public async Task AddClient()
        {
            var client = await GetAuthorizedHttpClient("admin");
            var url = "/api/adminpanel/";

            var date = DateTime.Now;
            long number = date.Ticks;
            var model = new RegisterViewModel()
            {
                Name = "Test",
                Surname = "Admin",
                Email = "testadmin." + number + "@swi.com.pl",
                Login = "test_admin_" + number,
                Password = "zaq1@WSX",
                ConfirmPassword = "zaq1@WSX",
                ClientURI = "https://localhost:5001/authentication/emailconfirmation"
            };
            var content = JsonContent.Create(model);
            var response = await client.PostAsync(url, content);
            await CheckOk(response);
            var responseContent = await response.Content.ReadFromJsonAsync<OperationSuccesfullViewModel<string>>();
            Assert.True(responseContent.Data.Length > 0);
            var id = responseContent.Data;

            await ActivateToken(client);
            url = "/api/clientpanel/" + id;
            response = await client.PostAsync(url, null);
            await CheckOk(response);
            var responseContent2 = await response.Content.ReadFromJsonAsync<OperationSuccesfullViewModel<ClientViewModel>>();
            Assert.Equal(id, responseContent2.Data.UserId);

            //Clearing database
            await ActivateToken(client);
            url = "/api/adminpanel/" + id;
            response = await client.DeleteAsync(url);
            await CheckOk(response);
        }

        [Fact]
        public async Task AddClientToConcreteCompany()
        {
            var client = await GetAuthorizedHttpClient("admin");
            var url = "/api/adminpanel/";

            var date = DateTime.Now;
            long number = date.Ticks;
            var model = new RegisterViewModel()
            {
                Name = "Test",
                Surname = "Admin",
                Email = "testadmin." + number + "@swi.com.pl",
                Login = "test_admin_" + number,
                Password = "zaq1@WSX",
                ConfirmPassword = "zaq1@WSX",
                ClientURI = "https://localhost:5001/authentication/emailconfirmation"
            };
            var content = JsonContent.Create(model);
            var response = await client.PostAsync(url, content);
            await CheckOk(response);
            var responseContent = await response.Content.ReadFromJsonAsync<OperationSuccesfullViewModel<string>>();
            Assert.True(responseContent.Data.Length > 0);
            var id = responseContent.Data;

            await ActivateToken(client);
            url = "/api/clientpanel/" + id;
            response = await client.PostAsync(url, null);
            await CheckOk(response);
            var responseContent2 = await response.Content.ReadFromJsonAsync<OperationSuccesfullViewModel<ClientViewModel>>();
            Assert.Equal(id, responseContent2.Data.UserId);

            await ActivateToken(client);
            url = "api/companypanel";
            var model2 = new CompanyViewModel()
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
            var content2 = JsonContent.Create(model2);
            response = await client.PostAsync(url, content2);
            await CheckOk(response);
            var responseContent3 = await response.Content.ReadFromJsonAsync<OperationSuccesfullViewModel<CompanyViewModel>>();

            await ActivateToken(client);
            url = "api/clientpanel/" + id + "/" + responseContent3.Data.Id;
            response = await client.PostAsync(url, null);
            await CheckOk(response);
            var responseContent4 = await response.Content.ReadFromJsonAsync<OperationSuccesfullViewModel<ClientCompanyViewModel>>();
            Assert.Equal(responseContent3.Data.Id, responseContent4.Data.CompanyId);

            //Clearing database
            await ActivateToken(client);
            url = "/api/adminpanel/" + id;
            response = await client.DeleteAsync(url);
            await CheckOk(response);

            url = "api/companypanel/" + responseContent3.Data.Id;
            response = await client.DeleteAsync(url);
            await CheckOk(response);
        }

        [Fact]
        public async Task SetClientAsBoardMember()
        {
            var client = await GetAuthorizedHttpClient("admin");
            var url = "/api/adminpanel/";

            var date = DateTime.Now;
            long number = date.Ticks;
            var model = new RegisterViewModel()
            {
                Name = "Test",
                Surname = "Admin",
                Email = "testadmin." + number + "@swi.com.pl",
                Login = "test_admin_" + number,
                Password = "zaq1@WSX",
                ConfirmPassword = "zaq1@WSX",
                ClientURI = "https://localhost:5001/authentication/emailconfirmation"
            };
            var content = JsonContent.Create(model);
            var response = await client.PostAsync(url, content);
            await CheckOk(response);
            var responseContent = await response.Content.ReadFromJsonAsync<OperationSuccesfullViewModel<string>>();
            Assert.True(responseContent.Data.Length > 0);
            var id = responseContent.Data;

            await ActivateToken(client);
            url = "/api/clientpanel/" + id;
            response = await client.PostAsync(url, null);
            await CheckOk(response);
            var responseContent2 = await response.Content.ReadFromJsonAsync<OperationSuccesfullViewModel<ClientViewModel>>();
            Assert.Equal(id, responseContent2.Data.UserId);

            await ActivateToken(client);
            url = "api/companypanel";
            var model2 = new CompanyViewModel()
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
            var content2 = JsonContent.Create(model2);
            response = await client.PostAsync(url, content2);
            await CheckOk(response);
            var responseContent3 = await response.Content.ReadFromJsonAsync<OperationSuccesfullViewModel<CompanyViewModel>>();

            await ActivateToken(client);
            url = "api/clientpanel/" + id + "/" + responseContent3.Data.Id;
            response = await client.PostAsync(url, null);
            await CheckOk(response);
            var responseContent4 = await response.Content.ReadFromJsonAsync<OperationSuccesfullViewModel<ClientCompanyViewModel>>();
            Assert.Equal(responseContent3.Data.Id, responseContent4.Data.CompanyId);

            await ActivateToken(client);
            url = "api/clientpanel/" + id + "/" + responseContent3.Data.Id;
            response = await client.PutAsync(url, null);
            await CheckOk(response);
            var responseContent5 = await response.Content.ReadFromJsonAsync<OperationSuccesfullViewModel<ClientCompanyViewModel>>();
            Assert.Equal(responseContent3.Data.Id, responseContent5.Data.CompanyId);

            //Clearing database
            await ActivateToken(client);
            url = "/api/adminpanel/" + id;
            response = await client.DeleteAsync(url);
            await CheckOk(response);

            url = "api/companypanel/" + responseContent3.Data.Id;
            response = await client.DeleteAsync(url);
            await CheckOk(response);
        }

        [Fact]
        public async Task AddEmployee()
        {
            var client = await GetAuthorizedHttpClient("admin");
            var url = "/api/adminpanel/";

            var date = DateTime.Now;
            long number = date.Ticks;
            var model = new RegisterViewModel()
            {
                Name = "Test",
                Surname = "Admin",
                Email = "testadmin." + number + "@swi.com.pl",
                Login = "test_admin_" + number,
                Password = "zaq1@WSX",
                ConfirmPassword = "zaq1@WSX",
                ClientURI = "https://localhost:5001/authentication/emailconfirmation"
            };
            var content = JsonContent.Create(model);
            var response = await client.PostAsync(url, content);
            await CheckOk(response);
            var responseContent = await response.Content.ReadFromJsonAsync<OperationSuccesfullViewModel<string>>();
            Assert.True(responseContent.Data.Length > 0);
            var id = responseContent.Data;

            await ActivateToken(client);
            url = "/api/employeepanel/" + id;
            response = await client.PostAsync(url, null);
            await CheckOk(response);
            var responseContent2 = await response.Content.ReadFromJsonAsync<OperationSuccesfullViewModel<EmployeeViewModel>>();
            Assert.Equal(id, responseContent2.Data.UserId);

            //Clearing database
            await ActivateToken(client);
            url = "/api/adminpanel/" + id;
            response = await client.DeleteAsync(url);
            await CheckOk(response);
        }

        [Fact]
        public async Task AddAndRemoveEmployeeToConcreteCompany()
        {
            var client = await GetAuthorizedHttpClient("admin");
            var url = "/api/adminpanel/";

            //Tworzenie użytkownika
            var date = DateTime.Now;
            long number = date.Ticks;
            var model = new RegisterViewModel()
            {
                Name = "Test",
                Surname = "Admin",
                Email = "testadmin." + number + "@swi.com.pl",
                Login = "test_admin_" + number,
                Password = "zaq1@WSX",
                ConfirmPassword = "zaq1@WSX",
                ClientURI = "https://localhost:5001/authentication/emailconfirmation"
            };
            var content = JsonContent.Create(model);
            var response = await client.PostAsync(url, content);
            await CheckOk(response);
            var responseContent = await response.Content.ReadFromJsonAsync<OperationSuccesfullViewModel<string>>();

            Assert.True(responseContent.Data.Length > 0);
            var id = responseContent.Data;

            //Nadanie roli pracownika
            await ActivateToken(client);
            url = "/api/employeepanel/" + id;
            response = await client.PostAsync(url, null);
            await CheckOk(response);
            var responseContent2 = await response.Content.ReadFromJsonAsync<OperationSuccesfullViewModel<EmployeeViewModel>>();
            Assert.Equal(id, responseContent2.Data.UserId);

            //Tworzenie firmy
            await ActivateToken(client);
            url = "api/companypanel";
            var model2 = new CompanyViewModel()
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
            var content2 = JsonContent.Create(model2);
            response = await client.PostAsync(url, content2);
            await CheckOk(response);
            var responseContent3 = await response.Content.ReadFromJsonAsync<OperationSuccesfullViewModel<CompanyViewModel>>();

            //Stworzenie departamentu firmy
            await ActivateToken(client);
            url = "api/departmentpanel/" + responseContent3.Data.Id;
            var model3 = new DepartmentViewModel()
            {
                Name = "Test Department",
                FolderName = "test",
                Type = 1
            };
            content = JsonContent.Create(model3);
            response = await client.PostAsync(url, content);
            await CheckOk(response);
            var responseContent4 = await response.Content.ReadFromJsonAsync<OperationSuccesfullViewModel<DepartmentViewModel>>();

            //Przypisanie dostępu do departamentu
            await ActivateToken(client);
            url = "api/employeepanel/access/" + id + "/" + responseContent4.Data.Id;
            response = await client.PostAsync(url, null);
            await CheckOk(response);
            var responseContent6 = await response.Content.ReadFromJsonAsync<OperationSuccesfullViewModel<EmployeeDepartmentViewModel>>();
            Assert.Equal(responseContent4.Data.Id, responseContent6.Data.DepartmentId);

            //Wycofanie dostępu do departamentu
            await ActivateToken(client);
            url = "api/employeepanel/access/" + id + "/" + responseContent4.Data.Id;
            response = await client.DeleteAsync(url);
            await CheckOk(response);
            var responseContent7 = await response.Content.ReadFromJsonAsync<OperationSuccesfullViewModel<EmployeeDepartmentViewModel>>();
            Assert.Equal(responseContent4.Data.Id, responseContent7.Data.DepartmentId);

            //Clearing database
            await ActivateToken(client);
            url = "api/departmentpanel/" + responseContent4.Data.Id;
            response = await client.DeleteAsync(url);
            await CheckOk(response);

            await ActivateToken(client);
            url = "/api/adminpanel/" + id;
            response = await client.DeleteAsync(url);
            await CheckOk(response);

            await ActivateToken(client);
            url = "api/companypanel/" + responseContent3.Data.Id;
            response = await client.DeleteAsync(url);
            await CheckOk(response);
        }

        [Fact]
        public async Task GetAccessableDepartmentsByEmployee()
        {
            var client = await GetAuthorizedHttpClient("employee");
            var url = "/api/employeepanel/access";
            var response = await client.GetAsync(url);
            await CheckOk(response);
            var responseContent = await response.Content.ReadFromJsonAsync<IEnumerable<GroupedDepartmentViewModel>>();
            Assert.True(responseContent.Count() > 0);
            Assert.True(responseContent.All(x => x.Deparments.Count() > 0));
        }
        [Fact]
        public async Task GetAccessableDepartmentsByAdmin()
        {
            var client = await GetAuthorizedHttpClient("employee");
            var url = "/api/user";
            var response = await client.GetAsync(url);
            await CheckOk(response);
            var responseContent = await response.Content.ReadFromJsonAsync<UserDetailsViewModel>();
            var id = responseContent.UserId;

            var adminclient = await GetAuthorizedHttpClient("admin");
            url = "/api/employeepanel/access/" + id;
            response = await adminclient.GetAsync(url);
            await CheckOk(response);
            var responseContent2 = await response.Content.ReadFromJsonAsync<IEnumerable<GroupedDepartmentViewModel>>();
            Assert.True(responseContent2.Count() > 0);
            Assert.True(responseContent2.All(x => x.Deparments.Count() > 0));
        }
    }
}

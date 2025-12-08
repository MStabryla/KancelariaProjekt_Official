using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using SWI2.Models;
using SWI2.Models.Company;
using SWI2.Models.Response;
using SWI2.Services.Static;
using SWI2DB.Models.Company;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using XUnit.Project.Attributes;

namespace SWIIntegarationTests
{
    [TestCaseOrderer("XUnit.Project.Orderers.TestPriority", "XUnit.Project")]
    public class DepartmentTest : BasicTests
    {
        public DepartmentTest(WebApplicationFactory<SWI2.Startup> factory) : base(factory)
        {

        }
        [Fact, TestPriority(0, TestType.Get)]
        public async Task GetDepartmentsByClient()
        {
            var client = await GetAuthorizedHttpClient("clientII");
            string url = "/api/companypanel";
            var response = await client.GetAsync(url);
            var content = await response.Content.ReadFromJsonAsync<TableViewModel<CompanyViewModel>>();
            var company = content.elements.First();

            url = "api/departmentpanel/company/" + company.Id;
            response = await client.GetAsync(url);
            await CheckOk(response);
            var responseContent = await response.Content.ReadFromJsonAsync<TableViewModel<DepartmentViewModel>>();
            Assert.True(responseContent.elements.Count() > 0);
        }
        [Fact, TestPriority(0, TestType.Get)]
        public async Task GetDepartmentByClient()
        {
            var client = await GetAuthorizedHttpClient("clientII");
            string url = "/api/companypanel";
            var response = await client.GetAsync(url);
            await CheckOk(response);
            var content = await response.Content.ReadFromJsonAsync<TableViewModel<CompanyViewModel>>();
            var company = content.elements.First();

            url = "api/departmentpanel/company/" + company.Id;
            response = await client.GetAsync(url);
            await CheckOk(response);
            var responseContent = await response.Content.ReadFromJsonAsync<TableViewModel<DepartmentViewModel>>();
            var department = responseContent.elements.First();

            await ActivateToken(client);
            url = "api/departmentpanel/" + department.Id;
            response = await client.GetAsync(url);
            await CheckOk(response);


            var responseContent2 = await response.Content.ReadFromJsonAsync<DepartmentViewModel>();
            Assert.Equal(department.Id, responseContent2.Id);
        }
        [Fact, TestPriority(0, TestType.Insert)]
        public async Task InsertDepartmentByClient()
        {
            var client = await GetAuthorizedHttpClient("clientII");
            string url = "/api/companypanel";
            var response = await client.GetAsync(url);
            await CheckOk(response);
            var content = await response.Content.ReadFromJsonAsync<TableViewModel<CompanyViewModel>>();
            var company = content.elements.First();

            url = "api/departmentpanel/" + company.Id;

            var model = new DepartmentViewModel()
            {
                Name = "Test Department",
                FolderName = "test_" + DateTime.Now.Ticks,
                Type = 1
            };
            var content2 = JsonContent.Create(model);
            response = await client.PostAsync(url, content2);
            await CheckOk(response);
            var responseContent = await response.Content.ReadFromJsonAsync<OperationSuccesfullViewModel<DepartmentViewModel>>();
            Assert.Equal(model.Name, responseContent.Data.Name);
            var department = responseContent.Data;

            await ActivateToken(client);
            url = "api/departmentpanel/" + department.Id;
            response = await client.DeleteAsync(url);
            await CheckOk(response);
        }
        [Fact, TestPriority(0, TestType.Edit)]
        public async Task EditDepartmentByClient()
        {
            await InsertDepartmentByClient();
            var client = await GetAuthorizedHttpClient("clientII");
            string url = "/api/companypanel";
            var response = await client.GetAsync(url);
            await CheckOk(response);
            var content = await response.Content.ReadFromJsonAsync<TableViewModel<CompanyViewModel>>();
            var company = content.elements.First();

            url = "api/departmentpanel/company/" + company.Id;
            response = await client.GetAsync(url);
            await CheckOk(response);
            var responseContent = await response.Content.ReadFromJsonAsync<TableViewModel<DepartmentViewModel>>();
            var department = responseContent.elements.First();

            await ActivateToken(client);
            url = "api/departmentpanel/" + department.Id;
            var model = new DepartmentViewModel()
            {
                Name = department.Name + "t",
                FolderName = department.FolderName,
                Type = department.Type
            };
            var content2 = JsonContent.Create(model);
            response = await client.PutAsync(url, content2);
            await CheckOk(response);
            var responseContent2 = await response.Content.ReadFromJsonAsync<OperationSuccesfullViewModel<DepartmentViewModel>>();
            Assert.Equal(department.Id, responseContent2.Data.Id);
            Assert.Equal(model.Name, responseContent2.Data.Name);

            await ActivateToken(client);
            url = "api/departmentpanel/" + department.Id;
            response = await client.DeleteAsync(url);
            await CheckOk(response);
        }
        [Fact, TestPriority(0, TestType.Remove)]
        public async Task RemoveDepartmentByClient()
        {
            await InsertDepartmentByClient();
            var client = await GetAuthorizedHttpClient("clientII");
            string url = "/api/companypanel";
            var response = await client.GetAsync(url);
            await CheckOk(response);
            var content = await response.Content.ReadFromJsonAsync<TableViewModel<CompanyViewModel>>();
            var company = content.elements.First();

            url = "api/departmentpanel/company/" + company.Id;
            response = await client.GetAsync(url);
            await CheckOk(response);
            var responseContent = await response.Content.ReadFromJsonAsync<TableViewModel<DepartmentViewModel>>();
            var department = responseContent.elements.Last();

            await ActivateToken(client);
            url = "api/departmentpanel/" + department.Id;
            response = await client.DeleteAsync(url);
            await CheckOk(response);
            var responseContent2 = await response.Content.ReadFromJsonAsync<OperationSuccesfullViewModel<DepartmentViewModel>>();
            Assert.Equal(department.Id, responseContent2.Data.Id);
        }
        
        [Fact, TestPriority(1, TestType.Get)]
        public async Task InsertDepartmentByAdmin()
        {
            var client = await GetAuthorizedHttpClient("admin");
            var query = "?ElementsPerPage=25";
            var url = "api/companypanel" + query;
            var response = await client.GetAsync(url);
            await CheckOk(response);
            var responseContent = await response.Content.ReadFromJsonAsync<TableViewModel<CompanyViewModel>>();
            var company = responseContent.elements.Last();

            //var client = await GetAuthorizedHttpClient("admin");
            await ActivateToken(client);
            url = "api/departmentpanel/" + company.Id;
            var model = new DepartmentViewModel()
            {
                Name = "Test Department",
                FolderName = "test_" + DateTime.Now.Ticks,
                Type = 1
            };
            var content = JsonContent.Create(model);
            response = await client.PostAsync(url, content);
            await CheckOk(response);
            var responseContent2 = await response.Content.ReadFromJsonAsync<OperationSuccesfullViewModel<DepartmentViewModel>>();
            Assert.Equal(model.Name, responseContent2.Data.Name);
            var department = responseContent2.Data;

            await ActivateToken(client);
            url = "api/departmentpanel/" + department.Id;
            response = await client.DeleteAsync(url);
            await CheckOk(response);

        }
        [Fact, TestPriority(1, TestType.Get)]
        public async Task GetDepartmentsByAdmin()
        {
            var client = await GetAuthorizedHttpClient("admin");
            var query = "?ElementsPerPage=25";
            var url = "api/companypanel" + query;
            var response = await client.GetAsync(url);
            var responseContent = await response.Content.ReadFromJsonAsync<TableViewModel<CompanyViewModel>>();
            var company = responseContent.elements.Last();

            await ActivateToken(client);
            url = "api/departmentpanel/company/" + company.Id;
            response = await client.GetAsync(url);
            await CheckOk(response);
            var responseContent2 = await response.Content.ReadFromJsonAsync<TableViewModel<DepartmentViewModel>>();
            Assert.True(responseContent2.elements.Count() > 0);
        }
        [Fact, TestPriority(1, TestType.Insert)]
        public async Task GetDepartmentByAdmin()
        {
            var client = await GetAuthorizedHttpClient("admin");
            var tableobject = new TableParamsModel() { PageSize = 25, PageNumber = 0, Sort = "created", Filters = new List<FilterModel> { } };
            var query = "?query=" + Encoding64.Base64Encode(JsonConvert.SerializeObject(tableobject));
            var url = "api/companypanel" + query;
            var response = await client.GetAsync(url);
            await CheckOk(response);
            var responseContent = await response.Content.ReadFromJsonAsync<TableViewModel<CompanyViewModel>>();
            var company = responseContent.elements.Last();

            await ActivateToken(client);
            url = "api/departmentpanel/company/" + company.Id;
            response = await client.GetAsync(url);
            await CheckOk(response);
            var responseContent2 = await response.Content.ReadFromJsonAsync<TableViewModel<DepartmentViewModel>>();
            var department = responseContent2.elements.First();

            await ActivateToken(client);
            url = "api/departmentpanel/" + department.Id;
            response = await client.GetAsync(url);
            await CheckOk(response);
            var responseContent3 = await response.Content.ReadFromJsonAsync<DepartmentViewModel>();
            Assert.Equal(department.Id, responseContent3.Id);
        }
        [Fact, TestPriority(1, TestType.Edit)]
        public async Task EditDepartmentByAdmin()
        {
            await InsertDepartmentByAdmin();
            var client = await GetAuthorizedHttpClient("admin");
            var query = "?ElementsPerPage=25";
            var url = "api/companypanel" + query;
            var response = await client.GetAsync(url);
            await CheckOk(response);
            var responseContent = await response.Content.ReadFromJsonAsync<TableViewModel<CompanyViewModel>>();
            var company = responseContent.elements.Last();

            await ActivateToken(client);
            url = "api/departmentpanel/company/" + company.Id;
            response = await client.GetAsync(url);
            await CheckOk(response);
            var responseContent2 = await response.Content.ReadFromJsonAsync<TableViewModel<DepartmentViewModel>>();
            var department = responseContent2.elements.Last();

            await ActivateToken(client);
            url = "api/departmentpanel/" + department.Id;
            var model = new DepartmentViewModel()
            {
                Name = department.Name + "t",
                FolderName = department.FolderName,
                Type = department.Type
            };
            var content = JsonContent.Create(model);
            response = await client.PutAsync(url, content);
            await CheckOk(response);
            var responseContent3 = await response.Content.ReadFromJsonAsync<OperationSuccesfullViewModel<DepartmentViewModel>>();
            Assert.Equal(department.Id, responseContent3.Data.Id);
            Assert.Equal(model.Name, responseContent3.Data.Name);

            await ActivateToken(client);
            url = "api/departmentpanel/" + department.Id;
            response = await client.DeleteAsync(url);
            await CheckOk(response);
        }
        [Fact, TestPriority(1, TestType.Remove)]
        public async Task RemoveDepartmentByAdmin()
        {
            var client = await GetAuthorizedHttpClient("admin");
            var query = "?ElementsPerPage=25";
            var url = "api/companypanel" + query;
            var response = await client.GetAsync(url);
            await CheckOk(response);
            var responseContent = await response.Content.ReadFromJsonAsync<TableViewModel<CompanyViewModel>>();
            var company = responseContent.elements.Last();

            await ActivateToken(client);
            url = "api/departmentpanel/" + company.Id;
            var model = new DepartmentViewModel()
            {
                Name = "Test Department",
                FolderName = "test_" + DateTime.Now.Ticks,
                Type = 1
            };
            var content = JsonContent.Create(model);
            response = await client.PostAsync(url, content);
            await CheckOk(response);
            var responseContent2 = await response.Content.ReadFromJsonAsync<OperationSuccesfullViewModel<DepartmentViewModel>>();
            Assert.Equal(model.Name, responseContent2.Data.Name);
            var department = responseContent2.Data;

            await ActivateToken(client);
            url = "api/departmentpanel/" + department.Id;
            response = await client.DeleteAsync(url);
            await CheckOk(response);
            var responseContent3 = await response.Content.ReadFromJsonAsync<OperationSuccesfullViewModel<DepartmentViewModel>>();
            Assert.Equal(department.Id, responseContent3.Data.Id);
        }
    }
}

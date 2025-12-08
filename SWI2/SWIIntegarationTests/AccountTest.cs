using Microsoft.AspNetCore.Mvc.Testing;
using SWI2.Models.Authentication;
using SWI2.Models.Response;
using SWI2.Models.Users;
using SWI2DB.Models.Account;
using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace SWIIntegarationTests
{
    [TestCaseOrderer("XUnit.Project.Orderers.TestPriority", "XUnit.Project")]
    public class AccountTest : BasicTests
    {
        public AccountTest(WebApplicationFactory<SWI2.Startup> factory) : base(factory)
        {

        }

        [Fact]
        public async Task GetUserDetailsByClient()
        {
            var client = await GetAuthorizedHttpClient("clientII");
            var url = "/api/user";
            var response = await client.GetAsync(url);
            await CheckOk(response);

            var content = await response.Content.ReadFromJsonAsync<UserDetailsViewModel>();
            Assert.True(content.Language.Length > 0);
        }
        [Theory]
        [InlineData("1")]
        [InlineData("15")]
        public async Task GetUserDetailsByAdmin(string id)
        {
            var client = await GetAuthorizedHttpClient("admin");
            var url = "/api/user/" + id;
            var response = await client.GetAsync(url);
            await CheckOk(response);
            var content = await response.Content.ReadFromJsonAsync<UserDetailsViewModel>();
            Assert.Equal(id,content.Id.ToString());
        }
        [Fact]
        public async Task EditUserDetailsByClient()
        {
            var client = await GetAuthorizedHttpClient("clientII");
            var url = "/api/user";
            var model = new UserDetailsViewModel()
            {
                Name = "Test",
                Surname = "Testowy",
                Language = "POL",
                FolderName = "/usr"
            };
            var content = JsonContent.Create(model);
            var response = await client.PutAsync(url,content);
            await CheckOk(response);
            var responseContent = await response.Content.ReadFromJsonAsync<OperationSuccesfullViewModel<UserDetailsViewModel>>();
            Assert.Equal(model.Name,responseContent.Data.Name);
        }

        [Fact]
        public async Task ChangePassword()
        {
            var client = await GetAuthorizedHttpClient("clientIII");
            var url = "/api/user/chpassword";
            var passwd = SWI2.Services.Static.Encoding64.Base64Decode(loginModels["clientIII"].Password);
            var model = new ChangePasswordViewModel()
            {
                PreviousPassword = passwd,
                Password = passwd + "1",
                ConfirmPassword = passwd + "1"
            };
            var content = JsonContent.Create(model);
            var response = await client.PostAsync(url, content);
            await CheckOk(response);
            var responseContent = await response.Content.ReadFromJsonAsync<OperationSuccesfullViewModel<string>>();
            Assert.Equal("Password changed", responseContent.Data);
            //powrót do porpzedniego stanu
            await ActivateToken(client);
            model = new ChangePasswordViewModel()
            {
                PreviousPassword = passwd + "1",
                Password = passwd,
                ConfirmPassword = passwd
            };
            content = JsonContent.Create(model);
            response = await client.PostAsync(url, content);
            await CheckOk(response);
        }

        //Ten test wymaga zwrócenia tokenu przez zapytanie http
        [Fact]
        public async Task ChangeEmail()
        {
            var client = await GetAuthorizedHttpClient("clientII");
            var url = "/api/user/chemail";
            var model = new ChangeEmailViewModel()
            {
                Password = "zaq1@WSX",
                Email = "maattizdabrowy@gmail.com",
                Url = "https://localhost:5001/user/chemail"
            };
            var content = JsonContent.Create(model);
            var response = await client.PostAsync(url, content);
            await CheckOk(response);
            var responseContent = await response.Content.ReadFromJsonAsync<OperationSuccesfullViewModel<ChangeEmailViewModel>> ();
            Assert.True(responseContent.Data.Token.Length > 0);

            await ActivateToken(client);
            url = "/api/user/conemail";
            model.Token = responseContent.Data.Token;
            content = JsonContent.Create(model);
            response = await client.PostAsync(url, content);
            await CheckOk(response);
            var responseContent2 = await response.Content.ReadFromJsonAsync<OperationSuccesfullViewModel<string>>();
            Assert.Equal("email changed",responseContent2.Data);
        }
    }
}

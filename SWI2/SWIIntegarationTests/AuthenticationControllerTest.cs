using Microsoft.AspNetCore.Mvc.Testing;
using System;
using System.Threading.Tasks;
using Xunit;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using SWI2.Models.Authentication;
using System.Collections.Generic;

namespace SWIIntegarationTests
{
    public class AuthenticationControllerTest : BasicTests
    {
        public AuthenticationControllerTest(WebApplicationFactory<SWI2.Startup> factory) : base(factory)
        {

        }
        [Fact]
        public async Task TestRegistration()
        {
            string url = "api/authentication/register";
            var client = await GetHttpClient();

            var date = DateTime.Now;
            long number = date.Ticks;
            var model = new RegisterViewModel()
            {
                Name = "Mateusz",
                Surname = "Stabryła",
                Email = "maattizdabrowy." + number + "@swi.com.pl",
                Login = "mstabryla_client_" + number,
                Password = "Soulstorm23",
                ConfirmPassword = "Soulstorm23",
                ClientURI = "https://localhost:5001/authentication/emailconfirmation"
            };
            var content = JsonContent.Create(model);
            var response = await client.PostAsync(url, content);

            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        }
        /*[Fact]
        public async Task TestRegistrationToAdmin()
        {
            string url = "api/authentication/register/2xu6t!@6j!J7tMS";
            var client = await GetHttpClient();

            var date = DateTime.Now;
            long number = date.Ticks;
            var model = new RegisterViewModel()
            {
                Name = "Administrator",
                Surname = "Serwera",
                Email = "admin." + number + "@swi.com.pl",
                Login = "main_admin",
                Password = "Soulstorm23",
                ConfirmPassword = "Soulstorm23",
                ClientURI = "https://localhost:5001/authentication/emailconfirmation"
            };
            var content = JsonContent.Create(model);
            var response = await client.PostAsync(url, content);

            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        }*/

        public static IEnumerable<object[]> LoginViewModelList
        {
            get
            {
                return loginModels.Select(x => new LoginViewModel[] { x.Value });
            }
        }

        [Theory]
        [MemberData(nameof(LoginViewModelList))]
        public async Task TestLogin(LoginViewModel model)
        {
            string url = "api/authentication/login";
            var client = await GetHttpClient();
            /*var model = new LoginViewModel()
            {
                Login = "user_90",
                Password = SWI2.Services.Static.Encoding64.Base64Encode("test")
                
            };*/
            var content = JsonContent.Create(model);
            var response = await client.PostAsync(url, content);

            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);

        }
    }
}

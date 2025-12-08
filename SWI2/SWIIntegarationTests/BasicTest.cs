using Microsoft.AspNetCore.Mvc.Testing;
using SWI2.Models.Authentication;
using SWI2.Models.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

namespace SWIIntegarationTests
{
    public class BasicTests : IClassFixture<WebApplicationFactory<SWI2.Startup>>
    {
        private static readonly string TestPassword = 
    Environment.GetEnvironmentVariable("TEST_PASSWORD") ?? "DefaultTestPassword";

        private static readonly string TestUser = 
    Environment.GetEnvironmentVariable("TEST_USER") ?? "test_user";

        protected static readonly Dictionary<string, LoginViewModel> loginModels = new Dictionary<string, LoginViewModel>()
        {
            {
                "client",
                new LoginViewModel() {
                        Login = "user_63",
                        Password = SWI2.Services.Static.Encoding64.Base64Encode(TestPassword)
                } 
            },
            {
                "clientII",
                new LoginViewModel() {
                        Login = "user_108",
                        Password = SWI2.Services.Static.Encoding64.Base64Encode(TestPassword)
                }
            },
            {
                "clientIII",
                new LoginViewModel() {
                        Login = "user_152",
                        Password = SWI2.Services.Static.Encoding64.Base64Encode(TestPassword)
                }
            },
            {
                "employee",
                new LoginViewModel() {
                        Login = "user_748",
                        Password = SWI2.Services.Static.Encoding64.Base64Encode(TestPassword)
                }
            },
            {
                "admin",
                new LoginViewModel() {
                        Login = "main_admin",
                        Password = SWI2.Services.Static.Encoding64.Base64Encode(TestPassword)
                }
            },
        };

        protected readonly WebApplicationFactory<SWI2.Startup> _factory;
        protected string _antiforgeryurl = "api/antiforgery";
        protected string _loginurl = "/api/authentication/login";

        

        public BasicTests(WebApplicationFactory<SWI2.Startup> factory)
        {
            _factory = factory;
        }

        protected async Task<HttpClient> GetHttpClient()
        {
            var client = _factory.CreateClient();
            var getResponse = await client.GetAsync(_antiforgeryurl);
            CookieLoading.LoadCookies(client, getResponse.Headers.First(x => x.Key == "Set-Cookie"));
            return client;
        }
        protected async Task<HttpClient> GetAuthorizedHttpClient(string role)
        {
            var client = await GetHttpClient();

            LoginViewModel model = loginModels[role];


            var content = JsonContent.Create(model);
            var response = await client.PostAsync(_loginurl, content);
            CookieLoading.LoadCookies(client, response.Headers.First(x => x.Key == "Set-Cookie"));
            //var stringResponse = await response.Content.ReadAsStringAsync();
            //var objResponse = await response.Content.ReadFromJsonAsync<object>();
            var objectResponse = await response.Content.ReadFromJsonAsync<LoginResponseViewModel>();
            client.DefaultRequestHeaders.Add("authorization", "Bearer " + objectResponse.token);
            await ActivateToken(client);
            return client;
        }
        protected async Task<HttpClient> ActivateToken(HttpClient client)
        {
            //client = _factory.CreateClient();
            var getResponse = await client.GetAsync(_antiforgeryurl);
            CookieLoading.LoadCookies(client, getResponse.Headers.First(x => x.Key == "Set-Cookie"));
            return client;
        }
        protected async Task<HttpClient> RefreshToken(string baseurl,HttpClient client)
        {
            //client = _factory.CreateClient();
            var getResponse = await client.GetAsync(baseurl + "/antiforgery");
            CookieLoading.LoadCookies(client, getResponse.Headers.First(x => x.Key == "Set-Cookie"));
            return client;
        }
        protected async Task CheckOk(HttpResponseMessage response)
        {
            if (response.StatusCode == System.Net.HttpStatusCode.InternalServerError)
                throw new Exception(await response.Content.ReadAsStringAsync());
            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        }
        /*[Theory]
        [InlineData("/testint")]
        public async Task Get_IntegrationTest(string url)
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync(url);

            // Assert
            response.EnsureSuccessStatusCode(); // Status Code 200-299
            Assert.Equal("text/plain; charset=utf-8",
                response.Content.Headers.ContentType.ToString());
            Assert.Equal("Hello, Integration Test!", await response.Content.ReadAsStringAsync());
        }*/
    }
}

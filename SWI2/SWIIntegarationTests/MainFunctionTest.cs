using Microsoft.AspNetCore.Mvc.Testing;
using System;
using System.Threading.Tasks;
using Xunit;
using System.Linq;
using System.Net.Http;

namespace SWIIntegarationTests
{
    public class MainFunctionTest : BasicTests
    {
        public MainFunctionTest(WebApplicationFactory<SWI2.Startup> factory) : base(factory)
        {

        }

        [Theory]
        [InlineData("/api/antiforgery")]
        public async Task AntiForgeryTest(string url)
        {
            var client = _factory.CreateClient();
            var getResponse = await client.GetAsync(url);

            Assert.Equal(System.Net.HttpStatusCode.OK, getResponse.StatusCode);
            Assert.Contains(getResponse.Headers, x => x.Key == "Set-Cookie");

        }
    }
}

using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SWI2.Services
{
    public interface IAntiForgery
    {
        public void AddAntiforgeryToken(HttpContext context);
    }
    public class AntiForgery : IAntiForgery
    {
        public IAntiforgery _antiforgery;
        public string aftoken;

        public AntiForgery(IAntiforgery antiforgery,IConfiguration configuration)
        {
            _antiforgery = antiforgery; aftoken = configuration["AF-Token"];
        }


        public void AddAntiforgeryToken(HttpContext context)
        {
            var tokens = _antiforgery.GetAndStoreTokens(context);
            context.Response.Cookies.Append(aftoken, tokens.RequestToken, new CookieOptions()
            {
                HttpOnly = false
            });
        }
    }
}

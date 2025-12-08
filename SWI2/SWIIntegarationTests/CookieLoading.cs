using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace SWIIntegarationTests
{
    public static class CookieLoading
    {

        public static void LoadCookies(HttpClient client, KeyValuePair<string, IEnumerable<string>> setCookie)
        {
            foreach(var cookie in setCookie.Value)
            {
                string[] realCookie = cookie.Split(';')[0].Split('=');
                string cookieName = realCookie[0];
                string cookieValue = realCookie[1];
                client.DefaultRequestHeaders.Remove(cookieName);
                client.DefaultRequestHeaders.Add(cookieName, cookieValue);
            }
        }
    }
}

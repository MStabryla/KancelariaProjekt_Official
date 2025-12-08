using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SWI2.Services.AuthorityHelper
{
    public static class AuthorityHelper
    {
        public static bool CheckIfHasPermitionForCompany(IEnumerable<Claim> claims, long companyid)
        {
            var companys = claims.FirstOrDefault(c => c.Type == "companys");
            var role = claims.FirstOrDefault(c => c.Type == ClaimTypes.Role);
            if (role.Value == "Administrator") {
                return true;
            }
            foreach (var c in companys.Value.Split("_"))
            {
                var companyPropertys = c.Split("|");
                if (companyPropertys[0] == companyid.ToString())
                {
                    return true;
                }
            }
            return false;
        }
    }
}

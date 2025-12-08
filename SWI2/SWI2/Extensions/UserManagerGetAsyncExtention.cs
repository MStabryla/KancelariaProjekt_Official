using Microsoft.AspNetCore.Identity;
using SWI2DB.Models.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SWI2.Extensions
{
    public static class UserManagerGetAsyncExtention
    {
        public static async Task<IQueryable<User>> GetUsersAsync(this UserManager<User> userManager)
        {
            return await Task.Run(() =>
            {
                return userManager.Users;
            });
        }
    }
}

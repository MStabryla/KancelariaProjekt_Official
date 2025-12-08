using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;
using SWI2.Controllers;
using SWI2.Models.Authentication;
using Moq;
using SWI2DB.Models.Authentication;
using Microsoft.AspNetCore.Identity;
using SWI2.Services.Email;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using SWI2.Services;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using SWI2;

namespace SWITest
{

    public class AuthenticationControllerTest

    {
        private AuthenticationController Controller { get; set; }

        [SetUp] 
        public void Setup() 
        {
            
        } 
        /*[Test]
        public async Task CreateUser()
        {
            var model = new RegisterViewModel()
            {
                Name = "Mateusz",
                Surname = "Stabryła",
                Email = "maattizdabrowy@gmail.com",
                Login = "mstabryla_client",
                Password = "Soulstorm23",
                ConfirmPassword = "Soulstorm23"
            };
            var result = await Controller.Register(model);
            Assert.IsInstanceOf<OkObjectResult>(result);
        }*/
    }
}

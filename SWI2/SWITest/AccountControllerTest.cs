using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;
using SWI2.Controllers;

namespace SWITest
{

    public class AccountControllerTest
    {
        private MainController Controller { get; set; }
        [SetUp]
        public void Setup()
        {
           // Controller = new MainController();
        }
    }
}

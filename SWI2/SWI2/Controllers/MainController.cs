using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace SWI2.Controllers
{
    [IgnoreAntiforgeryToken]
    public class MainController : Controller
    {
        private readonly ILogger<MainController> _logger;


        public MainController(ILogger<MainController> logger)
        {
            _logger = logger;

        }
        [Route("log")]
        [HttpPost]
        public IActionResult Log(string exception)
        {
            if (User.IsInRole("Client") || User.IsInRole("Employee") || User.IsInRole("Administrator")) 
            {
                _logger.LogError(new EventId(1000, "FrontEndError"), new Exception(exception), "frond end error");
                return Ok();
            }
            return Forbid();
        }
        public IActionResult Index()
        {
            return View();
        }
        [Route("about")]
        public IActionResult About()
        {
            return View();
        }
        [Route("contract")]
        public IActionResult Contract()
        {
            return View();
        }
        [Route("menu")]
        public IActionResult Menu()
        {
            return View();
        }
        [Route("companyworkers")]
        public IActionResult CompanyWorkers()
        {
            return View();
        }
        [Route("testint")]
        public IActionResult TestIntegration()
        {
            return Content("Hello, Integration Test!");
        }
    }
}

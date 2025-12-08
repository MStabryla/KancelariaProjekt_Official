using Microsoft.AspNetCore.Mvc;
using SWI2.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SWI2.Controllers
{
    [Route("api/antiforgery")]
    [ApiController]
    public class AntiForgeryController : Controller
    {
        private readonly IAntiForgery _antiForgery;

        public AntiForgeryController(IAntiForgery antiForgery) {
            _antiForgery = antiForgery;
        
        }
        [HttpGet("")]
        [IgnoreAntiforgeryToken]
        public IActionResult GetRegister()
        {
            _antiForgery.AddAntiforgeryToken(HttpContext);
            return Ok(new { Response = "forgery token sended" });
        }
    }
}

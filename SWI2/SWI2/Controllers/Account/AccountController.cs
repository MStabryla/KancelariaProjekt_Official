using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using SWI2.Models.Authentication;
using SWI2.Models.Email;
using SWI2.Models.Response;
using SWI2.Models.Users;
using SWI2.Persistence;
using SWI2.Services.Email;
using SWI2.Services.Static;
using SWI2DB.Models.Account;
using SWI2DB.Models.Authentication;
using Microsoft.Extensions.Logging;

namespace SWI2.Controllers
{
    //Czy takie role?
    [Authorize]
    [Route("/api/user")]
    [ApiController]
    public class AccountController : Controller
    {
        private readonly ILogger<AccountController> logger;
        private readonly IStore<UserDetails> userDetailsStore;
        private readonly UserManager<User> userManager;
        private readonly IEmailService emailService;

        public AccountController(
            ILogger<AccountController> _logger,
            IStore<UserDetails> _userDetailsStore,
            IEmailService _emailService,
            UserManager<User> _userManager
            )
        {
            logger = _logger;
            userManager = _userManager;
            userDetailsStore = _userDetailsStore;
            emailService = _emailService;
        }
        private async Task<User> ActUser()
        {
            var MainClaim = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);
            return await userManager.FindByNameAsync(MainClaim.Value);
        }
        //Pobranie danych o użytkowniku
        [HttpGet("")]
        public async Task<IActionResult> GetAccountData()
        {
            var user = await ActUser();
            var userDetails = userDetailsStore.GetById(user.UserDetailsId);
            UserDetailsViewModel model = new UserDetailsViewModel();
            ModelOperations.CopyValues(model, userDetails);
            model.UserId = user.Id;
            model.Email = user.Email;
            return Ok(model);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Administrator")]
        public IActionResult GetAccountData(long id)
        {
            var userDetails = userDetailsStore.GetById(id);
            if (userDetails == default)
                return NotFound();
            UserDetailsViewModel model = new UserDetailsViewModel();
            ModelOperations.CopyValues(model, userDetails,new string[] { });
            model.UserId = userDetails.User.Id;
            model.Email = userDetails.User.Email;
            return Ok(model);
        }
        /*[Route("chpassword")]
        public async Task<IActionResult> ChangePassword()
        {
            throw new NotImplementedException();
        }*/
        //Należy przesłać ViewModel do przesłania haseł
        [HttpPost("chpassword")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordViewModel model)
        {
            if (model.Password != model.ConfirmPassword)
                return BadRequest("Passwords not equal");
            await userManager.ChangePasswordAsync(await ActUser(), model.PreviousPassword, model.Password);
            var user = await ActUser();
            logger.LogInformation(new EventId(111, "ChangePassword"), "user: " + string.Join(",", user.UserName, user.Id));
            return Ok(new OperationSuccesfullViewModel<string>("Password changed"));
        }
        //Należy zaimplementować ViewModel odpowiedzialny za zmiane danych usera
        [HttpPut("")]
        public async Task<IActionResult> EditUser(UserDetailsViewModel model)
        {
            var user = await ActUser();
            var userDetails = userDetailsStore.GetById(user.UserDetailsId);
            ModelOperations.CopyValues(userDetails, model);
            await userDetailsStore.Update(userDetails);
            return Ok(new OperationSuccesfullViewModel<UserDetailsViewModel>(model));
        }
        [HttpPost("chemail")]
        //Dane potwierdzające zmianę poczty email
        public async Task<IActionResult> ChangeEmailSendMessage(ChangeEmailViewModel model)
        {
            var user = await ActUser();
            /*if(user.Email == model.Email)
                return BadRequest(new ErrorResponseViewModel(""))*/
            if(! await userManager.CheckPasswordAsync(user, model.Password))
            {
                return Unauthorized();
            }
            var token = await userManager.GenerateChangeEmailTokenAsync(user, model.Email);
            var param = new Dictionary<string, string>
            {
                {"token", token },
                {"email",model.Email }
            };
            var callback = QueryHelpers.AddQueryString(model.Url, param);
            var message = new EmailMessage(new string[] { model.Email }, "Email Confirmation token", callback, null);

            try
            {
                await emailService.SendEmailAsync(message);
                //return Ok(new OperationSuccesfullViewModel<string>("email sended"));
                model.Token = token;
                
                return Ok(new OperationSuccesfullViewModel<ChangeEmailViewModel>(model));
            }
            catch
            {
                return StatusCode(500, "Error with sending email");
            }
        }
        //Dane potwierdzające zmianę poczty email
        [HttpPost("conemail")]
        public async Task<IActionResult> ChangeEmailConfirm(ChangeEmailViewModel model)
        {
            var user = await ActUser();
            var result = await userManager.ChangeEmailAsync(user, model.Email, model.Token);
            if (result.Succeeded)
            {
                logger.LogInformation(new EventId(112, "Confirm"), "user: " + string.Join(",", user.UserName, user.Id));
                return Ok(new OperationSuccesfullViewModel<string>("email changed"));
            }  
            else
                return BadRequest("Wrong token or user");
        }
    }
}

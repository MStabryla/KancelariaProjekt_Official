using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using SWI2.Models.Authentication;
using SWI2.Models.Email;
using SWI2.Services;
using SWI2DB.Models.Account;
using SWI2DB.Models.Authentication;
using SWI2.Services.Static;
using Microsoft.AspNetCore.WebUtilities;
using SWI2.Services.Email;
using SWI2DB.Models.Client;
using SWI2DB.Models.Company;
using SWI2DB.Models.Department;
using SWI2.Persistence;
using SWI2DB.Models.Employee;
using SWI2DB.Models.Invoice;

namespace SWI2.Controllers
{
    [ApiController]
    [Route("api/authentication")]

    public class AuthenticationController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IEmailService _emailService;
        private readonly ILogger _logger;
        private readonly IStore<ClientCompany> _clientCompany;
        private readonly IStore<Department> _department;
        private readonly IStore<Employee> _employee;
        private readonly IConfiguration _configuration;
        private readonly IFileService _fileService;

        public AuthenticationController(
        UserManager<User> userManager,
        IEmailService emailService,
        SignInManager<User> signInManager,
        ILogger<AuthenticationController> logger,
        IStore<ClientCompany> clientCompany,
        IStore<Employee> employee,
        IStore<Department> department,
        IConfiguration configuration,
        IFileService fileService)
        {
            _userManager = userManager;
            _emailService = emailService;
            _signInManager = signInManager;
            _logger = logger;
            _configuration = configuration;
            _clientCompany = clientCompany;
            _employee = employee;
            _department = department;
            _fileService = fileService;
        }

        [TempData]
        public string ErrorMessage { get; set; }



        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginViewModel model)
        {

            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Login, Encoding64.Base64Decode(model.Password), model.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    var user = _userManager.FindByNameAsync(model.Login);
                    var role = _userManager.GetRolesAsync(user.Result);
                    _logger.LogInformation(new  EventId(1,"Login"),user.Result.UserName+"has logged");
                    return Ok(new { token = GenerateJwtToken(role.Result.FirstOrDefault(), user.Result) });
                }
                if (result.IsLockedOut)
                {
                    _logger.LogWarning(new EventId(1, "Login"), model.Login + "User account locked out");
                    return BadRequest("Użytkownik jest zablokowany. Skontaktuj sie z administratorem");
                }
                else
                {
                    //ModelState.AddModelError(string.Empty, "Invalid login attempt."); //jak dodawać model errory
                    _logger.LogInformation(new EventId(1, "Login"),"Unauthorized logging with model :" + model.ToString());
                    return Unauthorized();
                }
            }
            return BadRequest("Niepoprawne dane logowania");
        }



        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterViewModel model)
        {
            if (model == null || !ModelState.IsValid) {
                _logger.LogWarning(new EventId(2, "Register"), "Unvalid registration with model:" + model == null ? "null":model.ToString());
                return BadRequest();
            }

            UserDetails userDetails = new UserDetails { Name = model.Name, Surname = model.Surname, Registered = DateTime.Now, Language = model.Language };
            Department department = new Department { FolderName = "/users/" + model.Name + "/dla" + model.Name, Name = "Wydział Ogólny", Type = 0, Created = DateTime.Now, Updated = DateTime.Now };
            InvoiceIssuer invoiceIssuer = new InvoiceIssuer { Created= DateTime.Now };
            Company company = new Company { Departments = new List<Department>() { department }, InvoiceIssuers = new List<InvoiceIssuer>() { invoiceIssuer } };
            List<ClientCompany> clientCompanys = new List<ClientCompany>() { new ClientCompany { Company = company, IsInBoard = true } };
            Client client = new Client { ClientCompany = clientCompanys, Department = department };
            User user = new User { UserName = model.Login, Email = model.Email, UserDetails = userDetails, Client = client };
            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description);
                _logger.LogError(new EventId(2, "Register"), "error while creating user with errors :" + result.Errors);
                return StatusCode(500, new { Errors = errors });
            }
            try
            {
                result = await _userManager.AddToRoleAsync(user, "Guest");
                if (!result.Succeeded)
                {
                    var errors = result.Errors.Select(e => e.Description);
                    _logger.LogError(new EventId(2, "Register"), "error while addin user to role with errors :" + result.Errors);
                    return BadRequest(new { Errors = errors });
                }
                _fileService.CreateFolder("/users/" + user.UserName);
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var param = new Dictionary<string, string>
                    {
                        {"token", token },
                        {"login", user.UserName }
                    };
                var callback = QueryHelpers.AddQueryString(model.ClientURI, param);
                var message = new EmailMessage(new string[] { model.Email }, "Email Confirmation token", "<a href=" + callback + ">potwierdź</a>" , null);

                await _emailService.SendEmailAsync(message);
            }
            //wycofanie zmian
            catch (Exception e)
            {
                _logger.LogError(new EventId(2, "Register"),e, "error while sending mail");
                await _userManager.DeleteAsync(user);
                return StatusCode(500, "Error with sending email");
            }
            _logger.LogInformation(new EventId(2, "Register"), "new user created " +user.UserName);
            return Ok(); 
        }


        [HttpGet("emailConfirmation")]
        public async Task<IActionResult> EmailConfirmation([FromQuery] string token, [FromQuery] string login)
        {
            var user = await _userManager.FindByNameAsync(login);
            if (user == null) {
                _logger.LogWarning(new EventId(3, "EmailConfirmation"), "Invalid Email Confirmation with no users login :" +login);
                return BadRequest("Invalid Email Confirmation Request");
            }

            var confirmResult = await _userManager.ConfirmEmailAsync(user, token);

            if (!confirmResult.Succeeded) {
                _logger.LogWarning(new EventId(3, "EmailConfirmation"), "Invalid Email Confirmation Request with login :" + login);
                return BadRequest("Invalid Email Confirmation Request");
            }
            _logger.LogInformation(new EventId(3, "EmailConfirmation"), "Email Confirmation with login :" + login);
            return Ok();
        }

        [HttpPost("forgotPassword")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning(new EventId(4, "ForgotPassword"), "Model Invalid :" + model.ToString());
                return BadRequest();
            }
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                _logger.LogWarning(new EventId(4, "ForgotPassword"), "No user with given Email :" + model.Email);
                return BadRequest("No Emial found");
            }
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var param = new Dictionary<string, string>
            {
                {"token", token },
                {"email", model.Email }
            };
            var callback = QueryHelpers.AddQueryString(model.ClientURI, param);
            var message = new EmailMessage(new string[] { model.Email }, "Reset password token", callback, null);
            await _emailService.SendEmailAsync(message);
            _logger.LogInformation(new EventId(4, "ForgotPassword"), "Email to change sended to " + model.Email);
            return Ok();
        }

        [HttpPost("resetPassword")]
        public async Task<object> ResetPassword([FromBody] ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning(new EventId(5, "ResetPassword"), "Model Invoalid "+ model.ToString());
                return View(model);
            }
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                _logger.LogWarning(new EventId(5, "ResetPassword"), "no user found with email "+model.Email);
                return BadRequest("użytkownik nie istnieje");
            }

            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.Password);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description);
                _logger.LogError(new EventId(5, "ResetPassword"), "Reset Password error:" + errors);
                return BadRequest(new { Errors = errors });
            }
            _logger.LogInformation(new EventId(5, "ResetPassword"), "Password changed");
            return Ok();
        }

        #region Helpers

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        private string GenerateJwtToken(string role, User user)
        {
            var userAccess = "";
            if (role == "Employee")
            {
                foreach (var dep in _employee.AsQueryable().Where(c => c.Id == user.EmployeeId).Select(i => new { i.Departments }).FirstOrDefault().Departments)
                {
                    foreach (var c in _department.AsQueryable().Where(d => d.Id == dep.Id).Select(i => new { i.Company }))
                    {
                        if(c.Company != null)
                            userAccess += c.Company.Id + "|" + c.Company.Name + "|" + false.ToString() + "_";
                    }
                }
            }
            else
            {
                var clientsData = _clientCompany.AsQueryable().Where(c => c.Client.Id == user.ClientId).Select(i => new
                {
                    i.IsInBoard,
                    i.Company
                });
                foreach (var c in clientsData)
                {
                    userAccess += c.Company.Id + "|" + c.Company.Name + "|" + c.IsInBoard + "_";
                }
            }
            if(userAccess != "")
                userAccess = userAccess.Remove(userAccess.Length - 1);

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_configuration["JWT:Secret"]));

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                        new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                        new Claim(ClaimTypes.NameIdentifier, user.Id),
                        new Claim(ClaimTypes.Role, role),
                        new Claim("LoggedOn", DateTime.Now.ToString()),
                        new Claim("companys", userAccess)
                 }),
                SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature),
                Issuer = _configuration["JWT:ValidIssuer"],
                Audience = _configuration["JWT:ValidAudience"],
                Expires = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["JWT:ExpireTime"]))
            };

            // Generate Token

            var token = tokenHandler.CreateToken(tokenDescriptor);
            _logger.LogInformation(new EventId(6, "GenerateJwtToken"), "Token Generated :"+ token);
            return tokenHandler.WriteToken(token);
        }

        #endregion

        #region Backdoor
        /// <summary>
        /// WARNING!!! Only in Develoment mode
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
 /*       [HttpPost("register/2xu6t!@6j!J7tMS")]
        public async Task<IActionResult> RegisterAdmin([FromBody] RegisterViewModel model)
        {
            if (model == null || !ModelState.IsValid) {
                _logger.LogWarning(new EventId(7, "RegisterAdmin"), "Invalid Model"+ model.ToString());
                return BadRequest();
            }

            UserDetails userDetails = new UserDetails { Name = model.Name, Surname = model.Surname, Registered = DateTime.Now, Language = model.Language };
            User user = new User { UserName = model.Login, Email = model.Email, UserDetails = userDetails };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description);
                _logger.LogWarning(new EventId(7, "RegisterAdmin"), "error creating admin :"+errors);
                return BadRequest(new { Errors = errors });
            }
            try
            {
                result = await _userManager.AddToRoleAsync(user, "admin");
                if (!result.Succeeded)
                {
                    var errors = result.Errors.Select(e => e.Description);
                    _logger.LogWarning(new EventId(7, "RegisterAdmin"), "error adding admin to role :" + errors);
                    return BadRequest(new { Errors = errors });
                }
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var param = new Dictionary<string, string>
                    {
                        {"token", token },
                        {"login", user.UserName }
                    };
                var callback = QueryHelpers.AddQueryString(model.ClientURI, param);
                var message = new EmailMessage(new string[] { model.Email }, "Email Confirmation token", callback, null);

                await _emailService.SendEmailAsync(message);
            }
            //wycofanie zmian
            catch(Exception e)
            {

                await _userManager.DeleteAsync(user);
                _logger.LogError(new EventId(7, "RegisterAdmin"),e, "error adding admin");
                return StatusCode(500, "Error with sending email");
            }
            _logger.LogInformation(new EventId(7, "RegisterAdmin"), "admin added "+ user.UserName);
            return Ok();
        }*/

        #endregion
    }
}


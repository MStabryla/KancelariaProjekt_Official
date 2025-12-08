using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SWI2.Persistence;
using SWI2DB.Models.Messages;
using Microsoft.AspNetCore.Identity;
using SWI2DB.Models.Authentication;
using SWI2.Models;
using Newtonsoft.Json;
using SWI2.Services.Static;
using System.Linq.Dynamic.Core;
using SWI2.Models.Messages;
using SWI2.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using SWI2.Models.Response;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Logging;

namespace SWI2.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/messagepanel")]
    public class MessageController : Controller
    {
        private readonly ILogger<MessageController> logger;
        private readonly UserManager<User> userManager;
        private readonly IStore<Message> messagesStore;
        private readonly IStore<UserMessageTemplate> _userMessageTemplateStore;
        private readonly RoleManager<IdentityRole> roleStore;

        public MessageController(
            ILogger<MessageController> _logger,
            UserManager<User> _userManager,
            RoleManager<IdentityRole> _roleStore,
            IStore<UserMessageTemplate> userMessageTemplateStore,
            IStore<Message> _messagesStore)
        {
            logger = _logger;
            messagesStore = _messagesStore;
            roleStore = _roleStore;
            _userMessageTemplateStore = userMessageTemplateStore;
            userManager = _userManager;
        }

        private async Task<User> ActUser()
        {
            var MainClaim = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);
            return await userManager.FindByNameAsync(MainClaim.Value);
        }

        private async Task<User[]> UsersForClient()
        {
            if (User.IsInRole("Client"))
            {
                string userId = User.Claims.Where(x => x.Type == ClaimTypes.NameIdentifier).ElementAt(1).Value;
                var user = userManager.Users.Include(x => x.Client).ThenInclude(x => x.ClientCompany).ThenInclude(x => x.Company).ThenInclude(x => x.Departments).ThenInclude(x => x.Employees).ThenInclude(x => x.User).First(x => x.Id == userId);
                var clientCompanies = user.Client.ClientCompany.AsQueryable();
                var companies = clientCompanies.Select(x => x.Company);
                var departments = companies.SelectMany(x => x.Departments);
                var users = departments.SelectMany(x => x.Employees).Select(x => x.User);
                return users.ToArray();
            }
            return (await userManager.GetUsersInRoleAsync("Administrator")).Concat(await userManager.GetUsersInRoleAsync("Employee")).Concat(await userManager.GetUsersInRoleAsync("Client")).ToArray();
        }

        private async Task<bool> CheckAccessToMessage(long messageId)
        {
            var user = await ActUser();
            var message = messagesStore.AsQueryable().Include(x => x.MessageReceiver).ThenInclude(x => x.User).Include(x => x.MessageSender).ThenInclude(x => x.User).First(x => x.Id == messageId);
            if (message == null)
                throw new Exception("Message doesn't exits");
            return (message.MessageReceiver != null && message.MessageReceiver.User == user) || (message.MessageSender != null && message.MessageSender.User == user);
        }

        [HttpGet("received")]
        public async Task<IActionResult> GetReceivedMessages([FromQuery] string query)
        {
            var user = await ActUser();
            TableParamsModel tableParams = query != null ? JsonConvert.DeserializeObject<TableParamsModel>(Encoding64.Base64Decode(query)) : new TableParamsModel() { Sort = "posted", PageNumber = 0, PageSize = 25, Filters = new List<FilterModel>() };
            IQueryable<Message> IQuery = messagesStore.AsQueryable().Include(x => x.MessageReceiver).ThenInclude(x => x.User).Include(x => x.MessageSender).ThenInclude(x => x.User).Where(x => x.MessageReceiver.User == user && !x.Trashbox);
            foreach (FilterModel fm in tableParams.Filters)
            {
                switch (fm.Type)
                {
                    case "string":
                        IQuery = IQuery.Where(fm.Name + ".Contains(@0)", fm.Value);
                        break;
                    case "date":
                        IQuery = IQuery.Where($"" + fm.Name + "@0", Convert.ToDateTime(fm.Value));
                        break;
                    default:
                        IQuery = IQuery.Where(fm.Name + ".Contains(@0)", fm.Value);
                        break;
                }
            }

            var pagedResult = IQuery.ToList().Select(data =>
            {
                MessageViewModel model = new MessageViewModel();
                ModelOperations.CopyValues(model, data, new string[] { "Content", "MessageReceiverId" });
                model.MessageReceiverName = data.MessageReceiver != null && data.MessageReceiver.User != null ? data.MessageReceiver.User.UserName : "brak";
                model.MessageSenderName = data.MessageSender != null && data.MessageSender.User != null ? data.MessageSender.User.UserName : "brak";
                return model;
            }).AsQueryable().OrderBy(tableParams.Sort).GetPaged(tableParams.PageNumber, tableParams.PageSize);
            return Ok(new TableViewModel<MessageViewModel>
            {
                totalCount = pagedResult.RowCount,
                elements = pagedResult.Results.AsQueryable().ToList()
            });
        }
        [HttpGet("trashbox")]
        public async Task<IActionResult> GetThrownOutMessages([FromQuery] string query)
        {
            var user = await ActUser();
            TableParamsModel tableParams = query != null ? JsonConvert.DeserializeObject<TableParamsModel>(Encoding64.Base64Decode(query)) : new TableParamsModel() { Sort = "posted desc", PageNumber = 0, PageSize = 25, Filters = new List<FilterModel>() };
            IQueryable<Message> IQuery = messagesStore.AsQueryable().Include(x => x.MessageReceiver).ThenInclude(x => x.User).Include(x => x.MessageSender).ThenInclude(x => x.User).Where(x => x.MessageReceiver.User == user && x.Trashbox);
            foreach (FilterModel fm in tableParams.Filters)
            {
                switch (fm.Type)
                {
                    case "string":
                        IQuery = IQuery.Where(fm.Name + ".Contains(@0)", fm.Value);
                        break;
                    case "date":
                        IQuery = IQuery.Where($"" + fm.Name + "@0", Convert.ToDateTime(fm.Value));
                        break;
                    default:
                        IQuery = IQuery.Where(fm.Name + ".Contains(@0)", fm.Value);
                        break;
                }
            }

            var pagedResult = IQuery.ToList().Select(data =>
            {
                MessageViewModel model = new MessageViewModel();
                ModelOperations.CopyValues(model, data, new string[] { "Content", "MessageReceiverId" });
                model.MessageReceiverName = data.MessageReceiver != null && data.MessageReceiver.User != null ? data.MessageReceiver.User.UserName : "brak";
                model.MessageSenderName = data.MessageSender != null && data.MessageSender.User != null ? data.MessageSender.User.UserName : "brak";
                return model;
            }).AsQueryable().OrderBy(tableParams.Sort).GetPaged(tableParams.PageNumber, tableParams.PageSize);
            return Ok(new TableViewModel<MessageViewModel>
            {
                totalCount = pagedResult.RowCount,
                elements = pagedResult.Results.AsQueryable().ToList()
            });
        }
        [HttpGet("sended")]
        public async Task<IActionResult> GetSendedMessages([FromQuery] string query)
        {
            var user = await ActUser();
            TableParamsModel tableParams = query != null ? JsonConvert.DeserializeObject<TableParamsModel>(Encoding64.Base64Decode(query)) : new TableParamsModel() { Sort = "posted desc", PageNumber = 0, PageSize = 25, Filters = new List<FilterModel>() };
            IQueryable<Message> IQuery = messagesStore.AsQueryable().Include(x => x.MessageReceiver).ThenInclude(x => x.User).Include(x => x.MessageSender).ThenInclude(x => x.User).Where(x => x.MessageSender.User == user && !x.Trashbox);
            foreach (FilterModel fm in tableParams.Filters)
            {
                switch (fm.Type)
                {
                    case "string":
                        IQuery = IQuery.Where(fm.Name + ".Contains(@0)", fm.Value);
                        break;
                    case "date":
                        IQuery = IQuery.Where($"" + fm.Name + "@0", Convert.ToDateTime(fm.Value));
                        break;
                    default:
                        IQuery = IQuery.Where(fm.Name + ".Contains(@0)", fm.Value);
                        break;
                }
            }

            var pagedResult = IQuery.ToList().Select(data =>
            {
                MessageViewModel model = new MessageViewModel();
                ModelOperations.CopyValues(model, data, new string[] { "Content", "MessageReceiverId" });
                model.MessageReceiverName = data.MessageReceiver != null && data.MessageReceiver.User != null ? data.MessageReceiver.User.UserName : "brak";
                model.MessageSenderName = data.MessageSender != null && data.MessageSender.User != null ? data.MessageSender.User.UserName : "brak";
                return model;
            }).AsQueryable().OrderBy(tableParams.Sort).GetPaged(tableParams.PageNumber, tableParams.PageSize);
            return Ok(new TableViewModel<MessageViewModel>
            {
                totalCount = pagedResult.RowCount,
                elements = pagedResult.Results.AsQueryable().ToList()
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> SeeMessage(long? id)
        {
            var user = await ActUser();
            var message = await messagesStore.GetByIdAsync(id);
            if (message == null)
                return NotFound("message");
            if (!await CheckAccessToMessage((long)id))
                return Forbid();
            var messageReceiver = messagesStore.AsQueryable().Include(x => x.MessageReceiver).ThenInclude(x => x.User).First(x => x.Id == message.Id).MessageReceiver;
            if ((message.Readed == null || message.Readed.Ticks == 0) && messageReceiver.User == user)
            {
                message.Readed = DateTime.Now;
                message = await messagesStore.Update(message);
            }
            MessageViewModel model = new MessageViewModel();
            ModelOperations.CopyValues(model, message, new string[] { "MessageReceiverId" });
            model.MessageReceiverName = message.MessageReceiver != null && message.MessageReceiver.User != null ? message.MessageReceiver.User.UserName : "brak";
            model.MessageSenderName = message.MessageSender != null && message.MessageSender.User != null ? message.MessageSender.User.UserName : "brak";
            return Ok(model);
        }
        [HttpGet("receivers")]
        public async Task<IActionResult> GetMessageReceivers([FromQuery] string query)
        {
            TableParamsModel tableParams = query != null ? JsonConvert.DeserializeObject<TableParamsModel>(Encoding64.Base64Decode(query)) : new TableParamsModel() { Sort = "name", PageNumber = 0, PageSize = 25, Filters = new List<FilterModel>() };
            IQueryable<User> IQuery = (await UsersForClient()).AsQueryable();
            /*if (User.IsInRole("Client"))
            {
                //var test = UsersForClient();
                //IQuery = (await userManager.GetUsersInRoleAsync("Employee")).AsQueryable();
                IQuery = UsersForClient().AsQueryable();
            }*/
            foreach (FilterModel fm in tableParams.Filters)
            {
                switch (fm.Type)
                {
                    case "string":
                        IQuery = IQuery.Where(fm.Name + ".Contains(@0)", fm.Value);
                        break;
                    case "date":
                        IQuery = IQuery.Where($"" + fm.Name + "@0", Convert.ToDateTime(fm.Value));
                        break;
                    default:
                        IQuery = IQuery.Where(fm.Name + ".Contains(@0)", fm.Value);
                        break;
                }
            }

            var pagedResult = IQuery.ToList().AsQueryable().OrderBy(tableParams.Sort);
            return Ok(pagedResult.Select(x => new { Name = x.UserName, x.Id }).Take(tableParams.PageSize));
        }
        [HttpPost("")]
        public async Task<IActionResult> SendMessage(MessageViewModel model)
        {
            var user = await ActUser();
            Message message = new Message();
            ModelOperations.CopyValues(message, model, new string[] { "MessageReceiverId" });
            var messageReceiver = await userManager.FindByIdAsync(model.MessageReceiverId);
            if (messageReceiver == null)
                return NotFound("messageReceiver");
            message.MessageReceiver = new MessageReceiver()
            {
                Message = message,
                User = messageReceiver
            };
            message.MessageSender = new MessageSender()
            {
                Message = message,
                User = user
            };
            message.Posted = DateTime.Now;
            if (!await messagesStore.InsertAsync(message))
                return StatusCode(500, "Problem with creating message");
            ModelOperations.CopyValues(model, message, new string[] { "Content", "MessageReceiverId" });
            model.MessageReceiverName = message.MessageReceiver != null && message.MessageReceiver.User != null ? message.MessageReceiver.User.UserName : "brak";
            model.MessageSenderName = message.MessageSender != null && message.MessageSender.User != null ? message.MessageSender.User.UserName : "brak";
            logger.LogInformation(new EventId(191, "SendMessage"), "user: " + string.Join(",", user.UserName, user.Id) + "; message: " + message.Id);
            return Ok(new OperationSuccesfullViewModel<MessageViewModel>(model));
        }

        [HttpDelete]
        [Route("{id}")]
        public async Task<IActionResult> TransferMessageToTrashbox(long? id)
        {
            var user = await ActUser();
            var message = await messagesStore.GetByIdAsync(id);
            if (message == null)
                return NotFound("message");
            if (!await CheckAccessToMessage((long)id))
                return Forbid();
            if (message.Trashbox)
                return BadRequest("Message is already in trashbox");
            message.Trashbox = true;
            message.SendedToTrashbox = DateTime.Now;
            await messagesStore.Update(message);
            var model = new MessageViewModel();
            ModelOperations.CopyValues(model, message, new string[] { "Content", "MessageReceiverId" });
            model.MessageReceiverName = message.MessageReceiver != null && message.MessageReceiver.User != null ? message.MessageReceiver.User.UserName : "brak";
            model.MessageSenderName = message.MessageSender != null && message.MessageSender.User != null ? message.MessageSender.User.UserName : "brak";
            logger.LogInformation(new EventId(192, "TransferMessageToTrashbox"), "user: " + string.Join(",", user.UserName, user.Id) + "; message: " + message.Id);
            return Ok(new OperationSuccesfullViewModel<MessageViewModel>(model));
        }

        [HttpPut]
        [Route("{id}")]
        public async Task<IActionResult> RemoveMessageFromTrashbox(long? id)
        {
            var user = await ActUser();
            var message = await messagesStore.GetByIdAsync(id);
            if (message == null)
                return NotFound("message");
            if (!await CheckAccessToMessage((long)id))
                return Forbid();
            if (!message.Trashbox)
                return BadRequest("Message is not in trashbox");
            message.Trashbox = false;
            await messagesStore.Update(message);
            var model = new MessageViewModel();
            ModelOperations.CopyValues(model, message, new string[] { "Content", "MessageReceiverId" });
            model.MessageReceiverName = message.MessageReceiver != null && message.MessageReceiver.User != null ? message.MessageReceiver.User.UserName : "brak";
            model.MessageSenderName = message.MessageSender != null && message.MessageSender.User != null ? message.MessageSender.User.UserName : "brak";
            logger.LogInformation(new EventId(193, "RemoveMessageFromTrashbox"), "user: " + string.Join(",", user.UserName, user.Id) + "; message: " + message.Id);
            return Ok(new OperationSuccesfullViewModel<MessageViewModel>(model));
        }
        [HttpGet("templates")]
        public async Task<IActionResult> GetTemplates([FromQuery] string query)
        {
            var user = await ActUser();
            TableParamsModel tableParams = query != null ? JsonConvert.DeserializeObject<TableParamsModel>(Encoding64.Base64Decode(query)) : new TableParamsModel() { Sort = "created", PageNumber = 0, PageSize = 5, Filters = new List<FilterModel>() };
            IQueryable<UserMessageTemplate> userMessageTemplate = _userMessageTemplateStore.AsQueryable().Where(umt => umt.Users.Any(u => u.Id == user.Id)).OrderBy(tableParams.Sort);
            foreach (FilterModel fm in tableParams.Filters)
            {
                switch (fm.Type)
                {
                    case "string":
                        userMessageTemplate = userMessageTemplate.Where(fm.Name + ".Contains(@0)", fm.Value);
                        break;
                    case "date":
                        userMessageTemplate = userMessageTemplate.Where($"" + fm.Name + "@0", Convert.ToDateTime(fm.Value));
                        break;
                    default:
                        userMessageTemplate = userMessageTemplate.Where(fm.Name + ".Contains(@0)", fm.Value);
                        break;
                }
            }
            var pagedResult = userMessageTemplate.GetPaged(tableParams.PageNumber, tableParams.PageSize);
            return Ok(new TableViewModel<UserMessageTemplate>
            {
                totalCount = pagedResult.RowCount,
                elements = pagedResult.Results
            });
        }
        [HttpPost]
        [Route("templates")]
        public async Task<IActionResult> InsertTemplate(UserMessageTemplate userMessageTemplate)
        {
            var user = await ActUser();

            userMessageTemplate.Created = DateTime.Now;
            userMessageTemplate.Users = new List<User>() { user };

            if (await _userMessageTemplateStore.InsertAsync(userMessageTemplate))
            {
                logger.LogInformation(new EventId(194, "InsertTemplate"), "user: " + string.Join(",", user.UserName, user.Id) + "; template: " + userMessageTemplate.Id);
                return Ok(userMessageTemplate);
            }
            else
            {
                return BadRequest("Błąd w trakcie dodawania wiadomości");
            }

        }
        [HttpDelete]
        [Route("templates/{id}")]
        public async Task<IActionResult> RemoveTemplate(long id)
        {
            var user = await ActUser();
            var userMessageTemplate = await _userMessageTemplateStore.Table.FirstOrDefaultAsync(umt => umt.Users.Any(u => u.Id == user.Id) && umt.Id == id);
            if (userMessageTemplate != null)
            {
                if (_userMessageTemplateStore.Delete(userMessageTemplate))
                {
                    logger.LogInformation(new EventId(195, "RemoveTemplate"), "user: " + string.Join(",", user.UserName, user.Id) + "; template: " + userMessageTemplate.Id);
                    return Ok(userMessageTemplate);
                }
                else
                {
                    return BadRequest("Błąd w trakcie usuwania wiadomosci");
                }
            }
            else
            {
                return Unauthorized("Brak autoryzacji do szablonu");
            }
        }
        [HttpPut]
        [Route("templates/{id}")]
        public async Task<IActionResult> EditTemplate(long id, JObject changedData)
        {
            var user = await ActUser();
            var userMessageTemplate = await _userMessageTemplateStore.Table.FirstOrDefaultAsync(umt => umt.Users.Any(u => u.Id == user.Id) && umt.Id == id);
            if (userMessageTemplate != null)
            {
                var users = changedData.Property("users");
                if (users != null)
                {
                    users.Remove();
                }

                userMessageTemplate = await _userMessageTemplateStore.Update(userMessageTemplate, changedData);
                logger.LogInformation(new EventId(196, "EditTemplate"), "user: " + string.Join(",", user.UserName, user.Id) + "; template: " + userMessageTemplate.Id);
                return Ok(userMessageTemplate);
            }
            else
            {
                return Unauthorized("Brak autoryzacji do szablonu");
            }
        }

        [HttpPost]
        [Authorize(Roles = "Administrator")]
        [Route("{id}/access")]
        public async Task<IActionResult> GiveAccessToTemplate(long id, string userId)
        {
            var user = await ActUser();
            var userMessageTemplate = await _userMessageTemplateStore.Table.Include(umt => umt.Users).FirstOrDefaultAsync(umt => umt.Id == id && umt.Users.Any(u => u.Id == userId));
            if (!(userMessageTemplate is null))
            {
                userMessageTemplate.Users.Add(await userManager.FindByIdAsync(userId));
                await _userMessageTemplateStore.Update(userMessageTemplate);
                logger.LogInformation(new EventId(197, "GiveAccessToTemplate"), "user: " + string.Join(",", user.UserName, user.Id) + "; template: " + userMessageTemplate.Id);
                return Ok(true);
            }
            else
            {
                return BadRequest("Użytkownik nie posiadał dostepu do szablonu");
            }
        }
        [HttpDelete]
        [Authorize(Roles = "Administrator")]
        [Route("{id}/access")]
        public async Task<IActionResult> RemoveAccessToTemplate(long id, string userId)
        {
            var user = await ActUser();
            var userMessageTemplate = await _userMessageTemplateStore.Table.Include(umt => umt.Users).FirstOrDefaultAsync(umt => umt.Id == id && umt.Users.Any(u => u.Id == userId));
            if (!(userMessageTemplate is null))
            {
                userMessageTemplate.Users.Remove(await userManager.FindByIdAsync(userId));
                await _userMessageTemplateStore.Update(userMessageTemplate);
                logger.LogInformation(new EventId(198, "RemoveAccessToTemplate"), "user: " + string.Join(",", user.UserName, user.Id) + "; template: " + userMessageTemplate.Id);
                return Ok(true);
            }
            else
            {
                return BadRequest("Użytkownik nie posiadał dostepu do szablonu");
            }
        }
    }
}

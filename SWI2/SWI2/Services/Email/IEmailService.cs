using SWI2.Models.Email;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SWI2.Services.Email
{
    public interface IEmailService
    {
        void SendEmail(EmailMessage message);
        Task<bool> SendEmailAsync(EmailMessage message);
    }
}

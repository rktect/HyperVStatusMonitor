using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HyperVStatusMon
{
    public class NotificationHelpers
    {
        public async static Task<string> SendEmail(string message, EmailSettings settings)
        {
            try {
                var email = new SmtpMailNetCore();
                email.Server = settings.SmtpServer;
                email.Port = settings.SmtpPort;
                email.User = settings.SmtpUser;
                email.Password = settings.SmtpPassword;
                email.From = new MailboxAddress(settings.From.Name, settings.From.Address);
                email.Subject = settings.Subject;
                email.Body = message;
                foreach (EmailRecipient r in settings.To)
                    email.Recipients.Add(new MailboxAddress(r.Name, r.Address));
                
                string result = await email.SendEmailAsync();
                return result;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }

        }
    }
}

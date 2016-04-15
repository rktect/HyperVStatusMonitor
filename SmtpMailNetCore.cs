using System;
using System.Collections.Generic;
using MailKit.Net.Smtp;
using MimeKit;
using MailKit.Security;
using System.Threading.Tasks;
using Org.BouncyCastle.X509;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;

namespace HyperVStatusMon
{
    /// <summary>
    /// This is a total hack job just to get an email sending in ASP.NET Core 1.0
    /// I hope we can implement a better solution
    /// Also, another bad hack below to fix a server certificate problem on SendGrid but I didn't want to remove TLS
    /// </summary>
    public class SmtpMailNetCore
    {
        public string Server { get; set; }
        public int Port { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
        public MailboxAddress From { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public List<MailboxAddress> Recipients { get; set; }

        public SmtpMailNetCore()
        {
            Recipients = new List<MailboxAddress>();
        }

        public async Task<string> SendEmailAsync()
        {
            if (Recipients.Count < 1 || From == null) return "no params";           // TODO: check ALL params here
            try {
                var emailMessage = new MimeMessage();

                emailMessage.From.Add(new MailboxAddress(this.From.Name, this.From.Address));
                foreach (MailboxAddress recip in this.Recipients)
                    emailMessage.To.Add(recip);

                emailMessage.Subject = this.Subject;
                emailMessage.Body = new TextPart("html") { Text = this.Body };

                using (var client = new SmtpClient())
                {
                    client.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) =>
                    {
                        return true;        // Ignore SendGrid certificate problem:    http://stackoverflow.com/questions/35185112/authenticationexception-when-tryign-to-connect-to-mail-server-using-mailkit
                        //Maybe add a check against the cert thumbprint instead of trusting all
                        //if (certificate is X509Certificate2 && ((X509Certificate2) certificate).Thumbprint == "71DFBE124D89ED9218F539A82D4127FBB4BC9997") return true;
                    };
                    await client.ConnectAsync(this.Server, this.Port, SecureSocketOptions.StartTls).ConfigureAwait(false);
                    if (this.User.Length > 0)
                        await client.AuthenticateAsync(this.User, this.Password).ConfigureAwait(false);
                    await client.SendAsync(emailMessage).ConfigureAwait(false);
                    await client.DisconnectAsync(true).ConfigureAwait(false);
                }
                return "ok";
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

    }
}

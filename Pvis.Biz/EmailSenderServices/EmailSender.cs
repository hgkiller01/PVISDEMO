using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using MimeKit;
using Pvis.Biz.Extension;

namespace Pvis.Biz.EmailSenderServices
{
    public class EmailSender : IEmailSender
    {
        private readonly EmailSettings _emailSettings;
        private readonly IWebHostEnvironment _env;

        public EmailSender(
            IOptions<EmailSettings> emailSettings,
            IWebHostEnvironment env)
        {
            _emailSettings = emailSettings.Value;
            _env = env;
        }

        public void ReloadConfig()
        {
            _emailSettings.Reload();
        }

        public async Task SendEmailAsync(IEnumerable<string> ToList, IEnumerable<string> CcList, IEnumerable<string> BccList, string subject, string message)
        {
            var HasTo = ToList?.Any() ?? false;
            var HasCc = CcList?.Any() ?? false;
            var HasBcc = BccList?.Any() ?? false;

            if (!HasTo && !HasCc && !HasBcc) return;

            var mimeMessage = new MimeMessage();

            mimeMessage.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.Sender));

            if (HasTo) mimeMessage.To.AddRange(ToList.Select(x => new MailboxAddress(x)));

            if (HasCc) mimeMessage.Cc.AddRange(CcList.Select(x => new MailboxAddress(x)));

            if (HasBcc) mimeMessage.Bcc.AddRange(BccList.Select(x => new MailboxAddress(x)));

            mimeMessage.Subject = subject;

            mimeMessage.Body = new TextPart("html")
            {
                Text = message.ToSafehtml()
            };

            await SendEmailAsync(mimeMessage);
        }

        public async Task SendEmailAsync(MimeMessage mimeMessage)
        {
            using (var client = new SmtpClient())
            {
                var _SecureSocketOptions = _emailSettings.UseSSL ? MailKit.Security.SecureSocketOptions.Auto : MailKit.Security.SecureSocketOptions.None;

                client.ServerCertificateValidationCallback = (s, c, h, e) => true;

                await client.ConnectAsync(_emailSettings.MailServer, _emailSettings.MailPort, _SecureSocketOptions);

                if (!String.IsNullOrWhiteSpace(_emailSettings.Sender) && !String.IsNullOrWhiteSpace(_emailSettings.Password))
                {
                    await client.AuthenticateAsync(_emailSettings.Sender, _emailSettings.Password);
                }

                await client.SendAsync(mimeMessage);

                await client.DisconnectAsync(true);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MimeKit;

namespace Pvis.Biz.EmailSenderServices
{
    public interface IEmailSender
    {
        Task SendEmailAsync(IEnumerable<string> ToList, IEnumerable<string> CcList, IEnumerable<string> BccList, string subject, string message);

        Task SendEmailAsync(MimeMessage mimeMessage);

        void ReloadConfig();
    }
}

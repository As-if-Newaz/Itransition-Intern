using Iforms.BLL.DTOs;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Iforms.BLL.Services
{
    public class EmailService
    {
        private readonly SmtpSettings _smtpSettings;

        public EmailService(IOptions<SmtpSettings> smtpSettings)
        {
            _smtpSettings = smtpSettings.Value;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var message = new MailMessage
            {
                From = new MailAddress(_smtpSettings.SenderEmail),
                Subject = subject,
                Body = body,
                IsBodyHtml = false
            };
            message.To.Add(new MailAddress(toEmail));

            using (var client = new SmtpClient(_smtpSettings.Server, _smtpSettings.Port))
            {
                client.Credentials = new NetworkCredential(_smtpSettings.SenderEmail, _smtpSettings.Password);
                client.EnableSsl = true;
                await client.SendMailAsync(message);
            }
        }
    }
}

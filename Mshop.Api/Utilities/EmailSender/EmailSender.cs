using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;

namespace Mshop.Api.Utilities.EmailSender
{
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _configuration;

        public EmailSender(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            //var fromEmail = Environment.GetEnvironmentVariable("FROM_EMAIL");
            var fromEmail = _configuration["EmailSender:FromEmail"];
            //var password = Environment.GetEnvironmentVariable("PASSWORD");
            var password = _configuration["EmailSender:Password"];

            using SmtpClient smtpClient = new("smtp.gmail.com", 587)
            {
                UseDefaultCredentials=false,
                Credentials = new NetworkCredential(fromEmail, password),
                EnableSsl = true
            };

            MailMessage message = new(fromEmail!, email, subject, htmlMessage)
            {
                IsBodyHtml = true
            };

            await smtpClient.SendMailAsync(message);
        }
    }
}

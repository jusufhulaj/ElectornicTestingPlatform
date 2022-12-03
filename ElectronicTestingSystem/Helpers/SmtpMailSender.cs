using Microsoft.AspNetCore.Identity.UI.Services;
using System.Net.Mail;

namespace ElectronicTestingSystem.Helpers
{
    public class SmtpMailSender : IEmailSender
    {
        private readonly SmtpConfiguration _configuration;
        private readonly SmtpClient _client;

        public SmtpMailSender(SmtpConfiguration configuration)
        {
            _configuration = configuration;
            _client = new SmtpClient
            {
                Host = _configuration.Host,
                Port = _configuration.Port,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                EnableSsl = _configuration.UseSSL
            };

            if (!string.IsNullOrEmpty(_configuration.Password))
            {
                _client.Credentials = new System.Net.NetworkCredential(_configuration.Login, _configuration.Password);
            }
            else
            {
                _client.UseDefaultCredentials = true;
            }
        }
        
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            try
            {
                var from = string.IsNullOrEmpty(_configuration.From) ? _configuration.Login : _configuration.From;
                var mail = new MailMessage(from, email);
                mail.IsBodyHtml = true;
                mail.Subject = subject;
                mail.Body = htmlMessage;

                _client.Send(mail);

                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}

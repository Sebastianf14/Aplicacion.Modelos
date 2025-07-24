using System.Net;
using System.Net.Mail;

namespace Aplicacion.MVC.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string message);
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config) 
        {
            _config = config;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string message)
        {
            try
            {
                var smtpHost = _config["EmailSettings:SmtpHost"];
                var smtpPort = int.Parse(_config["EmailSettings:SmtpPort"]);
                var smtpUser = _config["EmailSettings:SmtpUser"];
                var smtpPassword = _config["EmailSettings:SmtpPassword"];
                var fromEmail = _config["EmailSettings:FromEmail"];
                var fromName = _config["EmailSettings:FromName"];

                using var client = new SmtpClient(smtpHost, smtpPort)
                {
                    Credentials = new NetworkCredential(smtpUser, smtpPassword),
                    EnableSsl = true
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(fromEmail, fromName),
                    Subject = subject,
                    Body = message,
                    IsBodyHtml = true
                };

                mailMessage.To.Add(toEmail);

                await client.SendMailAsync(mailMessage);

                
                System.Diagnostics.Debug.WriteLine($"Email enviado exitosamente a: {toEmail}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error enviando email a {toEmail}: {ex.Message}");
                Console.WriteLine($"Error email: {ex.Message}");
            }
        }
    }

}

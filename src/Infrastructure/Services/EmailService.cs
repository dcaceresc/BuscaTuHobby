using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;

namespace Infrastructure.Services;

public class EmailService(IConfiguration configuration) : IEmailService
{
    private readonly IConfiguration _configuration = configuration;

    public async Task SendEmailAsync(string email, string subject, string body)
    {
        var smtpServer = "smtp.gmail.com";
        var port = 587;
        var smtpUsername = "buscatuhobby.cl@gmail.com";
        var smtpPassword = "";

        using (var smtpClient = new SmtpClient(smtpServer, port))
        {
            smtpClient.EnableSsl = true;
            smtpClient.Credentials = new NetworkCredential(smtpUsername, smtpPassword);

            var mailMessage = new MailMessage
            {
                From = new MailAddress(smtpUsername),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            mailMessage.To.Add(email);

            await smtpClient.SendMailAsync(mailMessage);
        };
    }


    public string GetEmailRegisterTemplate(Guid userId, string token )
    {
        // Obtener la URL base desde appsettings
        var baseUrl = _configuration["BaseUrl"];

        var htmlContent = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "EmailTemplates", "Register.html"));

        htmlContent = htmlContent.Replace("{{url}}", $"{baseUrl}/security/account/confirm-email/{userId}/{token}");

        return htmlContent.ToString();
    }
}

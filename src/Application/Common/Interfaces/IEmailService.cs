namespace Application.Common.Interfaces;
public interface IEmailService
{
    Task SendEmailAsync(string email, string subject, string body);
    string GetEmailRegisterTemplate(Guid userId, string token);

}

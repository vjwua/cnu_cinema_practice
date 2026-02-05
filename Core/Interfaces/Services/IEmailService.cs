namespace Core.Interfaces.Services;

public interface IEmailService
{
    Task SendEmailAsync(string toEmail, string subject, string htmlBody);
    Task SendPasswordResetEmailAsync(string toEmail, string resetLink);
}

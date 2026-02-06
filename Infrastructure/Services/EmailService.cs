using System.Net;
using System.Net.Mail;
using Core.Interfaces.Services;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services;

public class EmailService(IOptions<EmailSettings> settings) : IEmailService
{
    private readonly EmailSettings _settings = settings.Value;

    public async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
    {
        var message = new MailMessage
        {
            From = new MailAddress(_settings.FromEmail, _settings.FromName),
            Subject = subject,
            Body = htmlBody,
            IsBodyHtml = true
        };

        message.To.Add(toEmail);

        using var client = new SmtpClient(_settings.SmtpServer, _settings.SmtpPort)
        {
            Credentials = new NetworkCredential(
                _settings.SmtpUsername,
                _settings.SmtpPassword
            ),
            EnableSsl = _settings.EnableSsl
        };

        await client.SendMailAsync(message);
    }

    public async Task SendPasswordResetEmailAsync(string toEmail, string resetLink)
    {
        const string subject = "Reset your password";
        var body = $"""

                            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: auto;'>
                                <h2>Password Reset</h2>
                                <p>You requested a password reset.</p>
                                <p>
                                    <a href='{resetLink}'
                                       style='display:inline-block;padding:10px 20px;
                                              background:#0d6efd;color:#fff;
                                              text-decoration:none;border-radius:4px;'>
                                        Reset Password
                                    </a>
                                </p>
                                <p>If you didn't request this, you can safely ignore this email.</p>
                            </div>
                    """;

        await SendEmailAsync(toEmail, subject, body);
    }
}
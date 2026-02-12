using System.Net;
using System.Net.Mail;
using Core.Interfaces.Services;
using Microsoft.Extensions.Options;
using System.Net.Mime;
using Core.Interfaces.Repositories;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Services;

public class EmailService(
    IOptions<EmailSettings> settings,
    IOrderRepository orderRepository,
    ITicketService ticketService,
    UserManager<ApplicationUser> userManager) : IEmailService
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

        using var client = new SmtpClient(_settings.SmtpServer, _settings.SmtpPort);
        client.Credentials = new NetworkCredential(
            _settings.SmtpUsername,
            _settings.SmtpPassword
        );
        client.EnableSsl = _settings.EnableSsl;

        await client.SendMailAsync(message);
    }

    private async Task SendEmailWithAttachmentAsync(string toEmail, string subject, string htmlBody, byte[] attachment,
        string attachmentName)
    {
        var message = new MailMessage
        {
            From = new MailAddress(_settings.FromEmail, _settings.FromName),
            Subject = subject,
            Body = htmlBody,
            IsBodyHtml = true
        };

        message.To.Add(toEmail);

        if (attachment is { Length: > 0 })
        {
            var stream = new MemoryStream(attachment);
            var attached = new Attachment(stream, attachmentName, MediaTypeNames.Application.Pdf);
            message.Attachments.Add(attached);
        }

        using var client = new SmtpClient(_settings.SmtpServer, _settings.SmtpPort);
        client.Credentials = new NetworkCredential(
            _settings.SmtpUsername,
            _settings.SmtpPassword
        );
        client.EnableSsl = _settings.EnableSsl;

        await client.SendMailAsync(message);
    }

    public async Task SendPasswordResetEmailAsync(string toEmail, string resetLink)
    {
        const string subject = "Reset your password for CNU Cinema";
        var body = $"""
                    <!DOCTYPE html>
                    <html>
                    <body style="margin: 0; padding: 0; background-color: #f8f9fa; color: #333333; font-family: 'Helvetica Neue', Helvetica, Arial, sans-serif;">
                    <div style="max-width: 600px; margin: 40px auto; background-color: #ffffff; border-radius: 8px; box-shadow: 0 4px 6px rgba(0,0,0,0.05); overflow: hidden; border: 1px solid #e5e7eb;">
                    <div style="background-color: #000000; padding: 30px 0; text-align: center;">
                        <h1 style="margin: 0; color: #ffffff; font-size: 24px; letter-spacing: 2px; font-weight: 700; text-transform: uppercase;">CNU Cinema</h1>
                    </div>
                    <div style="padding: 40px; text-align: center;">
                        <h2 style="color: #1a1a1a; margin-top: 0; font-size: 22px; font-weight: 600;">Reset Your Password</h2>
                        <p style="font-size: 16px; line-height: 1.6; color: #4a4a4a; margin-bottom: 30px;">
                            We received a request to reset your password. Click the button below to create a new one:
                        </p>
                        <a href="{resetLink}" style="display: inline-block; padding: 14px 28px; background-color: #000000; color: #ffffff; text-decoration: none; border-radius: 6px; font-weight: bold; font-size: 14px;">RESET PASSWORD</a>
                        <p style="margin-top: 30px; font-size: 13px; color: #888888;">
                            If you didn't request this change, you can safely ignore this email.
                        </p>
                    </div>
                    <div style="background-color: #f8f9fa; padding: 20px; text-align: center; font-size: 12px; color: #6b7280; border-top: 1px solid #e5e7eb;">
                        &copy; {DateTime.Now.Year} CNU Cinema. All rights reserved.
                    </div>
                    </div>
                    </body>
                    </html>
                    """;

        await SendEmailAsync(toEmail, subject, body);
    }

    public async Task SendTicketsAsync(int orderId)
    {
        var order = await orderRepository.GetByIdAsync(orderId);
        if (order == null) return;

        var user = await userManager.FindByIdAsync(order.UserId);
        if (user == null || string.IsNullOrEmpty(user.Email)) return;

        var pdfBytes = await ticketService.GeneratePdfAsync(orderId);

        var subject = $"Your Tickets for {order.Session.Movie.Name}";

        var body = $"""
                    <div style='font-family: Arial, sans-serif; max-width: 600px; margin: auto;'>
                         <h2>Your Tickets</h2>
                         <p>Thank you for your purchase!</p>
                         <p>Please find your tickets attached to this email.</p>
                         <p>
                             <strong>Movie:</strong> {order.Session.Movie.Name}<br/>
                             <strong>Time:</strong> {order.Session.StartTime:f}<br/>
                             <strong>Hall:</strong> {order.Session.Hall.Name}
                         </p>
                         <p>Order ID: {order.Id}</p>
                    </div>
                    """;

        await SendEmailWithAttachmentAsync(user.Email, subject, body, pdfBytes, $"Tickets_{order.Id}.pdf");
    }
}
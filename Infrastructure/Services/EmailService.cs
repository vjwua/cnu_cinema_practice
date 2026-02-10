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

    public async Task SendTicketsAsync(int orderId)
    {
        var order = await orderRepository.GetByIdAsync(orderId);
        if (order == null) return;

        var user = await userManager.FindByIdAsync(order.UserId);
        if (user == null || string.IsNullOrEmpty(user.Email)) return;

        var pdfBytes = await ticketService.GeneratePdfAsync(orderId);

        var subject = $"Your Tickets for {order.Session.Movie.Name}";

        var ticketsHtml = new System.Text.StringBuilder();
        foreach (var ticket in order.Tickets)
        {
            ticketsHtml.Append($"""
                                    <div style='border: 1px solid #ddd; padding: 10px; margin-bottom: 10px; border-radius: 5px;'>
                                        <p><strong>Seat:</strong> Row {ticket.SeatReservation.Seat.RowNum}, Seat {ticket.SeatReservation.Seat.SeatNum}</p>
                                        <img src="data:image/png;base64,{ticket.QrCodeBase64}" alt="QR Code" style="width: 150px; height: 150px;" />
                                    </div>
                                """);
        }

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
                         
                         <h3>QR Codes</h3>
                         {ticketsHtml}
                    </div>
                    """;

        await SendEmailWithAttachmentAsync(user.Email, subject, body, pdfBytes, $"Tickets_{order.Id}.pdf");
    }
}
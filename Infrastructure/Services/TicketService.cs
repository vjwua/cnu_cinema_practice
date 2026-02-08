using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Core.Interfaces.Services;
using Core.Interfaces.Repositories;
using Microsoft.Extensions.Options;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using QRCoder;
using Core.DTOs.Ticket;

namespace Infrastructure.Services;

public class TicketService : ITicketService
{
    private readonly IOrderRepository _orderRepository;
    private readonly TicketSettings _settings;
    private readonly ITicketRepository _ticketRepository;

    public TicketService(IOrderRepository orderRepository, IOptions<TicketSettings> settings, ITicketRepository ticketRepository)
    {
        _orderRepository = orderRepository;
        _settings = settings.Value;
        _ticketRepository = ticketRepository;
        
        if (string.IsNullOrWhiteSpace(_settings.SecretKey))
        {
            throw new InvalidOperationException("TicketSettings:SecretKey is not configured.");
        }
    }

    public async Task<string> GenerateQrCodeAsync(int ticketId, int sessionId)
    {
        var hash = ComputeSha256Hex($"{ticketId}:{sessionId}:{_settings.SecretKey}");
        var qrDataText = $"TICKET:{ticketId}:{sessionId}:{hash}";

        var pngBytes = GenerateQrPngBytes(qrDataText);

        return Convert.ToBase64String(pngBytes);
    }

    public async Task<List<string>> GenerateQrCodesForOrderAsync(int orderId)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null)
            throw new KeyNotFoundException($"Order with ID {orderId} not found.");

        var result = new List<string>(order.Tickets.Count);
        
        var sessionId = order.SessionId;
        
        foreach (var ticket in order.Tickets)
        {
            var base64 = await GenerateQrCodeAsync(ticket.Id, sessionId);
            ticket.QrCodeBase64 = base64;
            result.Add(base64);
        }

        await _orderRepository.UpdateAsync(order);
        return result;
    }

    public async Task<byte[]> GeneratePdfAsync(int orderId)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order is null)
            throw new KeyNotFoundException($"Order with ID {orderId} not found.");
        
        await GenerateQrCodesForOrderAsync(orderId);

        order = await _orderRepository.GetByIdAsync(orderId);
        
        if (order is null)
            throw new KeyNotFoundException($"Order with ID {orderId} not found.");

        var doc = new PdfDocument();
        
        var titleFont = new XFont("Helvetica", 20, XFontStyle.Bold);
        var labelFont = new XFont("Helvetica", 11, XFontStyle.Bold);
        var valueFont = new XFont("Helvetica", 11, XFontStyle.Regular);
        var smallFont = new XFont("Helvetica", 8, XFontStyle.Regular);

        foreach (var ticket in order.Tickets.OrderBy(t => t.Id))
        {
            var page = doc.AddPage();
            page.Size = PdfSharpCore.PageSize.A4;
            
            using var gfx = XGraphics.FromPdfPage(page);
            
            const double margin = 40;   
            var contentWidth = page.Width - 2 * margin;
            
            var movieName = order.Session.Movie.Name;
            gfx.DrawString("Ticket", titleFont, XBrushes.Black, new XRect(margin, 30, contentWidth, 30), XStringFormats.TopLeft);
            gfx.DrawString($"Movie: {movieName}", new XFont("Helvetica", 14, XFontStyle.Bold), XBrushes.Black, new XRect(margin, 60, contentWidth, 20), XStringFormats.TopLeft);

            var y = 100.0;
            
            DrawRow(gfx, "Order ID:", order.Id.ToString(CultureInfo.InvariantCulture), margin, ref y, labelFont, valueFont);
            DrawRow(gfx, "Ticket ID:", ticket.Id.ToString(CultureInfo.InvariantCulture), margin, ref y, labelFont, valueFont);
            
            var dt = order.Session.StartTime.ToString("dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture);
            DrawRow(gfx, "Start: ", dt, margin, ref y, labelFont, valueFont);
            DrawRow(gfx, "Hall: ", order.Session.Hall.Name, margin, ref y, labelFont, valueFont);

            var rowNum = ticket.SeatReservation.Seat.RowNum;
            var seatNum = ticket.SeatReservation.Seat.SeatNum;
            DrawRow(gfx, "Seat: ", $"Row {rowNum}, Seat {seatNum}", margin, ref y, labelFont, valueFont);
            
            DrawRow(gfx, "Price: ", ticket.Price.ToString("0.00", CultureInfo.InvariantCulture), margin, ref y, labelFont, valueFont);

            var qrBase64 = ticket.QrCodeBase64;
            if (!string.IsNullOrWhiteSpace(qrBase64))
            {
                var qrBytes = Convert.FromBase64String(qrBase64);
                using var qrImage = XImage.FromStream(() => new MemoryStream(qrBytes));
                
                const double qrSize = 180;
                var qrX = margin + contentWidth - qrSize;
                var qrY = 120;
                
                gfx.DrawRectangle(XPens.Black, qrX, qrY, qrSize, qrSize);
                gfx.DrawImage(qrImage, qrX + 8, qrY + 8, qrSize - 16, qrSize - 16);
                
                gfx.DrawString("Scan at entrance", smallFont, XBrushes.Black, new XRect(qrX, qrY + qrSize + 6, qrSize, 12), XStringFormats.CenterRight);
            }
            
            var payloadHash = ComputeSha256Hex($"{ticket.Id}:{order.SessionId}:{_settings.SecretKey}");
            var payload = $"TICKET:{ticket.Id}:{order.SessionId}:{payloadHash}";
            gfx.DrawString(payload, smallFont, XBrushes.Gray, new XRect(margin, page.Height - 60, contentWidth, 40), XStringFormats.TopLeft);
        }
        
        using var ms = new MemoryStream();
        doc.Save(ms);
        return ms.ToArray();
    }
    
    public Task<bool> ValidateTicketAsync(string qrData, int sessionId)
    {
        var parsed = TryParseQr(qrData, out var ticketId, out var qrSessionId, out var qrHash);
        if (!parsed || qrSessionId != sessionId) return Task.FromResult(false);
        
        var expectedHash = ComputeSha256Hex($"{ticketId}:{sessionId}:{_settings.SecretKey}");
        if (!FixedTimeEqualsHexString(expectedHash, qrHash)) return Task.FromResult(false);
        
        return ValidateTicketExistsAsync(ticketId, sessionId);
    }

    private async Task<bool> ValidateTicketExistsAsync(int ticketId, int sessionId)
    {
        var ticket = await _ticketRepository.GetForScanAsync(ticketId);
        if (ticket is null) return false;

        if (ticket.SeatReservation.SessionId != sessionId) return false;
        if (ticket.Order.Session.Id != sessionId) return false;
        
        return true;
    }

    public async Task<TicketValidationResult> ScanTicketAsync(string qrData, string scannedByUserId)
    {
        var result = new TicketValidationResult();
        
        if (!TryParseQr(qrData, out var ticketId, out var qrSessionId, out var qrHash))
        {
            result.IsValid = false;
            result.Error = "Invalid QR code.";
            return result;
        }
        
        result.TicketId = ticketId;
        result.SessionId = qrSessionId;
        
        var expectedHash = ComputeSha256Hex($"{ticketId}:{qrSessionId}:{_settings.SecretKey}");
        if (!FixedTimeEqualsHexString(expectedHash, qrHash))
        {
            result.IsValid = false;
            result.Error = "Invalid QR code.";
            return result;       
        }
        
        var ticket = await _ticketRepository.GetForScanAsync(ticketId);
        if (ticket is null)
        {
            result.IsValid = false;
            result.Error = "Ticket not found.";
            return result;
        }

        if (ticket.SeatReservation.SessionId != qrSessionId || ticket.Order.Session.Id != qrSessionId)
        {
            result.IsValid = false;
            result.Error = "Ticket does not belong to this session.";
            return result;       
        }
        
        result.AlreadyScanned = ticket.ScannedAt != null;
        result.ScannedAt = ticket.ScannedAt;
        result.ScannedBy = ticket.ScannedBy;
        
        result.MovieName = ticket.Order.Session.Movie.Name;
        result.StartTime = ticket.Order.Session.StartTime;
        result.HallName = ticket.Order.Session.Hall.Name;
        result.RowNum = ticket.SeatReservation.Seat.RowNum;
        result.SeatNum = ticket.SeatReservation.Seat.SeatNum;
        
        if (ticket.ScannedAt != null)
        {
            result.IsValid = true;
            return result;
        }

        ticket.ScannedAt = DateTime.UtcNow;
        ticket.ScannedBy = scannedByUserId;

        await _ticketRepository.SaveChangesAsync();

        result.IsValid = true;
        result.AlreadyScanned = false;
        result.ScannedAt = ticket.ScannedAt;
        result.ScannedBy = ticket.ScannedBy;
        
        return result;
    }

    private static bool TryParseQr(string qrData, out int ticketId, out int sessionId, out string hash)
    {
        ticketId = 0;
        sessionId = 0;
        hash = string.Empty;

        if (string.IsNullOrWhiteSpace(qrData))
        {
            return false;
        }
        
        var parts = qrData.Split(':', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length != 4) return false;
        if (!parts[0].Equals("TICKET", StringComparison.OrdinalIgnoreCase)) return false;
        if (!int.TryParse(parts[1], out ticketId)) return false;
        if (!int.TryParse(parts[2], out sessionId)) return false;
        hash = parts[3];
        return hash.Length == 64;
    }

    private static bool FixedTimeEqualsHexString(string a, string b)
    {
        if (a.Length != b.Length) return false;
        
        var aBytes = Convert.FromHexString(a);
        var bBytes = Convert.FromHexString(b);
        
        return CryptographicOperations.FixedTimeEquals(aBytes, bBytes);
    }

    private static void DrawRow(XGraphics gfx, string label, string value, double x, ref double y, XFont labelFont,
        XFont valueFont)
    {
        const double lineHeight = 18;
        gfx.DrawString(label, labelFont, XBrushes.Black, new XRect(x, y, 120, lineHeight), XStringFormats.TopLeft);
        gfx.DrawString(value, valueFont, XBrushes.Black, new XRect(x + 120, y, 360, lineHeight), XStringFormats.TopLeft);
        y += lineHeight + 6;
    }
    
    private byte[] GenerateQrPngBytes(string payload)
    {
        using var qrGenerator = new QRCodeGenerator();
        using var data = qrGenerator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.Q);
        var qrCode = new PngByteQRCode(data);
            
        var pixelsPerModule = Math.Max(5, _settings.QrCodeSize / 40);
        return qrCode.GetGraphic(pixelsPerModule);
    }

    private static string ComputeSha256Hex(string input)
    {
        var bytes = Encoding.UTF8.GetBytes(input);
        var hashBytes = SHA256.HashData(bytes);
            
        var sb = new StringBuilder(hashBytes.Length * 2);
        foreach (var b in hashBytes)
            sb.Append(b.ToString("x2"));
            
        return sb.ToString();
    }
}
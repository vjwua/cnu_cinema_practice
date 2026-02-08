using Core.DTOs.Ticket;

namespace Core.Interfaces.Services;

public interface ITicketService
{
    Task<string> GenerateQrCodeAsync(int ticketId, int sessionId);
    Task<List<string>> GenerateQrCodesForOrderAsync(int orderId);
    Task<byte[]> GeneratePdfAsync(int orderId);
    Task<bool> ValidateTicketAsync(string qrData, int sessionId);
    Task<TicketValidationResult> ScanTicketAsync(string qrData, string scannedByUserId);
}
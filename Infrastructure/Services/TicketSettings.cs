namespace Infrastructure.Services;

public sealed class TicketSettings
{
    public const string SectionName = "TicketSettings";
    public string SecretKey { get; set; } = string.Empty;
    public int QrCodeSize { get; set; } = 200;
    public int ExpirationMinutes { get; set; } = 15;
}
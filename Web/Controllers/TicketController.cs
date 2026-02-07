using Core.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace cnu_cinema_practice.Controllers;

[ApiController]
public class TicketController : ControllerBase
{
    private readonly ITicketService _ticketService;
    
    public TicketController(ITicketService ticketService)
    {
        _ticketService = ticketService;
    }

    [HttpGet("tickets/{ticketId:int}/qr")]
    public async Task<IActionResult> GetQrPng(int ticketId, [FromQuery] int sessionId)
    {
        var base64 = await _ticketService.GenerateQrCodeAsync(ticketId, sessionId);
        var bytes = Convert.FromBase64String(base64);
        
        return File(bytes, "image/png");
    }
}
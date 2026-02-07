using System.Security.Claims;
using Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace cnu_cinema_practice.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize]
public sealed class TicketsController(ITicketService ticketService) : Controller
{
    [HttpGet("/admin/tickets/scan")]
    public async Task<IActionResult> Scan([FromQuery] string qrData)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "unknown";
        var res = await ticketService.ScanTicketAsync(qrData, userId);
        return Ok(res);
    }
}
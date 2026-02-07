using Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace cnu_cinema_practice.Controllers;

[Authorize]
public class OrdersController : Controller
{
    private readonly ITicketService _ticketService;
    public OrdersController(ITicketService ticketService)
    {
        _ticketService = ticketService;
    }
    
    // Details page UI will be implemented later.
    [HttpGet("/orders/{id:int}")]
    public IActionResult Details(int id)
    {
        return StatusCode(StatusCodes.Status501NotImplemented,
            "Order details page is not implemented yet.");
    }
    
    [HttpGet("/orders/{orderId:int}/tickets.pdf")]
    public async Task<IActionResult> TicketsPdf(int orderId)
    {
        var pdfBytes = await _ticketService.GeneratePdfAsync(orderId);
        return File(pdfBytes, "application/pdf", $"tickets-order-{orderId}.pdf");
    }
}
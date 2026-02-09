using Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace cnu_cinema_practice.Controllers;

[Authorize]
public class OrderController(ITicketService ticketService, IOrderService orderService) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        try
        {
            var order = await orderService.GetByIdAsync(id);

            ViewBag.Order = order;
            ViewBag.SuccessMessage = TempData["Success"];

            return View();
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error loading order details: {ex.Message}";
            return RedirectToAction("Index", "Home");
        }
    }

    [HttpGet]
    public async Task<IActionResult> TicketsPdf(int orderId)
    {
        var pdfBytes = await ticketService.GeneratePdfAsync(orderId);
        return File(pdfBytes, "application/pdf", $"tickets-order-{orderId}.pdf");
    }
}
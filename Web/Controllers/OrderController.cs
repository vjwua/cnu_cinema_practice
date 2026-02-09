using AutoMapper;
using Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using cnu_cinema_practice.ViewModels.Account;

namespace cnu_cinema_practice.Controllers;

[Authorize]
public class OrderController(
    ITicketService ticketService, 
    IOrderService orderService,
    IMapper mapper) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        try
        {
            var orderDto = await orderService.GetByIdAsync(id);
            
            if (orderDto == null)
            {
                TempData["Error"] = "Order not found.";
                return RedirectToAction("Index", "Home");
            }
            
            var viewModel = mapper.Map<OrderViewModel>(orderDto);
            
            if (viewModel.Status == Core.Enums.OrderStatus.Pending)
            {
                var expirationTime = viewModel.CreatedAt.AddMinutes(15);
                ViewBag.ExpiresAt = expirationTime;
                ViewBag.IsExpired = DateTime.Now > expirationTime;
            }
            
            ViewBag.SuccessMessage = TempData["Success"];
            
            return View(viewModel);
        }
        catch (HttpRequestException ex)
        {
            TempData["Error"] = $"Error loading order details: {ex.Message}";
            return RedirectToAction("Index", "Home");
        }
    }

    [HttpGet]
    public async Task<IActionResult> TicketsPdf(int orderId)
    {
        try
        {
            var pdfBytes = await ticketService.GeneratePdfAsync(orderId);
            return File(pdfBytes, "application/pdf", $"tickets-order-{orderId}.pdf");
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = $"Error generating PDF: {ex.Message}";
            return RedirectToAction("Details", new { id = orderId });
        }
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CancelOrder(int id)
    {
        try
        {
            await orderService.CancelOrderAsync(id);
            TempData["Success"] = "Order cancelled successfully.";
            return RedirectToAction("Details", new { id });
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = $"Error cancelling order: {ex.Message}";
            return RedirectToAction("Details", new { id });
        }
    }
}
using AutoMapper;
using cnu_cinema_practice.ViewModels.Account;
using Core.Enums;
using Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http.HttpResults;

namespace cnu_cinema_practice.Controllers;

[Authorize]
public class OrderController(
    ITicketService ticketService,
    IOrderService orderService,
    IMapper mapper,
    ILogger<OrderController> logger) : Controller
{
    [HttpGet]
    public async Task<IResult> Details(int id)
    {
        try
        {
            var orderDto = await orderService.GetByIdAsync(id);

            if (orderDto == null)
            {
                TempData["Error"] = "Order not found.";
                return Results.Redirect(Url.Action("Index", "Home", new { area = "" })!);
            }

            var parsedOrderStatus = Enum.Parse<OrderStatus>(orderDto.Status);

            if (parsedOrderStatus == OrderStatus.Pending)
            {
                var expirationTime = orderDto.CreatedAt.AddMinutes(15);
                if (DateTime.UtcNow > expirationTime)
                {
                    await orderService.ExpireOrderAsync(id);
                    orderDto = await orderService.GetByIdAsync(id);
                }
            }

            var viewModel = mapper.Map<OrderViewModel>(orderDto);

            if (viewModel.Status == OrderStatus.Pending)
            {
                var createdAtUtc = viewModel.CreatedAt.Kind == DateTimeKind.Utc
                    ? viewModel.CreatedAt
                    : DateTime.SpecifyKind(viewModel.CreatedAt, DateTimeKind.Utc);

                var expirationTime = createdAtUtc.AddMinutes(15);
                viewModel.ExpiresAt = expirationTime;
            }

            return new RazorComponentResult<cnu_cinema_practice.Components.Pages.Order.OrderDetails>(new { Model = viewModel });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error loading order details for ID {OrderId}", id);
            TempData["Error"] = $"Error loading order details: {ex.Message}";
            return Results.Redirect(Url.Action("Index", "Home", new { area = "" })!);
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
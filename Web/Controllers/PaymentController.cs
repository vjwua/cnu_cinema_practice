using AutoMapper;
using cnu_cinema_practice.ViewModels;
using Core.DTOs.Orders;
using Core.DTOs.Payments;
using Core.Enums;
using Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace cnu_cinema_practice.Controllers
{
    [Authorize]
    public class PaymentController(
        IPaymentService paymentService,
        IOrderService orderService,
        ITicketService ticketService,
        IEmailService emailService,
        IMapper mapper) : Controller
    {
        [HttpGet]
        public async Task<IResult> Index(int orderId)
        {
            try
            {
                var order = await orderService.GetByIdAsync(orderId);

                var viewModel = mapper.Map<PaymentViewModel>(order);

                var ticketSubtotal = order.Tickets.Sum(t => t.Price);
                viewModel.TotalAmount = ticketSubtotal > 0 ? ticketSubtotal : order.TotalPrice;
                viewModel.BasePrice = order.Tickets.Count > 0 ? ticketSubtotal / order.Tickets.Count : 0;

                viewModel.SelectedSeats = GetSelectedSeats(order);

                viewModel.AvailablePaymentMethods = GetAvailablePaymentMethods();
                viewModel.SelectedPaymentMethod = PaymentMethod.Card;

                return new Microsoft.AspNetCore.Http.HttpResults.RazorComponentResult<cnu_cinema_practice.Components.Pages.Payment.Payment>(new { Model = viewModel });
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error loading payment page: {ex.Message}";
                return Results.Redirect(Url.Action("Index", "Home")!);
            }
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IResult> ProcessPayment(PaymentViewModel paymentViewModel)
        {
            ValidatePaymentModel(paymentViewModel);

            if (!ModelState.IsValid)
            {
                paymentViewModel.AvailablePaymentMethods = GetAvailablePaymentMethods();
                return new Microsoft.AspNetCore.Http.HttpResults.RazorComponentResult<cnu_cinema_practice.Components.Pages.Payment.Payment>(new { Model = paymentViewModel });
            }

            try
            {
                var createPaymentDto = mapper.Map<CreatePaymentDTO>(paymentViewModel);

                Console.WriteLine($"[DEBUG] Processing payment for OrderId: {paymentViewModel.OrderId}");
                await paymentService.ProcessPaymentAsync(createPaymentDto);

                Console.WriteLine($"[DEBUG] Updating order status to Paid for OrderId: {paymentViewModel.OrderId}");
                await orderService.UpdateOrderStatusAsync(paymentViewModel.OrderId, OrderStatus.Paid);

                try
                {
                    Console.WriteLine($"[DEBUG] Generating QR codes for OrderId: {paymentViewModel.OrderId}");
                    await ticketService.GenerateQrCodesForOrderAsync(paymentViewModel.OrderId);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] QR code generation failed for OrderId: {paymentViewModel.OrderId}: {ex}");
                    // We might still want to proceed if the order is marked as paid, 
                    // or handle this as a critical failure. For now, let's just log.
                }

                try
                {
                    Console.WriteLine($"[DEBUG] Sending tickets for OrderId: {paymentViewModel.OrderId}");
                    await emailService.SendTicketsAsync(paymentViewModel.OrderId);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] Email sending failed for OrderId: {paymentViewModel.OrderId}: {ex}");
                }

                TempData["Success"] = "Payment successful! Your tickets have been sent to your email.";
                return Results.Redirect(Url.Action("Details", "Order", new { id = paymentViewModel.OrderId, area = "" })!);
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"[ERROR] Payment processing failed (InvalidOperation): {ex}");
                ModelState.AddModelError(string.Empty, ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Unexpected error during payment processing: {ex}");
                ModelState.AddModelError(string.Empty,
                    "An unexpected error occurred while processing your payment. Please contact support.");
            }

            paymentViewModel.AvailablePaymentMethods = GetAvailablePaymentMethods();
            await ReloadPaymentViewModelAsync(paymentViewModel);
            paymentViewModel.Errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
            return new Microsoft.AspNetCore.Http.HttpResults.RazorComponentResult<cnu_cinema_practice.Components.Pages.Payment.Payment>(new { Model = paymentViewModel });
        }

        private async Task ReloadPaymentViewModelAsync(PaymentViewModel viewModel)
        {
            var order = await orderService.GetByIdAsync(viewModel.OrderId);

            viewModel.MovieTitle = order.MovieTitle;
            viewModel.MoviePosterUrl = order.MoviePosterUrl ?? string.Empty;
            viewModel.ShowDateTime = order.SessionStart;
            viewModel.HallName = order.HallName;

            var ticketSubtotal = order.Tickets.Sum(t => t.Price);
            viewModel.TotalAmount = ticketSubtotal > 0 ? ticketSubtotal : order.TotalPrice;
            viewModel.BasePrice = order.Tickets.Count > 0 ? ticketSubtotal / order.Tickets.Count : 0;

            viewModel.SelectedSeats = GetSelectedSeats(order);
        }

        #region Helper Methods

        private static List<string> GetSelectedSeats(OrderDTO order)
        {
            return (from ticket in order.Tickets
                    let rowLetter = (char)('A' + ticket.RowNum)
                    let seatNumber = ticket.SeatNum + 1
                    select $"{rowLetter}{seatNumber}").ToList();
        }

        private void ValidatePaymentModel(PaymentViewModel model)
        {
            if (model.SelectedPaymentMethod == PaymentMethod.Card)
            {
                if (string.IsNullOrWhiteSpace(model.CardNumber))
                    ModelState.AddModelError(nameof(model.CardNumber), "Card number is required.");
                if (string.IsNullOrWhiteSpace(model.ExpiryDate))
                    ModelState.AddModelError(nameof(model.ExpiryDate), "Expiry date is required.");
                if (string.IsNullOrWhiteSpace(model.Cvv))
                    ModelState.AddModelError(nameof(model.Cvv), "CVV is required.");
                if (string.IsNullOrWhiteSpace(model.CardholderName))
                    ModelState.AddModelError(nameof(model.CardholderName), "Cardholder name is required.");
            }
            else
            {
                ModelState.Remove(nameof(model.CardNumber));
                ModelState.Remove(nameof(model.ExpiryDate));
                ModelState.Remove(nameof(model.Cvv));
                ModelState.Remove(nameof(model.CardholderName));
            }
        }

        private static List<PaymentMethod> GetAvailablePaymentMethods()
        {
            return Enum.GetValues<PaymentMethod>()
                .Where(method => method != PaymentMethod.Cash)
                .ToList();
        }

        #endregion
    }
}
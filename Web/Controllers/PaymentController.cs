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
        public async Task<IActionResult> Index(int orderId)
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

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error loading payment page: {ex.Message}";
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessPayment(PaymentViewModel paymentViewModel)
        {
            ValidatePaymentModel(paymentViewModel);

            if (!ModelState.IsValid)
            {
                paymentViewModel.AvailablePaymentMethods = GetAvailablePaymentMethods();
                return View("Index", paymentViewModel);
            }

            try
            {
                var createPaymentDto = mapper.Map<CreatePaymentDTO>(paymentViewModel);

                await paymentService.ProcessPaymentAsync(createPaymentDto);

                await orderService.UpdateOrderStatusAsync(paymentViewModel.OrderId, OrderStatus.Paid);
                await ticketService.GenerateQrCodesForOrderAsync(paymentViewModel.OrderId);
                await emailService.SendTicketsAsync(paymentViewModel.OrderId);

                TempData["Success"] = "Payment successful! Your tickets have been sent to your email.";
                return RedirectToAction("Details", "Order", new { id = paymentViewModel.OrderId, area = "" });
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty,
                    $"An unexpected error occurred: {ex}");
            }

            paymentViewModel.AvailablePaymentMethods = GetAvailablePaymentMethods();
            return View("Index", paymentViewModel);
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
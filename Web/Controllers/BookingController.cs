using AutoMapper;
using cnu_cinema_practice.ViewModels;
using Core.DTOs.Seats;
using Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Core.DTOs.Sessions;
using Core.DTOs.Movies;
using Core.Entities;

namespace cnu_cinema_practice.Controllers
{
    public class BookingController(
        ISessionService sessionService,
        IMovieService movieService,
        ISeatService seatService,
        IHallService halLService,
        IOrderService orderService,
        IMapper mapper) : Controller
    {
        [HttpGet("Booking/SelectSeats")]
        [AllowAnonymous]
        [IgnoreAntiforgeryToken]
        public async Task<IResult> SelectSeats(int sessionId, string alert = "")
        {
            Console.WriteLine($"[DEBUG] BookingController.SelectSeats hit with sessionId={sessionId}");
            try
            {
                var session = await sessionService.GetSessionByIdAsync(sessionId);
                if (session == null)
                    return Results.NotFound($"Session not found {sessionId}");
                var movie = await movieService.GetByIdAsync(session.MovieId);

                var viewModel = mapper.Map<BookingViewModel>(session);

                viewModel.AvailableShowtimes = new List<ShowtimeOption>();
                foreach (var showTime in await sessionService.GetSessionsByMovieIdAsync(session.MovieId))
                {
                    viewModel.AvailableShowtimes.Add(mapper.Map<ShowtimeOption>(showTime));
                }

                viewModel.Name = movie.Name;
                viewModel.PosterUrl = movie.PosterUrl;
                viewModel.alertMessage = alert;

                Console.WriteLine($"[DEBUG] Fetching hall details for HallId={viewModel.HallId}");
                viewModel.HallData = await halLService.GetByIdAsync(viewModel.HallId);
                if (viewModel.HallData == null)
                {
                    Console.WriteLine($"[DEBUG] HallData is NULL for HallId={viewModel.HallId}");
                    return Results.NotFound($"Hall not found {viewModel.HallId}");
                }
                Console.WriteLine($"[DEBUG] Fetched hall: {viewModel.HallData.Name}");
                // Nullify collections to avoid circular references during Blazor parameter serialization
                viewModel.HallData.Seats = null;
                viewModel.HallData.Sessions = null;

                // Create seat layout
                viewModel.SeatLayout = await seatService.GetAvailableSeatsAsync(sessionId);
                byte[][] layout = new byte[viewModel.HallData.Rows][];
                for (int i = 0; i < viewModel.HallData.Rows; i++)
                {
                    layout[i] = new byte[viewModel.HallData.Columns];
                }

                foreach (var seat in viewModel.SeatLayout)
                {
                    if (seat.RowNum < viewModel.HallData.Rows
                        && seat.SeatNum < viewModel.HallData.Columns)
                    {
                        layout[seat.RowNum][seat.SeatNum] = (byte)seat.SeatTypeId;
                    }
                }

                viewModel.LayoutArray = layout;
                viewModel.alertMessage = alert;

                var seattypes = (await seatService.GetSeatTypesAsync()).ToList();
                decimal[] seatprices = new decimal[seattypes.Any() ? seattypes.Max(t => t.Id) + 1 : 10];
                foreach (var type in seattypes)
                {
                    seatprices[type.Id] = type.AddedPrice;
                }

                viewModel.addedPrice = seatprices;

                return new Microsoft.AspNetCore.Http.HttpResults.RazorComponentResult<cnu_cinema_practice.Components.Pages.Movies.SelectSeats>(new { Model = viewModel });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] BookingController.SelectSeats failed: {ex}");
                TempData["Error"] = $"Error loading booking page: {ex.Message}";
                return Results.Problem(ex.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IResult> Checkout(int sessionId, string selectedSeats, string selectedNumeric)
        {
            if (User.Identity is { IsAuthenticated: false })
            {
                return Results.Redirect(Url.Action("Login", "Account", new
                {
                    area = "",
                    returnUrl = Url.Action("ResumeCheckout", "Booking",
                        new { area = "", sessionId, selectedSeats, selectedNumeric })
                })!);
            }

            return await ProcessCheckout(sessionId, selectedSeats, selectedNumeric);
        }

        [HttpGet]
        [Authorize]
        public async Task<IResult> ResumeCheckout(int sessionId, string selectedSeats, string selectedNumeric)
        {
            return await ProcessCheckout(sessionId, selectedSeats, selectedNumeric);
        }

        private async Task<IResult> ProcessCheckout(int sessionId, string selectedSeats, string selectedNumeric)
        {
            if (string.IsNullOrEmpty(selectedSeats))
            {
                TempData["Error"] = "Please select at least one seat.";
                return Results.Redirect(Url.Action("SelectSeats", new { sessionId = sessionId, Area = "" })!);
            }

            try
            {
                var session = await sessionService.GetSessionByIdWithSeatsAsync(sessionId);
                if (session == null) return Results.NotFound("Session not found");
                var movie = await movieService.GetByIdAsync(session.MovieId);

                var seatList = selectedNumeric.Split(',').ToList();
                var allSeats = (await seatService.GetBySessionIdAsync(sessionId)).ToList();
                var desiredSeats = ParseSelectedSeats(seatList, allSeats);

                var unavailableSeats = await ValidateSeatAvailabilityAsync(desiredSeats, sessionId);
                if (unavailableSeats.Count != 0)
                {
                    var errorMsg =
                        $"The following seats are no longer available: {string.Join(", ", unavailableSeats.Select(s => $"Row {GetRowLetter(s.RowNum)} Seat {s.SeatNum + 1}"))}";
                    TempData["Error"] = errorMsg;
                    return Results.Redirect(Url.Action("SelectSeats", new { sessionId, alert = errorMsg, Area = "" })!);
                }

                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Results.Redirect(Url.Action("Login", "Account")!);
                }

                var seatTypes = await seatService.GetSeatTypesAsync();
                var reservationResult =
                    await ReserveSeatsAsync(desiredSeats, sessionId, userId, session.BasePrice, seatTypes);

                if (reservationResult.FailedResrvation != null)
                {
                    var errorMsg =
                        $"The following seats are no longer available: Row {GetRowLetter(reservationResult.FailedResrvation.RowNum)} Seat {reservationResult.FailedResrvation.SeatNum + 1}";
                    TempData["Error"] = errorMsg;
                    return Results.Redirect(Url.Action("SelectSeats", new { sessionId, alert = errorMsg, Area = "" })!);
                }

                var viewModel = BuildCheckoutViewModel(session, movie, seatList, reservationResult.ReservationIds,
                    reservationResult.TotalPrice, desiredSeats, seatTypes);

                return new Microsoft.AspNetCore.Http.HttpResults.RazorComponentResult<cnu_cinema_practice.Components.Pages.Movies.Checkout>(new { Model = viewModel });
            }
            catch (HttpRequestException ex)
            {
                TempData["Error"] = $"Error processing checkout: {ex.Message}";
                return Results.NotFound(ex.Message);
            }
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        [Authorize]
        public async Task<IResult> ConfirmBooking(CheckoutViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return new Microsoft.AspNetCore.Http.HttpResults.RazorComponentResult<cnu_cinema_practice.Components.Pages.Movies.Checkout>(new { Model = model });
            }

            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Results.Redirect(Url.Action("Login", "Account")!);
                }

                var seatReservationIds = model.ReservationIds;

                var createOrderDto = new Core.DTOs.Orders.CreateOrderDTO
                {
                    SeatReservationIds = seatReservationIds
                };

                var order = await orderService.CreateOrderAsync(userId, createOrderDto);

                return Results.Redirect(Url.Action("Index", "Payment", new { area = "", orderId = order.Id })!);
            }
            catch (HttpRequestException ex)
            {
                ModelState.AddModelError(string.Empty, $"Error confirming booking: {ex.Message}");
                return new Microsoft.AspNetCore.Http.HttpResults.RazorComponentResult<cnu_cinema_practice.Components.Pages.Movies.Checkout>(new { Model = model });
            }
        }

        [Authorize]
        public IResult Confirmation(int bookingId)
        {
            return new Microsoft.AspNetCore.Http.HttpResults.RazorComponentResult<cnu_cinema_practice.Components.Pages.Booking.Confirmation>(new { BookingId = bookingId });
        }

        #region Private Helper Methods

        private static string GetRowLetter(byte rowNum)
        {
            return ((char)('A' + rowNum)).ToString();
        }

        private static List<SeatDTO> ParseSelectedSeats(List<string> seatList, List<SeatDTO> allSeats)
        {
            var desiredSeats = new List<SeatDTO>();
            foreach (var coords in seatList.Select(seatStr => seatStr.Split('-')).Where(coords => coords.Length == 2))
            {
                if (!byte.TryParse(coords[0], out var row)) continue;
                if (!byte.TryParse(coords[1], out var col)) continue;

                var seat = allSeats.FirstOrDefault(s => s.RowNum == row && s.SeatNum == col);
                if (seat != null)
                {
                    desiredSeats.Add(seat);
                }
            }

            return desiredSeats;
        }

        private async Task<List<SeatDTO>> ValidateSeatAvailabilityAsync(List<SeatDTO> desiredSeats, int sessionId)
        {
            var availableSeats = (await seatService.GetAvailableSeatsAsync(sessionId)).Select(s => s.Id).ToHashSet();

            return desiredSeats.Where(seat => !availableSeats.Contains(seat.Id)).ToList();
        }

        private async Task<(List<int> ReservationIds, decimal TotalPrice, SeatDTO? FailedResrvation)> ReserveSeatsAsync(
            List<SeatDTO> seats, int sessionId, string userId, decimal basePrice, IEnumerable<SeatType> seatTypes)
        {
            var reservationIds = new List<int>();
            decimal totalPrice = 0;
            var typesList = seatTypes.ToList();

            foreach (var seat in seats)
            {
                var seatType = typesList.FirstOrDefault(st => st.Id == seat.SeatTypeId);
                var addedPrice = seatType?.AddedPrice ?? 0;
                var seatPrice = basePrice + addedPrice;

                var success = await seatService.ReserveSeatAsync(seat, sessionId, seatPrice, userId);
                if (!success)
                {
                    return ([], 0, seat);
                }

                totalPrice += seatPrice;

                var reservationId = await seatService.GetReservationIdAsync(seat.Id, sessionId);
                if (reservationId.HasValue)
                {
                    reservationIds.Add(reservationId.Value);
                }
            }

            return (reservationIds, totalPrice, null);
        }

        private CheckoutViewModel BuildCheckoutViewModel(
            SessionDetailDTO session, MovieDetailDTO movie, List<string> originalIds, List<int> reservationIds, decimal totalPrice,
            List<SeatDTO> seats, IEnumerable<SeatType> seatTypes)
        {
            var viewModel = mapper.Map<CheckoutViewModel>(session);
            mapper.Map(movie, viewModel);

            viewModel.SelectedSeats = originalIds;
            viewModel.ReservationIds = reservationIds;
            viewModel.Total = totalPrice;
            viewModel.SeatDetails = new List<SeatCheckoutItem>();

            var typesList = seatTypes.ToList();
            foreach (var seat in seats)
            {
                var seatType = typesList.FirstOrDefault(st => st.Id == seat.SeatTypeId);
                var addedPrice = seatType?.AddedPrice ?? 0;
                var seatPrice = session.BasePrice + addedPrice;

                var rowLetter = GetRowLetter(seat.RowNum);
                var seatNumber = $"{rowLetter}{seat.SeatNum + 1}";

                viewModel.SeatDetails.Add(new SeatCheckoutItem
                {
                    SeatNumber = seatNumber,
                    Type = seatType?.Name ?? "Standard",
                    Price = seatPrice
                });
            }

            return viewModel;
        }

        #endregion
    }
}
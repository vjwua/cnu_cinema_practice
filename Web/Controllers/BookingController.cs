using AutoMapper;
using cnu_cinema_practice.ViewModels;
using Core.DTOs.Seats;
using Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
        [HttpGet]
        public async Task<IActionResult> SelectSeats(int sessionId, string alert = "")
        {
            try
            {
                if (sessionId == 0) sessionId = 1075;
                // Get session details
                var session = await sessionService.GetSessionByIdAsync(sessionId);
                if (session == null)
                    return NotFound($"Session not found {sessionId}");
                var movie = await movieService.GetByIdAsync(session.MovieId);

                var viewModel = mapper.Map<BookingViewModel>(session);

                viewModel.AvailableShowtimes = new List<ShowtimeOption>();
                foreach (var showTime in await sessionService.GetSessionsByMovieIdAsync(session.MovieId))
                {
                    viewModel.AvailableShowtimes.Add(mapper.Map<ShowtimeOption>(showTime));
                }

                // Set movie details
                viewModel.Name = movie.Name;
                viewModel.PosterUrl = movie.PosterUrl;

                viewModel.HallData = await halLService.GetByIdAsync(viewModel.HallId);

                // Get seats for this session
                /*var seats = await seatService.GetBySessionIdAsync(sessionId);
                var seatsList = seats.ToList();*/

                // Create seat layout
                viewModel.SeatLayout = await seatService.GetAvailableSeatsAsync(sessionId);
                byte[,] layout = new byte[viewModel.HallData.Rows, viewModel.HallData.Columns];
                foreach (var seat in viewModel.SeatLayout)
                {
                    if (seat.RowNum < viewModel.HallData.Rows
                        && seat.SeatNum < viewModel.HallData.Columns)
                    {
                        layout[seat.RowNum, seat.SeatNum] = (byte)seat.SeatTypeId;
                    }
                }

                viewModel.LayoutArray = layout;
                viewModel.alertMessage = alert;

                var seattypes = await seatService.GetSeatTypesAsync();
                decimal[] seatprices = new decimal[seattypes.Count()];
                foreach (var type in seattypes)
                {
                    seatprices[type.Id] = type.AddedPrice;
                }

                viewModel.addedPrice = seatprices;

                return View(viewModel);
            }
            catch (HttpRequestException ex)
            {
                TempData["Error"] = $"Error loading booking page: {ex.Message}";
                return NotFound(ex.Message);
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Checkout(int sessionId, string selectedSeats, string selectedNumeric)
        {
            if (string.IsNullOrEmpty(selectedSeats))
            {
                TempData["Error"] = "Please select at least one seat.";
                return RedirectToAction("SelectSeats", new { sessionId = sessionId, Area = "" });
            }

            try
            {
                var seatList = selectedNumeric.Split(',').ToList();

                var session = await sessionService.GetSessionByIdWithSeatsAsync(sessionId);
                if (session == null)
                    return NotFound("Session not found");

                var movie = await movieService.GetByIdAsync(session.MovieId);

                // Verify seats are available
                var seats = (await seatService.GetBySessionIdAsync(sessionId)).ToList();
                var availableSeats = (await seatService.GetAvailableSeatsAsync(sessionId)).Select(s => s.Id);

                var reservations = (from seat in seatList
                    select seat.Split('-')
                    into coords
                    let row = Byte.Parse(coords[0])
                    let col = Byte.Parse(coords[1])
                    from s in seats
                    where s.RowNum == row && s.SeatNum == col
                    select s).ToList();

                foreach (var reserve in reservations.Where(reserve => availableSeats.Contains(reserve.Id) == false))
                {
                    TempData["Error"] =
                        $"The following seats are no longer available: {string.Join(", ", reserve)}";
                    return RedirectToAction("SelectSeats", new
                    {
                        sessionId,
                        alert = $"The following seats are no longer available: {string.Join(", ", reserve)}",
                        Area = ""
                    });
                }

                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return RedirectToAction("Login", "Account");
                }

                var seatTypes = await seatService.GetSeatTypesAsync();
                decimal totalPrice = 0;

                foreach (var reserve in reservations)
                {
                    var seatType = seatTypes.FirstOrDefault(st => st.Id == reserve.SeatTypeId);
                    var addedPrice = seatType?.AddedPrice ?? 0;
                    var seatPrice = session.BasePrice + addedPrice;
                    totalPrice += seatPrice;

                    if (await seatService.ReserveSeatAsync(reserve, sessionId, seatPrice, userId)) continue;
                    TempData["Error"] = $"The following seats are no longer available: {string.Join(", ", reserve)}";
                    return RedirectToAction("SelectSeats", new
                    {
                        sessionId,
                        alert = $"The following seats are no longer available: {string.Join(", ", reserve)}", Area = ""
                    });
                }

                var reservationIds = new List<int>();
                foreach (var seat in reservations)
                {
                    var reservationId = await seatService.GetReservationIdAsync(seat.Id, sessionId);
                    if (reservationId.HasValue)
                    {
                        reservationIds.Add(reservationId.Value);
                    }
                }

                var viewModel = new CheckoutViewModel
                {
                    Id = session.Id,
                    BasePrice = session.BasePrice,
                    Total = totalPrice,
                    ShowDateTime = session.StartTime,
                    Hall = session.HallName,
                    Name = session.MovieTitle,
                    PosterUrl = movie.PosterUrl,
                    SelectedSeats = seatList,
                    ReservationIds = reservationIds,
                    SeatDetails = []
                };

                foreach (var reserve in reservations)
                {
                    var seatType = seatTypes.FirstOrDefault(st => st.Id == reserve.SeatTypeId);
                    var addedPrice = seatType?.AddedPrice ?? 0;
                    var seatPrice = session.BasePrice + addedPrice;

                    var rowLetter = GetRowLetter(reserve.RowNum);
                    var seatNumber = $"{rowLetter}{reserve.SeatNum}";

                    viewModel.SeatDetails.Add(new SeatCheckoutItem
                    {
                        SeatNumber = seatNumber,
                        Type = seatType?.Name ?? "Standard",
                        Price = seatPrice
                    });
                }

                return View(viewModel);
            }
            catch (HttpRequestException ex)
            {
                TempData["Error"] = $"Error processing checkout: {ex.Message}";
                return NotFound(ex.Message);
                return RedirectToAction("SelectSeats", new { sessionId });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> ConfirmBooking(CheckoutViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Checkout", model);
            }

            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return RedirectToAction("Login", "Account");
                }

                var seatReservationIds = model.ReservationIds;

                var createOrderDto = new Core.DTOs.Orders.CreateOrderDTO
                {
                    SeatReservationIds = seatReservationIds
                };

                var order = await orderService.CreateOrderAsync(userId, createOrderDto);

                return RedirectToAction("Index", "Payment", new { area = "", orderId = order.Id });
            }
            catch (HttpRequestException ex)
            {
                ModelState.AddModelError(string.Empty, $"Error confirming booking: {ex.Message}");
                return View("Checkout", model);
            }
        }

        [Authorize]
        public IActionResult Confirmation(int bookingId)
        {
            // TODO: Fetch booking details from database
            ViewBag.BookingId = bookingId;
            return View();
        }

        #region Private Helper Methods

        private async Task<List<string>> GetOccupiedSeatsAsync(int sessionId, List<SeatDTO> seats)
        {
            var occupiedSeats = new List<string>();

            foreach (var seat in seats)
            {
                var isAvailable = await seatService.IsSeatAvailableAsync(seat, sessionId);
                if (isAvailable) continue;
                var rowLetter = GetRowLetter(seat.RowNum);
                var seatIdentifier = $"{rowLetter}{seat.SeatNum}";
                occupiedSeats.Add(seatIdentifier);
            }

            return occupiedSeats;
        }

        private static string GetRowLetter(byte rowNum)
        {
            return ((char)('A' + rowNum - 1)).ToString();
        }

        #endregion
    }
}
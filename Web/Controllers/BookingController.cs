using System.Runtime.InteropServices.JavaScript;
using AutoMapper;
using cnu_cinema_practice.ViewModels;
using Core.DTOs.Seats;
using Core.Entities;
using Core.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace cnu_cinema_practice.Controllers
{
    public class BookingController(
        ISessionService sessionService,
        IMovieService movieService,
        ISeatService seatService,
        IHallService halLService,
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
                        layout[seat.RowNum, seat.SeatNum] = (byte) seat.SeatTypeId;
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
            catch (Exception ex)
            {
                TempData["Error"] = $"Error loading booking page: {ex.Message}";
                return NotFound(ex.Message);
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
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
                
                var reservations = new List<SeatDTO>();

                foreach (var seat in seatList)
                {
                    var coords = seat.Split('-');
                    byte row = Byte.Parse(coords[0]);
                    byte col = Byte.Parse(coords[1]);

                    foreach (var s in seats)
                    {
                        if (s.RowNum == row && s.SeatNum == col)
                        {
                            reservations.Add(s);
                        }
                    }
                }

                foreach (var reserve in reservations)
                {
                    if (availableSeats.Contains(reserve.Id) == false)
                    {
                        TempData["Error"] = $"The following seats are no longer available: {string.Join(", ", reserve)}";
                        return RedirectToAction("SelectSeats", new { sessionId,
                            alert = $"The following seats are no longer available: {string.Join(", ", reserve)}", Area = "" });
                    }
                }

                foreach (var reserve in reservations)
                {
                    if ((await seatService.ReserveSeatAsync(reserve, sessionId)) == false)
                    {
                        TempData["Error"] = $"The following seats are no longer available: {string.Join(", ", reserve)}";
                        return RedirectToAction("SelectSeats", new { sessionId, 
                            alert = $"The following seats are no longer available: {string.Join(", ", reserve)}", Area = "" });
                    }
                }
                return RedirectToAction("SelectSeats", new { sessionId, Area = "" });

                // Map to CheckoutViewModel
                var viewModel = mapper.Map<CheckoutViewModel>(session);

                // Set movie details
                viewModel.Name = movie.Name;
                viewModel.PosterUrl = movie.PosterUrl;
                viewModel.SelectedSeats = seatList;

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error processing checkout: {ex.Message}";
                return NotFound(ex.Message);
                return RedirectToAction("SelectSeats", new { sessionId });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmBooking(CheckoutViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Checkout", model);
            }

            try
            {
                // TODO: Create booking in database
                // This would involve:
                // 1. Creating a Booking entity
                // 2. Reserving the selected seats
                // 3. Processing payment
                // 4. Sending confirmation email

                // For now, generate a mock booking ID
                var bookingId = new Random().Next(10000, 99999);

                TempData["Success"] = "Booking confirmed! Check your email for tickets.";
                return RedirectToAction("Confirmation", new { bookingId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error confirming booking: {ex.Message}");
                return View("Checkout", model);
            }
        }

        public IActionResult Confirmation(int bookingId)
        {
            // TODO: Fetch booking details from database
            ViewBag.BookingId = bookingId;
            return View();
        }

        #region Private Helper Methods

        private async Task<List<string>> GetOccupiedSeatsAsync(int sessionId, List<SeatDTO> seats)
        {
            // Get all seats and check which are not available
            var occupiedSeats = new List<string>();

            foreach (var seat in seats)
            {
                var isAvailable = await seatService.IsSeatAvailableAsync(seat, sessionId);
                if (!isAvailable)
                {
                    // Convert seat position to seat identifier (e.g., "A5")
                    var rowLetter = GetRowLetter(seat.RowNum);
                    var seatIdentifier = $"{rowLetter}{seat.SeatNum}";
                    occupiedSeats.Add(seatIdentifier);
                }
            }

            return occupiedSeats;
        }

        private static string GetRowLetter(byte rowNum)
        {
            // Convert row number to letter (1 = A, 2 = B, etc.)
            return ((char)('A' + rowNum - 1)).ToString();
        }

        #endregion
    }
}
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
        IMapper mapper) : Controller
    {
        public async Task<IActionResult> SelectSeats(int movieId, int showtimeId = 0)
        {
            try
            {
                // Get movie details
                var movie = await movieService.GetByIdAsync(movieId);
                if (movie == null)
                    return NotFound("Movie not found");

                // Get all sessions for this movie
                var allSessions = await sessionService.GetSessionsByMovieIdAsync(movieId);
                var sessionsList = allSessions.ToList();

                if (!sessionsList.Any())
                {
                    TempData["Error"] = "No showtimes available for this movie.";
                    return RedirectToAction("Index", "Home");
                }

                // Determine which session to show
                var selectedSessionId = showtimeId > 0 ? showtimeId : sessionsList.First().Id;
                var selectedSession = await sessionService.GetSessionByIdWithSeatsAsync(selectedSessionId);

                if (selectedSession == null)
                    return NotFound("Session not found");

                // Map to BookingViewModel
                var viewModel = mapper.Map<BookingViewModel>(selectedSession);

                // Set movie details
                viewModel.Name = movie.Name;
                viewModel.PosterUrl = movie.PosterUrl;

                // Map available showtimes
                viewModel.AvailableShowtimes = mapper.Map<List<ShowtimeOption>>(sessionsList);

                // Get seats for this session
                var seats = await seatService.GetBySessionIdAsync(selectedSessionId);
                var seatsList = seats.ToList();

                // Create seat layout
                viewModel.SeatLayout = new SeatLayout
                {
                    Rows = seatsList.Any() ? seatsList.Max(s => s.RowNum) : (byte)8,
                    SeatsPerRow = seatsList.Any() ? seatsList.Max(s => s.SeatNum) : (byte)12,
                    OccupiedSeats = await GetOccupiedSeatsAsync(selectedSessionId, seatsList)
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error loading booking page: {ex.Message}";
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Checkout(int movieId, int showtimeId, string selectedSeats)
        {
            if (string.IsNullOrEmpty(selectedSeats))
            {
                TempData["Error"] = "Please select at least one seat.";
                return RedirectToAction("SelectSeats", new { movieId, showtimeId });
            }

            try
            {
                var seatList = selectedSeats.Split(',').ToList();

                // Get movie and session details
                var movie = await movieService.GetByIdAsync(movieId);
                if (movie == null)
                    return NotFound("Movie not found");

                var session = await sessionService.GetSessionByIdWithSeatsAsync(showtimeId);
                if (session == null)
                    return NotFound("Session not found");

                // Verify seats are available
                var seats = await seatService.GetBySessionIdAsync(showtimeId);
                var occupiedSeats = await GetOccupiedSeatsAsync(showtimeId, seats.ToList());

                var unavailableSeats = seatList.Intersect(occupiedSeats).ToList();
                if (unavailableSeats.Any())
                {
                    TempData["Error"] =
                        $"The following seats are no longer available: {string.Join(", ", unavailableSeats)}";
                    return RedirectToAction("SelectSeats", new { movieId, showtimeId });
                }

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
                return RedirectToAction("SelectSeats", new { movieId, showtimeId });
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
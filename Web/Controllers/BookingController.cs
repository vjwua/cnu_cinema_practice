using cnu_cinema_practice.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace cnu_cinema_practice.Controllers
{
    public class BookingController : Controller
    {
        public IActionResult SelectSeats(int movieId, int showtimeId = 0)
        {
            // TODO: Fetch actual data from database
            var viewModel = new BookingViewModel
            {
                MovieId = movieId,
                MovieTitle = "The Grand Adventure",
                PosterUrl = "https://donaldthompson.com/wp-content/uploads/2024/10/placeholder-image-vertical.png",
                ShowtimeId = showtimeId > 0 ? showtimeId : 1,
                ShowDateTime = DateTime.Now.AddDays(1).Date.AddHours(19).AddMinutes(30),
                Theater = "Theater 1",
                TicketPrice = 13.50m,
                AvailableShowtimes = new List<ShowtimeOption>
                {
                    new ShowtimeOption { Id = 1, DateTime = DateTime.Now.AddDays(1).Date.AddHours(14).AddMinutes(30), Theater = "Theater 1" },
                    new ShowtimeOption { Id = 2, DateTime = DateTime.Now.AddDays(1).Date.AddHours(17).AddMinutes(0), Theater = "Theater 1" },
                    new ShowtimeOption { Id = 3, DateTime = DateTime.Now.AddDays(1).Date.AddHours(19).AddMinutes(30), Theater = "Theater 1" },
                    new ShowtimeOption { Id = 4, DateTime = DateTime.Now.AddDays(1).Date.AddHours(22).AddMinutes(0), Theater = "Theater 2" }
                },
                SeatLayout = new SeatLayout
                {
                    Rows = 8,
                    SeatsPerRow = 12,
                    OccupiedSeats = new List<string> { "A5", "A6", "B3", "B4", "B5", "C7", "D8", "D9", "E5", "E6", "F10" }
                }
            };

            return View(viewModel);
        }

        [HttpPost]
        public IActionResult Checkout(int movieId, int showtimeId, string selectedSeats)
        {
            if (string.IsNullOrEmpty(selectedSeats))
            {
                TempData["Error"] = "Please select at least one seat.";
                return RedirectToAction("SelectSeats", new { movieId, showtimeId });
            }

            var seatList = selectedSeats.Split(',').ToList();

            // TODO: Fetch actual data from database
            var viewModel = new CheckoutViewModel
            {
                MovieId = movieId,
                MovieTitle = "The Grand Adventure",
                PosterUrl = "https://donaldthompson.com/wp-content/uploads/2024/10/placeholder-image-vertical.png",
                ShowDateTime = DateTime.Now.AddDays(1).Date.AddHours(19).AddMinutes(30),
                Theater = "Theater 1",
                SelectedSeats = seatList,
                TicketPrice = 12.50m,
                ServiceFee = 2.50m
            };

            return View(viewModel);
        }

        [HttpPost]
        public IActionResult ConfirmBooking(CheckoutViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Checkout", model);
            }

            // TODO: Save booking to database
            TempData["Success"] = "Booking confirmed! Check your email for tickets.";
            return RedirectToAction("Confirmation", new { bookingId = 12345 });
        }

        public IActionResult Confirmation(int bookingId)
        {
            // TODO: Fetch booking details from database
            ViewBag.BookingId = bookingId;
            return View();
        }
    }
}

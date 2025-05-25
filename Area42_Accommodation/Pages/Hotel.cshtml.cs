using Area42_Accommodation.Pages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Area42_Accommodation.Pages
{
    public class HotelModel : PageModel
    {
        [BindProperty]
        public BookingViewModel Booking { get; set; } = new BookingViewModel();

        public void OnGet()
        {
            // Initialize default values
            Booking.CheckInDate = DateTime.Today.AddDays(1).ToString("yyyy-MM-dd");
            Booking.CheckOutDate = DateTime.Today.AddDays(2).ToString("yyyy-MM-dd");
            Booking.Guests = 1;
            
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Handle the booking form submission
            TempData["Message"] = $"Checking hotel room availability for {Booking.Guests} guests from {Booking.CheckInDate} to {Booking.CheckOutDate}";

            return RedirectToPage("/Reservation");
        }

        public class BookingViewModel
        {
            public string CheckInDate { get; set; } = string.Empty;
            public string CheckOutDate { get; set; } = string.Empty;
            public int Guests { get; set; } = 2;
        }
    }
}
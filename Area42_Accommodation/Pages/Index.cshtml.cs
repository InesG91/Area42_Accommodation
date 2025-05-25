using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Core.Domain.Interfaces;
using Core.Domain.Models;

namespace Area42_Accommodation.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IAccommodationRepository _accommodationRepository;

        public IndexModel(IAccommodationRepository accommodationRepository)
        {
            _accommodationRepository = accommodationRepository;
        }

        [BindProperty]
        public BookingViewModel Booking { get; set; } = new BookingViewModel();

        // ✅ Add Accommodations property
        public List<Accommodation> Accommodations { get; set; } = new List<Accommodation>();

        public void OnGet()
        {
            // Initialize default values
            Booking.CheckInDate = DateTime.Today.AddDays(1).ToString("yyyy-MM-dd");
            Booking.CheckOutDate = DateTime.Today.AddDays(2).ToString("yyyy-MM-dd");
            Booking.Guests = 1;

            // ✅ Load accommodations from database
            LoadAccommodations();
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                LoadAccommodations(); // Reload accommodations if validation fails
                return Page();
            }

            // Handle the booking form submission
            TempData["Message"] = $"Checking availability for {Booking.Guests} guests from {Booking.CheckInDate} to {Booking.CheckOutDate}";
            return RedirectToPage("/Index");
        }



        // ✅ Add method to load accommodations
        private void LoadAccommodations()
        {

            try
            {
                Accommodations = _accommodationRepository.GetAccommodations();
            }
            catch (Exception ex)
            {
                // Log the exception (you should add proper logging here)
                // For now, just leave the list empty
                Accommodations = new List<Accommodation>();
                // Optionally add an error message
                TempData["ErrorMessage"] = "Unable to load accommodations at this time.";
            }
        }




        public class BookingViewModel
        {
            public string CheckInDate { get; set; } = string.Empty;
            public string CheckOutDate { get; set; } = string.Empty;
            public int Guests { get; set; } = 2;
        }
    }
}
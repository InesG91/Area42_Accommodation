using Core.Domain.Services;
using Core.Domain.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace Area42_Accommodation.Pages
{
    public class IndexModel : PageModel
    {
        private readonly AvailabilityService _availabilityService;      
        private readonly AccommodationService _accommodationService;    
        private readonly PricingService _pricingService;                

        public IndexModel(
            AvailabilityService availabilityService,
            AccommodationService accommodationService,
            PricingService pricingService)
        {
            _availabilityService = availabilityService;
            _accommodationService = accommodationService;
            _pricingService = pricingService;
        }

        [BindProperty]
        public BookingViewModel Booking { get; set; } = new BookingViewModel();

        // Properties to control which accommodations are shown
        // Chalets
        public bool ShowSmallChalet { get; set; } = true;
        public bool ShowMediumChalet { get; set; } = true;
        public bool ShowXLChalet { get; set; } = true;

        // Hotels  
        public bool ShowSmallHotel { get; set; } = true;
        public bool ShowMediumHotel { get; set; } = true;
        public bool ShowXLHotel { get; set; } = true;

        public bool HasCheckedAvailability { get; set; } = false;

        public List<Accommodation> Accommodations { get; set; } = new List<Accommodation>();

        public void OnGet()
        {
            // Set default dates (tomorrow and day after)
            Booking.CheckInDate = DateTime.Today.AddDays(1).ToString("yyyy-MM-dd");
            Booking.CheckOutDate = DateTime.Today.AddDays(2).ToString("yyyy-MM-dd");
            Booking.Guests = 2;

            // ✅ Load accommodations using AccommodationService
            LoadAllAccommodations();

            // Show all accommodations by default
            ShowAllAccommodations();
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                LoadAllAccommodations();
                return Page();
            }

            try
            {
                // Parse dates
                if (!DateTime.TryParse(Booking.CheckInDate, out DateTime checkIn) ||
                    !DateTime.TryParse(Booking.CheckOutDate, out DateTime checkOut))
                {
                    ModelState.AddModelError("", "Invalid dates selected.");
                    LoadAllAccommodations();
                    return Page();
                }

                if (checkIn >= checkOut)
                {
                    ModelState.AddModelError("", "Check-out date must be after check-in date.");
                    LoadAllAccommodations();
                    return Page();
                }

                if (checkIn < DateTime.Today)
                {
                    ModelState.AddModelError("", "Check-in date cannot be in the past.");
                    LoadAllAccommodations();
                    return Page();
                }

                // ✅ Use AvailabilityService instead of duplicating logic
                CheckAvailabilityUsingService(checkIn, checkOut);

                HasCheckedAvailability = true;

                // Count available accommodations
                int availableChalets = (ShowSmallChalet ? 1 : 0) + (ShowMediumChalet ? 1 : 0) + (ShowXLChalet ? 1 : 0);
                int availableHotels = (ShowSmallHotel ? 1 : 0) + (ShowMediumHotel ? 1 : 0) + (ShowXLHotel ? 1 : 0);
                int totalAvailable = availableChalets + availableHotels;

                TempData["Message"] = $"Availability checked for {checkIn:dd/MM/yyyy} - {checkOut:dd/MM/yyyy} for {Booking.Guests} guests. {totalAvailable} accommodation(s) available ({availableChalets} chalets, {availableHotels} hotel rooms).";

                LoadAllAccommodations();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error checking availability: {ex.Message}");
                LoadAllAccommodations();
            }

            return Page();
        }

        
        private void CheckAvailabilityUsingService(DateTime checkIn, DateTime checkOut)
        {
            try
            {
                
                var availableAccommodations = _availabilityService.GetAvailableAccommodations(
                    startDate: checkIn,
                    endDate: checkOut,
                    guestCount: Booking.Guests
                );

                // ✅ ADD DEBUG OUTPUT
                var debugInfo = $"DEBUG: Searching for {Booking.Guests} guests. Found {availableAccommodations.Count} accommodations:\n";
                foreach (var acc in availableAccommodations)
                {
                    debugInfo += $"- {acc._subtype}: MaxGuests={acc._maxGuests}\n";
                }
                TempData["DebugInfo"] = debugInfo;
                //DEBUGGER

                ShowSmallChalet = availableAccommodations.Any(a =>
                    a._subtype.Equals("Small Chalet", StringComparison.OrdinalIgnoreCase));

                ShowMediumChalet = availableAccommodations.Any(a =>
                    a._subtype.Equals("Medium Chalet", StringComparison.OrdinalIgnoreCase));

                ShowXLChalet = availableAccommodations.Any(a =>
                    a._subtype.Equals("XL Chalet", StringComparison.OrdinalIgnoreCase));

                ShowSmallHotel = availableAccommodations.Any(a =>
                    a._subtype.Equals("Small Hotel Room", StringComparison.OrdinalIgnoreCase));

                ShowMediumHotel = availableAccommodations.Any(a =>
                    a._subtype.Equals("Medium Hotel Room", StringComparison.OrdinalIgnoreCase));

                ShowXLHotel = availableAccommodations.Any(a =>
                    a._subtype.Equals("XL Hotel Room", StringComparison.OrdinalIgnoreCase));
            }
            catch (Exception ex)
            {
                TempData["DebugInfo"] = $"ERROR in CheckAvailabilityUsingService: {ex.Message}";
                // If availability check fails, show all accommodations
                ShowAllAccommodations();
            }
        }

        
        private void LoadAllAccommodations()
        {
            try
            {
                Accommodations = _accommodationService.GetAllAccommodations();
            }
            catch (Exception)
            {
                Accommodations = new List<Accommodation>();
            }
        }

        private void ShowAllAccommodations()
        {
            ShowSmallChalet = true;
            ShowMediumChalet = true;
            ShowXLChalet = true;
            ShowSmallHotel = true;
            ShowMediumHotel = true;
            ShowXLHotel = true;
        }

        // ✅ Optional: Add method to get prices for display (if needed on index page)
        public decimal GetAccommodationPrice(string roomType)
        {
            try
            {
                return _pricingService.GetPricePerNight(roomType);
            }
            catch
            {
                return 0;
            }
        }
    }

    public class BookingViewModel
    {
        [Required(ErrorMessage = "Check-in date is required")]
        [DataType(DataType.Date)]
        public string CheckInDate { get; set; } = string.Empty;

        [Required(ErrorMessage = "Check-out date is required")]
        [DataType(DataType.Date)]
        public string CheckOutDate { get; set; } = string.Empty;

        [Range(1, 8, ErrorMessage = "Number of guests must be between 1 and 8")]
        public int Guests { get; set; } = 1;
    }
}
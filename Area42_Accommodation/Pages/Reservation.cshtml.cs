using Core.Domain.Interfaces;
using Core.Domain.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using Core.Domain.Services;
using Core.Domain.Interfaces;

namespace Area42_Accommodation.Pages
{
    public class ReservationModel : PageModel
    {
        // ✅ Use services and interfaces instead of concrete repositories
        private readonly BookingService _bookingService;
        private readonly IAccommodationRepository _accommodationRepository;

        [BindProperty]
        public GuestViewModel Guest { get; set; } = new GuestViewModel();

        [BindProperty]
        public BookingSummaryViewModel BookingSummary { get; set; } = new BookingSummaryViewModel();

        // ✅ Updated constructor - use services
        public ReservationModel(
            BookingService bookingService,
            IAccommodationRepository accommodationRepository)
        {
            _bookingService = bookingService;
            _accommodationRepository = accommodationRepository;
        }

        public void OnGet(string accommodationType = "Hotel", string roomType = "Small",
            string checkIn = "", string checkOut = "", int totalGuests = 1)
        {
            // Initialize booking summary from query parameters or defaults
            this.BookingSummary.AccommodationType = accommodationType;
            this.BookingSummary.RoomType = roomType + " " + accommodationType + (accommodationType == "Hotel" ? " Room" : "");
            this.BookingSummary.CheckInDate = !string.IsNullOrEmpty(checkIn) ? checkIn : DateTime.Today.AddDays(1).ToString("yyyy-MM-dd");
            this.BookingSummary.CheckOutDate = !string.IsNullOrEmpty(checkOut) ? checkOut : DateTime.Today.AddDays(2).ToString("yyyy-MM-dd");
            this.BookingSummary.TotalGuests = totalGuests;

            // Calculate pricing using your backend
            CalculatePricingFromDatabase();
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                CalculatePricingFromDatabase();
                return Page();
            }

            try
            {
                // ✅ Get selected accommodation ID
                int accommodationId = GetSelectedAccommodationId();

                // ✅ Simple service call - handles all the complexity!
                int bookingId = _bookingService.CreateBooking(
                    firstName: Guest.FirstName,
                    lastName: Guest.LastName,
                    dateOfBirth: Guest.DateOfBirth,
                    email: Guest.Email,
                    accommodationId: accommodationId,
                    startDate: DateTime.Parse(this.BookingSummary.CheckInDate),
                    endDate: DateTime.Parse(this.BookingSummary.CheckOutDate),
                    numberOfGuests: this.BookingSummary.TotalGuests
                );

                TempData["SuccessMessage"] = $"Boeking bevestigd voor {Guest.FirstName} {Guest.LastName}! Booking ID: {bookingId}. Totaal: €{this.BookingSummary.TotalAmount:F2} voor {this.BookingSummary.TotalGuests} personen.";

                return RedirectToPage("/BookingConfirmation");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Er is een fout opgetreden: {ex.Message}");
                CalculatePricingFromDatabase();
                return Page();
            }
        }

        private void CalculatePricingFromDatabase()
        {
            try
            {
                // Get accommodation details from database
                var accommodations = _accommodationRepository.GetAccommodations();
                var selectedAccommodation = accommodations.FirstOrDefault(a =>
                    this.BookingSummary.RoomType.Contains(a._subtype, StringComparison.OrdinalIgnoreCase));

                if (selectedAccommodation != null)
                {
                    this.BookingSummary.PricePerNight = selectedAccommodation._pricePerNight;
                }
                else
                {
                    // Fallback to hardcoded pricing if not found
                    this.BookingSummary.PricePerNight = 100.00m;
                }

                // Calculate number of nights
                if (DateTime.TryParse(this.BookingSummary.CheckInDate, out DateTime checkIn) &&
                    DateTime.TryParse(this.BookingSummary.CheckOutDate, out DateTime checkOut))
                {
                    this.BookingSummary.NumberOfNights = (checkOut - checkIn).Days;
                    if (this.BookingSummary.NumberOfNights <= 0)
                        this.BookingSummary.NumberOfNights = 1;
                }
                else
                {
                    this.BookingSummary.NumberOfNights = 1;
                }

                // Calculate total amount
                this.BookingSummary.TotalAmount = this.BookingSummary.PricePerNight * this.BookingSummary.NumberOfNights;
            }
            catch (Exception)
            {
                // Fallback to default pricing if database fails
                this.BookingSummary.PricePerNight = 100.00m;
                this.BookingSummary.NumberOfNights = 1;
                this.BookingSummary.TotalAmount = 100.00m;
            }
        }

        // ✅ Simplified - just return the accommodation ID
        private int GetSelectedAccommodationId()
        {
            var accommodations = _accommodationRepository.GetAccommodations();
            var selectedAccommodation = accommodations.FirstOrDefault(a =>
                this.BookingSummary.RoomType.Contains(a._subtype, StringComparison.OrdinalIgnoreCase));

            if (selectedAccommodation != null)
            {
                return selectedAccommodation._accommodationID;
            }

            // Fallback - return first accommodation if none found
            return accommodations.First()._accommodationID;
        }
    }

    // ViewModels for Razor Page binding
    public class GuestViewModel
    {
        [Required(ErrorMessage = "Voornaam is verplicht")]
        [StringLength(50, ErrorMessage = "Voornaam mag maximaal 50 karakters bevatten")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Achternaam is verplicht")]
        [StringLength(50, ErrorMessage = "Achternaam mag maximaal 50 karakters bevatten")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Geboortedatum is verplicht")]
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }

        [Required(ErrorMessage = "Email is verplicht")]
        [EmailAddress(ErrorMessage = "Voer een geldig emailadres in")]
        [StringLength(100, ErrorMessage = "Email mag maximaal 100 karakters bevatten")]
        public string Email { get; set; } = string.Empty;
    }

    public class BookingSummaryViewModel
    {
        public string AccommodationType { get; set; } = string.Empty;
        public string RoomType { get; set; } = string.Empty;
        public string CheckInDate { get; set; } = string.Empty;
        public string CheckOutDate { get; set; } = string.Empty;
        public int TotalGuests { get; set; } = 1;
        public decimal PricePerNight { get; set; }
        public int NumberOfNights { get; set; }
        public decimal TotalAmount { get; set; }
    }
}

using Core.Domain.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Core.Domain.Models;

namespace Area42_Accommodation.Pages
{
    public class ReservationModel : PageModel
    {
        private readonly BookingService _bookingService;
        private readonly PricingService _pricingService;
        private readonly AccommodationService _accommodationService;
        private readonly PaymentService _paymentService;

        public ReservationModel(
            BookingService bookingService,
            PricingService pricingService,
            AccommodationService accommodationService,
            PaymentService paymentService)
        {
            _bookingService = bookingService;
            _pricingService = pricingService;
            _accommodationService = accommodationService;
            _paymentService = paymentService;
        }

        // Om input van de index over te brengen
        [BindProperty]
        public BookingSummaryViewModel BookingSummary { get; set; } = new BookingSummaryViewModel();

        // Om input in de veldjes op deze pagina te binden
        [BindProperty]
        public GuestViewModel Guest { get; set; } = new GuestViewModel();

        // Om payment input te binden
        [BindProperty]
        public PaymentViewModel Payment { get; set; } = new PaymentViewModel();

        // Runt op het moment dat re-direct vanuit index gebeurt - vult bookingsummary attributes met URL-parameters
        public void OnGet(string accommodationType = "Hotel", string roomType = "Small",
            string checkIn = "", string checkOut = "", int totalGuests = 1)
        {
            // Initialize booking summary from query parameters
            BookingSummary.AccommodationType = accommodationType;
            BookingSummary.RoomType = roomType + " " + accommodationType + (accommodationType == "Hotel" ? " Room" : "");
            BookingSummary.CheckInDate = !string.IsNullOrEmpty(checkIn) ? checkIn : DateTime.Today.AddDays(1).ToString("yyyy-MM-dd");
            BookingSummary.CheckOutDate = !string.IsNullOrEmpty(checkOut) ? checkOut : DateTime.Today.AddDays(2).ToString("yyyy-MM-dd");
            BookingSummary.TotalGuests = totalGuests;

            // ✅ IMPORTANT: Initialize Payment object
            Payment = new PaymentViewModel();

            // Delegate pricing calculation to service
            CalculatePricing();
        }

        // Runt op het moment dat "bevestig boeking" geklikt. Vult SQL-table met nieuwe boeking.
        public IActionResult OnPost()
        {
            // Clear validation errors for unused payment fields
            var fieldsToRemove = new[] {
        "Payment.PaypalEmail", "Payment.CardName", "Payment.CardNumber",
        "Payment.CardExpiry", "Payment.CardCvv", "Payment.AccountHolder",
        "Payment.Iban", "Payment.BankName"
    };

            foreach (var field in fieldsToRemove)
            {
                if (ModelState.ContainsKey(field))
                {
                    ModelState.Remove(field);
                }
            }

            // Validate payment method selection
            if (string.IsNullOrEmpty(Payment?.PaymentMethod))
            {
                ModelState.AddModelError("Payment.PaymentMethod", "Selecteer een betaalmethode");
                CalculatePricing();
                return Page();
            }

            // Conditional validation based on selected payment method
            switch (Payment.PaymentMethod.ToLower())
            {
                case "paypal":
                    if (string.IsNullOrEmpty(Payment.PaypalEmail))
                    {
                        ModelState.AddModelError("Payment.PaypalEmail", "PayPal email is verplicht");
                    }
                    break;

                case "creditcard":
                    if (string.IsNullOrEmpty(Payment.CardName))
                        ModelState.AddModelError("Payment.CardName", "Naam op kaart is verplicht");
                    if (string.IsNullOrEmpty(Payment.CardNumber))
                        ModelState.AddModelError("Payment.CardNumber", "Kaartnummer is verplicht");
                    if (string.IsNullOrEmpty(Payment.CardExpiry))
                        ModelState.AddModelError("Payment.CardExpiry", "Vervaldatum is verplicht");
                    if (string.IsNullOrEmpty(Payment.CardCvv))
                        ModelState.AddModelError("Payment.CardCvv", "CVV is verplicht");
                    break;

                case "banktransfer":
                    if (string.IsNullOrEmpty(Payment.AccountHolder))
                        ModelState.AddModelError("Payment.AccountHolder", "Rekeninghouder is verplicht");
                    if (string.IsNullOrEmpty(Payment.Iban))
                        ModelState.AddModelError("Payment.Iban", "IBAN is verplicht");
                    if (string.IsNullOrEmpty(Payment.BankName))
                        ModelState.AddModelError("Payment.BankName", "Bank naam is verplicht");
                    break;
            }

            // Check if form is valid
            if (!ModelState.IsValid)
            {
                CalculatePricing();
                return Page();
            }

            // Proceed with booking creation
            try
            {
                int accommodationId = _accommodationService.GetAccommodationIdByRoomType(BookingSummary.RoomType);

                int bookingId = _bookingService.CreateBooking(
                    firstName: Guest.FirstName,
                    lastName: Guest.LastName,
                    dateOfBirth: Guest.DateOfBirth,
                    email: Guest.Email,
                    accommodationId: accommodationId,
                    startDate: DateTime.Parse(BookingSummary.CheckInDate),
                    endDate: DateTime.Parse(BookingSummary.CheckOutDate),
                    numberOfGuests: BookingSummary.TotalGuests
                );

                var paymentDetails = CreatePaymentDetailsDictionary();
                string paymentResult = _paymentService.ProcessBookingPayment(
                    bookingId: bookingId,
                    amount: BookingSummary.TotalAmount,
                    paymentMethod: Payment.PaymentMethod,
                    paymentDetails: paymentDetails
                );

                if (paymentResult.Contains("Error", StringComparison.OrdinalIgnoreCase))
                {
                    ModelState.AddModelError("Payment", $"Betaling mislukt: {paymentResult}");
                    CalculatePricing();
                    return Page();
                }

                string paymentMethodDisplay = GetPaymentMethodDisplayName();
                TempData["SuccessMessage"] = $"Boeking bevestigd voor {Guest.FirstName} {Guest.LastName}! Booking ID: {bookingId}. Betaalmethode: {paymentMethodDisplay}. Totaal: €{BookingSummary.TotalAmount:F2} voor {BookingSummary.TotalGuests} personen.";
                TempData["PaymentDetails"] = paymentResult;

                return RedirectToPage("/BookingConfirmation");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Er is een fout opgetreden: {ex.Message}");
                CalculatePricing();
                return Page();
            }
        }
        // Simplified method that delegates all pricing logic to the service
        private void CalculatePricing()
        {
            if (DateTime.TryParse(BookingSummary.CheckInDate, out DateTime checkIn) &&
                DateTime.TryParse(BookingSummary.CheckOutDate, out DateTime checkOut))
            {
                // Use pricing service for all calculations
                BookingSummary.PricePerNight = _pricingService.GetPricePerNight(BookingSummary.RoomType);
                BookingSummary.NumberOfNights = _pricingService.CalculateNumberOfNights(checkIn, checkOut);
                BookingSummary.TotalAmount = _pricingService.CalculateTotalPrice(BookingSummary.RoomType, checkIn, checkOut);
            }
            else
            {
                // Handle invalid dates with fallback values
                BookingSummary.PricePerNight = 100.00m;
                BookingSummary.NumberOfNights = 1;
                BookingSummary.TotalAmount = 100.00m;
            }
        }

        // Create payment details dictionary for Strategy Pattern
        private Dictionary<string, string> CreatePaymentDetailsDictionary()
        {
            var paymentDetails = new Dictionary<string, string>();

            switch (Payment.PaymentMethod?.ToLower())
            {
                case "paypal":
                    paymentDetails["email"] = Payment.PaypalEmail ?? string.Empty;
                    break;

                case "creditcard":
                    paymentDetails["cardName"] = Payment.CardName ?? string.Empty;
                    paymentDetails["cardNumber"] = Payment.CardNumber ?? string.Empty;
                    paymentDetails["expiry"] = Payment.CardExpiry ?? string.Empty;
                    paymentDetails["cvv"] = Payment.CardCvv ?? string.Empty;
                    break;

                case "banktransfer":
                    paymentDetails["accountHolder"] = Payment.AccountHolder ?? string.Empty;
                    paymentDetails["iban"] = Payment.Iban ?? string.Empty;
                    paymentDetails["bankName"] = Payment.BankName ?? string.Empty;
                    break;
            }

            return paymentDetails;
        }

        // Get display name for payment method
        private string GetPaymentMethodDisplayName()
        {
            return Payment.PaymentMethod?.ToLower() switch
            {
                "paypal" => "PayPal",
                "creditcard" => "Credit Card",
                "banktransfer" => "Bank Transfer (EU)",
                _ => Payment.PaymentMethod
            };
        }

        // Helper method for clearing model state errors
        private void ClearModelStateErrors(params string[] keys)
        {
            foreach (var key in keys)
            {
                if (ModelState.ContainsKey(key))
                {
                    ModelState[key].Errors.Clear();
                }
            }
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

    // ✅ CRITICAL: PaymentViewModel with NO [Required] attributes on individual fields
    public class PaymentViewModel
    {
        [Required(ErrorMessage = "Selecteer een betaalmethode")]
        public string PaymentMethod { get; set; } = string.Empty;

        // PayPal fields - NO [Required] attribute!
        [StringLength(100, ErrorMessage = "PayPal email mag maximaal 100 karakters bevatten")]
        public string PaypalEmail { get; set; } = string.Empty;

        // Credit Card fields - NO [Required] attributes!
        [StringLength(50, ErrorMessage = "Naam op kaart mag maximaal 50 karakters bevatten")]
        public string CardName { get; set; } = string.Empty;

        [StringLength(20, ErrorMessage = "Kaartnummer mag maximaal 20 karakters bevatten")]
        public string CardNumber { get; set; } = string.Empty;

        [StringLength(5, ErrorMessage = "Vervaldatum mag maximaal 5 karakters bevatten")]
        public string CardExpiry { get; set; } = string.Empty;

        [StringLength(4, ErrorMessage = "CVV mag maximaal 4 karakters bevatten")]
        public string CardCvv { get; set; } = string.Empty;

        // Bank Transfer fields - NO [Required] attributes!
        [StringLength(100, ErrorMessage = "Rekeninghouder mag maximaal 100 karakters bevatten")]
        public string AccountHolder { get; set; } = string.Empty;

        [StringLength(50, ErrorMessage = "IBAN mag maximaal 50 karakters bevatten")]
        public string Iban { get; set; } = string.Empty;

        [StringLength(50, ErrorMessage = "Bank naam mag maximaal 50 karakters bevatten")]
        public string BankName { get; set; } = string.Empty;
    }
}
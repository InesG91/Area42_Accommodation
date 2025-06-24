using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Area42_Accommodation.Pages
{
    public class AccountModel : PageModel
    {
        private const string _connectionString = "Data Source=mssqlstud.fhict.local;Initial Catalog=dbi557335_area42;User ID=dbi557335_area42;Password=5Kty4gkkBYNyyYnkXAr6;TrustServerCertificate=True";

        [BindProperty]
        public UserProfileModel UserProfile { get; set; } = new();

        public List<BookingHistoryModel> BookingHistory { get; set; } = new();

        public class UserProfileModel
        {
            [Required]
            [Display(Name = "Voornaam")]
            public string FirstName { get; set; }

            [Required]
            [Display(Name = "Achternaam")]
            public string LastName { get; set; }

            [Required]
            [DataType(DataType.Date)]
            [Display(Name = "Geboortedatum")]
            public DateTime DateOfBirth { get; set; }

            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }
        }

        public class BookingHistoryModel
        {
            public int BookingID { get; set; }
            public string AccommodationType { get; set; }
            public DateTime CheckInDate { get; set; }
            public DateTime CheckOutDate { get; set; }
            public int NumberOfNights { get; set; }
            public int TotalGuests { get; set; }
            public decimal PricePerNight { get; set; }
            public decimal TotalAmount { get; set; }
            public string BookingStatus { get; set; }
            public string PaymentStatus { get; set; }
        }

        public IActionResult OnGet()
        {
            var guestId = HttpContext.Session.GetInt32("GuestID");
            if (guestId == null)
            {
                return RedirectToPage("/Login");
            }

            LoadUserProfile(guestId.Value);
            LoadBookingHistory(guestId.Value);

            return Page();
        }

        public IActionResult OnPost()
        {
            var guestId = HttpContext.Session.GetInt32("GuestID");
            if (guestId == null)
            {
                return RedirectToPage("/Login");
            }

            if (!ModelState.IsValid)
            {
                LoadBookingHistory(guestId.Value);
                return Page();
            }

            try
            {
                using SqlConnection connection = new(_connectionString);
                connection.Open();

                var query = @"
                    UPDATE Guest 
                    SET FirstName = @FirstName, LastName = @LastName, 
                        DateOfBirth = @DateOfBirth, Email = @Email
                    WHERE GuestID = @GuestID";

                using SqlCommand command = connection.CreateCommand();
                command.CommandText = query;
                command.Parameters.AddWithValue("@FirstName", UserProfile.FirstName);
                command.Parameters.AddWithValue("@LastName", UserProfile.LastName);
                command.Parameters.AddWithValue("@DateOfBirth", UserProfile.DateOfBirth);
                command.Parameters.AddWithValue("@Email", UserProfile.Email);
                command.Parameters.AddWithValue("@GuestID", guestId.Value);

                command.ExecuteNonQuery();

                // Update session with new info
                HttpContext.Session.SetString("FirstName", UserProfile.FirstName);
                HttpContext.Session.SetString("LastName", UserProfile.LastName);
                HttpContext.Session.SetString("Email", UserProfile.Email);

                TempData["SuccessMessage"] = "Profile updated successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while updating your profile.";
            }

            LoadBookingHistory(guestId.Value);
            return Page();
        }

        private void LoadUserProfile(int guestId)
        {
            using SqlConnection connection = new(_connectionString);
            connection.Open();

            var query = "SELECT FirstName, LastName, DateOfBirth, Email FROM Guest WHERE GuestID = @GuestID";
            using SqlCommand command = connection.CreateCommand();
            command.CommandText = query;
            command.Parameters.AddWithValue("@GuestID", guestId);

            using SqlDataReader reader = command.ExecuteReader();
            if (reader.Read())
            {
                UserProfile = new UserProfileModel
                {
                    FirstName = reader.GetString("FirstName"),
                    LastName = reader.GetString("LastName"),
                    DateOfBirth = reader.GetDateTime("DateOfBirth"),
                    Email = reader.GetString("Email")
                };
            }
        }

        private void LoadBookingHistory(int guestId)
        {
            using SqlConnection connection = new(_connectionString);
            connection.Open();

            var query = @"
                SELECT 
                    b.BookingID,
                    a.Subtype as AccommodationType,
                    b.StartDate,
                    b.EndDate,
                    DATEDIFF(day, b.StartDate, b.EndDate) as NumberOfNights,
                    b.AmountOfPeople,
                    a.PricePerNight,
                    (DATEDIFF(day, b.StartDate, b.EndDate) * a.PricePerNight) as TotalAmount,
                    b.BookingStatus,
                    ISNULL(p.PaymentStatus, 'Pending') as PaymentStatus
                FROM Bookings b
                INNER JOIN Accommodation a ON b.AccommodationID = a.AccommodationID
                LEFT JOIN Accommodation_Payments p ON b.BookingID = p.BookingID
                WHERE b.GuestID = @GuestID
                ORDER BY b.StartDate DESC";

            using SqlCommand command = connection.CreateCommand();
            command.CommandText = query;
            command.Parameters.AddWithValue("@GuestID", guestId);

            using SqlDataReader reader = command.ExecuteReader();
            BookingHistory = new List<BookingHistoryModel>();

            while (reader.Read())
            {
                BookingHistory.Add(new BookingHistoryModel
                {
                    BookingID = reader.GetInt32("BookingID"),
                    AccommodationType = reader.GetString("AccommodationType"),
                    CheckInDate = reader.GetDateTime("StartDate"),
                    CheckOutDate = reader.GetDateTime("EndDate"),
                    NumberOfNights = reader.GetInt32("NumberOfNights"),
                    TotalGuests = reader.GetInt32("AmountOfPeople"),
                    PricePerNight = reader.GetDecimal("PricePerNight"),
                    TotalAmount = reader.GetDecimal("TotalAmount"),
                    BookingStatus = reader.GetString("BookingStatus"),
                    PaymentStatus = reader.GetString("PaymentStatus")
                });
            }
        }
    }
}

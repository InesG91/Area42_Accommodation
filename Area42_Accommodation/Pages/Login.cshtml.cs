using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using Microsoft.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using System.Data;

namespace Area42_Accommodation.Pages
{
    public class LoginModel : PageModel
    {
        private const string _connectionString = "Data Source=mssqlstud.fhict.local;Initial Catalog=dbi557335_area42;User ID=dbi557335_area42;Password=5Kty4gkkBYNyyYnkXAr6;TrustServerCertificate=True";

        [BindProperty]
        public LoginInputModel Input { get; set; } = new();

        public class LoginInputModel
        {
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; } = string.Empty;

            [Required]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; } = string.Empty;
        }

        public void OnGet()
        {
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid) return Page();

            try
            {
                using SqlConnection connection = new(_connectionString);
                connection.Open();

                using SqlCommand command = connection.CreateCommand();
                command.CommandText = "SELECT GuestID, FirstName, LastName, Email, PasswordHash FROM Guest WHERE Email = @Email";
                command.Parameters.AddWithValue("@Email", Input.Email);

                using SqlDataReader reader = command.ExecuteReader();

                if (reader.Read())
                {
                    var storedPasswordHash = reader.GetString("PasswordHash");
                    var inputPasswordHash = HashPassword(Input.Password);

                    if (storedPasswordHash == inputPasswordHash)
                    {
                        
                        HttpContext.Session.SetInt32("GuestID", reader.GetInt32("GuestID"));
                        HttpContext.Session.SetString("FirstName", reader.GetString("FirstName"));
                        HttpContext.Session.SetString("LastName", reader.GetString("LastName"));
                        HttpContext.Session.SetString("Email", reader.GetString("Email"));

                        TempData["SuccessMessage"] = "Login successful!";
                        return RedirectToPage("/Account");
                    }
                }

                TempData["ErrorMessage"] = "Invalid email or password.";
                return Page();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred during login. Please try again.";
                return Page();
            }
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }
    }
}

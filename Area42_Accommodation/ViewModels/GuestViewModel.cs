using System.ComponentModel.DataAnnotations;

namespace Area42_Accommodation.ViewModels
{
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
}

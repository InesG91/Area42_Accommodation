using Core.Domain.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Domain.Models
{
    public class Guest
    {
        public int GuestID { get; private set; }
        public string FirstName { get; private set; }
        public string LastName { get; private set; }
        public DateTime DateOfBirth { get; private set; }
        public string Email { get; private set; }

        public string PasswordHash { get; private set; } // ✅ ADD THIS LINE

        public Guest(int guestID, string firstName, string lastName, DateTime dateOfBirth, string email, string passwordHash = "")
        {
            if (AgeHelper.CalculateAge(dateOfBirth) < 18)
                throw new InvalidOperationException("Guest must be at least 18 years old to book.");
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentNullException(nameof(email), "Email cannot be null or empty.");
            if (string.IsNullOrWhiteSpace(firstName))
                throw new ArgumentNullException(nameof(firstName), "First name cannot be null or empty.");
            if (string.IsNullOrWhiteSpace(lastName))
                throw new ArgumentNullException(nameof(lastName), "Last name cannot be null or empty.");

            GuestID = guestID;
            FirstName = firstName;
            LastName = lastName;
            DateOfBirth = dateOfBirth;
            Email = email;
            PasswordHash = passwordHash;

        }
    }
}

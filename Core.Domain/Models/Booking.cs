using Core.Domain.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Domain.Models
{
    public class Booking
    {
        public int BookingID { get; private set; }
        public Guest Guest { get; private set; }
        public List<Accommodation> Accommodations { get; private set; } // Multiple accommodations
        public int TotalGuests { get; private set; }
        public DateTime StartDate { get; private set; }
        public DateTime EndDate { get; private set; }
        public string BookingStatus { get; private set; }

        public Booking(int bookingID, Guest guest, List<Accommodation> accommodations, int totalGuests, DateTime startDate, DateTime endDate, string bookingStatus)
        {
            Guest = guest ?? throw new ArgumentNullException(nameof(guest), "Guest cannot be null.");
            Accommodations = accommodations ?? throw new ArgumentNullException(nameof(accommodations), "Accommodations cannot be null.");
            BookingStatus = bookingStatus ?? throw new ArgumentNullException(nameof(bookingStatus), "Booking status cannot be null.");

            if (startDate >= endDate)
                throw new ArgumentException("Start date must be earlier than end date.");

            if (totalGuests <= 0)
                throw new ArgumentException("TotalGuests must be at least 1.");

            BookingID = bookingID;
            TotalGuests = totalGuests;
            StartDate = startDate;
            EndDate = endDate;

            ValidateGuestAge();
        }

        private void ValidateGuestAge()
        {
            int age = AgeHelper.CalculateAge(Guest.DateOfBirth); // Fixed - removed GetDateOfBirth()
            if (age < 18)
            {
                throw new InvalidOperationException("Guest must be at least 18 years old to book.");
            }
        }

    }
}

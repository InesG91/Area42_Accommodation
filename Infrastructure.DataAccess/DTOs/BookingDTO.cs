using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.DataAccess.DTOs
{
    public class BookingDTO
    {
        public int BookingID { get; set; }
        public int GuestID { get; set; } // Changed from Guest object to GuestID
        public List<AccommodationDTO> Accommodations { get; set; } = new List<AccommodationDTO>();
        public int TotalGuests { get; set; } // Changed from uint to int for consistency
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string BookingStatus { get; set; } = string.Empty;

        public void Validate()
        {
            if (GuestID <= 0)
                throw new ArgumentException("Valid Guest ID must be provided.");

            if (Accommodations.Count == 0)
                throw new ArgumentException("At least one accommodation must be booked.");

            if (StartDate >= EndDate)
                throw new ArgumentException("Start date must be earlier than end date.");
        }
    }
}

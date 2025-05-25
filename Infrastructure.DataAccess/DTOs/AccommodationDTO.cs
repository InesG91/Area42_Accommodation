using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.DataAccess.DTOs
{
    public class AccommodationDTO
    {
        public int AccommodationID { get; set; }
        public decimal PricePerNight { get; set; }
        public string Subtype { get; set; }
        public uint MaxGuests { get; set; }  // Changed from int to uint

    }
}

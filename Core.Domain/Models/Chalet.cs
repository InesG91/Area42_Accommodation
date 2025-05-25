using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Domain.Models
{
    public class Chalet : Accommodation
    {
        public Chalet(int accommodationID, decimal pricePerNight, string subtype, uint maxGuests)
        : base(accommodationID, pricePerNight, subtype, maxGuests)
        {
        }
    }
}

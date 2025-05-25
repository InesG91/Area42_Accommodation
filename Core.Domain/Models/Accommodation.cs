using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Domain.Models
{
    public abstract class Accommodation
    {
        public int _accommodationID { get; private set; }
        public decimal _pricePerNight { get; private set; }
        public string _subtype { get; private set; }
        public uint _maxGuests { get; private set; }

        protected Accommodation(int accommodationID, decimal pricePerNight, string subtype, uint maxGuests)
        {
            _accommodationID = accommodationID;
            _pricePerNight = pricePerNight;
            _subtype = subtype;
            _maxGuests = maxGuests;
        }

    }
}

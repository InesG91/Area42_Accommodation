using Core.Domain.Interfaces;
using Core.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Domain.Services
{
    public class PricingService 
    {
        private readonly IAccommodationRepository _accommodationRepository;

        public PricingService(IAccommodationRepository accommodationRepository)
        {
            _accommodationRepository = accommodationRepository;
        }

        public decimal CalculateTotalPrice(string roomType, DateTime checkInDate, DateTime checkOutDate)
        {
            var pricePerNight = GetPricePerNight(roomType);
            var numberOfNights = CalculateNumberOfNights(checkInDate, checkOutDate);
            return pricePerNight * numberOfNights;
        }

        public decimal GetPricePerNight(string roomType)
        {
            try
            {
                var accommodations = _accommodationRepository.GetAccommodations();

                if (accommodations != null && accommodations.Any())
                {
                    var selectedAccommodation = accommodations.FirstOrDefault(a =>
                        roomType.Contains(a._subtype, StringComparison.OrdinalIgnoreCase));

                    if (selectedAccommodation != null)
                    {
                        return selectedAccommodation._pricePerNight;
                    }
                }

                // Default fallback price
                return 100.00m;
            }
            catch (Exception)
            {
                return 100.00m;
            }
        }

        public int CalculateNumberOfNights(DateTime checkInDate, DateTime checkOutDate)
        {
            var nights = (checkOutDate - checkInDate).Days;
            return nights <= 0 ? 1 : nights;
        }
    }
}


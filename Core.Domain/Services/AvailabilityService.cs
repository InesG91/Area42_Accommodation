using Core.Domain.Interfaces;
using Core.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Domain.Services
{
    public class AvailabilityService
    {
        private readonly IAvailabilityRepository _availabilityRepository;
        private readonly IAccommodationRepository _accommodationRepository;

        public AvailabilityService(IAvailabilityRepository availabilityRepository, IAccommodationRepository accommodationRepository)
        {
            _availabilityRepository = availabilityRepository;
            _accommodationRepository = accommodationRepository;
        }

        // Main method: Get available accommodations for date range
        public List<Accommodation> GetAvailableAccommodations(DateTime startDate, DateTime endDate, int guestCount = 1)
        {
            // 1. Get accommodation IDs that are available for all days in range
            var availableIds = _availabilityRepository.GetAvailableAccommodations(startDate, endDate);

            // 2. Get full accommodation details (returns domain models)
            var allAccommodations = _accommodationRepository.GetAccommodations();

            // 3. Filter by availability and guest capacity
            var availableAccommodations = allAccommodations
                .Where(a => availableIds.Contains(a._accommodationID)) // ✅ Use underscore property
                .Where(a => a._maxGuests >= guestCount) // ✅ Use underscore property
                .ToList();

            return availableAccommodations; // ✅ Return domain models, not DTOs
        }

        // Reserve dates when booking is created
        public void ReserveAccommodation(int accommodationId, DateTime startDate, DateTime endDate)
        {
            _availabilityRepository.MarkUnavailable(accommodationId, startDate, endDate);
        }
    }
}
using Core.Domain.Interfaces;
using Core.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;

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

       
        public List<Accommodation> GetAvailableAccommodations(DateTime startDate, DateTime endDate, int guestCount)
        {
            // Get list of accommodation IDs that are available for the date range
            var availableIds = _availabilityRepository.GetAvailableAccommodations(startDate, endDate);

            // Get all accommodations as domain objects
            var allAccommodations = _accommodationRepository.GetAccommodations();

            // Filter accommodations by availability AND guest capacity
            var availableAccommodations = allAccommodations
                .Where(a => availableIds.Contains(a._accommodationID))  // Available on these dates
                .Where(a => a._maxGuests >= guestCount)                 // Can fit the guests
                .ToList();

            return availableAccommodations;
        }

        
        public bool IsAvailable(int accommodationId, DateTime startDate, DateTime endDate)
        {
            // Delegate to repository for the actual availability check
            // This could also use _availabilityRepository if you have that method there
            return _accommodationRepository.IsAvailable(accommodationId, startDate, endDate);
        }

        
        public void ValidateAndReserve(int accommodationId, DateTime startDate, DateTime endDate)
        {
            // Check if accommodation is available for the requested dates
            if (!IsAvailable(accommodationId, startDate, endDate))
            {
                throw new InvalidOperationException("Selected accommodation is not available for the chosen dates.");
            }

            // If available, reserve it
            ReserveAccommodation(accommodationId, startDate, endDate);
        }

      
        public void ReserveAccommodation(int accommodationId, DateTime startDate, DateTime endDate)
        {
            _availabilityRepository.MarkUnavailable(accommodationId, startDate, endDate);
        }
    }
}
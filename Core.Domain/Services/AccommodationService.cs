using Core.Domain.Interfaces;
using Core.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Core.Domain.Services
{
    public class AccommodationService
    {
        private readonly IAccommodationRepository _accommodationRepository;

        public AccommodationService(IAccommodationRepository accommodationRepository)
        {
            _accommodationRepository = accommodationRepository;
        }

        public Accommodation ValidateAccommodation(int accommodationId, int numberOfGuests)
        {
            // Get accommodation details
            var accommodation = _accommodationRepository.GetById(accommodationId);

            if (accommodation == null)
            {
                throw new ArgumentException($"Accommodation with ID {accommodationId} not found.");
            }

            // Validate guest capacity
            if (accommodation._maxGuests < numberOfGuests)
            {
                throw new InvalidOperationException(
                    $"Accommodation can only accommodate {accommodation._maxGuests} guests. " +
                    $"You selected {numberOfGuests} guests.");
            }

            return accommodation;
        }

        public int GetAccommodationIdByRoomType(string roomType)
        {
            var accommodations = _accommodationRepository.GetAccommodations();

            if (accommodations == null || !accommodations.Any())
            {
                throw new InvalidOperationException("No accommodations available in the system.");
            }

            // Find accommodation that matches the room type
            var selectedAccommodation = accommodations.FirstOrDefault(a =>
                roomType.Contains(a._subtype, StringComparison.OrdinalIgnoreCase));

            if (selectedAccommodation != null)
            {
                return selectedAccommodation._accommodationID;
            }

            // Fallback - return first accommodation if none found
            // Note: In production, you might want to throw an exception instead
            return accommodations.First()._accommodationID;
        }

       
        public Accommodation? GetAccommodationById(int accommodationId)
        {
            return _accommodationRepository.GetById(accommodationId);
        }

       
        public List<Accommodation> GetAllAccommodations()
        {
            return _accommodationRepository.GetAccommodations();
        }
    }
}

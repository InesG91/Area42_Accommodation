using Core.Domain.Interfaces;
using Core.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Domain.Services
{
    public class BookingService
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IAccommodationRepository _accommodationRepository;
        private readonly IGuestRepository _guestRepository;
        private readonly IAvailabilityRepository _availabilityRepository;

        public BookingService(
            IBookingRepository bookingRepository,
            IAccommodationRepository accommodationRepository,
            IGuestRepository guestRepository,
            IAvailabilityRepository availabilityRepository)
        {
            _bookingRepository = bookingRepository;
            _accommodationRepository = accommodationRepository;
            _guestRepository = guestRepository;
            _availabilityRepository = availabilityRepository;
        }

        /// <summary>
        /// Creates a new booking with guest details
        /// </summary>
        public int CreateBooking(
            string firstName,
            string lastName,
            DateTime dateOfBirth,
            string email,
            int accommodationId,
            DateTime startDate,
            DateTime endDate,
            int numberOfGuests)
        {
            // 1. Validate accommodation is available
            var availableIds = _availabilityRepository.GetAvailableAccommodations(startDate, endDate);
            if (!availableIds.Contains(accommodationId))
            {
                throw new InvalidOperationException("Selected accommodation is not available for the chosen dates.");
            }

            // 2. Get accommodation details
            var accommodation = _accommodationRepository.GetById(accommodationId);
            if (accommodation == null)
            {
                throw new ArgumentException("Accommodation not found.");
            }

            // 3. Validate guest capacity
            if (accommodation._maxGuests < numberOfGuests)
            {
                throw new InvalidOperationException($"Accommodation can only accommodate {accommodation._maxGuests} guests. You selected {numberOfGuests} guests.");
            }

            // 4. Create or get existing guest
            var existingGuest = _guestRepository.GetByEmail(email);
            Guest guest;

            if (existingGuest != null)
            {
                guest = existingGuest;
            }
            else
            {
                // Create new guest
                var newGuest = new Guest(0, firstName, lastName, dateOfBirth, email);
                int guestId = _guestRepository.Save(newGuest);
                guest = new Guest(guestId, firstName, lastName, dateOfBirth, email);
            }

            // 5. Create booking domain object
            var accommodations = new List<Accommodation> { accommodation };
            var booking = new Booking(
                bookingID: 0, // Will be set by database
                guest: guest,
                accommodations: accommodations,
                totalGuests: numberOfGuests,
                startDate: startDate,
                endDate: endDate,
                bookingStatus: "Confirmed"
            );

            // 6. Save booking
            int bookingId = _bookingRepository.CreateBooking(booking);

            // 7. Mark accommodation as unavailable for those dates
            _availabilityRepository.MarkUnavailable(accommodationId, startDate, endDate);

            return bookingId;
        }

        /// <summary>
        /// Gets booking details by ID
        /// </summary>
        public Booking? GetBookingById(int bookingId)
        {
            return _bookingRepository.GetBookingById(bookingId);
        }

        /// <summary>
        /// Gets all bookings for a specific guest
        /// </summary>
        public List<Booking> GetGuestBookings(string email)
        {
            var guest = _guestRepository.GetByEmail(email);
            if (guest == null) return new List<Booking>();

            return _bookingRepository.GetBookingsByGuest(guest.GuestID);
        }
    }
}

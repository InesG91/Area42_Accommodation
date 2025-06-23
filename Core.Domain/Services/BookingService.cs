using Core.Domain.Interfaces;
using Core.Domain.Models;
using Core.Domain.Services;
using System;
using System.Collections.Generic;

namespace Core.Domain.Services
{
    public class BookingService
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly GuestService _guestService;
        private readonly AccommodationService _accommodationService;
        private readonly AvailabilityService _availabilityService;
        private readonly PaymentService _paymentService;

        public BookingService(
            IBookingRepository bookingRepository,
            GuestService guestService,
            AccommodationService accommodationService,
            AvailabilityService availabilityService,
            PaymentService paymentService)
        {
            _bookingRepository = bookingRepository;
            _guestService = guestService;
            _accommodationService = accommodationService;
            _availabilityService = availabilityService;
            _paymentService = paymentService;
        }

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
            // 1. Delegate guest management to GuestService
            int guestId = _guestService.GetOrCreateGuest(firstName, lastName, dateOfBirth, email);
            var guest = _guestService.GetGuestById(guestId);

            // 2. Delegate accommodation validation to AccommodationService
            var accommodation = _accommodationService.ValidateAccommodation(accommodationId, numberOfGuests);

            // 3. Delegate availability checking and reservation to AvailabilityService
            _availabilityService.ValidateAndReserve(accommodationId, startDate, endDate);

            // 4. Create booking domain object (BookingService's actual responsibility)
            var accommodations = new List<Accommodation> { accommodation };
            var booking = new Booking(
                bookingID: 0,
                guest: guest,
                accommodations: accommodations,
                totalGuests: numberOfGuests,
                startDate: startDate,
                endDate: endDate,
                bookingStatus: "Confirmed"
            );

            // 5. Save booking to database
            int bookingId = _bookingRepository.CreateBooking(booking);

            return bookingId;
        }

        public Booking? GetBookingById(int bookingId)
        {
            return _bookingRepository.GetBookingById(bookingId);
        }

        public List<Booking> GetGuestBookings(string email)
        {
            var guest = _guestService.GetGuestByEmail(email);
            if (guest == null) return new List<Booking>();

            return _bookingRepository.GetBookingsByGuest(guest.GuestID);
        }
    }
}

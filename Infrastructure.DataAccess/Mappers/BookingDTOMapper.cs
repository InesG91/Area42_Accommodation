using Core.Domain.Models;
using Infrastructure.DataAccess.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Domain.Models;

namespace Infrastructure.DataAccess.Mappers
{
    public static class BookingDTOMapper
    {
        // Domain → DTO (for saving to database)
        internal static BookingDTO Map(this Booking booking)
        {
            List<AccommodationDTO> accommodationDTOs = new();

            if (booking.Accommodations != null)
            {
                foreach (Accommodation accommodation in booking.Accommodations)
                {
                    accommodationDTOs.Add(accommodation.Map()); // Uses AccommodationDtoMapper
                }
            }

            return new BookingDTO
            {
                BookingID = booking.BookingID,
                GuestID = booking.Guest.GuestID, // Map to GuestID instead of Guest object
                Accommodations = accommodationDTOs,
                TotalGuests = booking.TotalGuests,
                StartDate = booking.StartDate,
                EndDate = booking.EndDate,
                BookingStatus = booking.BookingStatus
            };
        }

        // DTO → Domain (for loading from database)
        internal static Booking Map(this BookingDTO bookingDto, Guest guest, List<Accommodation> accommodations)
        {
            return new Booking(
                bookingDto.BookingID,
                guest,
                accommodations,
                bookingDto.TotalGuests,
                bookingDto.StartDate,
                bookingDto.EndDate,
                bookingDto.BookingStatus
            );
        }

    }
}

using Core.Domain.Models;

namespace Core.Domain.Interfaces
{
    public interface IBookingRepository
    {
        int CreateBooking(Booking booking);
        Booking? GetBookingById(int bookingId);
        List<Booking> GetBookingsByGuest(int guestId);
        void UpdateBookingStatus(int bookingId, string status);
    }
}

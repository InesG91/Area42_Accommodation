using Microsoft.Data.SqlClient;
using Core.Domain.Interfaces;
using Infrastructure.DataAccess.DTOs;

using Core.Domain.Models;
using Infrastructure.DataAccess.Mappers;
using System.Data;
using System.Diagnostics;
using System.Net;
using System;

namespace Infrastructure.DataAccess.Repositories
{
    public class BookingRepository : IBookingRepository
    {
        // ✅ Same connection string across all repositories
        private const string _connectionString = "Data Source=mssqlstud.fhict.local;Initial Catalog=dbi557335_area42;User ID=dbi557335_area42;Password=5Kty4gkkBYNyyYnkXAr6;TrustServerCertificate=True";
        private readonly IGuestRepository _guestRepository;

        // ✅ Proper dependency injection
        public BookingRepository(IGuestRepository guestRepository)
        {
            _guestRepository = guestRepository;
        }

        public int CreateBooking(Booking booking)
        {
            var bookingDto = booking.Map();
            var guestDto = booking.Guest.Map();

            using SqlConnection connection = new(_connectionString);
            connection.Open();
            using SqlTransaction transaction = connection.BeginTransaction();

            try
            {
                Console.WriteLine("DEBUG: Starting booking creation...");

                int guestId = EnsureGuestExists(connection, transaction, guestDto);
                Console.WriteLine($"DEBUG: Guest ID obtained: {guestId}");

                using SqlCommand command = connection.CreateCommand();
                command.Transaction = transaction;
                command.CommandText = "INSERT INTO Bookings (GuestID, AccommodationID, StartDate, EndDate, BookingStatus, AmountOfPeople) " +
                    "OUTPUT INSERTED.BookingID " +
                    "VALUES (@GuestID, @AccommodationID, @StartDate, @EndDate, @BookingStatus, @AmountOfPeople)";

                command.Parameters.AddWithValue("@GuestID", guestId);
                command.Parameters.AddWithValue("@AccommodationID", booking.Accommodations.First()._accommodationID);
                command.Parameters.AddWithValue("@StartDate", bookingDto.StartDate);
                command.Parameters.AddWithValue("@EndDate", bookingDto.EndDate);
                command.Parameters.AddWithValue("@BookingStatus", bookingDto.BookingStatus);
                command.Parameters.AddWithValue("@AmountOfPeople", bookingDto.TotalGuests);

                Console.WriteLine($"DEBUG: About to execute SQL with parameters:");
                Console.WriteLine($"  - GuestID: {guestId}");
                Console.WriteLine($"  - AccommodationID: {booking.Accommodations.First()._accommodationID}");
                Console.WriteLine($"  - StartDate: {bookingDto.StartDate}");
                Console.WriteLine($"  - EndDate: {bookingDto.EndDate}");
                Console.WriteLine($"  - BookingStatus: {bookingDto.BookingStatus}");
                Console.WriteLine($"  - AmountOfPeople: {bookingDto.TotalGuests}");

                int bookingId = (int)command.ExecuteScalar();
                Console.WriteLine($"DEBUG: BookingID returned from database: {bookingId}");

                transaction.Commit();
                Console.WriteLine($"DEBUG: Transaction committed successfully!");

                return bookingId;
            }
            catch (Exception exception)
            {
                Console.WriteLine($"DEBUG: ERROR occurred - {exception.Message}");
                Console.WriteLine($"DEBUG: Full exception: {exception}");
                transaction.Rollback();
                Console.WriteLine($"DEBUG: Transaction rolled back");
                throw;
            }
        }

        public Booking? GetBookingById(int bookingId)
        {
            using SqlConnection connection = new(_connectionString);
            connection.Open();

            using SqlCommand command = connection.CreateCommand();
            command.CommandText = @"SELECT B.BookingID, B.GuestID, B.AmountOfPeople, B.StartDate, B.EndDate, B.BookingStatus
                                   FROM Bookings B 
                                   WHERE B.BookingID = @BookingID";
            command.Parameters.AddWithValue("@BookingID", bookingId);

            using SqlDataReader reader = command.ExecuteReader();
            if (reader.Read())
            {
                var bookingDto = new BookingDTO
                {
                    BookingID = reader.GetInt32("BookingID"),
                    GuestID = reader.GetInt32("GuestID"),
                    TotalGuests = reader.GetInt32("AmountOfPeople"),
                    StartDate = reader.GetDateTime("StartDate"),
                    EndDate = reader.GetDateTime("EndDate"),
                    BookingStatus = reader.GetString("BookingStatus")
                };

                // Get guest through interface (returns domain object)
                Guest? guest = _guestRepository.GetById(bookingDto.GuestID);
                if (guest == null) return null;

                // Get accommodations
                List<Accommodation> accommodations = GetBookingAccommodations(bookingDto.BookingID);

                // Map DTO → Domain
                return bookingDto.Map(guest, accommodations);
            }

            return null;
        }

        public List<Booking> GetBookingsByGuest(int guestId)
        {
            var bookings = new List<Booking>();

            using SqlConnection connection = new(_connectionString);
            connection.Open();
            using SqlCommand command = connection.CreateCommand();
            command.CommandText = "SELECT BookingID FROM Bookings WHERE GuestID = @GuestID";
            command.Parameters.AddWithValue("@GuestID", guestId);

            using SqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                int bookingId = reader.GetInt32("BookingID");
                var booking = GetBookingById(bookingId);
                if (booking != null)
                {
                    bookings.Add(booking);
                }
            }

            return bookings;
        }

        public void UpdateBookingStatus(int bookingId, string status)
        {
            using SqlConnection connection = new(_connectionString);
            connection.Open();
            using SqlCommand command = connection.CreateCommand();
            command.CommandText = "UPDATE Bookings SET BookingStatus = @Status WHERE BookingID = @BookingID";
            command.Parameters.AddWithValue("@Status", status);
            command.Parameters.AddWithValue("@BookingID", bookingId);
            command.ExecuteNonQuery();
        }

        public void DeleteBooking(int bookingId)
        {
            using SqlConnection connection = new(_connectionString);
            connection.Open();

            using SqlTransaction transaction = connection.BeginTransaction();
            try
            {
                // Delete booking accommodations first (foreign key constraint)
                using SqlCommand deleteAccommodationsCommand = connection.CreateCommand();
                deleteAccommodationsCommand.Transaction = transaction;
                deleteAccommodationsCommand.CommandText = "DELETE FROM BookingAccommodations WHERE BookingID = @BookingID";
                deleteAccommodationsCommand.Parameters.AddWithValue("@BookingID", bookingId);
                deleteAccommodationsCommand.ExecuteNonQuery();

                // Delete booking
                using SqlCommand deleteBookingCommand = connection.CreateCommand();
                deleteBookingCommand.Transaction = transaction;
                deleteBookingCommand.CommandText = "DELETE FROM Bookings WHERE BookingID = @BookingID";
                deleteBookingCommand.Parameters.AddWithValue("@BookingID", bookingId);
                deleteBookingCommand.ExecuteNonQuery();

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        private int EnsureGuestExists(SqlConnection connection, SqlTransaction transaction, GuestDTO guest)
        {
            // Check if guest exists by email
            using SqlCommand checkCommand = connection.CreateCommand();
            checkCommand.Transaction = transaction;
            checkCommand.CommandText = "SELECT GuestID FROM Guest WHERE Email = @Email";
            checkCommand.Parameters.AddWithValue("@Email", guest.Email);

            object result = checkCommand.ExecuteScalar();

            if (result != null)
            {
                return (int)result;
            }
            else
            {
                // Insert new guest
                using SqlCommand insertCommand = connection.CreateCommand();
                insertCommand.Transaction = transaction;
                insertCommand.CommandText = "INSERT INTO Guest (FirstName, LastName, DateOfBirth, Email) " +
                    "OUTPUT INSERTED.GuestID " +
                    "VALUES (@FirstName, @LastName, @DateOfBirth, @Email)";

                insertCommand.Parameters.AddWithValue("@FirstName", guest.FirstName);
                insertCommand.Parameters.AddWithValue("@LastName", guest.LastName);
                insertCommand.Parameters.AddWithValue("@DateOfBirth", guest.DateOfBirth);
                insertCommand.Parameters.AddWithValue("@Email", guest.Email);

                return (int)insertCommand.ExecuteScalar();
            }
        }

        private void StoreBookingAccommodations(SqlConnection connection, SqlTransaction transaction, int bookingId, List<AccommodationDTO> accommodations, int totalGuests)
        {
            foreach (AccommodationDTO accommodation in accommodations)
            {
                using SqlCommand accommodationCommand = connection.CreateCommand();
                accommodationCommand.Transaction = transaction;
                accommodationCommand.CommandText = "INSERT INTO BookingAccommodations (BookingID, AccommodationID, TotalGuests) " +
                    "VALUES (@BookingID, @AccommodationID, @TotalGuests)";

                accommodationCommand.Parameters.AddWithValue("@BookingID", bookingId);
                accommodationCommand.Parameters.AddWithValue("@AccommodationID", accommodation.AccommodationID);
                accommodationCommand.Parameters.AddWithValue("@TotalGuests", totalGuests);

                accommodationCommand.ExecuteNonQuery();
            }
        }

        private List<Accommodation> GetBookingAccommodations(int bookingId)
        {
            var accommodations = new List<Accommodation>();

            using SqlConnection connection = new(_connectionString);
            connection.Open();

            using SqlCommand command = connection.CreateCommand();
            command.CommandText = @"SELECT A.AccommodationID, A.PricePerNight, A.Subtype, A.MaxGuests
                                   FROM BookingAccommodations BA
                                   INNER JOIN Accommodation A ON BA.AccommodationID = A.AccommodationID
                                   WHERE BA.BookingID = @BookingID";
            command.Parameters.AddWithValue("@BookingID", bookingId);

            using SqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                var accommodationDto = new AccommodationDTO
                {
                    AccommodationID = reader.GetInt32("AccommodationID"),
                    PricePerNight = reader.GetDecimal("PricePerNight"),
                    Subtype = reader.GetString("Subtype"),
                    MaxGuests = (uint)reader.GetInt32("MaxGuests")
                };

                accommodations.Add(accommodationDto.Map());
            }

            return accommodations;
        }
    }
}

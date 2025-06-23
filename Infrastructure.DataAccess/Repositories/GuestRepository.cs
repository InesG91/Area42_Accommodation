using Microsoft.Data.SqlClient;
using Core.Domain.Interfaces;
using Infrastructure.DataAccess.DTOs;
using Infrastructure.DataAccess.Mappers; 
using Core.Domain.Models;

using System.Data;


namespace Infrastructure.DataAccess.Repositories
{
    public class GuestRepository : IGuestRepository
    {
        private const string _connectionString = "Data Source=mssqlstud.fhict.local;Initial Catalog=dbi557335_area42;User ID=dbi557335_area42;Password=5Kty4gkkBYNyyYnkXAr6;TrustServerCertificate=True";

        // ✅ Matches interface: int Save(Guest guest)
        public int Save(Guest guest)
        {
            var guestDto = guest.Map(); // Domain → DTO

            using SqlConnection connection = new(_connectionString);
            connection.Open();
            using SqlCommand command = connection.CreateCommand();

            // ✅ Fixed table name: "Guest" not "Guests"
            command.CommandText = @"INSERT INTO Guest (FirstName, LastName, DateOfBirth, Email) 
                                   VALUES (@FirstName, @LastName, @DateOfBirth, @Email);
                                   SELECT CAST(SCOPE_IDENTITY() AS int);";

            command.Parameters.AddWithValue("@FirstName", guestDto.FirstName);
            command.Parameters.AddWithValue("@LastName", guestDto.LastName);
            command.Parameters.AddWithValue("@DateOfBirth", guestDto.DateOfBirth);
            command.Parameters.AddWithValue("@Email", guestDto.Email);

            return (int)command.ExecuteScalar();
        }

        // ✅ Matches interface: Guest? GetById(int guestId)
        public Guest? GetById(int guestId)
        {
            using SqlConnection connection = new(_connectionString);
            connection.Open();
            using SqlCommand command = connection.CreateCommand();
            command.CommandText = "SELECT GuestID, FirstName, LastName, DateOfBirth, Email FROM Guest WHERE GuestID = @GuestID";
            command.Parameters.AddWithValue("@GuestID", guestId);

            using SqlDataReader reader = command.ExecuteReader();
            if (reader.Read())
            {
                var guestDto = new GuestDTO
                {
                    GuestID = reader.GetInt32("GuestID"),
                    FirstName = reader.GetString("FirstName"),
                    LastName = reader.GetString("LastName"),
                    DateOfBirth = reader.GetDateTime("DateOfBirth"),
                    Email = reader.GetString("Email")
                };
                return guestDto.Map(); // DTO → Domain
            }
            return null;
        }

        // ✅ Matches interface: Guest? GetByEmail(string email)
        public Guest? GetByEmail(string email)
        {
            using SqlConnection connection = new(_connectionString);
            connection.Open();
            using SqlCommand command = connection.CreateCommand();
            command.CommandText = "SELECT GuestID, FirstName, LastName, DateOfBirth, Email FROM Guest WHERE Email = @Email";
            command.Parameters.AddWithValue("@Email", email);

            using SqlDataReader reader = command.ExecuteReader();
            if (reader.Read())
            {
                var guestDto = new GuestDTO
                {
                    GuestID = reader.GetInt32("GuestID"),
                    FirstName = reader.GetString("FirstName"),
                    LastName = reader.GetString("LastName"),
                    DateOfBirth = reader.GetDateTime("DateOfBirth"),
                    Email = reader.GetString("Email")
                };
                return guestDto.Map(); // DTO → Domain
            }
            return null;
        }
    }
}
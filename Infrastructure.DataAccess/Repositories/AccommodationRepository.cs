using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infrastructure.DataAccess.DTOs;
using Microsoft.Data.SqlClient;
using Core.Domain.Models;
using Infrastructure.DataAccess.Mappers;
using Infrastructure.DataAccess.Repositories;
using Core.Domain.Services;
using Core.Domain.Interfaces;
using System.Data;

namespace Infrastructure.DataAccess.Repositories
{
    public class AccommodationRepository : IAccommodationRepository
    {
        // Replace this with your actual connection string
        private const string _connectionString = "Data Source=mssqlstud.fhict.local;Initial Catalog=dbi557335_area42;User ID=dbi557335_area42;Password=***********;TrustServerCertificate=True";

        public AccommodationRepository()
        {
        }

        public List<Accommodation> GetAccommodations()
        {
            System.Console.WriteLine($"DEBUG: Connection string: {_connectionString}");

            var accommodationDTOs = new List<AccommodationDTO>();
            using SqlConnection connection = new(_connectionString);

            System.Console.WriteLine("DEBUG: About to open connection...");
            connection.Open();
            System.Console.WriteLine("DEBUG: Connection opened successfully!");

            using SqlCommand command = connection.CreateCommand();
            command.CommandText = "SELECT AccommodationID, PricePerNight, Subtype, MaxGuests " +
                "FROM Accommodation ORDER BY Subtype";

            System.Console.WriteLine("DEBUG: About to execute query...");
            using SqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                accommodationDTOs.Add(new AccommodationDTO
                {
                    AccommodationID = reader.GetInt32("AccommodationID"),
                    PricePerNight = reader.GetDecimal("PricePerNight"),
                    Subtype = reader.GetString("Subtype"),
                    MaxGuests = (uint)reader.GetInt32("MaxGuests")
                });
            }

            System.Console.WriteLine($"DEBUG: Found {accommodationDTOs.Count} accommodations");

            // Map DTOs to domain models
            return accommodationDTOs.Select(dto => dto.Map()).ToList();
        }
        

        public Accommodation? GetById(int id)
        {
            AccommodationDTO? accommodationDTO = null;

            using SqlConnection connection = new(_connectionString);
            connection.Open();
            using SqlCommand command = connection.CreateCommand();
            command.CommandText = "SELECT AccommodationID, PricePerNight, Subtype, MaxGuests " +
                "FROM Accommodation " +
                "WHERE AccommodationID = @ID";
            command.Parameters.AddWithValue("@ID", id);

            using SqlDataReader reader = command.ExecuteReader();
            if (reader.Read())
            {
                accommodationDTO = new AccommodationDTO
                {
                    AccommodationID = reader.GetInt32("AccommodationID"),
                    PricePerNight = reader.GetDecimal("PricePerNight"),
                    Subtype = reader.GetString("Subtype"),
                    MaxGuests = (uint)reader.GetInt32("MaxGuests")
                };
            }

            // Map DTO to domain model
            return accommodationDTO?.Map();
        }

        public List<Accommodation> GetByType(string type)
        {
            var accommodationDTOs = new List<AccommodationDTO>();

            using SqlConnection connection = new(_connectionString);
            connection.Open();
            using SqlCommand command = connection.CreateCommand();
            command.CommandText = "SELECT AccommodationID, PricePerNight, Subtype, MaxGuests " +
                "FROM Accommodation " +
                "WHERE Subtype LIKE @Type " +
                "ORDER BY PricePerNight";
            command.Parameters.AddWithValue("@Type", $"%{type}%");

            using SqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                accommodationDTOs.Add(new AccommodationDTO
                {
                    AccommodationID = reader.GetInt32("AccommodationID"),
                    PricePerNight = reader.GetDecimal("PricePerNight"),
                    Subtype = reader.GetString("Subtype"),
                    MaxGuests = (uint)reader.GetInt32("MaxGuests")
                });
            }

            // Map DTOs to domain models
            return accommodationDTOs.Select(dto => dto.Map()).ToList();
        }
    }
}

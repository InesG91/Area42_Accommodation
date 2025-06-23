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
        
        private const string _connectionString = "Data Source=mssqlstud.fhict.local;Initial Catalog=dbi557335_area42;User ID=dbi557335_area42;Password=5Kty4gkkBYNyyYnkXAr6;TrustServerCertificate=True";

        public AccommodationRepository()
        {
        }

        public List<Accommodation> GetAccommodations()
        {
            System.Console.WriteLine($"DEBUG: Connection string: {_connectionString}");

            // Maakt lege lijst van AccommodationDTO objecten
            var accommodationDTOs = new List<AccommodationDTO>();
            using SqlConnection connection = new(_connectionString);

            System.Console.WriteLine("DEBUG: About to open connection...");
            connection.Open();
            System.Console.WriteLine("DEBUG: Connection opened successfully!");

            // Maakt Command object met specifieke velden uit het tabel
            using SqlCommand command = connection.CreateCommand();
            command.CommandText = "SELECT AccommodationID, PricePerNight, Subtype, MaxGuests " +
                "FROM Accommodation ORDER BY Subtype";

            // Een reader om de command object rows uit te lezen
            System.Console.WriteLine("DEBUG: About to execute query...");
            using SqlDataReader reader = command.ExecuteReader();

            // Elke rij wordt gematcht met een attribuut in de DTO-object en een object wordt aangemaakt
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

            // Maakt van de DTO-object een domain-model object van de klasse Accommodation
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

        public bool IsAvailable(int accommodationId, DateTime startDate, DateTime endDate)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                // Check if there are any unavailable dates in the requested range
                string sql = @"
            SELECT COUNT(*) 
            FROM Availability 
            WHERE AccommodationID = @AccommodationId 
            AND StartDate >= @StartDate 
            AND StartDate < @EndDate 
            AND IsAvailable = 0";

                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@AccommodationId", accommodationId);
                    command.Parameters.AddWithValue("@StartDate", startDate);
                    command.Parameters.AddWithValue("@EndDate", endDate);

                    int unavailableDays = (int)command.ExecuteScalar();

                    // If no unavailable days found, it's available
                    return unavailableDays == 0;
                }
            }
        }


    }
}

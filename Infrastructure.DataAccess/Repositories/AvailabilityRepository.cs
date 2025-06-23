using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Domain.Models;
using Infrastructure.DataAccess.Mappers;
using Infrastructure.DataAccess.Repositories;
using Core.Domain.Services;
using Microsoft.Data.SqlClient;
using Core.Domain.Interfaces;
using System.Data;

namespace Infrastructure.DataAccess.Repositories
{
    public class AvailabilityRepository: IAvailabilityRepository
    {
        private const string connectionString = "Data Source=mssqlstud.fhict.local;Initial Catalog=dbi557335_area42;User ID=dbi557335_area42;Password=5Kty4gkkBYNyyYnkXAr6;TrustServerCertificate=True";

        // Get all accommodations that are fully available for the date range
        public List<int> GetAvailableAccommodations(DateTime startDate, DateTime endDate)
        {
            // nieuwe lege lijst aanmaken
            var availableAccommodationIds = new List<int>();

            using SqlConnection connection = new(connectionString);
            connection.Open();
            using SqlCommand command = connection.CreateCommand();

            // Gaat in SQL op zoek naar accommodaties waarbij de Availability = 0, om deze te excluden van de lijst met 
            // resultaten. Uiteindelijk is er alleen een lijst met accommodaties + availability=1 in de datumperiode over.
            command.CommandText = @"
        SELECT DISTINCT a.AccommodationID
        FROM Accommodation a
        WHERE NOT EXISTS (
            SELECT 1 
            FROM Availability av 
            WHERE av.AccommodationID = a.AccommodationID
            AND av.StartDate >= @StartDate 
            AND av.StartDate < @EndDate
            AND av.IsAvailable = 0
        )";

            command.Parameters.AddWithValue("@StartDate", startDate);
            command.Parameters.AddWithValue("@EndDate", endDate);

            // reader gaat door elke row heen en slaat de ID op in de lege lijst.
            using SqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                availableAccommodationIds.Add(reader.GetInt32("AccommodationID"));
            }

            // Opgevulde lijst wordt teruggegeven
            return availableAccommodationIds;
        }

        public void MarkUnavailable(int accommodationId, DateTime startDate, DateTime endDate)
        {
            using SqlConnection connection = new(connectionString);
            connection.Open();
            using SqlCommand command = connection.CreateCommand();

            command.CommandText = @"UPDATE Availability 
                               SET IsAvailable = 0 
                               WHERE AccommodationID = @AccommodationID 
                               AND StartDate >= @StartDate 
                               AND StartDate <= @EndDate";

            command.Parameters.AddWithValue("@AccommodationID", accommodationId);
            command.Parameters.AddWithValue("@StartDate", startDate);
            command.Parameters.AddWithValue("@EndDate", endDate);

            command.ExecuteNonQuery();
        }
    }
}


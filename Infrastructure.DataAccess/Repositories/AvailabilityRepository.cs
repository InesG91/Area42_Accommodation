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

namespace Infrastructure.DataAccess.Repositories
{
    public class AvailabilityRepository: IAvailabilityRepository
    {
        private const string connectionString = "Data Source=mssqlstud.fhict.local;Initial Catalog=dbi557335_area42;User ID=dbi557335_area42;Password=***********;TrustServerCertificate=True";

        // Get all accommodations that are fully available for the date range
        public List<int> GetAvailableAccommodations(DateTime startDate, DateTime endDate)
        {
            var availableAccommodationIds = new List<int>();
            using SqlConnection connection = new(connectionString);
            connection.Open();
            using SqlCommand command = connection.CreateCommand();

            int daysNeeded = (endDate - startDate).Days + 1;

            // Debug: Print the parameters
            Console.WriteLine($"StartDate: {startDate:yyyy-MM-dd}");
            Console.WriteLine($"EndDate: {endDate:yyyy-MM-dd}");
            Console.WriteLine($"DaysNeeded: {daysNeeded}");

            // First, let's test a simpler query
            command.CommandText = @"SELECT TOP 5 AccommodationID, StartDate, IsAvailable 
                           FROM Availability 
                           WHERE IsAvailable = 1";

            using SqlDataReader reader = command.ExecuteReader();
            Console.WriteLine("Raw data from Availability table:");
            while (reader.Read())
            {
                Console.WriteLine($"AccommodationID: {reader.GetValue(0)} (Type: {reader.GetFieldType(0)})");
                Console.WriteLine($"StartDate: {reader.GetValue(1)}");
                Console.WriteLine($"IsAvailable: {reader.GetValue(2)}");
                Console.WriteLine("---");
            }

            return availableAccommodationIds; // Return empty for now
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


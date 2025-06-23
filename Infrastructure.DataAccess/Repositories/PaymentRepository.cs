using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Domain.Interfaces;
using Microsoft.Data.SqlClient;

namespace Infrastructure.DataAccess.Repositories
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly string _connectionString = "Data Source=mssqlstud.fhict.local;Initial Catalog=dbi557335_area42;User ID=dbi557335_area42;Password=5Kty4gkkBYNyyYnkXAr6;TrustServerCertificate=True";

        public int CreatePayment(int bookingId, decimal amount, string paymentStatus)
        {
            using SqlConnection connection = new(_connectionString);
            connection.Open();

            using SqlCommand command = connection.CreateCommand();
            command.CommandText = @"INSERT INTO Accommodation_Payments (BookingID, Amount, PaymentStatus) 
                                   OUTPUT INSERTED.PaymentID 
                                   VALUES (@BookingID, @Amount, @PaymentStatus)";

            command.Parameters.AddWithValue("@BookingID", bookingId);
            command.Parameters.AddWithValue("@Amount", amount);
            command.Parameters.AddWithValue("@PaymentStatus", paymentStatus);

            return (int)command.ExecuteScalar();
        }
    }
}

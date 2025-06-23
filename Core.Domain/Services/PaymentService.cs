using Core.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Domain.Services
{
    public class PaymentService
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly PaymentContext _paymentContext;

        public PaymentService(IPaymentRepository paymentRepository)
        {
            _paymentRepository = paymentRepository;
            _paymentContext = new PaymentContext();
        }

        public string ProcessBookingPayment(int bookingId, decimal amount, string paymentMethod, Dictionary<string, string> paymentDetails)
        {
            try
            {
                // 1. Use Strategy Pattern to validate and process payment
                _paymentContext.SetPaymentStrategy(paymentMethod);
                string strategyResult = _paymentContext.ProcessPayment(amount, paymentDetails);

                // 2. Check if strategy processing was successful
                if (strategyResult.Contains("Error", StringComparison.OrdinalIgnoreCase))
                {
                    return strategyResult; // Return error from strategy
                }

                // 3. If strategy succeeded, save payment record to database
                int paymentId = _paymentRepository.CreatePayment(bookingId, amount, "Confirmed");

                // 4. Return combined success message
                return $"{strategyResult} | Payment saved with ID: {paymentId}";
            }
            catch (ArgumentException ex)
            {
                // Strategy pattern error (unknown payment method)
                return $"Payment method error: {ex.Message}";
            }
            catch (Exception ex)
            {
                // Database or other error
                return $"Payment processing failed: {ex.Message}";
            }
        } 
    }
}


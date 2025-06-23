using Core.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Domain.Services.PaymentStrategies
{
    public class PayPalStrategy : IPaymentStrategy
    {
        public string ProcessPayment(decimal amount, Dictionary<string, string> paymentDetails)
        {
            if (!paymentDetails.ContainsKey("email"))
            {
                return "Error: PayPal email is required";
            }

            string paypalEmail = paymentDetails["email"];

            if (string.IsNullOrEmpty(paypalEmail))
            {
                return "Error: Invalid PayPal email";
            }

            return $"PayPal payment of €{amount:F2} processed successfully for {paypalEmail}";
        }
    }
}

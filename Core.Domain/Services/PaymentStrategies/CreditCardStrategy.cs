using Core.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Domain.Services.PaymentStrategies
{
    public class CreditCardStrategy : IPaymentStrategy
    {
        public string ProcessPayment(decimal amount, Dictionary<string, string> paymentDetails)
        {
            var requiredFields = new[] { "cardName", "cardNumber", "expiry", "cvv" };

            foreach (var field in requiredFields)
            {
                if (!paymentDetails.ContainsKey(field) || string.IsNullOrEmpty(paymentDetails[field]))
                {
                    return $"Error: {field} is required for credit card payment";
                }
            }

            string cardName = paymentDetails["cardName"];
            string cardNumber = paymentDetails["cardNumber"];
            string expiry = paymentDetails["expiry"];
            string cvv = paymentDetails["cvv"];

            if (cardNumber.Replace(" ", "").Length < 13)
            {
                return "Error: Invalid card number";
            }

            if (cvv.Length < 3)
            {
                return "Error: Invalid CVV";
            }

            return $"Credit card payment of €{amount:F2} processed successfully for {cardName} (****{cardNumber.Substring(cardNumber.Length - 4)})";
        }



    }
}

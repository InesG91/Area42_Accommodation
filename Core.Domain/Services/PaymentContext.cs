using Core.Domain.Interfaces;
using Core.Domain.Services.PaymentStrategies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Domain.Services
{
    public class PaymentContext
    {
        private IPaymentStrategy _paymentStrategy;

        public void SetPaymentStrategy(string paymentMethod)
        {
            _paymentStrategy = paymentMethod.ToLower() switch
            {
                "paypal" => new PayPalStrategy(),
                "creditcard" => new CreditCardStrategy(),
                "banktransfer" => new BankTransferStrategy(),
                _ => throw new ArgumentException($"Unknown payment method: {paymentMethod}")
            };
        }

        public string ProcessPayment(decimal amount, Dictionary<string, string> paymentDetails)
        {
            if (_paymentStrategy == null)
            {
                throw new InvalidOperationException("Payment strategy not set. Call SetPaymentStrategy first.");
            }

            return _paymentStrategy.ProcessPayment(amount, paymentDetails);
        }

    }
}

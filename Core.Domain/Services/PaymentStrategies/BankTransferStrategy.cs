using Core.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Domain.Services.PaymentStrategies
{
    public class BankTransferStrategy : IPaymentStrategy
    {
        public string ProcessPayment(decimal amount, Dictionary<string, string> paymentDetails)
        {
            var requiredFields = new[] { "accountHolder", "iban", "bankName" };

            foreach (var field in requiredFields)
            {
                if (!paymentDetails.ContainsKey(field) || string.IsNullOrEmpty(paymentDetails[field]))
                {
                    return $"Error: {field} is required for bank transfer";
                }
            }

            string accountHolder = paymentDetails["accountHolder"];
            string iban = paymentDetails["iban"];
            string bankName = paymentDetails["bankName"];

            string cleanIban = iban.Replace(" ", "").ToUpper();
            if (cleanIban.Length < 15 || !cleanIban.StartsWith("NL"))
            {
                return "Error: Invalid IBAN format";
            }
            return $"Bank transfer of €{amount:F2} initiated for {accountHolder}. " +
                   $"Payment will be processed within 3 business days via {bankName}. " +
                   $"Reference: {cleanIban.Substring(0, 6)}****";
        }
    }
}

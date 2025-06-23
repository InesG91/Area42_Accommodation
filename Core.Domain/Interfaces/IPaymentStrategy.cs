using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Domain.Interfaces
{
    public interface IPaymentStrategy
    {
        string ProcessPayment(decimal amount, Dictionary<string, string> paymentDetails);
    }
}

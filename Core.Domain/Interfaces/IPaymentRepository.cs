using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Domain.Interfaces
{
    public interface IPaymentRepository
    {
        int CreatePayment(int bookingId, decimal amount, string paymentStatus);
    }
}

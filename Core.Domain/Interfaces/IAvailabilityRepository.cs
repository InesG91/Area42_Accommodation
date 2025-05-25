using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Domain.Interfaces
{
    public interface IAvailabilityRepository
    {
        List<int> GetAvailableAccommodations(DateTime startDate, DateTime endDate);
        void MarkUnavailable(int accommodationId, DateTime startDate, DateTime endDate);
    }
}

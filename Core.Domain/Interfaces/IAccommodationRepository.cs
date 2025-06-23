using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Domain.Models;

namespace Core.Domain.Interfaces
{
    public interface IAccommodationRepository
    {
        List<Accommodation> GetAccommodations();
        Accommodation? GetById(int id);
        List<Accommodation> GetByType(string type);

        bool IsAvailable(int accommodationId, DateTime startDate, DateTime endDate);
    }
}

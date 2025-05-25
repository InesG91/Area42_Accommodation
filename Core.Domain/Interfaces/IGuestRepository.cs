using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Domain.Models;

namespace Core.Domain.Interfaces
{
    public interface IGuestRepository
    {
        int Save(Guest Guest);
        Guest? GetById(int guestId);
        Guest? GetByEmail(string email);
       
        //void Delete(int guestId);
    }
}

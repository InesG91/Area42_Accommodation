using Core.Domain.Interfaces;
using Core.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Domain.Services
{
    public class GuestService
    {
        private readonly IGuestRepository _guestRepository;

        public GuestService(IGuestRepository guestRepository)
        {
            _guestRepository = guestRepository;
        }

        /// <summary>
        /// Stores a new guest in the database and returns the generated GuestID
        /// </summary>
        public int StoreGuest(Guest guest)
        {
            return _guestRepository.Save(guest);
        }

        /// <summary>
        /// Retrieves a guest by their ID
        /// </summary>
        public Guest? GetGuestById(int guestId)
        {
            return _guestRepository.GetById(guestId);
        }

        /// <summary>
        /// Retrieves a guest by their email address
        /// </summary>
        public Guest? GetGuestByEmail(string email)
        {
            return _guestRepository.GetByEmail(email);
        }

        /// <summary>
        /// Checks if a guest already exists with the given email
        /// </summary>
        public bool GuestExistsByEmail(string email)
        {
            return _guestRepository.GetByEmail(email) != null;
        }
        
        
        
    }
}

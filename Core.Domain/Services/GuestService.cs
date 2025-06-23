using Core.Domain.Interfaces;
using Core.Domain.Models;
using System;

namespace Core.Domain.Services
{
    public class GuestService
    {
        private readonly IGuestRepository _guestRepository;

        public GuestService(IGuestRepository guestRepository)
        {
            _guestRepository = guestRepository;
        }

        public int GetOrCreateGuest(string firstName, string lastName, DateTime dateOfBirth, string email)
        {
            // First, check if guest already exists by email
            var existingGuest = GetGuestByEmail(email);

            if (existingGuest != null)
            {
                // Guest exists, return their ID
                return existingGuest.GuestID;
            }

            // Guest doesn't exist, create a new one
            // Note: We pass 0 as placeholder ID - SQL Server IDENTITY will generate the real ID
            var newGuest = new Guest(0, firstName, lastName, dateOfBirth, email);

            // Save to database and return the generated ID
            return StoreGuest(newGuest);
        }

       
        public int StoreGuest(Guest guest)
        {
            return _guestRepository.Save(guest);
        }

       
        public Guest? GetGuestById(int guestId)
        {
            return _guestRepository.GetById(guestId);
        }

       
        public Guest? GetGuestByEmail(string email)
        {
            return _guestRepository.GetByEmail(email);
        }

     
        public bool GuestExistsByEmail(string email)
        {
            return _guestRepository.GetByEmail(email) != null;
        }
    }
}
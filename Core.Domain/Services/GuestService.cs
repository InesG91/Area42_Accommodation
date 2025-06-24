using System.Security.Cryptography;
using System.Text;
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

        public (int GuestId, string? GeneratedPassword) GetOrCreateGuest(string firstName, string lastName, DateTime dateOfBirth, string email)
        {
            var existingGuest = GetGuestByEmail(email);
            if (existingGuest != null)
            {
                return (existingGuest.GuestID, null);
            }

            string plainPassword = _guestRepository.GenerateIncrementalPassword();
            string hashedPassword = HashPassword(plainPassword);

            var newGuest = new Guest(0, firstName, lastName, dateOfBirth, email, hashedPassword);
            int newGuestId = StoreGuest(newGuest);

            return (newGuestId, plainPassword);
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

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }
    }
}
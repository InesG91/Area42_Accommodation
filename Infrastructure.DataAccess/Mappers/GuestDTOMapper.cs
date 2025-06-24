using Core.Domain.Models;
using Infrastructure.DataAccess.DTOs;

namespace Infrastructure.DataAccess.Mappers
{
    public static class GuestDTOMapper
    {
        // DTO -> Domain (for login/lookup)
        internal static Guest Map(this GuestDTO dto)
        {
            return new Guest(
                dto.GuestID,
                dto.FirstName,
                dto.LastName,
                dto.DateOfBirth,
                dto.Email,
                dto.PasswordHash
            );
        }

        // Domain → DTO (for registration)
        internal static GuestDTO Map(this Guest guest)
        {
            return new GuestDTO
            {
                GuestID = guest.GuestID,
                FirstName = guest.FirstName,
                LastName = guest.LastName,
                DateOfBirth = guest.DateOfBirth,
                Email = guest.Email,
                PasswordHash = guest.PasswordHash
            };
        }

    }
}

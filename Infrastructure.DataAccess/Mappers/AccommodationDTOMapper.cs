using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Domain.Models;
using Infrastructure.DataAccess.DTOs;

namespace Infrastructure.DataAccess.Mappers
{
    public static class AccommodationDTOMapper
    {
        // DTO → Domain
        internal static Accommodation Map(this AccommodationDTO dto)
        {
            if (dto.Subtype.Contains("Chalet", StringComparison.OrdinalIgnoreCase))
            {
                return new Chalet(dto.AccommodationID, dto.PricePerNight, dto.Subtype, dto.MaxGuests);
            }
            else
            {
                return new Hotel(dto.AccommodationID, dto.PricePerNight, dto.Subtype, dto.MaxGuests);
            }
        }

        // Domain → DTO
        internal static AccommodationDTO Map(this Accommodation accommodation)
        {
            return new AccommodationDTO
            {
                AccommodationID = accommodation._accommodationID,
                PricePerNight = accommodation._pricePerNight,
                Subtype = accommodation._subtype,
                MaxGuests = accommodation._maxGuests
            };
        }
    }
}
    
    


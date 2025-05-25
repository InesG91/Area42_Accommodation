using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Domain.Models;

namespace Core.Domain.Helpers
{
    internal static class AgeHelper
    {
        public static int CalculateAge(DateTime dateOfBirth)
        {
            DateTime today = DateTime.Today;

            if (dateOfBirth > today)
                throw new ArgumentException("Date of birth cannot be in the future."); // 🔥 Prevents invalid input

            int age = today.Year - dateOfBirth.Year;
            if (dateOfBirth.Date > today.AddYears(-age)) age--;

            return age;
        }

    }
}

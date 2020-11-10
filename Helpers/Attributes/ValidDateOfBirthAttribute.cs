using System;
using System.ComponentModel.DataAnnotations;

namespace DatingApp.API.Helpers.Attributes
{
    public class ValidDateOfBirthAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            var now = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0);

            var dateOfBirth = Convert.ToDateTime(value);

            var leapYears = 0;

            for (int i = dateOfBirth.Year; i <= now.Year; i++)
            {
                if (DateTime.IsLeapYear(i))
                {
                    leapYears++;
                }
            }

            TimeSpan timeSpan = now.Subtract(dateOfBirth);
            var days = timeSpan.Days - leapYears;

            return days / 365 >= 18;
        }
    }
}

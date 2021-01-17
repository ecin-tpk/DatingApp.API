using System;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace DatingApp.API.Helpers
{
    public static class Extensions
    {
        public static void AddApplicationError(this HttpResponse response, string message)
        {
            response.Headers.Add("Application-Error", message);
            response.Headers.Add("Access-Control-Expose-Headers", "Application-Error");
            response.Headers.Add("Access-Control-Allow-Origin", "*");
        }

        public static void AddPagination(this HttpResponse response, int currentPage, int itemsPerPage, int totalItems, int totalPages)
        {
            var paginationHeader = new PaginationHeader(currentPage, itemsPerPage, totalItems, totalPages);

            var camelCaseFormatter = new JsonSerializerSettings();
            camelCaseFormatter.ContractResolver = new CamelCasePropertyNamesContractResolver();

            response.Headers.Add("Pagination", JsonConvert.SerializeObject(paginationHeader, camelCaseFormatter));
            response.Headers.Add("Access-Control-Expose-Headers", "Pagination");
        }

        public static int CalculateAge(this DateTime? theDateTime)
        {
            var age = 0;
            if (theDateTime.HasValue)
            {
                age = DateTime.Today.Year - theDateTime.Value.Year;
                if (theDateTime.Value.AddYears(age) > DateTime.Today)
                {
                    age--;
                }
            }

            return age;
        }

        // Set refresh token to cookie for generating new jwt token
        public static void SetTokenCookie(this HttpResponse response, string token)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = DateTime.UtcNow.AddDays(7)
            };
            response.Cookies.Append("refreshToken", token, cookieOptions);
        }

        // Get which quarter a date belongs to
        public static int GetQuarter(this DateTime date)
        {
            return (date.Month + 2) / 3;
        }

        public static DateTime GetLocalTime(this double milliseconds)
        {
            TimeSpan dateTimeSpan = TimeSpan.FromMilliseconds(milliseconds);
            DateTime dateAfterEpoch = new DateTime(1970, 1, 1) + dateTimeSpan;
            return dateAfterEpoch.ToLocalTime();
        }
    }
}
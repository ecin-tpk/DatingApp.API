using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using DatingApp.API.Models.Photos;

namespace DatingApp.API.Models.Account
{
    public class LoginResponse
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Email { get; set; }

        public bool IsVerified { get; set; }

        public DateTime Created { get; set; }

        public DateTime? Updated { get; set; }

        public string Phone { get; set; }

        public string Gender { get; set; }

        public DateTime DateOfBirth { get; set; }

        public DateTime LastActive { get; set; }

        public string Introduction { get; set; }

        public string LookingFor { get; set; }

        public string Interests { get; set; }

        public string City { get; set; }

        public string Country { get; set; }

        public string PhotoUrl { get; set; }

        public string Role { get; set; }

        public string Status { get; set; }

        public string JwtToken { get; set; }

        public ICollection<PhotoResponse> Photos { get; set; }

        // Refresh token is returned in http only cookie
        [JsonIgnore]
        public string RefreshToken { get; set; }
    }
}

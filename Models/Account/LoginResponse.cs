using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using DatingApp.API.Models.Interests;
using DatingApp.API.Models.Photos;

namespace DatingApp.API.Models.Account
{
    public class LoginResponse
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public bool IsVerified { get; set; }
        public string Role { get; set; }
        public string Status { get; set; }
        public string JwtToken { get; set; }

        public string Name { get; set; }
        public string Gender { get; set; }
        public string LookingFor { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string Location { get; set; }
        public string Bio { get; set; }
        public string JobTitle { get; set; }
        public string School { get; set; }
        public string Company { get; set; }
        public string Ethnicity { get; set; }
        public string Religion { get; set; }
        public byte Height { get; set; }
        public byte Weight { get; set; }
        public string SexualOrientation { get; set; }
        public string HairColor { get; set; }
        public string EyeColor { get; set; }
        public string LiveWith { get; set; }
        public string Children { get; set; }
        public string FamilyPlan { get; set; }
        public string Smoking { get; set; }
        public string Drinking { get; set; }
        public bool HideAge { get; set; }

        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public string PhotoUrl { get; set; }
        //public ICollection<InterestForCardResponse> Activities { get; set; }
        public ICollection<PhotoResponse> Photos { get; set; }

        // Refresh token is returned in http only cookie
        [JsonIgnore]
        public string RefreshToken { get; set; }
    }
}

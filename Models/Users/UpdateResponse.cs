using System;
namespace DatingApp.API.Models.Users
{
    public class UpdateResponse
    {
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Role { get; set; }
        public string Status { get; set; }

        public string Name { get; set; }
        public string Gender { get; set; }
        public string LookingFor { get; set; }
        public DateTime DateOfBirth { get; set; }
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
        public string Smoking { get; set; }
        public string Drinking { get; set; }

        public bool HideAge { get; set; }
    }
}

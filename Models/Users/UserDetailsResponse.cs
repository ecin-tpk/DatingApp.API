using System;
namespace DatingApp.API.Models.Users
{
    public class UserDetailsResponse : UserResponse
    {
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
    }
}

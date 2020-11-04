using System;
namespace DatingApp.API.Models.Account
{
    public class FacebookLoginResponse
    {
        public string Name { get; set; }

        public string Email { get; set; }

        public string Gender { get; set; }

        public DateTime? DateOfBirth { get; set; }

        public string City { get; set; }

        public string FacebookUID { get; set; }

        public string Picture { get; set; }
    }
}

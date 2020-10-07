using System;
namespace DatingApp.API.Models.Account
{
    public class FacebookLoginResponse
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Email { get; set; }

        public string Picture { get; set; }
    }
}

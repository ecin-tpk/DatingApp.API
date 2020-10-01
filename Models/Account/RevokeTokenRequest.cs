using System;
namespace DatingApp.API.Models.Account
{
    public class RevokeTokenRequest
    {
        // This token can also be passed in the cookie
        public string Token { get; set; }
    }
}

using System;
using System.ComponentModel.DataAnnotations;

namespace DatingApp.API.Models.Account
{
    public class FacebookLoginRequest
    {
        [Required]
        public string facebookUserId { get; set; }

        [Required]
        public string facebookToken { get; set; }
    }
}

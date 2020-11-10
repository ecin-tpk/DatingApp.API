using System;
using System.ComponentModel.DataAnnotations;

namespace DatingApp.API.Models.Account
{
    public class FacebookLoginRequest
    {
        [Required]
        public string FacebookUserId { get; set; }

        [Required]
        public string FacebookToken { get; set; }
    }
}

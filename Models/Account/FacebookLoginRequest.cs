using System;
using System.ComponentModel.DataAnnotations;

namespace DatingApp.API.Models.Account
{
    public class FacebookLoginRequest
    {
        [Required]
        [StringLength(255)]
        public string facebookToken { get; set; }
    }
}

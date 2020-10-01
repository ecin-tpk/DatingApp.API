using System;
using System.ComponentModel.DataAnnotations;
using DatingApp.API.Entities;

namespace DatingApp.API.Models.Account
{
    public class LoginRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        public Role Role { get; set; }
    }
}

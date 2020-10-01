using System;
using System.ComponentModel.DataAnnotations;

namespace DatingApp.API.Models.Account
{
    public class RegisterRequest
    {
        [Required]
        public string Email { get; set; }

        [Required]
        [Compare("Email")]
        public string ConfirmEmail { get; set; }

        [Required]
        [MinLength(6)]
        public string Password { get; set; }

        [Required]
        [MaxLength(30)]
        public string Name { get; set; }

        [Required]
        public string Gender { get; set; }

        [Required]
        public DateTime DateOfBirth { get; set; }

        [Required]
        public string City { get; set; }

        [Required]
        public string Country { get; set; }
    }
}

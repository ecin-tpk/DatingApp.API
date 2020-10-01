using System;
using System.ComponentModel.DataAnnotations;

namespace DatingApp.API.Models
{
    public class RegisterRequest
    {
        [Required]
        public string Email { get; set; }

        [Required]
        [Compare("Email")]
        public bool ConfirmEmail { get; set; }

        [Required]
        [MinLength(6)]
        public string Password { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Gender { get; set; }

        [Required]
        public DateTime DateOfBirth { get; set; }

        public DateTime Created { get; set; }

        public DateTime LastActive { get; set; }

        public string City { get; set; }

        public string Country { get; set; }
    }
}

using System;
using System.ComponentModel.DataAnnotations;
using DatingApp.API.Helpers.Attributes;

namespace DatingApp.API.Models.Account
{
    public class RegisterRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [MinLength(6)]
        public string Password { get; set; }

        [Required]
        [Compare("Password")]
        public string ConfirmPassword { get; set; }

        [Required]
        [MaxLength(30)]
        public string Name { get; set; }

        //[Required]
        //public string Gender { get; set; }

        //[Required]
        //public string LookingFor { get; set; }

        //[Required]
        //[ValidDateOfBirth(ErrorMessage ="You must be 18 years old or over to register")]
        //public DateTime DateOfBirth { get; set; }

        //[Required]
        //[MaxLength(200)]
        //public string Location { get; set; }
    }
}

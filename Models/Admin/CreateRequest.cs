using System;
using System.ComponentModel.DataAnnotations;
using DatingApp.API.Entities;

namespace DatingApp.API.Models.Admin
{
    public class CreateRequest
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
        [EnumDataType(typeof(Role))]
        public string Role { get; set; }

        [EnumDataType(typeof(Status))]
        public string Status { get; set; }

        [Required]
        public DateTime DateOfBirth { get; set; }
    }
}

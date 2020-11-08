using System;
using System.ComponentModel.DataAnnotations;
using DatingApp.API.Entities;
using DatingApp.API.Helpers;

namespace DatingApp.API.Models.Users
{
    public class UpdateRequest
    {
        private string _email;
        private string _role;

        [MaxLength(15)]
        [RegularExpression("^[0-9]*$", ErrorMessage ="Phone number cannot have any characters")]
        public string Phone { get; set; }

        public string Name { get; set; }
        public string Gender { get; set; }
        public byte Height { get; set; }
        public byte Weight { get; set; }
        public string Interests { get; set; }
        public string SexualOrientation { get; set; }
        public string LookingFor { get; set; }

        [ValidDateOfBirth]
        public DateTime? DateOfBirth { get; set; }

        public string Location { get; set; }
        public string Bio { get; set; }
        public string JobTitle { get; set; }
        public string School { get; set; }
        public string Company { get; set; }
        public string BodyType { get; set; }
        public string HairColor { get; set; }
        public string EyeColor { get; set; }
        public string LiveWith { get; set; }
        public string Children { get; set; }
        public string Smoking { get; set; }
        public string Drinking { get; set; }
        public bool HideAge { get; set; }

        [EnumDataType(typeof(Status))]
        public string Status { get; set; }

        [EmailAddress]
        public string Email { get => _email != null ? _email.ToLower(): _email; set => _email = ReplaceEmptyWithNull(value.ToLower()); }

        [EnumDataType(typeof(Role))]
        public string Role { get => _role; set => _role = ReplaceEmptyWithNull(value); }

        // Replace empty string with null to ensure validation works
        private string ReplaceEmptyWithNull(string value)
        {
            return string.IsNullOrEmpty(value) ? null : value;
        }
    }
}

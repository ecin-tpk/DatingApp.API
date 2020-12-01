using System;
using System.ComponentModel.DataAnnotations;
using DatingApp.API.Entities;
using DatingApp.API.Helpers.Attributes;

namespace DatingApp.API.Models.Users
{
    public class UpdateRequest
    {
        //private string _email;

        //[EmailAddress]
        //public string Email { get => _email != null ? _email.ToLower() : _email; set => _email = ReplaceEmptyWithNull(value.ToLower()); }

        private string _role;

        [EnumDataType(typeof(Role))]
        public string Role { get => _role; set => _role = ReplaceEmptyWithNull(value); }

        [MaxLength(15)]
        [RegularExpression("^[0-9]*$", ErrorMessage ="Invalid phone number")]
        public string Phone { get; set; }

        [EnumDataType(typeof(Status))]
        public string Status { get; set; }

        public string Name { get; set; }
        public string Gender { get; set; }
        public string Interests { get; set; }
        public string LookingFor { get; set; }

        [ValidDateOfBirth(ErrorMessage ="User age must be greater than or equal to 18")]
        public DateTime? DateOfBirth { get; set; }

        public string Location { get; set; }
        public string Bio { get; set; }
        public string JobTitle { get; set; }
        public string School { get; set; }
        public string Company { get; set; }

        public string Ethnicity { get; set; }
        public string Religion { get; set; }
        public byte Height { get; set; }
        public byte Weight { get; set; }
        public string SexualOrientation { get; set; }
        public string HairColor { get; set; }
        public string EyeColor { get; set; }
        public string LiveWith { get; set; }
        public string Children { get; set; }
        public string Smoking { get; set; }
        public string Drinking { get; set; }

        public bool HideAge { get; set; }

        // Replace empty string with null to ensure validation works
        private string ReplaceEmptyWithNull(string value)
        {
            return string.IsNullOrEmpty(value) ? null : value;
        }
    }
}

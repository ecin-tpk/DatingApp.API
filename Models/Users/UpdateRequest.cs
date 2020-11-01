using System;
using System.ComponentModel.DataAnnotations;
using DatingApp.API.Entities;

namespace DatingApp.API.Models.Users
{
    public class UpdateRequest
    {
        private string _email;

        private string _role;

        public string Name { get; set; }

        public string Phone { get; set; }

        public string Gender { get; set; }

        public DateTime? DateOfBirth { get; set; }

        public string Introduction { get; set; }

        public string LookingFor { get; set; }

        public string Interests { get; set; }

        public string City { get; set; }

        public string Country { get; set; }

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

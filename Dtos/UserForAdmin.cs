using System;
using System.Collections.Generic;

namespace DatingApp.API.Dtos
{
    public class UserForAdmin
    {
        public int Id { get; set; }

        public string Email { get; set; }

        public string Phone { get; set; }

        public string Name { get; set; }

        public string Gender { get; set; }

        public DateTime DateOfBirth { get; set; }

        public int Age { get; set; }

        public DateTime Created { get; set; }

        public DateTime LastActive { get; set; }

        public string Introduction { get; set; }

        public string LookingFor { get; set; }

        public string Interests { get; set; }

        public string City { get; set; }

        public string Country { get; set; }

        public string PhotoUrl { get; set; }

        public bool Verified { get; set; }

        public string Status { get; set; }

        public string Role { get; set; }

        public ICollection<PhotosForDetailedDto> Photos { get; set; }
    }
}

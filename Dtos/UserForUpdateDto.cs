using System;
namespace DatingApp.API.Dtos
{
    public class UserForUpdateDto
    {
        public string Email { get; set; }

        public string Phone { get; set; }

        public string Name { get; set; }

        public DateTime DateOfBirth { get; set; }

        public string Introduction { get; set; }

        public string LookingFor { get; set; }

        public string Interests { get; set; }

        public string City { get; set; }

        public string Country { get; set; }
    }
}

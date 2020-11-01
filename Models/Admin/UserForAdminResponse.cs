using System;

namespace DatingApp.API.Models.Admin
{
    public class UserForAdminResponse
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Email { get; set; }

        public bool IsVerified { get; set; }

        public string Phone { get; set; }

        public string Gender { get; set; }

        public DateTime DateOfBirth { get; set; }

        public string Role { get; set; }

        public string Status { get; set; }

        public string PhotoUrl { get; set; }
    }
}

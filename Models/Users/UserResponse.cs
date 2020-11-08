using System;
using System.Collections.Generic;
using DatingApp.API.Models.Photos;

namespace DatingApp.API.Models.Users
{
    public class UserResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Gender { get; set; }
        public string Interests { get; set; }
        public DateTime DateOfBirth { get; set; }
        public DateTime LastActive { get; set; }
        public string Location { get; set; }
        public string Bio { get; set; }
        public string JobTitle { get; set; }
        public string School { get; set; }
        public string Company { get; set; }
        public ICollection<PhotoResponse> Photos { get; set; }
    }
}

using System;
namespace DatingApp.API.Models.Users
{
    // User response for matched users, users that liked me
    public class SimpleUserResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime LastActive { get; set; }
        public string PhotoUrl { get; set; }
        public string Gender { get; set; }
    }
}

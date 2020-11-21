using System;
namespace DatingApp.API.Models.Users
{
    public class MatchedUserResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime LastActive { get; set; }
        public string PhotoUrl { get; set; }
    }
}
